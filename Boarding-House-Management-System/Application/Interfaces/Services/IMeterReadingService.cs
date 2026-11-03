using Application.DTOs.MeterReadings;

namespace Application.Interfaces.Services
{
    public interface IMeterReadingService
    {
        Task<List<MeterReadingResponse>> GetReadingsByHouseAndMonthAsync(int houseId, int month, int year);
        Task<MeterReadingResponse> RecordMeterReadingAsync(RecordMeterReadingRequest request, string? recordedByAccountId = null);
        Task<BulkRecordResponse> BulkRecordMeterReadingsAsync(BulkMeterReadingRequest request, string? recordedByAccountId = null);
        Task<PreviousReadingResponse?> GetPreviousReadingAsync(int roomId, int month, int year);
    }
}
