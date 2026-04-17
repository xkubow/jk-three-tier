namespace JK.Playground.Database.Repositories;

public interface IProductRepository
{
    Task<bool> ExistsAmountAsync(IDictionary<Guid, int> productsToCheck);
}