using Application.Algorithms;

namespace UnitTests.Algorithms
{
    public class MeterReadingAnomalyDetectorTests
    {
        [Fact]
        public void Detect_WithNormalUsage_ShouldReturnNoAnomaly()
        {
            // Arrange
            decimal currentElec = 60;
            decimal? prevElec = 50;
            decimal currentWater = 12;
            decimal? prevWater = 10;

            // Act
            var result = MeterReadingAnomalyDetector.Detect(currentElec, prevElec, currentWater, prevWater);

            // Assert
            Assert.False(result.IsAnomaly);
            Assert.Null(result.WarningMessage);
        }

        [Fact]
        public void Detect_WhenUsageIsNegative_ShouldDetectAnomaly()
        {
            // Arrange
            decimal currentElec = -10;
            decimal? prevElec = 50;
            decimal currentWater = 10;
            decimal? prevWater = 10;

            // Act
            var result = MeterReadingAnomalyDetector.Detect(currentElec, prevElec, currentWater, prevWater);

            // Assert
            Assert.True(result.IsAnomaly);
            Assert.Contains("Chỉ số điện mới nhỏ hơn chỉ số cũ", result.WarningMessage);
        }

        [Fact]
        public void Detect_WhenElectricitySpikesOver3x_ShouldDetectAnomaly()
        {
            // Arrange
            decimal currentElec = 150; // 3x of 50
            decimal? prevElec = 50;
            decimal currentWater = 10;
            decimal? prevWater = 10;

            // Act
            var result = MeterReadingAnomalyDetector.Detect(currentElec, prevElec, currentWater, prevWater);

            // Assert
            Assert.True(result.IsAnomaly);
            Assert.Contains("tăng đột biến gấp", result.WarningMessage);
        }

        [Fact]
        public void Detect_WhenWaterSpikesOver3x_ShouldDetectAnomaly()
        {
            // Arrange
            decimal currentElec = 50;
            decimal? prevElec = 50;
            decimal currentWater = 30; // 3x of 10
            decimal? prevWater = 10;

            // Act
            var result = MeterReadingAnomalyDetector.Detect(currentElec, prevElec, currentWater, prevWater);

            // Assert
            Assert.True(result.IsAnomaly);
            Assert.Contains("Lượng nước dùng", result.WarningMessage);
            Assert.Contains("tăng đột biến", result.WarningMessage);
        }

        [Fact]
        public void Detect_WhenElectricityExceeds1000KwhWithoutHistory_ShouldDetectAnomaly()
        {
            // Arrange
            decimal currentElec = 1200;
            decimal? prevElec = null;
            decimal currentWater = 10;
            decimal? prevWater = null;

            // Act
            var result = MeterReadingAnomalyDetector.Detect(currentElec, prevElec, currentWater, prevWater);

            // Assert
            Assert.True(result.IsAnomaly);
            Assert.Contains("vượt ngưỡng cao bất thường", result.WarningMessage);
        }

        [Fact]
        public void Detect_WhenWaterExceeds100M3WithoutHistory_ShouldDetectAnomaly()
        {
            // Arrange
            decimal currentElec = 50;
            decimal? prevElec = null;
            decimal currentWater = 150;
            decimal? prevWater = null;

            // Act
            var result = MeterReadingAnomalyDetector.Detect(currentElec, prevElec, currentWater, prevWater);

            // Assert
            Assert.True(result.IsAnomaly);
            Assert.Contains("nguy cơ rò rỉ nước", result.WarningMessage);
        }
    }
}
