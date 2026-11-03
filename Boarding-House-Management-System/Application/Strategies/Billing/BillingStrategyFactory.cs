namespace Application.Strategies.Billing
{
    public interface IBillingStrategyFactory
    {
        IBillingCalculationStrategy GetStrategy(string? strategyType);
    }

    public class BillingStrategyFactory : IBillingStrategyFactory
    {
        private readonly IEnumerable<IBillingCalculationStrategy> _strategies;

        public BillingStrategyFactory(IEnumerable<IBillingCalculationStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IBillingCalculationStrategy GetStrategy(string? strategyType)
        {
            if (string.IsNullOrWhiteSpace(strategyType))
            {
                return _strategies.FirstOrDefault(s => s.StrategyName.Equals("Standard", StringComparison.OrdinalIgnoreCase))
                    ?? new StandardBillingCalculationStrategy();
            }

            var strategy = _strategies.FirstOrDefault(s => s.StrategyName.Equals(strategyType.Trim(), StringComparison.OrdinalIgnoreCase));
            return strategy ?? new StandardBillingCalculationStrategy();
        }
    }
}
