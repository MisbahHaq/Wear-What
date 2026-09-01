import React, { useState } from 'react';
import { 
  Heart, 
  Sparkles, 
  Edit3, 
  Trash2, 
  Maximize2, 
  X, 
  Shirt, 
  Tag, 
  Palette, 
  Calendar,
  Layers,
  ArrowRight,
  Flame,
  Star
} from 'lucide-react';
import { ClothingCategory, ClothingItem } from '../types';
import { WashiTape, CircularSticker, DoodleSparkle, DoodleSquiggle, NeonStickerBadge, RetroSmileyBadge } from './DoodleDecorations';
import { COLOR_PALETTE_PRESETS } from '../data/initialWardrobe';

interface EditorialMoodBoardProps {
  wardrobe: ClothingItem[];
  onToggleFavorite: (id: string) => void;
  onEditItem: (item: ClothingItem) => void;
  onDeleteItem: (id: string) => void;
  onOpenUploadModal: () => void;
}

// Editorial aesthetic pull-quotes to attach randomly to mood cards
const EDITORIAL_NOTES = [
  '⭐ Heavy rotation essential',
  '✨ Iconic texture & silhouette',
  '🔥 Masterpiece tailoring',
  '💖 Unmatched versatility',
  '⚡ Statement centerpiece',
  '💎 Timeless signature drape',
  '🎨 Rich color saturation',
  '🍸 Weekend hero piece',
  '🕶️ Pure high-fashion mood',
  '🌸 Flawless layer candidate'
];

