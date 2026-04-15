using JK.Messaging.Contracts;

namespace JK.Messaging.Client.Grpc;

public interface IMessagingGrpcClient
{
    Task<MessagingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<MessagingDto>> ListAsync(ListMessagingRequest request,
        CancellationToken cancellationToken = default);

    Task<MessagingDto> CreateAsync(CreateMessagingRequest request, CancellationToken cancellationToken = default);

    Task<MessagingDto?> UpdateAsync(Guid id, UpdateMessagingRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<string> EchoViaOrleansAsync(EchoViaOrleansRequest request, CancellationToken cancellationToken = default);
}
