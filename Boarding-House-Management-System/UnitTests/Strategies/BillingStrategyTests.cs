using Application.Strategies.Billing;

namespace UnitTests.Strategies
{
    public class BillingStrategyTests
    {
        [Theory]
        [InlineData(100, 3500, 350000)]
        [InlineData(0, 3500, 0)]
        [InlineData(-5, 3500, 0)]
        public void StandardBilling_CalculateElectricityCost_ShouldMultiplyDirectly(decimal usage, decimal unitPrice, decimal expected)
        {
            var strategy = new StandardBillingCalculationStrategy();
            var actual = strategy.CalculateElectricityCost(usage, unitPrice);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(10, 20000, 200000)]
        [InlineData(0, 20000, 0)]
        public void StandardBilling_CalculateWaterCost_ShouldMultiplyDirectly(decimal usage, decimal unitPrice, decimal expected)
        {
            var strategy = new StandardBillingCalculationStrategy();
            var actual = strategy.CalculateWaterCost(usage, unitPrice);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TieredBilling_Tier1_ShouldApply0Point9Multiplier()
        {
            var strategy = new TieredBillingCalculationStrategy();
            // 40 kWh * (3000 * 0.9) = 40 * 2700 = 108000
            var actual = strategy.CalculateElectricityCost(40, 3000);
            Assert.Equal(108000, actual);
        }

        [Fact]
        public void TieredBilling_Tier2_ShouldApplyCalculationsCorrectly()
        {
            var strategy = new TieredBillingCalculationStrategy();
            // 70 kWh: 50 * (3000 * 0.9) + 20 * (3000 * 1.0) = 135000 + 60000 = 195000
            var actual = strategy.CalculateElectricityCost(70, 3000);
            Assert.Equal(195000, actual);
        }

        [Fact]
        public void TieredBilling_Tier3AndTier4_ShouldCalculateAllTiers()
        {
            var strategy = new TieredBillingCalculationStrategy();
            // 250 kWh:
            // Tier 1: 50 * 2700 = 135000
            // Tier 2: 50 * 3000 = 150000
            // Tier 3: 100 * 3450 = 345000
            // Tier 4: 50 * 3900 = 195000
            // Total = 825000
            var actual = strategy.CalculateElectricityCost(250, 3000);
            Assert.Equal(825000, actual);
        }

        [Fact]
        public void BillingStrategyFactory_ShouldResolveExpectedStrategy()
        {
            var standard = new StandardBillingCalculationStrategy();
            var tiered = new TieredBillingCalculationStrategy();
            var factory = new BillingStrategyFactory(new IBillingCalculationStrategy[] { standard, tiered });

            Assert.IsType<StandardBillingCalculationStrategy>(factory.GetStrategy(null));
            Assert.IsType<StandardBillingCalculationStrategy>(factory.GetStrategy("Standard"));
            Assert.IsType<TieredBillingCalculationStrategy>(factory.GetStrategy("Tiered"));
            Assert.IsType<StandardBillingCalculationStrategy>(factory.GetStrategy("Unknown"));
        }
    }
}
