namespace Application.Strategies.Billing
{
    public class StandardBillingCalculationStrategy : IBillingCalculationStrategy
    {
        public string StrategyName => "Standard";

        public decimal CalculateElectricityCost(decimal usage, decimal unitPrice)
        {
            if (usage <= 0 || unitPrice <= 0) return 0;
            return usage * unitPrice;
        }

        public decimal CalculateWaterCost(decimal usage, decimal unitPrice)
        {
            if (usage <= 0 || unitPrice <= 0) return 0;
            return usage * unitPrice;
        }
    }
}
