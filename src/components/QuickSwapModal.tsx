import React, { useState } from 'react';
import { X, Search, Check, Sparkles, Filter, Trash2 } from 'lucide-react';
import { ClothingCategory, ClothingItem, DayOfWeek, StyleClass } from '../types';
import { NeonStickerBadge, WashiTape } from './DoodleDecorations';

interface QuickSwapModalProps {
  isOpen: boolean;
  onClose: () => void;
  day: DayOfWeek;
  slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory';
  currentItemId?: string;
  wardrobe: ClothingItem[];
  onSelectItem: (day: DayOfWeek, slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory', item: ClothingItem) => void;
  onClearSlot: (day: DayOfWeek, slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory') => void;
}

export const QuickSwapModal: React.FC<QuickSwapModalProps> = ({
  isOpen,
  onClose,
  day,
  slot,
  currentItemId,
  wardrobe,
  onSelectItem,
  onClearSlot,
}) => {
  if (!isOpen) return null;

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStyle, setSelectedStyle] = useState<string>('all');
  const [selectedSeason, setSelectedSeason] = useState<string>('all');

  // Filter items matching the requested slot category
  const slotCategory: ClothingCategory = 
    slot === 'top' ? 'top' :
    slot === 'bottom' ? 'bottom' :
    slot === 'dress' ? 'dress' :
    slot === 'outerwear' ? 'outerwear' :
    slot === 'shoes' ? 'shoes' : 'accessory';

  const categoryItems = wardrobe.filter(item => {
    // Exact category match or if slot is top/dress, also allow dress/top swapping
    if (slot === 'dress') return item.category === 'dress';
    if (slot === 'top') return item.category === 'top';
    return item.category === slotCategory;
  });

  const filteredItems = categoryItems.filter(item => {
    const matchesSearch = 
      item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.color.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (item.vibeTag && item.vibeTag.toLowerCase().includes(searchQuery.toLowerCase())) ||
      (item.subCategory && item.subCategory.toLowerCase().includes(searchQuery.toLowerCase()));

    const matchesStyle = selectedStyle === 'all' || item.styleClass === selectedStyle;
    const matchesSeason = selectedSeason === 'all' || item.season === selectedSeason || item.season === 'all-season';

    return matchesSearch && matchesStyle && matchesSeason;
  });

  const getSlotTitle = () => {
    switch (slot) {
      case 'top': return 'Top / Shirt';
      case 'bottom': return 'Bottom / Pants / Skirt';
      case 'dress': return 'Dress / One-Piece';
      case 'outerwear': return 'Outerwear / Jacket / Coat';
      case 'shoes': return 'Shoes / Kicks / Heels';
      case 'accessory': return 'Accessory / Bag / Jewelry';
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 bg-black/75 backdrop-blur-sm overflow-y-auto">
      <div className="relative w-full max-w-3xl bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="bg-[#FF5C00] border-b-4 border-[#111111] px-4 sm:px-6 py-3 flex items-center justify-between gap-3 text-white">
          <div className="flex items-center gap-2">
            <h3 className="big-display text-xl sm:text-2xl tracking-wide">
              SWAP {getSlotTitle()}
            </h3>
            <span className="bg-[#FFF500] text-[#111111] font-black text-[11px] px-2 py-0.5 border border-[#111111] uppercase shadow-[1px_1px_0px_#111111]">
              {day}
            </span>
          </div>

          <button
            onClick={onClose}
            className="w-8 h-8 bg-[#111111] text-white border-2 border-white flex items-center justify-center hover:bg-gray-800 transition-colors"
          >
            <X className="w-5 h-5 stroke-[3]" />
          </button>
        </div>

        {/* Filters & Search Toolbar */}
        <div className="bg-white border-b-2 border-[#111111] p-3 sm:px-6 flex flex-wrap items-center justify-between gap-3">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder={`Search ${getSlotTitle()} by name, color, vibe...`}
              className="w-full pl-9 pr-3 py-1.5 bg-[#FFF9ED] border-2 border-[#111111] text-xs font-mono focus:outline-none focus:bg-white"
            />
          </div>

          {/* Style Filter */}
          <div className="flex items-center gap-1.5 flex-wrap text-xs">
            <span className="font-black uppercase text-gray-700">Style:</span>
            {['all', 'Office Wear', 'Casual', 'Weekend Wear'].map(st => (
              <button
                key={st}
                onClick={() => setSelectedStyle(st)}
                className={`px-2 py-1 text-[11px] font-black uppercase border border-[#111111] transition-all ${
                  selectedStyle === st 
                    ? 'bg-[#00D1FF] text-[#111111] chunky-shadow' 
                    : 'bg-[#FFF9ED] text-black hover:bg-gray-100'
                }`}
              >
                {st === 'all' ? 'All Styles' : st}
              </button>
            ))}
          </div>
        </div>

        {/* Items Grid */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 bg-[#FFF9ED]">
          {filteredItems.length > 0 ? (
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3.5">
              {filteredItems.map(item => {
                const isSelected = item.id === currentItemId;
                return (
                  <div
                    key={item.id}
                    onClick={() => {
                      onSelectItem(day, slot, item);
                      onClose();
                    }}
                    className={`cursor-pointer group relative bg-white border-2 border-[#111111] p-2.5 transition-all hover:-translate-y-1 ${
                      isSelected 
                        ? 'ring-4 ring-[#FF007A] bg-[#FFF0F5] chunky-shadow-lg' 
                        : 'chunky-shadow'
                    }`}
                  >
                    {isSelected && (
                      <span className="absolute top-2 right-2 bg-[#FF007A] text-white p-1 border border-[#111111] shadow-[1px_1px_0px_#111111] z-10">
                        <Check className="w-3.5 h-3.5 stroke-[3]" />
                      </span>
                    )}

                    <div className="relative aspect-square w-full mb-2 overflow-hidden border border-[#111111] bg-gray-50">
                      <img
                        src={item.imageUrl}
                        alt={item.name}
                        referrerPolicy="no-referrer"
                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
                      />
                      <span className="absolute bottom-1 left-1 bg-black/80 text-white text-[9px] font-mono px-1 py-0.2">
                        {item.styleClass}
                      </span>
                    </div>

                    <h5 className="font-black uppercase text-xs text-[#111111] truncate">{item.name}</h5>
                    <div className="flex items-center justify-between mt-1">
                      <div className="flex items-center gap-1">
                        <span 
                          className="w-2.5 h-2.5 border border-[#111111] inline-block"
                          style={{ backgroundColor: item.colorHex }}
                        />
                        <span className="text-[10px] font-mono text-gray-600 truncate">{item.color}</span>
                      </div>
                      <span className="text-[9px] font-mono bg-[#FFF500] px-1 border border-[#111111]">
                        {item.season}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="text-center py-12">
              <p className="big-display text-lg text-gray-700">No matching items found in your closet!</p>
              <p className="text-xs font-mono text-gray-500 mt-1">Try changing search filters or upload more items.</p>
            </div>
          )}
        </div>

        {/* Footer Actions */}
        <div className="bg-white border-t-3 border-[#111111] p-3 px-6 flex items-center justify-between gap-3">
          {currentItemId ? (
            <button
              onClick={() => {
                onClearSlot(day, slot);
                onClose();
              }}
              className="text-red-600 hover:text-red-800 text-xs font-black uppercase flex items-center gap-1.5 px-3 py-1.5 border border-red-300 hover:border-red-600"
            >
              <Trash2 className="w-4 h-4" />
              <span>Remove item from this slot</span>
            </button>
          ) : <div />}

          <button
            onClick={onClose}
            className="bg-[#111111] text-white px-5 py-2 font-black uppercase text-xs chunky-shadow hover:bg-gray-800"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
};
