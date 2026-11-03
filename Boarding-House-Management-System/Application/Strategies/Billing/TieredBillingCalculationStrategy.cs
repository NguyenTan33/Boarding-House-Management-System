namespace Application.Strategies.Billing
{
    public class TieredBillingCalculationStrategy : IBillingCalculationStrategy
    {
        public string StrategyName => "Tiered";

        // Thuật toán tính tiền điện bậc thang (Tiered algorithm)
        public decimal CalculateElectricityCost(decimal usage, decimal baseUnitPrice)
        {
            if (usage <= 0 || baseUnitPrice <= 0) return 0;

            decimal totalCost = 0;
            decimal remaining = usage;

            // Bậc 1: 0 - 50 kWh (hệ số 0.9)
            decimal tier1Usage = Math.Min(remaining, 50);
            totalCost += tier1Usage * (baseUnitPrice * 0.9m);
            remaining -= tier1Usage;
            if (remaining <= 0) return Math.Round(totalCost, 0);

            // Bậc 2: 51 - 100 kWh (hệ số 1.0)
            decimal tier2Usage = Math.Min(remaining, 50);
            totalCost += tier2Usage * (baseUnitPrice * 1.0m);
            remaining -= tier2Usage;
            if (remaining <= 0) return Math.Round(totalCost, 0);

            // Bậc 3: 101 - 200 kWh (hệ số 1.15)
            decimal tier3Usage = Math.Min(remaining, 100);
            totalCost += tier3Usage * (baseUnitPrice * 1.15m);
            remaining -= tier3Usage;
            if (remaining <= 0) return Math.Round(totalCost, 0);

            // Bậc 4: Trên 200 kWh (hệ số 1.3)
            totalCost += remaining * (baseUnitPrice * 1.3m);

            return Math.Round(totalCost, 0);
        }

        public decimal CalculateWaterCost(decimal usage, decimal baseUnitPrice)
        {
            if (usage <= 0 || baseUnitPrice <= 0) return 0;
            return usage * baseUnitPrice;
        }
    }
}
