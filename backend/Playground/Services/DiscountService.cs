namespace JK.Playground.Services;

public class DiscountService
{
    public decimal Apply(decimal amount, bool premium)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        return premium ? amount * 0.9m : amount;
    }
}