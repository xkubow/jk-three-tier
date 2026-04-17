using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using Grpc.Core;
using JK.Platform.Core.Configuration;
using JK.Platform.Grpc.Client.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Grpc.Client.Decorators;

public abstract class ClientBaseDecorator<TGrpcClient, TClient>
    where TGrpcClient : ClientBase<TGrpcClient>
    where TClient : ClientBaseDecorator<TGrpcClient, TClient>
{
    private readonly IGrpcClientFactory<TGrpcClient> _clientFactory;

    protected TGrpcClient Client => GetClient();
    protected readonly IConfiguration Configuration;
    protected readonly ILogger<TClient> Logger;

    protected abstract string ServerUrlConfigurationKey { get; }

    public readonly string EntryAssemblyName = Assembly.GetEntryAssembly()!.GetName().Name ?? Assembly.GetEntryAssembly()!.GetName().FullName;

    protected ClientBaseDecorator(IGrpcClientFactory<TGrpcClient> clientFactory, IConfiguration configuration, ILogger<TClient> logger)
    {
        _clientFactory = clientFactory;
        Configuration = configuration;
        Logger = logger;
    }

    protected TGrpcClient GetClient(string? customUrl = null)
    {
        var channelUrl = customUrl ?? Configuration.GetRequired(ServerUrlConfigurationKey);
        return _clientFactory.GetClient(channelUrl);
    }

    protected async Task GetDataForExportAsync<TExport, TGrpcExportResponse, TGrpcExport, TId>(
        IObserver<IEnumerable<TExport>> observer,
        CancellationToken cancellationToken,
        int batchSize,
        Func<TId, AsyncServerStreamingCall<TGrpcExportResponse>> getDataForExportFunc,
        Func<TGrpcExportResponse, IEnumerable<TGrpcExport>> getResponseListFunc,
        Func<TExport, TId> getIdFunc,
        IMapper mapper,
        TId? lastId = null,
        [CallerMemberName] string callerName = "")
        where TId : struct
    {
        var sw = Stopwatch.StartNew();
        var finished = false;
        lastId ??= default(TId);
        int counter;

        while (!finished)
        {
            try
            {
                using (var call = getDataForExportFunc(lastId.Value))
                {
                    counter = 0;
                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        var orders = mapper.Map<IEnumerable<TExport>>(getResponseListFunc(response));
                        if (orders.Any())
                        {
                            lastId = getIdFunc(orders.Last());
                            counter += orders.Count();
                            observer.OnNext(orders);
                        }
                    }

                    if (counter == 0)
                    {
                        Console.WriteLine($"Export completed. Elapsed: {sw.Elapsed:c}");
                        finished = true;
                        observer.OnCompleted();
                    }
                    else
                        Console.WriteLine($"Batch completed with lastId: {lastId}. Elapsed: {sw.Elapsed:c}");
                }
            }
            catch (Exception ex)
            {
                finished = true;
                observer.OnError(ex);
            }
        }
    }

    protected async Task GetDataForExportAsync<TExport, TGrpcExportResponse, TGrpcExport>(
        IObserver<IEnumerable<TExport>> observer,
        CancellationToken cancellationToken,
        int batchSize,
        Func<int, AsyncServerStreamingCall<TGrpcExportResponse>> getDataForExportFunc,
        Func<TGrpcExportResponse, IEnumerable<TGrpcExport>> getResponseListFunc,
        Func<TExport, int> getIdFunc,
        IMapper mapper,
        int? lastId = null,
        [CallerMemberName] string callerName = "") => await GetDataForExportAsync<TExport, TGrpcExportResponse, TGrpcExport, int>(
        observer,
        cancellationToken,
        batchSize,
        getDataForExportFunc,
        getResponseListFunc,
        getIdFunc,
        mapper,
        lastId,
        callerName);
}