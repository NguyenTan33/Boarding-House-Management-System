namespace Application.DTOs.MeterReadings
{
    public class RecordMeterReadingRequest
    {
        public int RoomId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal OldElectricity { get; set; }
        public decimal NewElectricity { get; set; }
        public decimal OldWater { get; set; }
        public decimal NewWater { get; set; }
        public string? Note { get; set; }
    }

    public class BulkMeterReadingItem
    {
        public int RoomId { get; set; }
        public decimal OldElectricity { get; set; }
        public decimal NewElectricity { get; set; }
        public decimal OldWater { get; set; }
        public decimal NewWater { get; set; }
        public string? Note { get; set; }
    }

    public class BulkMeterReadingRequest
    {
        public int HouseId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<BulkMeterReadingItem> Readings { get; set; } = new();
    }

    public class MeterReadingResponse
    {
        public int MeterReadingId { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal OldElectricityIndex { get; set; }
        public decimal NewElectricityIndex { get; set; }
        public decimal ElectricityUsage { get; set; }
        public decimal OldWaterIndex { get; set; }
        public decimal NewWaterIndex { get; set; }
        public decimal WaterUsage { get; set; }
        public bool IsBilled { get; set; }
        public bool IsAnomaly { get; set; }
        public string? AnomalyWarning { get; set; }
        public string? Note { get; set; }
    }

    public class BulkRecordResponse
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<MeterReadingResponse> RecordedReadings { get; set; } = new();
    }

    public class PreviousReadingResponse
    {
        public int RoomId { get; set; }
        public int PreviousMonth { get; set; }
        public int PreviousYear { get; set; }
        public decimal LastElectricityIndex { get; set; }
        public decimal LastWaterIndex { get; set; }
    }
}
