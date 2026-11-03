namespace Application.DTOs.Dashboard
{
    public class RoomStatusBreakdownDto
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int ReservedRooms { get; set; }
    }

    public class DashboardSummaryResponse
    {
        public int HouseId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public decimal ActualRevenueCollected { get; set; }
        public decimal TotalUnpaidDebt { get; set; }
        public RoomStatusBreakdownDto RoomStatus { get; set; } = new();
    }
}
