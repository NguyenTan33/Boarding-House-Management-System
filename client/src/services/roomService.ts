import { apiClient } from './apiClient';
import { type PagedResult, type Room, RoomStatus } from '../types';

// Danh sách phòng thực tế chất lượng cao làm Showcase (KHÔNG HIỆN GIÁ)
export const SHOWCASE_ROOMS: Room[] = [
  {
    roomId: 101,
    houseId: 1,
    houseName: 'GreenHouse Linh Trung',
    roomNumber: 'P.302',
    price: 3600000, // Chỉ lưu ngầm trong state, không render ra ngoài
    floor: 3,
    size: 28,
    maxRenters: 2,
    hasBalcony: true,
    status: RoomStatus.Available,
    statusName: 'Available',
    category: 'studio',
    imageUrl: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=800&q=80',
    amenities: ['Máy lạnh Inverter', 'Ban công đón nắng', 'Nước nóng NLMT', 'Wifi 6 riêng', 'Tủ áo âm tường'],
    description: 'Thiết kế mở hiện đại, đón gió trời và ánh sáng tự nhiên 100%, trang bị đầy đủ nệm cao su, tủ áo âm tường.',
  },
  {
    roomId: 102,
    houseId: 1,
    houseName: 'GreenHouse Linh Trung',
    roomNumber: 'P.204',
    price: 4200000,
    floor: 2,
    size: 34,
    maxRenters: 4,
    hasBalcony: false,
    status: RoomStatus.Available,
    statusName: 'Available',
    category: 'duplex',
    imageUrl: 'https://images.unsplash.com/photo-1554995207-c18c203602cb?auto=format&fit=crop&w=800&q=80',
    amenities: ['Gác lửng đúc cao 2m', 'Kệ bếp đá hoa cương', 'Máy lạnh 1.5HP', 'Khóa số thông minh', 'Giếng trời thoáng'],
    description: 'Phòng duplex rộng rãi cho 2-4 người sinh hoạt thoải mái. Gác cao không đụng đầu, bếp riêng sạch sẽ.',
  },
  {
    roomId: 103,
    houseId: 1,
    houseName: 'GreenHouse Linh Trung',
    roomNumber: 'P.401',
    price: 5200000,
    floor: 4,
    size: 38,
    maxRenters: 3,
    hasBalcony: true,
    status: RoomStatus.Reserved,
    statusName: 'Reserved',
    category: '1pn',
    imageUrl: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=800&q=80',
    amenities: ['1 Phòng ngủ riêng', 'Sofa phòng khách', 'Máy giặt riêng', 'Smart TV 43 inch', 'Ban công giặt phơi'],
    description: 'Căn hộ mini cao cấp tách biệt phòng khách và phòng ngủ. Phù hợp chuyên gia, người đi làm hoặc gia đình nhỏ.',
  },
  {
    roomId: 104,
    houseId: 1,
    houseName: 'GreenHouse Linh Trung',
    roomNumber: 'P.102',
    price: 3200000,
    floor: 1,
    size: 24,
    maxRenters: 2,
    hasBalcony: false,
    status: RoomStatus.Available,
    statusName: 'Available',
    category: 'studio',
    imageUrl: 'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=800&q=80',
    amenities: ['Tầng trệt thuận tiện', 'Máy lạnh Inverter', 'Tủ lạnh 150L', 'Bếp từ mini', 'Cửa sổ kính lớn'],
    description: 'Phòng tầng trệt không cần leo thang, thích hợp cho người có nhiều đồ đạc hoặc thích không gian yên ắng.',
  },
];

export const roomService = {
  // Lấy danh sách phòng từ Backend
  async getRooms(params?: {
    houseId?: number;
    status?: string;
    floor?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedResult<Room>> {
    try {
      const response = await apiClient.get<PagedResult<Room>>('/rooms', { params });
      return response.data;
    } catch {
      // Fallback về dữ liệu mẫu showcase
      return {
        items: SHOWCASE_ROOMS,
        totalCount: SHOWCASE_ROOMS.length,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      };
    }
  },

  // Chi tiết phòng
  async getRoomById(id: number): Promise<Room | null> {
    try {
      const response = await apiClient.get<Room>(`/rooms/${id}`);
      return response.data;
    } catch {
      return SHOWCASE_ROOMS.find((r) => r.roomId === id) || null;
    }
  },
};
