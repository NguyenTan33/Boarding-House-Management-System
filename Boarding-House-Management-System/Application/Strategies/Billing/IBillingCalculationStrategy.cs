namespace Application.Strategies.Billing
{
    public interface IBillingCalculationStrategy
    {
        string StrategyName { get; }
        decimal CalculateElectricityCost(decimal usage, decimal unitPrice);
        decimal CalculateWaterCost(decimal usage, decimal unitPrice);
    }
}
