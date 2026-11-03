// ============================================================================
// API & SYSTEM TYPES (Đồng bộ chuẩn xác từ TAILIEU_API_CHO_FRONTEND.md)
// ============================================================================

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiErrorResponse {
  statusCode: number;
  title: string;
  message: string;
  errors?: Record<string, string[]> | null;
  path?: string;
  timestamp?: string;
  retryAfterSeconds?: number;
}

// ----------------------------------------------------------------------------
// MODULE 1: AUTH & IDENTITY
// ----------------------------------------------------------------------------
export interface LoginRequest {
  username: string; // Số điện thoại
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  fullName: string;
  phoneNumber: string;
  citizenId?: string;
  role: 'Tenant' | 'Landlord';
}

export interface UserSummary {
  userId: string;
  fullName: string;
  phoneNumber: string;
  role: 'Tenant' | 'Landlord' | 'Admin';
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: UserSummary;
}

export interface CurrentResidence {
  roomId: number;
  roomNumber: string;
  houseId: number;
  houseName: string;
  houseAddress: string;
  checkInDate: string;
  contractEndDate: string;
  rentalPrice: number;
  depositAmount: number;
  contractNumber: string;
}

export interface UserProfile {
  userId: string;
  userName: string;
  fullName: string;
  phoneNumber: string;
  citizenId?: string;
  email?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  avatarUrl?: string;
  role: 'Tenant' | 'Landlord' | 'Admin';
  isActive: boolean;
  currentResidence?: CurrentResidence;
}

// ----------------------------------------------------------------------------
// MODULE 2: HOUSES & ROOMS
// ----------------------------------------------------------------------------
export interface House {
  houseId: number;
  name: string;
  address: string;
  defaultElectricityPrice: number;
  defaultWaterPrice: number;
  defaultInternetPrice?: number;
  defaultCleaningPrice?: number;
  defaultParkingPrice?: number;
  description?: string;
  totalRooms?: number;
  occupiedRooms?: number;
  availableRooms?: number;
}

export const RoomStatus = {
  Available: 0,
  Occupied: 1,
  Reserved: 2,
  UnderMaintenance: 3,
} as const;

export type RoomStatus = typeof RoomStatus[keyof typeof RoomStatus];

export interface Room {
  roomId: number;
  houseId: number;
  houseName?: string;
  roomNumber: string;
  price: number;
  floor: number;
  size: number;
  maxRenters: number;
  hasBalcony: boolean;
  status: RoomStatus;
  statusName: 'Available' | 'Occupied' | 'Reserved' | 'UnderMaintenance';
  // Client-side display attributes for showcase
  category?: 'studio' | 'duplex' | '1pn';
  imageUrl?: string;
  amenities?: string[];
  description?: string;
}

// ----------------------------------------------------------------------------
// MODULE 3: INVOICES & VIETQR PAYMENTS
// ----------------------------------------------------------------------------
export interface VietQRResponse {
  invoiceId: number;
  invoiceCode: string;
  amount: number;
  bankAccountNo: string;
  bankCode: string;
  bankName: string;
  accountName: string;
  description: string;
  qrImageUrl: string;
  qrQuickLink: string;
}

export interface Invoice {
  invoiceId: number;
  invoiceCode: string;
  contractNumber: string;
  roomNumber: string;
  houseName: string;
  houseAddress: string;
  tenantName: string;
  month: number;
  year: number;
  issueDate: string;
  dueDate: string;
  roomPrice: number;
  electricityUsage: number;
  electricityUnitPrice: number;
  electricityCost: number;
  waterUsage: number;
  waterUnitPrice: number;
  waterCost: number;
  internetCost: number;
  cleaningCost: number;
  parkingCost: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  status: number; // 0: Unpaid, 1: PartiallyPaid, 2: Paid, 3: Overdue, 4: Cancelled
  statusName: string;
}

// ----------------------------------------------------------------------------
// APPOINTMENT / BOOKING
// ----------------------------------------------------------------------------
export interface BookingAppointment {
  fullName: string;
  phoneNumber: string;
  roomPreference: string;
  preferredTime: string;
  note?: string;
}
