import React, { useState } from 'react';
import { Maximize2, Users, Sun, PhoneCall, CheckCircle2 } from 'lucide-react';
import type { Room } from '../types';

interface RoomShowcaseProps {
  rooms: Room[];
  onSelectRoomForBooking: (roomNumber: string) => void;
}

export const RoomShowcase: React.FC<RoomShowcaseProps> = ({ rooms, onSelectRoomForBooking }) => {
  const [activeTab, setActiveTab] = useState<'all' | 'studio' | 'duplex' | '1pn'>('all');

  const filteredRooms = rooms.filter((room) => {
    if (activeTab === 'all') return true;
    return room.category === activeTab;
  });

  return (
    <section id="khong-gian-phong" className="py-20 bg-slate-100/70 border-y border-slate-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto space-y-3 mb-12">
          <span className="text-xs font-bold text-emerald-700 uppercase tracking-widest bg-emerald-100 px-3.5 py-1.5 rounded-full border border-emerald-200">
            Bộ Sưu Tập Không Gian Sống Thực Tế
          </span>
          <h2 className="text-3xl font-extrabold text-slate-900">
            Khám Phá Các Loại Phòng Tiện Nghi
          </h2>
          <p className="text-slate-600 text-sm sm:text-base">
            Tất cả phòng đều được thiết kế tối ưu ánh sáng tự nhiên, trang bị sẵn nội thất hiện đại và vệ sinh khép kín riêng biệt.
          </p>
        </div>

        {/* Filter Category Tabs */}
        <div className="flex items-center justify-center flex-wrap gap-2.5 mb-10">
          <button
            onClick={() => setActiveTab('all')}
            className={`px-5 py-2.5 rounded-xl font-bold text-xs sm:text-sm transition-all ${
              activeTab === 'all'
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'bg-white text-slate-700 hover:bg-slate-200 border border-slate-200'
            }`}
          >
            Tất Cả Loại Phòng
          </button>
          <button
            onClick={() => setActiveTab('studio')}
            className={`px-5 py-2.5 rounded-xl font-semibold text-xs sm:text-sm transition-all ${
              activeTab === 'studio'
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'bg-white text-slate-700 hover:bg-slate-200 border border-slate-200'
            }`}
          >
            Studio Ban Công Thoáng
          </button>
          <button
            onClick={() => setActiveTab('duplex')}
            className={`px-5 py-2.5 rounded-xl font-semibold text-xs sm:text-sm transition-all ${
              activeTab === 'duplex'
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'bg-white text-slate-700 hover:bg-slate-200 border border-slate-200'
            }`}
          >
            Phòng Gác Lửng Sang Trọng
          </button>
          <button
            onClick={() => setActiveTab('1pn')}
            className={`px-5 py-2.5 rounded-xl font-semibold text-xs sm:text-sm transition-all ${
              activeTab === '1pn'
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'bg-white text-slate-700 hover:bg-slate-200 border border-slate-200'
            }`}
          >
            Căn Hộ Mini 1 Phòng Ngủ
          </button>
        </div>

        {/* Room Grid (Strictly NO Price Displayed) */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {filteredRooms.map((room) => {
            const isAvailable = room.status === 0;
            return (
              <div
                key={room.roomId}
                className="group bg-white rounded-3xl overflow-hidden border border-slate-200/90 shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 flex flex-col"
              >
                {/* Image Frame with badges */}
                <div className="relative aspect-16/10 overflow-hidden bg-slate-200">
                  <img
                    src={room.imageUrl}
                    alt={room.roomNumber}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                  />
                  
                  {/* Status Badge */}
                  <span
                    className={`absolute top-3 left-3 text-white text-[11px] font-bold px-3 py-1 rounded-lg backdrop-blur-xs ${
                      isAvailable ? 'bg-emerald-600/90' : 'bg-amber-600/90'
                    }`}
                  >
                    {isAvailable ? `Còn phòng • Tầng ${room.floor}` : 'Sắp trống • Nhận giữ chỗ'}
                  </span>

                  {/* Size Badge */}
                  <span className="absolute top-3 right-3 bg-slate-900/80 backdrop-blur-xs text-white text-[11px] font-bold px-2.5 py-1 rounded-lg flex items-center gap-1">
                    <Maximize2 className="w-3 h-3" /> {room.size} m²
                  </span>
                </div>

                {/* Content */}
                <div className="p-6 flex-1 flex flex-col justify-between">
                  <div>
                    {/* Floor & Capacity */}
                    <div className="flex items-center gap-3 text-xs font-semibold text-emerald-700 mb-1">
                      <span className="flex items-center gap-1">
                        <Sun className="w-3.5 h-3.5" /> Tầng {room.floor}
                      </span>
                      <span>•</span>
                      <span className="flex items-center gap-1">
                        <Users className="w-3.5 h-3.5" /> Tối đa {room.maxRenters} người
                      </span>
                    </div>

                    <h3 className="text-lg font-bold text-slate-900 group-hover:text-emerald-700 transition-colors">
                      {room.description ? room.description.split('.')[0] : `Phòng ${room.roomNumber}`}
                    </h3>

                    <p className="text-xs text-slate-500 mt-1 line-clamp-2">
                      {room.description || 'Trang bị đầy đủ tiện ích cơ bản, ánh sáng tự nhiên thông thoáng, vệ sinh riêng.'}
                    </p>

                    {/* Amenities Tag Cloud */}
                    <div className="flex flex-wrap gap-1.5 mt-4">
                      {room.amenities?.map((amenity, idx) => (
                        <span
                          key={idx}
                          className="text-[11px] bg-slate-100 text-slate-700 px-2.5 py-1 rounded-md font-medium flex items-center gap-1"
                        >
                          <CheckCircle2 className="w-3 h-3 text-emerald-600" />
                          {amenity}
                        </span>
                      ))}
                    </div>
                  </div>

                  {/* Bottom Action (NO PRICE) */}
                  <div className="mt-6 pt-4 border-t border-slate-100 flex items-center justify-between">
                    <div className="text-xs">
                      <span className="block text-slate-500 font-medium">Tình trạng:</span>
                      <span className="font-bold text-emerald-600">
                        {isAvailable ? 'Sẵn sàng dọn vào' : 'Đang cho thuê'}
                      </span>
                    </div>

                    <button
                      onClick={() => onSelectRoomForBooking(`${room.roomNumber} (${room.size}m² - Tầng ${room.floor})`)}
                      className="px-4 py-2.5 rounded-xl bg-emerald-50 hover:bg-emerald-600 text-emerald-700 hover:text-white font-bold text-xs transition-colors border border-emerald-200 hover:border-emerald-600 flex items-center gap-1.5"
                    >
                      <PhoneCall className="w-3.5 h-3.5" />
                      Liên hệ xem phòng
                    </button>
                  </div>

                </div>

              </div>
            );
          })}
        </div>

      </div>
    </section>
  );
};
