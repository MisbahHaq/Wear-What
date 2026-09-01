import React from 'react';
import { 
  Sparkles, 
  RotateCw, 
  Maximize2, 
  Lock, 
  Unlock, 
  Plus, 
  Trash2, 
  ArrowRightLeft
} from 'lucide-react';
import { ClothingItem, DailyOutfit, DayOfWeek, DayType } from '../types';
import { DoodleSparkle, NeonStickerBadge, WashiTape } from './DoodleDecorations';

interface DayCardProps {
  outfit: DailyOutfit;
  wardrobe: ClothingItem[];
  dayIndex: number;
  onShuffleDay: (day: DayOfWeek) => void;
  onToggleLock: (day: DayOfWeek) => void;
  onOpenSwapModal: (day: DayOfWeek, slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory', currentItemId?: string) => void;
  onOpenFlatLay: (day: DayOfWeek) => void;
  onChangeDayType: (day: DayOfWeek, newType: DayType) => void;
  onChangeWeather: (day: DayOfWeek) => void;
  onRemoveAccessory: (day: DayOfWeek, accessoryId: string) => void;
  onToggleStatus: (day: DayOfWeek) => void;
}

const ROTATION_ANGLES = ['-rotate-1', 'rotate-1', '-rotate-1.5', 'rotate-1.5', '-rotate-0.5', 'rotate-1', '-rotate-1'];
const WASHI_PATTERNS: ('pink' | 'yellow' | 'cyan' | 'solid-yellow' | 'leopard')[] = ['yellow', 'pink', 'cyan', 'solid-yellow', 'pink', 'yellow', 'cyan'];

export const DayCard: React.FC<DayCardProps> = ({
  outfit,
  wardrobe,
  dayIndex,
  onShuffleDay,
  onToggleLock,
  onOpenSwapModal,
  onOpenFlatLay,
  onChangeDayType,
  onChangeWeather,
  onRemoveAccessory,
  onToggleStatus,
}) => {
  const topItem = wardrobe.find(i => i.id === outfit.topId);
  const bottomItem = wardrobe.find(i => i.id === outfit.bottomId);
  const dressItem = wardrobe.find(i => i.id === outfit.dressId);
  const outerItem = wardrobe.find(i => i.id === outfit.outerwearId);
  const shoesItem = wardrobe.find(i => i.id === outfit.shoesId);
  const accessoryItems = outfit.accessoryIds.map(id => wardrobe.find(i => i.id === id)).filter(Boolean) as ClothingItem[];

  const rotationClass = ROTATION_ANGLES[dayIndex % ROTATION_ANGLES.length];
  const washiPattern = WASHI_PATTERNS[dayIndex % WASHI_PATTERNS.length];

  const getDayTypeColor = (type: DayType) => {
    switch (type) {
      case 'Office Wear':
        return 'bg-[#00D1FF] text-[#111111] border-[#111111]';
      case 'Casual':
        return 'bg-[#FF5C00] text-white border-[#111111]';
      case 'Weekend Wear':
        return 'bg-[#FF007A] text-white border-[#111111]';
    }
  };

  const cycleDayType = () => {
    const types: DayType[] = ['Office Wear', 'Casual', 'Weekend Wear'];
    const currentIdx = types.indexOf(outfit.dayType);
    const nextType = types[(currentIdx + 1) % types.length];
    onChangeDayType(outfit.day, nextType);
  };

  return (
    <div 
      className={`relative bg-white border-3 border-[#111111] chunky-shadow hover:chunky-shadow-lg transition-all duration-200 flex flex-col justify-between p-4 group ${rotationClass} hover:rotate-0 hover:z-10`}
      id={`day-card-${outfit.day.toLowerCase()}`}
    >
      {/* Top Editorial Washi Tape Strip with Multiply effect */}
      <div className="absolute -top-3 left-1/2 -translate-x-1/2 z-20">
        <WashiTape pattern={washiPattern as any} angle={dayIndex % 2 === 0 ? -2 : 3} className="w-24" />
      </div>

      {/* Card Header: Day, Date, Weather & Day Type */}
      <div>
        <div className="flex items-start justify-between gap-1 pt-1 mb-2">
          <div>
            <div className="flex items-center gap-1.5">
              <span className="big-display text-xl sm:text-2xl text-[#111111] tracking-tight">
                {outfit.dayName}
              </span>
              {outfit.isLocked && (
                <span className="bg-[#FFF500] text-[#111111] border border-[#111111] p-0.5 rounded-none text-[10px] font-black" title="Outfit Locked">
                  <Lock className="w-3 h-3" />
                </span>
              )}
            </div>
            <p className="text-[11px] font-mono font-bold text-gray-500">{outfit.dateLabel}</p>
          </div>

          {/* Weather Tag Button */}
          <button
            onClick={() => onChangeWeather(outfit.day)}
            className="flex items-center gap-1 bg-[#FFF9ED] border-2 border-[#111111] px-2 py-0.5 text-xs font-black hover:bg-[#FFF500] active:scale-95 transition-all shadow-[2px_2px_0px_#111111]"
            title="Click to adjust weather"
          >
            <span>{outfit.weather.icon}</span>
            <span>{outfit.weather.tempC}°C</span>
          </button>
        </div>

        {/* Day Type Selector Badge */}
        <div className="flex items-center justify-between gap-1 mb-3">
          <button
            onClick={cycleDayType}
            className={`text-[10px] font-black px-2 py-0.5 border-2 uppercase tracking-wider shadow-[2px_2px_0px_#111111] hover:scale-105 active:scale-95 transition-transform ${getDayTypeColor(
              outfit.dayType
            )}`}
            title="Click to cycle: Office → Casual → Weekend"
          >
            {outfit.dayType} ↺
          </button>

          <div className="flex items-center gap-1">
            <button
              onClick={() => onToggleLock(outfit.day)}
              className={`p-1 border-2 border-[#111111] text-xs font-bold transition-all ${
                outfit.isLocked 
                  ? 'bg-[#FFF500] text-[#111111] shadow-[2px_2px_0px_#111111]' 
                  : 'bg-white text-gray-500 hover:text-black hover:bg-gray-100'
              }`}
              title={outfit.isLocked ? 'Unlock Day' : 'Lock Outfit from Auto-Shuffle'}
            >
              {outfit.isLocked ? <Lock className="w-3.5 h-3.5" /> : <Unlock className="w-3.5 h-3.5" />}
            </button>

            <button
              onClick={() => onShuffleDay(outfit.day)}
              disabled={outfit.isLocked}
              className="p-1 border-2 border-[#111111] bg-[#FFF500] text-[#111111] text-xs font-bold hover:bg-yellow-300 active:scale-90 transition-all shadow-[2px_2px_0px_#111111] disabled:opacity-40 disabled:cursor-not-allowed"
              title="Shuffle Outfit for this day"
            >
              <RotateCw className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>

        {/* Main Outfit Visual Stack (Editorial flat-lay preview) */}
        <div className="space-y-2 mb-3">
          {/* Dress OR Top + Bottom */}
          {dressItem ? (
            <div className="relative bg-[#FFF9ED] border-2 border-[#111111] p-2 shadow-[3px_3px_0px_#111111] group/item">
              <span className="absolute -top-2 left-2 bg-[#FF007A] text-white text-[9px] font-black uppercase px-1.5 py-0.2 border border-[#111111]">
                DRESS
              </span>
              <div className="flex items-center gap-2 mt-1">
                <img 
                  src={dressItem.imageUrl} 
                  alt={dressItem.name}
                  referrerPolicy="no-referrer"
                  className="w-14 h-16 object-cover border border-[#111111]" 
                />
                <div className="flex-1 min-w-0">
                  <p className="text-xs font-black truncate text-[#111111] uppercase tracking-tight">{dressItem.name}</p>
                  <div className="flex items-center gap-1 mt-0.5">
                    <span 
                      className="w-2.5 h-2.5 rounded-full border border-black inline-block" 
                      style={{ backgroundColor: dressItem.colorHex }}
                    />
                    <span className="text-[10px] font-mono text-gray-600 truncate">{dressItem.color}</span>
                  </div>
                  <span className="scribble text-xs text-[#FF007A] font-bold block truncate">
                    #{dressItem.vibeTag || 'Statement'}
                  </span>
                </div>
                <button
                  onClick={() => onOpenSwapModal(outfit.day, 'dress', dressItem.id)}
                  className="opacity-90 sm:opacity-0 group-hover/item:opacity-100 p-1 bg-white border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-[#FFF500] text-black transition-opacity"
                  title="Swap Dress"
                >
                  <ArrowRightLeft className="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          ) : (
            <>
              {/* Top Slot */}
              <div className="relative bg-[#FFF9ED] border-2 border-[#111111] p-2 shadow-[3px_3px_0px_#111111] group/item">
                <span className="absolute -top-2 left-2 bg-[#00D1FF] text-[#111111] text-[9px] font-black uppercase px-1.5 py-0.2 border border-[#111111]">
                  TOP
                </span>
                {topItem ? (
                  <div className="flex items-center gap-2 mt-1">
                    <img 
                      src={topItem.imageUrl} 
                      alt={topItem.name} 
                      referrerPolicy="no-referrer"
                      className="w-13 h-14 object-cover border border-[#111111]" 
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-xs font-black truncate text-[#111111] uppercase tracking-tight">{topItem.name}</p>
                      <div className="flex items-center gap-1 mt-0.5">
                        <span 
                          className="w-2.5 h-2.5 rounded-full border border-black inline-block" 
                          style={{ backgroundColor: topItem.colorHex }}
                        />
                        <span className="text-[10px] font-mono text-gray-600 truncate">{topItem.color}</span>
                      </div>
                    </div>
                    <button
                      onClick={() => onOpenSwapModal(outfit.day, 'top', topItem.id)}
                      className="opacity-90 sm:opacity-0 group-hover/item:opacity-100 p-1 bg-white border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-[#FFF500] text-black transition-opacity"
                      title="Swap Top"
                    >
                      <ArrowRightLeft className="w-3.5 h-3.5" />
                    </button>
                  </div>
                ) : (
                  <button 
                    onClick={() => onOpenSwapModal(outfit.day, 'top')}
                    className="w-full py-2 text-center text-xs font-black uppercase text-gray-500 hover:text-black flex items-center justify-center gap-1"
                  >
                    <Plus className="w-3.5 h-3.5" /> Pick Top
                  </button>
                )}
              </div>

              {/* Bottom Slot */}
              <div className="relative bg-[#FFF9ED] border-2 border-[#111111] p-2 shadow-[3px_3px_0px_#111111] group/item">
                <span className="absolute -top-2 left-2 bg-[#FF5C00] text-white text-[9px] font-black uppercase px-1.5 py-0.2 border border-[#111111]">
                  BOTTOM
                </span>
                {bottomItem ? (
                  <div className="flex items-center gap-2 mt-1">
                    <img 
                      src={bottomItem.imageUrl} 
                      alt={bottomItem.name} 
                      referrerPolicy="no-referrer"
                      className="w-13 h-14 object-cover border border-[#111111]" 
                    />
                    <div className="flex-1 min-w-0">
                      <p className="text-xs font-black truncate text-[#111111] uppercase tracking-tight">{bottomItem.name}</p>
                      <div className="flex items-center gap-1 mt-0.5">
                        <span 
                          className="w-2.5 h-2.5 rounded-full border border-black inline-block" 
                          style={{ backgroundColor: bottomItem.colorHex }}
                        />
                        <span className="text-[10px] font-mono text-gray-600 truncate">{bottomItem.color}</span>
                      </div>
                    </div>
                    <button
                      onClick={() => onOpenSwapModal(outfit.day, 'bottom', bottomItem.id)}
                      className="opacity-90 sm:opacity-0 group-hover/item:opacity-100 p-1 bg-white border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-[#FFF500] text-black transition-opacity"
                      title="Swap Bottom"
                    >
                      <ArrowRightLeft className="w-3.5 h-3.5" />
                    </button>
                  </div>
                ) : (
                  <button 
                    onClick={() => onOpenSwapModal(outfit.day, 'bottom')}
                    className="w-full py-2 text-center text-xs font-black uppercase text-gray-500 hover:text-black flex items-center justify-center gap-1"
                  >
                    <Plus className="w-3.5 h-3.5" /> Pick Bottom
                  </button>
                )}
              </div>
            </>
          )}

          {/* Outerwear Slot */}
          {outerItem && (
            <div className="relative bg-[#FFF9ED] border-2 border-[#111111] p-2 shadow-[3px_3px_0px_#111111] group/item">
              <span className="absolute -top-2 left-2 bg-[#FFF500] text-[#111111] text-[9px] font-black uppercase px-1.5 py-0.2 border border-[#111111]">
                LAYER / JACKET
              </span>
              <div className="flex items-center gap-2 mt-1">
                <img 
                  src={outerItem.imageUrl} 
                  alt={outerItem.name} 
                  referrerPolicy="no-referrer"
                  className="w-11 h-12 object-cover border border-[#111111]" 
                />
                <div className="flex-1 min-w-0">
                  <p className="text-xs font-black truncate text-[#111111] uppercase tracking-tight">{outerItem.name}</p>
                  <span className="text-[10px] font-mono text-gray-600 truncate block">{outerItem.color}</span>
                </div>
                <button
                  onClick={() => onOpenSwapModal(outfit.day, 'outerwear', outerItem.id)}
                  className="opacity-90 sm:opacity-0 group-hover/item:opacity-100 p-1 bg-white border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-[#FFF500] text-black transition-opacity"
                  title="Swap Outerwear"
                >
                  <ArrowRightLeft className="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          )}

          {/* Shoes Slot */}
          <div className="relative bg-[#FFF9ED] border-2 border-[#111111] p-2 shadow-[3px_3px_0px_#111111] group/item">
            <span className="absolute -top-2 left-2 bg-[#111111] text-white text-[9px] font-black uppercase px-1.5 py-0.2 border border-white">
              FOOTWEAR
            </span>
            {shoesItem ? (
              <div className="flex items-center gap-2 mt-1">
                <img 
                  src={shoesItem.imageUrl} 
                  alt={shoesItem.name} 
                  referrerPolicy="no-referrer"
                  className="w-12 h-12 object-cover border border-[#111111]" 
                />
                <div className="flex-1 min-w-0">
                  <p className="text-xs font-black truncate text-[#111111] uppercase tracking-tight">{shoesItem.name}</p>
                  <span className="text-[10px] font-mono text-gray-600 truncate block">{shoesItem.color}</span>
                </div>
                <button
                  onClick={() => onOpenSwapModal(outfit.day, 'shoes', shoesItem.id)}
                  className="opacity-90 sm:opacity-0 group-hover/item:opacity-100 p-1 bg-white border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-[#FFF500] text-black transition-opacity"
                  title="Swap Shoes"
                >
                  <ArrowRightLeft className="w-3.5 h-3.5" />
                </button>
              </div>
            ) : (
              <button 
                onClick={() => onOpenSwapModal(outfit.day, 'shoes')}
                className="w-full py-2 text-center text-xs font-black uppercase text-gray-500 hover:text-black flex items-center justify-center gap-1"
              >
                <Plus className="w-3.5 h-3.5" /> Pick Shoes
              </button>
            )}
          </div>

          {/* Accessories Rail */}
          <div className="bg-[#FAF5E8] border-2 border-dashed border-[#111111] p-2">
            <div className="flex items-center justify-between mb-1.5">
              <span className="text-[10px] font-black text-[#111111] uppercase flex items-center gap-1">
                ✨ Accessories ({accessoryItems.length})
              </span>
              <button
                onClick={() => onOpenSwapModal(outfit.day, 'accessory')}
                className="text-[10px] font-black uppercase bg-[#FFF500] px-1.5 py-0.2 border border-[#111111] shadow-[1px_1px_0px_#111111] hover:bg-yellow-300"
                title="Add Accessory"
              >
                + Add
              </button>
            </div>

            {accessoryItems.length > 0 ? (
              <div className="grid grid-cols-2 gap-1.5">
                {accessoryItems.map(acc => (
                  <div 
                    key={acc.id}
                    className="relative bg-white border border-[#111111] p-1 flex items-center gap-1.5 shadow-[1px_1px_0px_#111111] group/acc"
                  >
                    <img 
                      src={acc.imageUrl} 
                      alt={acc.name} 
                      referrerPolicy="no-referrer"
                      className="w-7 h-7 object-cover border border-[#111111] flex-shrink-0" 
                    />
                    <p className="text-[10px] font-black text-black truncate flex-1">{acc.name}</p>
                    <button
                      onClick={() => onRemoveAccessory(outfit.day, acc.id)}
                      className="text-gray-400 hover:text-red-600 p-0.5"
                      title="Remove Accessory"
                    >
                      <Trash2 className="w-3 h-3" />
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="scribble text-[11px] text-gray-500 text-center py-1">
                No accessories pinned yet
              </p>
            )}
          </div>
        </div>
      </div>

      {/* Card Footer: Flat-Lay Expand Button */}
      <div className="pt-2 border-t-2 border-[#111111] border-dashed flex items-center justify-between gap-2">
        <button
          onClick={() => onToggleStatus(outfit.day)}
          className={`px-2 py-1 border-2 border-[#111111] text-xs font-black uppercase flex items-center gap-1 transition-all ${
            outfit.status === 'loved'
              ? 'bg-[#FF007A] text-white shadow-[2px_2px_0px_#111111]'
              : outfit.status === 'worn'
              ? 'bg-[#00D1FF] text-[#111111] shadow-[2px_2px_0px_#111111]'
              : 'bg-white text-black hover:bg-gray-100 shadow-[2px_2px_0px_#111111]'
          }`}
          title="Toggle Outfit Status"
        >
          {outfit.status === 'loved' ? (
            <>★ Loved</>
          ) : outfit.status === 'worn' ? (
            <>✓ Worn</>
          ) : (
            <>○ Planned</>
          )}
        </button>

        <button
          onClick={() => onOpenFlatLay(outfit.day)}
          className="flex-1 bg-[#FFF500] text-[#111111] border-2 border-[#111111] px-2.5 py-1 text-xs font-black uppercase tracking-wider chunky-shadow hover:bg-yellow-300 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none flex items-center justify-center gap-1.5 transition-all"
        >
          <Maximize2 className="w-3.5 h-3.5" />
          <span>Flat-Lay Board</span>
        </button>
      </div>
    </div>
  );
};

