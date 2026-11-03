using Domain.Exceptions;

namespace Domain.Entities
{
    public class House
    {
        public int HouseId { get; private set; }
        public string HouseName { get; private set; }
        public decimal ElectricityUnitPrice { get; private set; }
        public decimal WaterUnitPrice { get; private set; }

        public House() { }
    }
}
