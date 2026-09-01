import React, { useState } from 'react';
import { 
  ChevronUp, 
  ChevronDown, 
  Sparkles, 
  Plus, 
  Check, 
  Tag, 
  Layers,
  Search,
  Pin
} from 'lucide-react';
import { ClothingItem, DayOfWeek } from '../types';
import { DAYS_OF_WEEK } from '../utils/outfitGenerator';
import { NeonStickerBadge, WashiTape } from './DoodleDecorations';

interface AccessoriesDrawerProps {
  wardrobe: ClothingItem[];
  onAddAccessoryToDay: (day: DayOfWeek, accessory: ClothingItem) => void;
  onOpenUploadModal: () => void;
}

export const AccessoriesDrawer: React.FC<AccessoriesDrawerProps> = ({
  wardrobe,
  onAddAccessoryToDay,
  onOpenUploadModal,
}) => {
  const [isOpen, setIsOpen] = useState(true);
  const [selectedSubtype, setSelectedSubtype] = useState<string>('all');
  const [activeAssignItem, setActiveAssignItem] = useState<ClothingItem | null>(null);
  const [justAddedDay, setJustAddedDay] = useState<DayOfWeek | null>(null);

  const accessories = wardrobe.filter(
    i => i.category === 'accessory' || i.styleClass === 'Accessories & Add-ons'
  );

  const filteredAccessories = accessories.filter(acc => {
    if (selectedSubtype === 'all') return true;
    return acc.subCategory?.toLowerCase() === selectedSubtype.toLowerCase();
  });

  const handleAssign = (day: DayOfWeek, acc: ClothingItem) => {
    onAddAccessoryToDay(day, acc);
    setJustAddedDay(day);
    setTimeout(() => {
      setJustAddedDay(null);
      setActiveAssignItem(null);
    }, 1200);
  };

  return (
    <div className="fixed bottom-0 left-0 right-0 z-30 pointer-events-none">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 pointer-events-auto">
        <div className="bg-[#FFF9ED] border-t-4 border-x-4 border-[#111111] chunky-shadow-xl overflow-hidden transition-all duration-300">
          {/* Drawer Header Toggle */}
          <div 
            onClick={() => setIsOpen(!isOpen)}
            className="bg-[#FFF500] border-b-2 border-[#111111] px-4 py-2 flex items-center justify-between cursor-pointer hover:bg-[#ffe600] select-none"
          >
            <div className="flex items-center gap-2">
              <div className="bg-[#FF007A] text-white p-1 border border-[#111111] -rotate-6">
                <Sparkles className="w-3.5 h-3.5" />
              </div>
              <span className="big-display text-sm sm:text-base text-[#111111] tracking-wide">
                ACCESSORIES & ADD-ONS DRAWER
              </span>
              <span className="bg-[#111111] text-white text-[11px] font-mono font-bold px-2 py-0.2">
                {accessories.length} PIECES
              </span>
              <span className="hidden md:inline scribble text-xs text-[#FF007A] font-bold">
                (Click to pin onto any day's moodboard)
              </span>
            </div>

            <div className="flex items-center gap-2">
              <span className="text-xs font-black uppercase text-[#111111] hidden sm:inline">
                {isOpen ? 'Minimize Rail' : 'Expand Drawer'}
              </span>
              <div className="w-6 h-6 bg-white border-2 border-[#111111] flex items-center justify-center chunky-shadow">
                {isOpen ? <ChevronDown className="w-4 h-4 stroke-[3]" /> : <ChevronUp className="w-4 h-4 stroke-[3]" />}
              </div>
            </div>
          </div>

          {/* Drawer Content */}
          {isOpen && (
            <div className="p-3 bg-[#FFF9ED]">
              {/* Filter Subtypes */}
              <div className="flex items-center justify-between gap-2 mb-2 overflow-x-auto pb-1">
                <div className="flex items-center gap-1.5 flex-shrink-0">
                  {['all', 'bag', 'jewelry', 'sunglasses', 'scarf', 'belt', 'watch'].map(sub => (
                    <button
                      key={sub}
                      onClick={() => setSelectedSubtype(sub)}
                      className={`px-2.5 py-1 text-xs font-black uppercase border border-[#111111] transition-all ${
                        selectedSubtype === sub 
                          ? 'bg-[#00D1FF] text-[#111111] chunky-shadow' 
                          : 'bg-white text-[#111111] hover:bg-gray-100'
                      }`}
                    >
                      {sub === 'all' ? '★ All Add-ons' : sub}
                    </button>
                  ))}
                </div>

                <button
                  onClick={onOpenUploadModal}
                  className="flex items-center gap-1 bg-[#FFF500] text-[#111111] px-2.5 py-1 border border-[#111111] text-xs font-black uppercase hover:bg-yellow-300 flex-shrink-0 shadow-[1px_1px_0px_#111111]"
                >
                  <Plus className="w-3.5 h-3.5" />
                  <span>New Accessory</span>
                </button>
              </div>

              {/* Horizontal Scrollable Rail of Accessories */}
              <div className="flex items-center gap-3 overflow-x-auto py-2 px-1 scrollbar-thin">
                {filteredAccessories.map(acc => (
                  <div
                    key={acc.id}
                    className="relative flex-shrink-0 w-28 sm:w-32 bg-white border-2 border-[#111111] p-2 chunky-shadow hover:-translate-y-1 transition-all group cursor-pointer"
                    onClick={() => setActiveAssignItem(activeAssignItem?.id === acc.id ? null : acc)}
                  >
                    <div className="relative aspect-square w-full border border-[#111111] overflow-hidden mb-1.5 bg-gray-50">
                      <img 
                        src={acc.imageUrl} 
                        alt={acc.name} 
                        referrerPolicy="no-referrer"
                        className="w-full h-full object-cover group-hover:scale-105 transition-transform" 
                      />
                      <span className="absolute bottom-0.5 left-0.5 bg-black/80 text-white text-[8px] font-mono px-1 uppercase">
                        {acc.subCategory || 'Acc'}
                      </span>
                    </div>

                    <p className="font-black uppercase text-[11px] text-[#111111] truncate">{acc.name}</p>
                    <div className="flex items-center justify-between text-[9px] font-mono text-gray-600 mt-0.5">
                      <span className="truncate">{acc.color}</span>
                      <span className="bg-[#FFF500] text-[#111111] font-bold px-1 border border-[#111111]">
                        Pin 📌
                      </span>
                    </div>
                  </div>
                ))}
              </div>

              {/* Quick Assign Popover for the clicked accessory */}
              {activeAssignItem && (
                <div className="mt-2.5 p-2.5 bg-[#FFF0F5] border-2 border-[#111111] chunky-shadow flex flex-wrap items-center justify-between gap-2 animate-fadeIn">
                  <div className="flex items-center gap-2">
                    <Pin className="w-4 h-4 text-[#FF007A]" />
                    <span className="text-xs font-black uppercase text-[#111111]">
                      Pin "{activeAssignItem.name}" to:
                    </span>
                  </div>

                  <div className="flex items-center gap-1 flex-wrap">
                    {DAYS_OF_WEEK.map(day => (
                      <button
                        key={day}
                        onClick={() => handleAssign(day, activeAssignItem)}
                        className="bg-white hover:bg-[#FFF500] text-[#111111] border border-[#111111] px-2 py-1 text-xs font-black uppercase shadow-[1px_1px_0px_#111111] active:scale-95 transition-all"
                      >
                        {justAddedDay === day ? `✓ Added to ${day}!` : day}
                      </button>
                    ))}
                    <button
                      onClick={() => setActiveAssignItem(null)}
                      className="text-xs text-gray-600 hover:text-black font-mono ml-2 underline"
                    >
                      Dismiss
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
