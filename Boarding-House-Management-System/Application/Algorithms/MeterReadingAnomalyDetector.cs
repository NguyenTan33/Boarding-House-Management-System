namespace Application.Algorithms
{
    public class AnomalyDetectionResult
    {
        public bool IsAnomaly { get; set; }
        public string? WarningMessage { get; set; }
    }

    public static class MeterReadingAnomalyDetector
    {
        // Thuật toán phát hiện chỉ số bất thường (So với tháng trước hoặc ngưỡng trung bình)
        public static AnomalyDetectionResult Detect(
            decimal currentElectricityUsage,
            decimal? previousElectricityUsage,
            decimal currentWaterUsage,
            decimal? previousWaterUsage)
        {
            var warnings = new List<string>();

            // 1. Kiểm tra số âm
            if (currentElectricityUsage < 0)
                warnings.Add("Chỉ số điện mới nhỏ hơn chỉ số cũ.");
            if (currentWaterUsage < 0)
                warnings.Add("Chỉ số nước mới nhỏ hơn chỉ số cũ.");

            // 2. Phát hiện đột biến điện (> 3 lần tháng trước và mức dùng tối thiểu > 20 kWh)
            if (previousElectricityUsage.HasValue && previousElectricityUsage.Value > 20)
            {
                if (currentElectricityUsage >= previousElectricityUsage.Value * 3)
                {
                    warnings.Add($"Lượng điện dùng ({currentElectricityUsage} kWh) tăng đột biến gấp {(currentElectricityUsage / previousElectricityUsage.Value):F1} lần so với tháng trước ({previousElectricityUsage.Value} kWh).");
                }
            }
            else if (currentElectricityUsage > 1000)
            {
                warnings.Add($"Lượng điện dùng vượt ngưỡng cao bất thường ({currentElectricityUsage} kWh). Vui lòng kiểm tra lại đồng hồ.");
            }

            // 3. Phát hiện đột biến nước (> 3 lần tháng trước và mức dùng tối thiểu > 5 m3)
            if (previousWaterUsage.HasValue && previousWaterUsage.Value > 5)
            {
                if (currentWaterUsage >= previousWaterUsage.Value * 3)
                {
                    warnings.Add($"Lượng nước dùng ({currentWaterUsage} m3) tăng đột biến gấp {(currentWaterUsage / previousWaterUsage.Value):F1} lần so với tháng trước ({previousWaterUsage.Value} m3).");
                }
            }
            else if (currentWaterUsage > 100)
            {
                warnings.Add($"Lượng nước dùng vượt ngưỡng cao bất thường ({currentWaterUsage} m3). Vui lòng kiểm tra nguy cơ rò rỉ nước.");
            }

            return new AnomalyDetectionResult
            {
                IsAnomaly = warnings.Count > 0,
                WarningMessage = warnings.Count > 0 ? string.Join(" | ", warnings) : null
            };
        }
    }
}
