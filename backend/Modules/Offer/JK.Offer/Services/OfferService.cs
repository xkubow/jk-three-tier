using AutoMapper;
using JK.Offer.Configurations;
using JK.Offer.Contracts;
using JK.Offer.Database.Repositories;
using JK.Offer.Database;
using JK.Platform.Persistence.EfCore;
using JK.Offer.Models;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JK.Offer.Services;

[Injectable(ServiceLifetime.Scoped)]
public class OfferService : IOfferService
{
    private readonly IUnitOfWork<OfferDbContext> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IOptionsSnapshot<OfferConfiguration> _configuration;

    public OfferService(IUnitOfWorkFactory<OfferDbContext> unitOfWorkFactory, IMapper mapper, IOptionsSnapshot<OfferConfiguration> configuration)
    {
        _unitOfWork = unitOfWorkFactory.Create();
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<OfferDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.GetRepository<IOfferRepository>().GetByIdAsync(id, cancellationToken);
        return _mapper.Map<OfferDto>(model);
    }

    public async Task<PagedResponse<OfferDto>> ListAsync(ListOffersRequest request, CancellationToken cancellationToken = default)
    {
        var pagedResponse = await _unitOfWork.GetRepository<IOfferRepository>().ListAsync(request, cancellationToken);
        return _mapper.Map<PagedResponse<OfferDto>>(pagedResponse);
    }

    public async Task<OfferDto> CreateAsync(CreateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var model = new OfferModel
        {
            Id = Guid.NewGuid(),
            OfferNumber = request.Number.Trim(),
            TotalAmount = request.TotalAmount,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt
        };

        await _unitOfWork.GetRepository<IOfferRepository>().AddAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var insertedModel = await _unitOfWork.GetRepository<IOfferRepository>().GetByIdAsync(model.Id, cancellationToken);
        return _mapper.Map<OfferDto>(insertedModel);
    }

    public async Task<OfferDto?> UpdateAsync(Guid id, UpdateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var model = await _unitOfWork.GetRepository<IOfferRepository>().GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        model.Status = request.Status.Trim();
        model.TotalAmount = request.TotalAmount;
        model.ExpiresAt = request.ExpiresAt;
        model.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.GetRepository<IOfferRepository>().UpdateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedModel = await _unitOfWork.GetRepository<IOfferRepository>().GetByIdAsync(id, cancellationToken);
        return _mapper.Map<OfferDto>(updatedModel);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.GetRepository<IOfferRepository>().ExistsAsync(id, cancellationToken);
        if (!exists) return false;

        await _unitOfWork.GetRepository<IOfferRepository>().DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public void Test()
    {
        Console.WriteLine("Test from Offer Service");
    }
}