export const EditorialMoodBoard: React.FC<EditorialMoodBoardProps> = ({
  wardrobe,
  onToggleFavorite,
  onEditItem,
  onDeleteItem,
  onOpenUploadModal,
}) => {
  // Favorite items only
  const favoriteItems = wardrobe.filter(item => item.favorite);

  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [selectedColorHex, setSelectedColorHex] = useState<string | null>(null);
  const [activeZoomItem, setActiveZoomItem] = useState<ClothingItem | null>(null);
  const [layoutSeed, setLayoutSeed] = useState(0);

  // Filter within favorites
  const filteredFavorites = favoriteItems.filter(item => {
    const matchesCategory = selectedCategory === 'all' || item.category === selectedCategory;
    const matchesColor = !selectedColorHex || item.colorHex === selectedColorHex;
    return matchesCategory && matchesColor;
  });

  // Extract unique colors from favorites
  const favoriteColors = Array.from(
    new Set(favoriteItems.map(i => JSON.stringify({ hex: i.colorHex, name: i.color })))
  ).map((s: string): { hex: string; name: string } => JSON.parse(s));

  // Quick favorite batch if user has 0 or few
  const handleQuickAddSuggestedFavorites = () => {
    const nonFavorites = wardrobe.filter(i => !i.favorite);
    const toAdd = nonFavorites.slice(0, 5);
    toAdd.forEach(item => onToggleFavorite(item.id));
  };

  return (
    <div className="space-y-6">
      {/* Editorial Mood Board Header Banner */}
      <div className="relative bg-[#FF007A] text-white border-4 border-[#111111] p-6 chunky-shadow-xl overflow-hidden">
        {/* Halftone Texture Overlay */}
        <div className="halftone opacity-20"></div>

        {/* Decorative Stamps & Stickers */}
        <div className="absolute top-2 right-4 pointer-events-none opacity-95 hidden sm:block">
          <CircularSticker 
            text="PRIVATE" 
            subtext="VOL. 04" 
            bg="bg-[#FFF500]" 
            textColor="text-[#111111]" 
            rotate="-rotate-6" 
          />
        </div>

        <div className="absolute -bottom-4 right-28 pointer-events-none opacity-80 hidden md:block">
          <WashiTape pattern="cyan" angle={6} className="w-28" />
        </div>

        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 relative z-10">
          <div>
            <div className="flex items-center gap-2 mb-1 flex-wrap">
              <span className="bg-[#FFF500] text-[#111111] text-xs font-black uppercase px-2.5 py-0.5 border-2 border-[#111111] shadow-[2px_2px_0px_#111111] rotate-1">
                ★ THE LOVED ARCHIVE
              </span>
              <span className="bg-[#00D1FF] text-[#111111] text-xs font-mono font-black uppercase px-2 py-0.5 border-2 border-[#111111] shadow-[2px_2px_0px_#111111] -rotate-1">
                MASONRY MOOD BOARD
              </span>
            </div>

            <h2 className="big-display text-3xl sm:text-5xl text-white tracking-tight leading-none mt-2 drop-shadow-[3px_3px_0px_#111111]">
              EDITORIAL MOOD BOARD
            </h2>

            <p className="scribble text-base sm:text-lg text-pink-100 mt-1 max-w-2xl font-semibold">
              An inspired visual scrapbook of your most treasured, top-tier garments. Pinned, taped, and curated into a high-fashion collage.
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex items-center gap-2.5 flex-wrap">
            <button
              onClick={() => setLayoutSeed(prev => prev + 1)}
              className="bg-white hover:bg-yellow-100 text-[#111111] border-3 border-[#111111] px-3.5 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-1.5 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
              title="Shuffle pin angles and layout vibes"
            >
              <Sparkles className="w-4 h-4 text-[#FF007A]" />
              <span>Remix Angles</span>
            </button>

            <button
              onClick={onOpenUploadModal}
              className="bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-3 border-[#111111] px-4 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-2 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
            >
              <Heart className="w-4 h-4 fill-[#FF007A] text-[#FF007A]" />
              <span>Add New Piece</span>
            </button>
          </div>
        </div>

        {/* Live Loved Stats Strip */}
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-6 relative z-10 text-[#111111]">
          <div className="bg-[#FFF9ED] border-2 border-[#111111] p-3 chunky-shadow">
            <p className="text-[10px] font-mono font-black uppercase text-gray-600">Loved Pieces</p>
            <p className="font-display text-2xl font-black text-[#FF007A]">{favoriteItems.length} curated</p>
          </div>

          <div className="bg-[#FFF9ED] border-2 border-[#111111] p-3 chunky-shadow">
            <p className="text-[10px] font-mono font-black uppercase text-gray-600">Total Wardrobe</p>
            <p className="font-display text-2xl font-black text-[#111111]">{wardrobe.length} items</p>
          </div>

          <div className="bg-[#FFF9ED] border-2 border-[#111111] p-3 chunky-shadow">
            <p className="text-[10px] font-mono font-black uppercase text-gray-600">Loved Ratio</p>
            <p className="font-display text-2xl font-black text-[#00D1FF]">
              {wardrobe.length > 0 ? Math.round((favoriteItems.length / wardrobe.length) * 100) : 0}%
            </p>
          </div>

          <div className="bg-[#FFF9ED] border-2 border-[#111111] p-3 chunky-shadow">
            <p className="text-[10px] font-mono font-black uppercase text-gray-600">Color Spectrum</p>
            <p className="font-display text-2xl font-black text-[#FF5C00]">{favoriteColors.length} tones</p>
          </div>
        </div>
      </div>

      {/* Mood Board Filter & Palette Toolbar */}
      <div className="bg-white border-3 border-[#111111] p-4 chunky-shadow space-y-3">
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
          {/* Category Filter Pills */}
          <div className="flex items-center gap-1.5 overflow-x-auto pb-1 w-full sm:w-auto">
            <span className="text-xs font-black text-[#111111] uppercase mr-1 flex items-center gap-1 shrink-0">
              <Shirt className="w-3.5 h-3.5 text-[#FF007A]" /> Mood Filter:
            </span>
            {[
              { id: 'all', label: 'All Loved', icon: '💖' },
              { id: 'top', label: 'Tops', icon: '👕' },
              { id: 'bottom', label: 'Bottoms', icon: '👖' },
              { id: 'dress', label: 'Dresses', icon: '👗' },
              { id: 'outerwear', label: 'Coats & Jackets', icon: '🧥' },
              { id: 'shoes', label: 'Footwear', icon: '👟' },
              { id: 'accessory', label: 'Acc & Jewels', icon: '💎' },
            ].map(cat => {
              const count = cat.id === 'all' 
                ? favoriteItems.length 
                : favoriteItems.filter(i => i.category === cat.id).length;

              return (
                <button
                  key={cat.id}
                  onClick={() => setSelectedCategory(cat.id)}
                  className={`px-3 py-1.5 text-xs font-black uppercase tracking-wider border-2 border-[#111111] flex items-center gap-1.5 shrink-0 transition-all ${
                    selectedCategory === cat.id
                      ? 'bg-[#FFF500] text-[#111111] chunky-shadow -translate-y-0.5'
                      : 'bg-[#FFF9ED] text-[#111111] hover:bg-yellow-50 shadow-[2px_2px_0px_#111111]'
                  }`}
                >
                  <span>{cat.icon}</span>
                  <span>{cat.label}</span>
                  <span className="text-[10px] font-mono opacity-75">({count})</span>
                </button>
              );
            })}
          </div>

          {/* Color Filter Swatches */}
          {favoriteColors.length > 0 && (
            <div className="flex items-center gap-1.5 shrink-0 flex-wrap">
              <span className="text-xs font-mono font-black uppercase text-gray-700">Palette:</span>
              {selectedColorHex && (
                <button
                  onClick={() => setSelectedColorHex(null)}
                  className="text-[10px] font-mono bg-[#111111] text-white px-2 py-0.5 font-bold hover:bg-gray-800"
                >
                  Clear ✕
                </button>
              )}
              {favoriteColors.map(c => (
                <button
                  key={c.hex}
                  onClick={() => setSelectedColorHex(selectedColorHex === c.hex ? null : c.hex)}
                  className={`w-5 h-5 border-2 border-[#111111] transition-transform ${
                    selectedColorHex === c.hex ? 'scale-125 ring-2 ring-[#FF007A]' : 'hover:scale-110'
                  }`}
                  style={{ backgroundColor: c.hex }}
                  title={`${c.name} (${c.hex})`}
                />
              ))}
            </div>
          )}
        </div>
      </div>

      {/* --- MASONRY MOOD BOARD CANVAS --- */}
      {filteredFavorites.length > 0 ? (
        <div className="relative bg-[#FAF5E8] border-4 border-[#111111] p-4 sm:p-8 chunky-shadow-xl overflow-hidden min-h-[500px]">
          {/* Halftone / Corkboard Grid Texture */}
          <div className="halftone opacity-15"></div>

          {/* Editorial Watermarks & Stamps */}
          <div className="absolute top-4 left-4 pointer-events-none opacity-30 text-5xl font-black font-display tracking-widest uppercase text-[#111111] select-none">
            CURATION
          </div>
          <div className="absolute bottom-6 right-8 pointer-events-none opacity-20 text-6xl font-black font-display tracking-widest uppercase text-[#FF007A] select-none">
            INSPIRATION
          </div>

          {/* Masonry Columns Layout */}
          <div className="columns-1 sm:columns-2 md:columns-3 lg:columns-4 gap-6 [column-fill:_balance] relative z-10">
            {filteredFavorites.map((item, idx) => {
              // Deterministic aesthetic rotation & tape patterns based on item ID & layout seed
              const rotAngles = ['-rotate-2', 'rotate-1.5', '-rotate-1', 'rotate-2', '-rotate-3', 'rotate-1'];
              const rotation = rotAngles[(idx + layoutSeed) % rotAngles.length];
              const washiPatterns: Array<'pink' | 'yellow' | 'cyan' | 'leopard' | 'solid-yellow'> = [
                'pink', 'yellow', 'cyan', 'leopard', 'solid-yellow'
              ];
              const washiPattern = washiPatterns[(idx + layoutSeed) % washiPatterns.length];
              const noteText = EDITORIAL_NOTES[(idx * 3 + layoutSeed) % EDITORIAL_NOTES.length];
              const hasPolaroidTag = idx % 2 === 0;

              return (
                <div
                  key={item.id}
                  className={`break-inside-avoid mb-6 relative group transition-all duration-300 ${rotation} hover:rotate-0 hover:scale-[1.02] hover:z-30`}
                >
                  {/* Polaroid Frame Container */}
                  <div className="bg-white border-3 border-[#111111] p-3 pb-4 chunky-shadow-lg group-hover:chunky-shadow-xl transition-all relative">
                    {/* Washi Tape Strip at the Top or Corner */}
                    <div className="absolute -top-3 left-1/2 -translate-x-1/2 z-20 pointer-events-none">
                      <WashiTape
                        pattern={washiPattern}
                        angle={idx % 2 === 0 ? -4 : 4}
                        className="w-20 sm:w-24 shadow-sm"
                      />
                    </div>

                    {/* Corner Pushpin or Sticker Badge */}
                    {idx % 3 === 0 && (
                      <div className="absolute -top-2 -right-2 z-20 pointer-events-none">
                        <span className="w-5 h-5 bg-[#FF007A] text-white rounded-full border-2 border-[#111111] flex items-center justify-center text-[10px] font-black shadow-[2px_2px_0px_#111111]">
                          ★
                        </span>
                      </div>
                    )}

                    {/* Image Area with Dynamic Aspect Ratio */}
                    <div className="relative w-full border-2 border-[#111111] overflow-hidden bg-[#FFF9ED] shadow-[2px_2px_0px_#111111] group-hover:border-black transition-colors">
                      <img
                        src={item.imageUrl}
                        alt={item.name}
                        referrerPolicy="no-referrer"
                        className={`w-full object-cover transition-transform duration-300 group-hover:scale-105 ${
                          item.category === 'dress' || item.category === 'outerwear'
                            ? 'aspect-[3/4.2]'
                            : item.category === 'accessory' || item.category === 'shoes'
                            ? 'aspect-[4/3.8]'
                            : 'aspect-[4/4.5]'
                        }`}
                      />

                      {/* Image Overlay Controls (Heart Toggle & Zoom) */}
                      <div className="absolute top-2 right-2 flex items-center gap-1 z-10">
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            onToggleFavorite(item.id);
                          }}
                          className="w-7 h-7 bg-[#FF007A] text-white border-2 border-[#111111] rounded-none flex items-center justify-center shadow-[2px_2px_0px_#111111] hover:scale-110 active:scale-95 transition-all"
                          title="Remove from Loved Mood Board"
                        >
                          <Heart className="w-4 h-4 fill-white stroke-[2.5]" />
                        </button>
                      </div>

                      {/* Zoom Button */}
                      <button
                        onClick={() => setActiveZoomItem(item)}
                        className="absolute bottom-2 right-2 w-7 h-7 bg-white text-[#111111] border-2 border-[#111111] flex items-center justify-center opacity-0 group-hover:opacity-100 shadow-[2px_2px_0px_#111111] hover:bg-yellow-100 transition-opacity z-10"
                        title="View Full Editorial Zoom"
                      >
                        <Maximize2 className="w-3.5 h-3.5 stroke-[2.5]" />
                      </button>

                      {/* Category Label */}
                      <span className="absolute bottom-2 left-2 bg-[#111111] text-white text-[9px] font-black uppercase px-2 py-0.5 border border-white tracking-widest">
                        {item.category}
                      </span>
                    </div>

                    {/* Polaroid Handwritten Caption Section */}
                    <div className="mt-3 space-y-1.5">
                      <div className="flex items-start justify-between gap-1">
                        <h4 className="big-display text-sm text-[#111111] leading-tight truncate uppercase">
                          {item.name}
                        </h4>
                        <span className="bg-[#FFF500] text-[#111111] text-[9px] font-black uppercase px-1.5 py-0.5 border border-[#111111] shrink-0 shadow-[1px_1px_0px_#111111]">
                          {item.styleClass.split(' ')[0]}
                        </span>
                      </div>

                      {/* Color Swatch & Season Pill */}
                      <div className="flex items-center justify-between text-[10px] font-mono text-gray-700 pt-0.5">
                        <div className="flex items-center gap-1.5">
                          <span
                            className="w-3 h-3 border border-[#111111] inline-block shadow-[1px_1px_0px_#111111]"
                            style={{ backgroundColor: item.colorHex }}
                          />
                          <span className="font-bold">{item.color}</span>
                        </div>
                        <span className="font-bold uppercase text-gray-500">
                          {item.season}
                        </span>
                      </div>

                      {/* Vibe tag & Stylist Editorial Note */}
                      <div className="pt-1.5 border-t border-dashed border-gray-300 space-y-1">
                        {item.vibeTag && (
                          <span className="inline-block bg-[#00D1FF] text-[#111111] text-[9px] font-black uppercase px-1.5 py-0.5 border border-[#111111] shadow-[1px_1px_0px_#111111] -rotate-1">
                            #{item.vibeTag}
                          </span>
                        )}

                        <p className="scribble text-xs text-[#FF007A] font-bold leading-tight">
                          {noteText}
                        </p>
                      </div>

                      {/* Card Action Drawer (Edit & Delete) */}
                      <div className="flex items-center justify-between pt-2 border-t border-gray-200 text-xs">
                        <span className="text-[9px] font-mono uppercase text-gray-400 font-bold">
                          VAULT ITEM
                        </span>
                        <div className="flex items-center gap-1.5">
                          <button
                            onClick={() => onEditItem(item)}
                            className="p-1 text-gray-700 hover:text-[#111111] hover:bg-yellow-100 border border-transparent hover:border-[#111111] transition-all"
                            title="Edit Piece"
                          >
                            <Edit3 className="w-3.5 h-3.5" />
                          </button>
                          <button
                            onClick={() => onDeleteItem(item.id)}
                            className="p-1 text-gray-400 hover:text-red-600 hover:bg-red-50 border border-transparent hover:border-red-600 transition-all"
                            title="Delete Item"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      ) : (
        /* Empty Mood Board State with Quick-Add Suggestions */
        <div className="relative bg-white border-4 border-[#111111] p-8 sm:p-12 text-center chunky-shadow-xl space-y-5 overflow-hidden">
          <div className="halftone opacity-10"></div>
          
          <div className="w-16 h-16 bg-[#FFF500] text-[#FF007A] border-3 border-[#111111] rounded-full flex items-center justify-center mx-auto chunky-shadow -rotate-6">
            <Heart className="w-8 h-8 fill-[#FF007A]" />
          </div>

          <div className="max-w-md mx-auto space-y-2 relative z-10">
            <h3 className="big-display text-2xl sm:text-3xl text-[#111111]">
              YOUR LOVED ARCHIVE IS EMPTY
            </h3>
            <p className="scribble text-base text-gray-700">
              Heart your favorite pieces in your wardrobe vault to pin them onto your personal editorial mood board.
            </p>
          </div>

          {/* Quick-Action suggestions */}
          <div className="pt-3 relative z-10 flex flex-wrap items-center justify-center gap-3">
            <button
              onClick={handleQuickAddSuggestedFavorites}
              className="bg-[#FF007A] hover:bg-[#e0006c] text-white border-3 border-[#111111] px-5 py-2.5 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-2 active:translate-x-0.5 active:translate-y-0.5 transition-all"
            >
              <Star className="w-4 h-4 fill-white" />
              <span>★ Auto-Curate Top 5 Favorites</span>
            </button>

            <button
              onClick={onOpenUploadModal}
              className="bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-3 border-[#111111] px-5 py-2.5 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-2 active:translate-x-0.5 active:translate-y-0.5 transition-all"
            >
              <Shirt className="w-4 h-4" />
              <span>Upload New Garments</span>
            </button>
          </div>

          {/* Preview Teaser of closet items to favorite */}
          {wardrobe.length > 0 && (
            <div className="pt-6 mt-6 border-t-2 border-dashed border-gray-300 max-w-xl mx-auto">
              <p className="text-xs font-mono font-black uppercase text-gray-500 mb-3">
                Click any piece below to instantly add to your Mood Board:
              </p>
              <div className="flex items-center justify-center gap-2 flex-wrap">
                {wardrobe.slice(0, 6).map(item => (
                  <button
                    key={item.id}
                    onClick={() => onToggleFavorite(item.id)}
                    className="group relative w-16 h-20 border-2 border-[#111111] overflow-hidden bg-gray-100 shadow-[2px_2px_0px_#111111] hover:scale-105 hover:border-[#FF007A] transition-all"
                    title={`Favorite ${item.name}`}
                  >
                    <img
                      src={item.imageUrl}
                      alt={item.name}
                      referrerPolicy="no-referrer"
                      className="w-full h-full object-cover"
                    />
                    <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center transition-opacity">
                      <Heart className="w-5 h-5 text-white fill-white" />
                    </div>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* --- EDITORIAL LIGHTBOX ZOOM MODAL --- */}
      {activeZoomItem && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-sm overflow-y-auto">
          <div className="relative w-full max-w-2xl bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto">
            {/* Header */}
            <div className="bg-[#111111] text-white px-5 py-3 flex items-center justify-between border-b-4 border-[#111111]">
              <div className="flex items-center gap-2">
                <span className="text-[#FFF500] font-black text-sm">★</span>
                <span className="font-display uppercase text-sm tracking-wider">EDITORIAL SPOTLIGHT // ARCHIVE VIEW</span>
              </div>
              <button
                onClick={() => setActiveZoomItem(null)}
                className="w-7 h-7 bg-[#FF007A] text-white border-2 border-white flex items-center justify-center hover:bg-pink-600 transition-colors"
              >
                <X className="w-4 h-4 stroke-[3]" />
              </button>
            </div>

            {/* Lightbox Body */}
            <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Photo Frame */}
              <div className="relative border-3 border-[#111111] bg-white p-2 chunky-shadow -rotate-1">
                <div className="absolute -top-3 left-1/2 -translate-x-1/2 z-10">
                  <WashiTape pattern="pink" angle={2} className="w-24" />
                </div>
                <img
                  src={activeZoomItem.imageUrl}
                  alt={activeZoomItem.name}
                  referrerPolicy="no-referrer"
                  className="w-full aspect-[4/5] object-cover border-2 border-[#111111]"
                />
              </div>

              {/* Garment Details & Editorial Breakdown */}
              <div className="space-y-4 flex flex-col justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="bg-[#FF007A] text-white text-[10px] font-black uppercase px-2 py-0.5 border border-[#111111]">
                      {activeZoomItem.category}
                    </span>
                    <span className="bg-[#00D1FF] text-[#111111] text-[10px] font-black uppercase px-2 py-0.5 border border-[#111111]">
                      {activeZoomItem.styleClass}
                    </span>
                  </div>

                  <h3 className="big-display text-2xl text-[#111111] uppercase tracking-tight mt-2">
                    {activeZoomItem.name}
                  </h3>

                  <p className="scribble text-base text-gray-700 mt-2">
                    {activeZoomItem.notes || 'Curated high-rotation fashion piece. Masterfully tailored with premium texture saturation.'}
                  </p>
                </div>

                {/* Specs Box */}
                <div className="bg-white border-2 border-[#111111] p-3 chunky-shadow space-y-2 text-xs font-mono">
                  <div className="flex items-center justify-between border-b border-gray-200 pb-1.5">
                    <span className="text-gray-500 font-bold uppercase">Color Tone:</span>
                    <div className="flex items-center gap-1.5">
                      <span
                        className="w-3.5 h-3.5 border border-black inline-block"
                        style={{ backgroundColor: activeZoomItem.colorHex }}
                      />
                      <span className="font-black text-[#111111]">{activeZoomItem.color} ({activeZoomItem.colorHex})</span>
                    </div>
                  </div>

                  <div className="flex items-center justify-between border-b border-gray-200 pb-1.5">
                    <span className="text-gray-500 font-bold uppercase">Season Suitability:</span>
                    <span className="font-black text-[#111111] uppercase bg-[#FFF500] px-1.5 border border-black">
                      {activeZoomItem.season}
                    </span>
                  </div>

                  {activeZoomItem.vibeTag && (
                    <div className="flex items-center justify-between">
                      <span className="text-gray-500 font-bold uppercase">Vibe Tag:</span>
                      <span className="font-black text-[#FF007A]">#{activeZoomItem.vibeTag}</span>
                    </div>
                  )}
                </div>

                {/* Actions */}
                <div className="flex items-center gap-2 pt-2">
                  <button
                    onClick={() => {
                      onToggleFavorite(activeZoomItem.id);
                      setActiveZoomItem(null);
                    }}
                    className="flex-1 bg-[#FF007A] text-white border-2 border-[#111111] py-2 px-3 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center justify-center gap-1.5 hover:bg-[#e0006c]"
                  >
                    <Heart className="w-4 h-4 fill-white" />
                    <span>Toggle Favorite</span>
                  </button>

                  <button
                    onClick={() => {
                      onEditItem(activeZoomItem);
                      setActiveZoomItem(null);
                    }}
                    className="bg-[#FFF500] text-[#111111] border-2 border-[#111111] py-2 px-3 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center justify-center gap-1 hover:bg-yellow-300"
                  >
                    <Edit3 className="w-4 h-4" />
                    <span>Edit</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
