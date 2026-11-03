using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Rooms
    {
        public string HouseId { get; private set; }
        public int RoomNumbers { get; private set; }
        public double Price { get; private set; }
        public int Floor { get; private set; }
        public int Size { get; private set; }
        public int MaxRenters { get; private set; }
        public int HasBalcony { get; private set; }
        public int IsActive { get; private set; }

        private Rooms() { }

        public Rooms(string houseId, int roomNumbers, double price, int floor, int size, int maxRenters, int hasBalcony, int isActive)
        {
            if (RoomNumbers < 0)
            {
                throw new Exception();
            }
            HouseId = houseId;
            RoomNumbers = roomNumbers;
            Price = price;
            Floor = floor;
            Size = size;
            MaxRenters = maxRenters;
            HasBalcony = hasBalcony;
            IsActive = isActive;
        }
    }
}
