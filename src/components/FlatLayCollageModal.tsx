import React, { useState } from 'react';
import { 
  X, 
  Sparkles, 
  RotateCw, 
  Download, 
  Share2, 
  ArrowRightLeft, 
  Palette, 
  Tag, 
  Heart, 
  Check, 
  Plus,
  StickyNote
} from 'lucide-react';
import confetti from 'canvas-confetti';
import { ClothingItem, DailyOutfit, DayOfWeek } from '../types';
import { DoodleArrow, DoodleSparkle, DoodleSquiggle, NeonStickerBadge, RetroSmileyBadge, SafetyPinIcon, WashiTape } from './DoodleDecorations';

interface FlatLayCollageModalProps {
  isOpen: boolean;
  onClose: () => void;
  day: DayOfWeek;
  outfit: DailyOutfit;
  wardrobe: ClothingItem[];
  onOpenSwapModal: (day: DayOfWeek, slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory', currentItemId?: string) => void;
  onUpdateNotes: (day: DayOfWeek, notes: string) => void;
}

const BG_CANVAS_THEMES = [
  { name: 'Warm Cream Grid', bg: 'bg-[#FFF9ED]', border: 'border-black' },
  { name: 'Electric Lilac', bg: 'bg-[#F3E8FF]', border: 'border-black' },
  { name: 'Butter Yellow', bg: 'bg-[#FEF08A]', border: 'border-black' },
  { name: 'Neon Mint', bg: 'bg-[#DCFCE7]', border: 'border-black' },
  { name: 'Blush Pink', bg: 'bg-[#FCE7F3]', border: 'border-black' },
];

export const FlatLayCollageModal: React.FC<FlatLayCollageModalProps> = ({
  isOpen,
  onClose,
  day,
  outfit,
  wardrobe,
  onOpenSwapModal,
  onUpdateNotes,
}) => {
  if (!isOpen) return null;

  const [selectedBgIndex, setSelectedBgIndex] = useState(0);
  const [memoNote, setMemoNote] = useState(outfit.notes || '');
  const [copied, setCopied] = useState(false);
  const [activeStickers, setActiveStickers] = useState<string[]>([
    'VIBE CHECK: 10/10',
    'OOTD SLAY',
    'MAIN CHARACTER'
  ]);

  const topItem = wardrobe.find(i => i.id === outfit.topId);
  const bottomItem = wardrobe.find(i => i.id === outfit.bottomId);
  const dressItem = wardrobe.find(i => i.id === outfit.dressId);
  const outerItem = wardrobe.find(i => i.id === outfit.outerwearId);
  const shoesItem = wardrobe.find(i => i.id === outfit.shoesId);
  const accessoryItems = outfit.accessoryIds.map(id => wardrobe.find(i => i.id === id)).filter(Boolean) as ClothingItem[];

  const allOutfitItems = [
    dressItem,
    topItem,
    bottomItem,
    outerItem,
    shoesItem,
    ...accessoryItems,
  ].filter(Boolean) as ClothingItem[];

  const handleTriggerConfetti = () => {
    confetti({
      particleCount: 80,
      spread: 70,
      origin: { y: 0.6 },
      colors: ['#FF007A', '#0055FF', '#D4FF00', '#FFE600', '#FF5500']
    });
  };

  const handleSaveNotes = () => {
    onUpdateNotes(day, memoNote);
    handleTriggerConfetti();
  };

  const handleCopySummary = () => {
    const summary = `✨ WEAR WHAT — ${outfit.dayName} OOTD Flat-Lay ✨\n` +
      `👗 Style: ${outfit.dayType} | Weather: ${outfit.weather.label} (${outfit.weather.tempC}°C)\n` +
      allOutfitItems.map(i => `• ${i.category.toUpperCase()}: ${i.name} (${i.color})`).join('\n') +
      (memoNote ? `\n📝 Notes: ${memoNote}` : '');

    navigator.clipboard.writeText(summary);
    setCopied(true);
    setTimeout(() => setCopied(false), 2500);
    handleTriggerConfetti();
  };

  const toggleSticker = (stickerText: string) => {
    if (activeStickers.includes(stickerText)) {
      setActiveStickers(activeStickers.filter(s => s !== stickerText));
    } else {
      setActiveStickers([...activeStickers, stickerText]);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-2 sm:p-4 bg-black/75 backdrop-blur-sm overflow-y-auto">
      <div className="relative w-full max-w-5xl bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto max-h-[92vh] flex flex-col">
        {/* Modal Top Header */}
        <div className="bg-[#FFF500] border-b-4 border-[#111111] px-4 sm:px-6 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <div className="bg-[#FF007A] text-white p-2 border-2 border-[#111111] -rotate-3 chunky-shadow">
              <Sparkles className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h2 className="big-display text-2xl sm:text-3xl text-[#111111]">
                  {outfit.dayName} Moodboard Edit
                </h2>
                <span className="bg-[#00D1FF] text-[#111111] text-xs font-black uppercase px-2 py-0.5 border-2 border-[#111111] shadow-[2px_2px_0px_#111111] rotate-2">
                  {outfit.dayType}
                </span>
              </div>
              <p className="text-xs font-mono font-bold text-gray-800">
                Weather: {outfit.weather.icon} {outfit.weather.tempC}°C ({outfit.weather.label}) • {outfit.dateLabel}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={handleCopySummary}
              className="bg-white hover:bg-gray-100 text-[#111111] border-2 border-[#111111] px-3 py-1.5 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-1.5 active:translate-y-0.5 transition-all"
            >
              {copied ? <Check className="w-4 h-4 text-green-600" /> : <Share2 className="w-4 h-4" />}
              <span>{copied ? 'Copied Edit!' : 'Share Edit'}</span>
            </button>
            <button
              onClick={onClose}
              className="w-9 h-9 bg-[#FF007A] text-white border-2 border-[#111111] chunky-shadow flex items-center justify-center hover:bg-red-700 active:scale-95 transition-all"
            >
              <X className="w-5 h-5 stroke-[3]" />
            </button>
          </div>
        </div>

        {/* Toolbar: Background Canvas Switcher & Sticker Bar */}
        <div className="bg-[#FAF5E8] border-b-2 border-[#111111] px-4 py-2 flex flex-wrap items-center justify-between gap-3 text-xs">
          <div className="flex items-center gap-2">
            <span className="font-black uppercase text-gray-800 flex items-center gap-1">
              <Palette className="w-3.5 h-3.5" /> Board Backing:
            </span>
            <div className="flex items-center gap-1.5">
              {BG_CANVAS_THEMES.map((theme, idx) => (
                <button
                  key={theme.name}
                  onClick={() => setSelectedBgIndex(idx)}
                  className={`w-6 h-6 border-2 border-[#111111] transition-transform ${theme.bg} ${
                    selectedBgIndex === idx ? 'scale-125 ring-2 ring-[#FF007A]' : 'opacity-80 hover:opacity-100'
                  }`}
                  title={theme.name}
                />
              ))}
            </div>
          </div>

          {/* Quick Sticker Toggles */}
          <div className="flex items-center gap-1.5 overflow-x-auto py-1">
            <span className="font-black uppercase text-gray-800 flex items-center gap-1">
              <Tag className="w-3.5 h-3.5" /> Stickers:
            </span>
            {['VIBE CHECK: 10/10', 'OOTD SLAY', 'POWER LOOK', 'MATCHING ENERGY', 'HOT FIT'].map(stk => (
              <button
                key={stk}
                onClick={() => toggleSticker(stk)}
                className={`px-2 py-0.5 text-[10px] font-black uppercase border border-[#111111] transition-all ${
                  activeStickers.includes(stk)
                    ? 'bg-[#FF007A] text-white chunky-shadow'
                    : 'bg-white text-gray-600 hover:text-black'
                }`}
              >
                {activeStickers.includes(stk) ? '✓ ' : '+ '} {stk}
              </button>
            ))}
          </div>
        </div>

        {/* Flat-Lay Editorial Canvas */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-8">
          <div 
            className={`relative min-h-[480px] p-6 border-3 border-[#111111] chunky-shadow-lg transition-colors ${BG_CANVAS_THEMES[selectedBgIndex].bg}`}
            style={{
              backgroundImage: selectedBgIndex === 0 
                ? 'linear-gradient(to right, #00000010 1px, transparent 1px), linear-gradient(to bottom, #00000010 1px, transparent 1px)' 
                : undefined,
              backgroundSize: '24px 24px'
            }}
          >
            {/* Editorial Doodle Accents */}
            <div className="absolute top-3 left-4 pointer-events-none">
              <SafetyPinIcon className="w-8 h-8 -rotate-12" />
            </div>
            <div className="absolute top-4 right-6 pointer-events-none">
              <DoodleSparkle size={36} color="#FF007A" className="rotate-12" />
            </div>
            <div className="absolute bottom-4 left-6 pointer-events-none">
              <DoodleSquiggle color="#00D1FF" />
            </div>

            {/* Active Neon Stickers pinned on the board */}
            <div className="flex flex-wrap gap-3 mb-6 relative z-10">
              {activeStickers.map((stk, idx) => (
                <NeonStickerBadge
                  key={stk}
                  text={stk}
                  bg={idx % 2 === 0 ? 'bg-[#FFF500]' : 'bg-[#FF5C00]'}
                  textColor={idx % 2 === 0 ? 'text-[#111111]' : 'text-white'}
                  rotate={idx % 2 === 0 ? '-rotate-3' : 'rotate-2'}
                />
              ))}
            </div>

            {/* Flat-Lay Items Layout Grid (Asymmetrical Editorial Style) */}
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 relative z-10 items-start">
              {/* Slot 1: Dress OR Top */}
              {dressItem ? (
                <div className="relative bg-white border-3 border-[#111111] p-3 chunky-shadow -rotate-2 hover:rotate-0 transition-transform group">
                  <div className="absolute -top-3 left-6">
                    <WashiTape pattern="pink" angle={-4} className="w-20" />
                  </div>
                  <div className="flex justify-between items-start mb-2 pt-1">
                    <span className="bg-[#FF007A] text-white text-[10px] font-black px-2 py-0.5 border border-[#111111] uppercase">
                      👗 Main Dress
                    </span>
                    <button
                      onClick={() => onOpenSwapModal(day, 'dress', dressItem.id)}
                      className="p-1 bg-[#FFF500] border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-yellow-400 text-xs font-bold flex items-center gap-1"
                      title="Swap Dress"
                    >
                      <ArrowRightLeft className="w-3 h-3" /> Swap
                    </button>
                  </div>
                  <img 
                    src={dressItem.imageUrl} 
                    alt={dressItem.name} 
                    referrerPolicy="no-referrer"
                    className="w-full h-56 object-cover border-2 border-[#111111] mb-2" 
                  />
                  <h4 className="font-black text-sm text-[#111111] uppercase tracking-tight">{dressItem.name}</h4>
                  <div className="flex items-center justify-between mt-1 text-xs font-mono">
                    <span className="text-gray-600">{dressItem.color}</span>
                    <span className="scribble font-bold text-[#FF007A]">#{dressItem.vibeTag}</span>
                  </div>
                </div>
              ) : topItem ? (
                <div className="relative bg-white border-3 border-[#111111] p-3 chunky-shadow -rotate-1.5 hover:rotate-0 transition-transform group">
                  <div className="absolute -top-3 left-6">
                    <WashiTape pattern="cyan" angle={-3} className="w-20" />
                  </div>
                  <div className="flex justify-between items-start mb-2 pt-1">
                    <span className="bg-[#00D1FF] text-[#111111] text-[10px] font-black px-2 py-0.5 border border-[#111111] uppercase">
                      👕 Top Slot
                    </span>
                    <button
                      onClick={() => onOpenSwapModal(day, 'top', topItem.id)}
                      className="p-1 bg-[#FFF500] border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-yellow-400 text-xs font-bold flex items-center gap-1"
                      title="Swap Top"
                    >
                      <ArrowRightLeft className="w-3 h-3" /> Swap
                    </button>
                  </div>
                  <img 
                    src={topItem.imageUrl} 
                    alt={topItem.name} 
                    referrerPolicy="no-referrer"
                    className="w-full h-48 object-cover border-2 border-[#111111] mb-2" 
                  />
                  <h4 className="font-black text-sm text-[#111111] uppercase tracking-tight">{topItem.name}</h4>
                  <div className="flex items-center justify-between mt-1 text-xs font-mono">
                    <span className="text-gray-600">{topItem.color}</span>
                    <span className="scribble font-bold text-[#00D1FF]">#{topItem.vibeTag}</span>
                  </div>
                </div>
              ) : null}

              {/* Slot 2: Bottom (if no dress) */}
              {!dressItem && bottomItem && (
                <div className="relative bg-white border-3 border-[#111111] p-3 chunky-shadow rotate-2 hover:rotate-0 transition-transform group">
                  <div className="absolute -top-3 right-6">
                    <WashiTape pattern="yellow" angle={4} className="w-20" />
                  </div>
                  <div className="flex justify-between items-start mb-2 pt-1">
                    <span className="bg-[#FF5C00] text-white text-[10px] font-black px-2 py-0.5 border border-[#111111] uppercase">
                      👖 Bottom Slot
                    </span>
                    <button
                      onClick={() => onOpenSwapModal(day, 'bottom', bottomItem.id)}
                      className="p-1 bg-[#FFF500] border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-yellow-400 text-xs font-bold flex items-center gap-1"
                      title="Swap Bottom"
                    >
                      <ArrowRightLeft className="w-3 h-3" /> Swap
                    </button>
                  </div>
                  <img 
                    src={bottomItem.imageUrl} 
                    alt={bottomItem.name} 
                    referrerPolicy="no-referrer"
                    className="w-full h-48 object-cover border-2 border-[#111111] mb-2" 
                  />
                  <h4 className="font-black text-sm text-[#111111] uppercase tracking-tight">{bottomItem.name}</h4>
                  <div className="flex items-center justify-between mt-1 text-xs font-mono">
                    <span className="text-gray-600">{bottomItem.color}</span>
                    <span className="scribble font-bold text-[#FF5C00]">#{bottomItem.vibeTag}</span>
                  </div>
                </div>
              )}

              {/* Slot 3: Outerwear */}
              {outerItem ? (
                <div className="relative bg-white border-3 border-[#111111] p-3 chunky-shadow -rotate-2 hover:rotate-0 transition-transform group">
                  <div className="absolute -top-3 left-8">
                    <WashiTape pattern="solid-yellow" angle={-2} className="w-20" />
                  </div>
                  <div className="flex justify-between items-start mb-2 pt-1">
                    <span className="bg-[#FFF500] text-[#111111] text-[10px] font-black px-2 py-0.5 border border-[#111111] uppercase">
                      🧥 Layer / Outerwear
                    </span>
                    <button
                      onClick={() => onOpenSwapModal(day, 'outerwear', outerItem.id)}
                      className="p-1 bg-[#FFF500] border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-yellow-400 text-xs font-bold flex items-center gap-1"
                      title="Swap Outerwear"
                    >
                      <ArrowRightLeft className="w-3 h-3" /> Swap
                    </button>
                  </div>
                  <img 
                    src={outerItem.imageUrl} 
                    alt={outerItem.name} 
                    referrerPolicy="no-referrer"
                    className="w-full h-48 object-cover border-2 border-[#111111] mb-2" 
                  />
                  <h4 className="font-black text-sm text-[#111111] uppercase tracking-tight">{outerItem.name}</h4>
                  <p className="text-xs font-mono text-gray-600">{outerItem.color}</p>
                </div>
              ) : (
                <div 
                  onClick={() => onOpenSwapModal(day, 'outerwear')}
                  className="border-3 border-dashed border-[#111111]/40 p-6 flex flex-col items-center justify-center text-center cursor-pointer hover:border-[#111111] hover:bg-white/50 transition-all min-h-[220px]"
                >
                  <span className="text-3xl mb-1">🧥</span>
                  <p className="font-black text-sm text-gray-700 uppercase">+ Add Layer / Jacket</p>
                  <p className="scribble text-xs text-gray-500">Perfect for breeze or rainy forecast</p>
                </div>
              )}

              {/* Slot 4: Shoes */}
              {shoesItem && (
                <div className="relative bg-white border-3 border-[#111111] p-3 chunky-shadow rotate-1.5 hover:rotate-0 transition-transform group">
                  <div className="absolute -top-3 right-8">
                    <WashiTape pattern="cyan" angle={3} className="w-20" />
                  </div>
                  <div className="flex justify-between items-start mb-2 pt-1">
                    <span className="bg-[#111111] text-white text-[10px] font-black px-2 py-0.5 border border-white uppercase">
                      👟 Footwear
                    </span>
                    <button
                      onClick={() => onOpenSwapModal(day, 'shoes', shoesItem.id)}
                      className="p-1 bg-[#FFF500] border border-[#111111] shadow-[2px_2px_0px_#111111] hover:bg-yellow-400 text-xs font-bold flex items-center gap-1"
                      title="Swap Shoes"
                    >
                      <ArrowRightLeft className="w-3 h-3" /> Swap
                    </button>
                  </div>
                  <img 
                    src={shoesItem.imageUrl} 
                    alt={shoesItem.name} 
                    referrerPolicy="no-referrer"
                    className="w-full h-44 object-cover border-2 border-[#111111] mb-2" 
                  />
                  <h4 className="font-black text-sm text-[#111111] uppercase tracking-tight">{shoesItem.name}</h4>
                  <p className="text-xs font-mono text-gray-600">{shoesItem.color}</p>
                </div>
              )}

              {/* Slot 5: Accessories Cluster */}
              <div className="relative bg-[#FFF9ED] border-3 border-[#111111] p-3 chunky-shadow -rotate-1 hover:rotate-0 transition-transform">
                <div className="flex justify-between items-center mb-2">
                  <span className="bg-[#FFF500] text-[#111111] text-[10px] font-black px-2 py-0.5 border border-[#111111] uppercase">
                    ✨ Add-Ons & Accs
                  </span>
                  <button
                    onClick={() => onOpenSwapModal(day, 'accessory')}
                    className="text-xs font-black uppercase bg-[#FF007A] text-white px-2 py-0.5 border border-[#111111] shadow-[1px_1px_0px_#111111]"
                  >
                    + Add More
                  </button>
                </div>

                <div className="space-y-2">
                  {accessoryItems.length > 0 ? (
                    accessoryItems.map(acc => (
                      <div key={acc.id} className="flex items-center gap-2 bg-white p-1.5 border-2 border-[#111111] shadow-[2px_2px_0px_#111111]">
                        <img 
                          src={acc.imageUrl} 
                          alt={acc.name} 
                          referrerPolicy="no-referrer"
                          className="w-12 h-12 object-cover border border-[#111111]" 
                        />
                        <div className="flex-1 min-w-0">
                          <p className="text-xs font-black uppercase truncate text-black">{acc.name}</p>
                          <span className="text-[10px] font-mono text-gray-600">{acc.color}</span>
                        </div>
                      </div>
                    ))
                  ) : (
                    <p className="scribble text-xs text-gray-500 py-4 text-center">
                      No accessories added yet!
                    </p>
                  )}
                </div>
              </div>

              {/* Slot 6: Styling Memo & Color Palette Chip */}
              <div className="relative bg-[#FFF500] border-3 border-[#111111] p-3 chunky-shadow rotate-2 hover:rotate-0 transition-transform">
                <div className="flex items-center gap-1.5 mb-2">
                  <StickyNote className="w-4 h-4 text-black" />
                  <span className="big-display text-sm text-[#111111]">Stylist Note:</span>
                </div>
                <textarea
                  value={memoNote}
                  onChange={(e) => setMemoNote(e.target.value)}
                  placeholder="E.g. Layer gold hoops + red bag for high-contrast pop! Sunglasses ready for sunny lunch..."
                  className="w-full h-24 p-2 text-xs font-mono bg-white border-2 border-[#111111] resize-none focus:outline-none"
                />
                <div className="flex items-center justify-between mt-2">
                  <div className="flex items-center gap-1">
                    {allOutfitItems.slice(0, 4).map((it, idx) => (
                      <div 
                        key={idx}
                        className="w-4 h-4 border border-[#111111] shadow-[1px_1px_0px_#111111]"
                        style={{ backgroundColor: it.colorHex }}
                        title={it.color}
                      />
                    ))}
                  </div>
                  <button
                    onClick={handleSaveNotes}
                    className="bg-[#00D1FF] text-[#111111] px-2.5 py-1 border border-[#111111] text-[11px] font-black uppercase hover:bg-cyan-300 shadow-[1px_1px_0px_#111111]"
                  >
                    Save Note
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Modal Bottom Bar */}
        <div className="bg-[#FAF5E8] border-t-3 border-[#111111] p-3 px-6 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <span className="bg-[#FFF500] text-[#111111] border border-[#111111] text-xs font-black uppercase px-2 py-0.5 shadow-[1px_1px_0px_#111111]">
              ★ FIT READY
            </span>
            <span className="text-xs font-mono text-gray-600 hidden sm:inline">
              Total Pieces: {allOutfitItems.length} items styled
            </span>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={handleTriggerConfetti}
              className="bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-2 border-[#111111] px-4 py-2 font-black text-xs uppercase tracking-wider chunky-shadow active:translate-y-0.5 flex items-center gap-1.5 transition-all"
            >
              <Sparkles className="w-4 h-4 text-[#FF007A]" />
              <span>Celebrate Fit!</span>
            </button>
            <button
              onClick={onClose}
              className="bg-[#111111] hover:bg-gray-800 text-white border-2 border-[#111111] px-5 py-2 font-black text-xs uppercase tracking-wider chunky-shadow active:translate-y-0.5 transition-all"
            >
              Done / Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
