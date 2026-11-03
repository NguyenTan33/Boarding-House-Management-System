using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IMeterReadingRepository : IGenericRepository<MeterReading>
    {
        Task<MeterReading?> GetPreviousReadingAsync(int roomId, int currentMonth, int currentYear);
        Task<List<MeterReading>> GetReadingsByHouseAndMonthAsync(int houseId, int month, int year);
        Task<MeterReading?> GetReadingByRoomAndMonthAsync(int roomId, int month, int year);
    }
}
