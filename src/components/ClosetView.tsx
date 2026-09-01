import React, { useState } from 'react';
import { 
  Search, 
  Plus, 
  Trash2, 
  Edit3, 
  Heart, 
  Filter, 
  Sparkles, 
  Shirt, 
  RotateCcw,
  SlidersHorizontal,
  Layers,
  LayoutGrid,
  Check
} from 'lucide-react';
import { ClothingCategory, ClothingItem, SeasonTag, StyleClass } from '../types';
import { COLOR_PALETTE_PRESETS } from '../data/initialWardrobe';
import { CircularSticker, DoodleSparkle, DoodleSquiggle, NeonStickerBadge, RetroSmileyBadge, WashiTape } from './DoodleDecorations';
import { EditorialMoodBoard } from './EditorialMoodBoard';

interface ClosetViewProps {
  wardrobe: ClothingItem[];
  onOpenUploadModal: () => void;
  onDeleteItem: (id: string) => void;
  onToggleFavorite: (id: string) => void;
  onEditItem: (item: ClothingItem) => void;
  onResetToDemo: () => void;
}

export const ClosetView: React.FC<ClosetViewProps> = ({
  wardrobe,
  onOpenUploadModal,
  onDeleteItem,
  onToggleFavorite,
  onEditItem,
  onResetToDemo,
}) => {
  // Mode: 'archive' (catalog grid) | 'moodboard' (editorial masonry of loved/favorites)
  const [closetTab, setClosetTab] = useState<'archive' | 'moodboard'>('archive');

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [selectedStyle, setSelectedStyle] = useState<string>('all');
  const [selectedSeason, setSelectedSeason] = useState<string>('all');
  const [selectedColorHex, setSelectedColorHex] = useState<string | null>(null);
  const [onlyFavorites, setOnlyFavorites] = useState(false);

  const favoriteCount = wardrobe.filter(i => i.favorite).length;

  const filteredWardrobe = wardrobe.filter(item => {
    const matchesSearch = 
      item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.color.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (item.vibeTag && item.vibeTag.toLowerCase().includes(searchQuery.toLowerCase())) ||
      (item.subCategory && item.subCategory.toLowerCase().includes(searchQuery.toLowerCase()));

    const matchesCategory = selectedCategory === 'all' || item.category === selectedCategory;
    const matchesStyle = selectedStyle === 'all' || item.styleClass === selectedStyle;
    const matchesSeason = selectedSeason === 'all' || item.season === selectedSeason || item.season === 'all-season';
    const matchesColor = !selectedColorHex || item.colorHex === selectedColorHex;
    const matchesFav = !onlyFavorites || item.favorite;

    return matchesSearch && matchesCategory && matchesStyle && matchesSeason && matchesColor && matchesFav;
  });

  // Category counts
  const categoryCounts = {
    all: wardrobe.length,
    top: wardrobe.filter(i => i.category === 'top').length,
    bottom: wardrobe.filter(i => i.category === 'bottom').length,
    dress: wardrobe.filter(i => i.category === 'dress').length,
    outerwear: wardrobe.filter(i => i.category === 'outerwear').length,
    shoes: wardrobe.filter(i => i.category === 'shoes').length,
    accessory: wardrobe.filter(i => i.category === 'accessory').length,
  };

  const styleCounts = {
    office: wardrobe.filter(i => i.styleClass === 'Office Wear').length,
    casual: wardrobe.filter(i => i.styleClass === 'Casual').length,
    weekend: wardrobe.filter(i => i.styleClass === 'Weekend Wear').length,
    acc: wardrobe.filter(i => i.styleClass === 'Accessories & Add-ons').length,
  };

  return (
    <div className="space-y-6 pb-28">
      {/* Editorial View Switcher Bar (Archive Grid vs Mood Board) */}
      <div className="flex items-center justify-between gap-3 bg-[#111111] p-2 border-4 border-[#111111] chunky-shadow text-white flex-wrap">
        <div className="flex items-center gap-1.5 pl-2">
          <span className="w-2.5 h-2.5 bg-[#FFF500] rounded-full animate-pulse"></span>
          <span className="text-xs font-mono font-black uppercase tracking-widest text-[#FFF9ED]">
            CURATION DISPLAY MODE:
          </span>
        </div>

        <div className="flex items-center gap-2">
          <button
            id="tab-closet-archive"
            onClick={() => setClosetTab('archive')}
            className={`px-4 py-2 text-xs font-black uppercase tracking-wider border-2 border-white flex items-center gap-2 transition-all ${
              closetTab === 'archive'
                ? 'bg-[#FFF500] text-[#111111] font-black chunky-shadow -translate-y-0.5'
                : 'bg-transparent text-gray-300 hover:text-white hover:bg-gray-800'
            }`}
          >
            <LayoutGrid className="w-4 h-4" />
            <span>Vault Archive ({wardrobe.length})</span>
          </button>

          <button
            id="tab-closet-moodboard"
            onClick={() => setClosetTab('moodboard')}
            className={`px-4 py-2 text-xs font-black uppercase tracking-wider border-2 border-white flex items-center gap-2 transition-all ${
              closetTab === 'moodboard'
                ? 'bg-[#FF007A] text-white font-black chunky-shadow -translate-y-0.5'
                : 'bg-transparent text-gray-300 hover:text-white hover:bg-gray-800'
            }`}
          >
            <Heart className="w-4 h-4 fill-current" />
            <span>Editorial Mood Board ({favoriteCount} Loved)</span>
            <span className="bg-[#FFF500] text-[#111111] text-[9px] px-1 py-0.2 font-black border border-[#111111] -rotate-3">
              HOT
            </span>
          </button>
        </div>
      </div>

      {/* --- CONDITIONALLY RENDER VIEW --- */}
      {closetTab === 'moodboard' ? (
        <EditorialMoodBoard
          wardrobe={wardrobe}
          onToggleFavorite={onToggleFavorite}
          onEditItem={onEditItem}
          onDeleteItem={onDeleteItem}
          onOpenUploadModal={onOpenUploadModal}
        />
      ) : (
        <>
          {/* Closet Hero Banner & Editorial Header */}
          <div className="relative bg-[#FFF500] border-4 border-[#111111] p-6 chunky-shadow-lg overflow-hidden">
            {/* Halftone Texture Overlay */}
            <div className="halftone opacity-10"></div>

            <div className="absolute top-2 right-4 pointer-events-none opacity-90 hidden sm:block">
              <CircularSticker text="WARDROBE" subtext="VAULT" bg="bg-[#FF007A]" textColor="text-white" rotate="rotate-6" />
            </div>
            <div className="absolute bottom-2 right-24 pointer-events-none opacity-60">
              <DoodleSquiggle color="#00D1FF" />
            </div>

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 relative z-10">
              <div>
                <div className="flex items-center gap-2 mb-1 flex-wrap">
                  <h2 className="big-display text-3xl sm:text-4xl text-[#111111]">
                    MY WARDROBE VAULT
                  </h2>
                  <span className="bg-[#FF007A] text-white text-xs font-black uppercase px-2 py-0.5 border-2 border-[#111111] shadow-[2px_2px_0px_#111111] -rotate-2">
                    {wardrobe.length} PIECES TOTAL
                  </span>
                </div>
                <p className="scribble text-base text-gray-800">
                  Your curated capsule archive. Tag, filter, and style every garment in one place.
                </p>
              </div>

              <div className="flex items-center gap-2 flex-wrap">
                <button
                  onClick={() => setClosetTab('moodboard')}
                  className="bg-[#00D1FF] hover:bg-cyan-300 text-[#111111] border-3 border-[#111111] px-3.5 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-1.5 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
                  title="Switch to Editorial Mood Board"
                >
                  <Heart className="w-3.5 h-3.5 fill-[#FF007A] text-[#FF007A]" />
                  <span>Loved Mood Board ({favoriteCount})</span>
                </button>

                <button
                  onClick={onResetToDemo}
                  className="bg-white hover:bg-gray-100 text-[#111111] border-2 border-[#111111] px-3 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-1.5 active:translate-y-0.5 transition-all"
                  title="Reload curated demo collection"
                >
                  <RotateCcw className="w-3.5 h-3.5" />
                  <span>Reset Demo Fits</span>
                </button>

                <button
                  onClick={onOpenUploadModal}
                  className="bg-[#FF007A] hover:bg-[#e0006c] text-white border-3 border-[#111111] px-4 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-2 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
                >
                  <Plus className="w-4 h-4 stroke-[3]" />
                  <span>Upload Garment</span>
                </button>
              </div>
            </div>

            {/* Quick Style Stats Chips */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-5 relative z-10">
              <div className="bg-white border-2 border-[#111111] p-2.5 chunky-shadow flex items-center justify-between">
                <div>
                  <p className="text-[10px] font-mono font-black uppercase text-gray-500">Office Wear</p>
                  <p className="font-display text-xl font-black text-[#00D1FF]">{styleCounts.office} pieces</p>
                </div>
                <span className="text-xl">💼</span>
              </div>

              <div className="bg-white border-2 border-[#111111] p-2.5 chunky-shadow flex items-center justify-between">
                <div>
                  <p className="text-[10px] font-mono font-black uppercase text-gray-500">Casual</p>
                  <p className="font-display text-xl font-black text-[#FF5C00]">{styleCounts.casual} pieces</p>
                </div>
                <span className="text-xl">🧢</span>
              </div>

              <div className="bg-white border-2 border-[#111111] p-2.5 chunky-shadow flex items-center justify-between">
                <div>
                  <p className="text-[10px] font-mono font-black uppercase text-gray-500">Weekend</p>
                  <p className="font-display text-xl font-black text-[#FF007A]">{styleCounts.weekend} pieces</p>
                </div>
                <span className="text-xl">🍸</span>
              </div>

              <div className="bg-white border-2 border-[#111111] p-2.5 chunky-shadow flex items-center justify-between">
                <div>
                  <p className="text-[10px] font-mono font-black uppercase text-gray-500">Accessories</p>
                  <p className="font-display text-xl font-black text-[#111111]">{styleCounts.acc} pieces</p>
                </div>
                <span className="text-xl">✨</span>
              </div>
            </div>
          </div>

          {/* Filter & Search Dashboard */}
          <div className="bg-white border-3 border-[#111111] p-4 sm:p-5 chunky-shadow space-y-4">
            {/* Search Bar & Favorite Toggle */}
            <div className="flex flex-col sm:flex-row items-center justify-between gap-3">
              <div className="relative w-full sm:flex-1">
                <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search by name, color, vibe (e.g. 'silk', 'pink', 'leather', 'power')..."
                  className="w-full pl-10 pr-4 py-2 bg-[#FFF9ED] border-2 border-[#111111] text-xs sm:text-sm font-funky focus:outline-none focus:bg-white"
                />
              </div>

              <div className="flex items-center gap-2 w-full sm:w-auto">
                <button
                  onClick={() => setOnlyFavorites(!onlyFavorites)}
                  className={`flex-1 sm:flex-none flex items-center justify-center gap-1.5 px-3.5 py-2 border-2 border-[#111111] text-xs font-black uppercase tracking-wider transition-all ${
                    onlyFavorites
                      ? 'bg-[#FF007A] text-white chunky-shadow'
                      : 'bg-[#FFF9ED] text-[#111111] hover:bg-gray-100 shadow-[2px_2px_0px_#111111]'
                  }`}
                >
                  <Heart className={`w-4 h-4 ${onlyFavorites ? 'fill-white' : ''}`} />
                  <span>Favorites Only ({favoriteCount})</span>
                </button>

                <button
                  onClick={() => setClosetTab('moodboard')}
                  className="flex items-center gap-1 bg-[#FFF500] hover:bg-yellow-300 text-[#111111] px-3 py-2 border-2 border-[#111111] text-xs font-black uppercase tracking-wider shadow-[2px_2px_0px_#111111]"
                  title="Open in Editorial Masonry Mood Board"
                >
                  <Sparkles className="w-3.5 h-3.5 text-[#FF007A]" />
                  <span>Mood Board Mode →</span>
                </button>
              </div>
            </div>

            {/* Category Pills */}
            <div>
              <div className="flex items-center gap-1.5 overflow-x-auto pb-1">
                <span className="text-xs font-black text-[#111111] uppercase mr-1 flex items-center gap-1 flex-shrink-0">
                  <Shirt className="w-3.5 h-3.5" /> Category:
                </span>
                {[
                  { id: 'all', label: 'All', icon: '✨' },
                  { id: 'top', label: 'Tops', icon: '👕' },
                  { id: 'bottom', label: 'Bottoms', icon: '👖' },
                  { id: 'dress', label: 'Dresses', icon: '👗' },
                  { id: 'outerwear', label: 'Outerwear', icon: '🧥' },
                  { id: 'shoes', label: 'Shoes', icon: '👟' },
                  { id: 'accessory', label: 'Accessories', icon: '💎' },
                ].map(cat => (
                  <button
                    key={cat.id}
                    onClick={() => setSelectedCategory(cat.id)}
                    className={`px-3 py-1.5 text-xs font-black uppercase tracking-wider border-2 border-[#111111] flex items-center gap-1 flex-shrink-0 transition-all ${
                      selectedCategory === cat.id
                        ? 'bg-[#00D1FF] text-[#111111] chunky-shadow -translate-y-0.5'
                        : 'bg-[#FFF9ED] text-[#111111] hover:bg-gray-100 shadow-[2px_2px_0px_#111111]'
                    }`}
                  >
                    <span>{cat.icon}</span>
                    <span>{cat.label}</span>
                    <span className="text-[10px] opacity-80">
                      ({categoryCounts[cat.id as keyof typeof categoryCounts] || 0})
                    </span>
                  </button>
                ))}
              </div>
            </div>

            {/* Secondary Filters: Style Class & Season & Color Swatches */}
            <div className="pt-3 border-t-2 border-dashed border-[#111111]/30 flex flex-wrap items-center justify-between gap-3 text-xs">
              {/* Style Class */}
              <div className="flex items-center gap-1.5 flex-wrap">
                <span className="font-black text-[#111111] uppercase mr-1">Style Class:</span>
                {['all', 'Office Wear', 'Casual', 'Weekend Wear', 'Accessories & Add-ons'].map(style => (
                  <button
                    key={style}
                    onClick={() => setSelectedStyle(style)}
                    className={`px-2.5 py-1 text-[11px] font-black uppercase border border-[#111111] transition-all ${
                      selectedStyle === style 
                        ? 'bg-[#FF5C00] text-white chunky-shadow' 
                        : 'bg-[#FFF9ED] text-[#111111] hover:bg-gray-100'
                    }`}
                  >
                    {style === 'all' ? 'All Styles' : style}
                  </button>
                ))}
              </div>

              {/* Season Filter */}
              <div className="flex items-center gap-1.5 flex-wrap">
                <span className="font-black text-[#111111] uppercase mr-1">Season:</span>
                {['all', 'summer', 'winter', 'all-season'].map(s => (
                  <button
                    key={s}
                    onClick={() => setSelectedSeason(s)}
                    className={`px-2 py-0.5 text-[11px] font-mono font-black border border-[#111111] uppercase transition-all ${
                      selectedSeason === s 
                        ? 'bg-[#FFF500] text-[#111111] chunky-shadow' 
                        : 'bg-[#FFF9ED] text-[#111111] hover:bg-gray-100'
                    }`}
                  >
                    {s}
                  </button>
                ))}
              </div>

              {/* Color Swatch Filters */}
              <div className="flex items-center gap-1 flex-wrap">
                <span className="font-black text-[#111111] uppercase mr-1">Color:</span>
                {selectedColorHex && (
                  <button
                    onClick={() => setSelectedColorHex(null)}
                    className="text-[10px] font-mono bg-[#111111] text-white px-1.5 py-0.5 mr-1 font-bold"
                  >
                    Clear ✕
                  </button>
                )}
                {COLOR_PALETTE_PRESETS.slice(0, 8).map(col => (
                  <button
                    key={col.hex}
                    onClick={() => setSelectedColorHex(selectedColorHex === col.hex ? null : col.hex)}
                    className={`w-5 h-5 rounded-none border-2 border-[#111111] transition-transform ${
                      selectedColorHex === col.hex ? 'scale-125 ring-2 ring-[#FF007A]' : 'hover:scale-110'
                    }`}
                    style={{ backgroundColor: col.hex }}
                    title={col.name}
                  />
                ))}
              </div>
            </div>
          </div>

          {/* Wardrobe Grid */}
          <div>
            <div className="flex items-center justify-between mb-3 px-1">
              <p className="text-xs font-mono font-bold text-gray-700">
                Showing <span className="text-[#111111] font-black">{filteredWardrobe.length}</span> of {wardrobe.length} pieces
                {onlyFavorites && <span className="text-[#FF007A] ml-1 font-black">(Favorites Only)</span>}
              </p>

              <div className="flex items-center gap-2">
                <span className="bg-[#FFF500] text-[#111111] border border-[#111111] text-xs font-black uppercase px-2 py-0.5 shadow-[2px_2px_0px_#111111]">
                  ★ CLOSET ARCHIVE
                </span>
              </div>
            </div>

            {filteredWardrobe.length > 0 ? (
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                {filteredWardrobe.map((item, idx) => {
                  const rotation = idx % 4 === 0 ? '-rotate-1' : idx % 4 === 1 ? 'rotate-1' : idx % 4 === 2 ? '-rotate-0.5' : 'rotate-1.5';
                  const washiPattern = idx % 3 === 0 ? 'pink' : idx % 3 === 1 ? 'yellow' : 'cyan';

                  return (
                    <div
                      key={item.id}
                      className={`relative bg-white border-3 border-[#111111] p-3 chunky-shadow hover:chunky-shadow-lg transition-all duration-200 group flex flex-col justify-between ${rotation} hover:rotate-0 hover:z-10`}
                    >
                      {/* Washi tape accent on top */}
                      <div className="absolute -top-2.5 left-1/2 -translate-x-1/2 z-10">
                        <WashiTape pattern={washiPattern as any} angle={idx % 2 === 0 ? -3 : 3} className="w-16 h-3.5 text-[8px]" />
                      </div>

                      <div>
                        {/* Image frame */}
                        <div className="relative aspect-[4/5] w-full border-2 border-[#111111] overflow-hidden mb-2 bg-[#FFF9ED] mt-1 shadow-[2px_2px_0px_#111111]">
                          <img
                            src={item.imageUrl}
                            alt={item.name}
                            referrerPolicy="no-referrer"
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
                          />

                          {/* Favorite Button on Image */}
                          <button
                            onClick={() => onToggleFavorite(item.id)}
                            className={`absolute top-1.5 right-1.5 p-1 border border-[#111111] shadow-[1px_1px_0px_#111111] transition-transform active:scale-90 ${
                              item.favorite ? 'bg-[#FF007A] text-white' : 'bg-white/90 text-gray-400 hover:text-[#FF007A]'
                            }`}
                            title="Toggle Favorite"
                          >
                            <Heart className={`w-3.5 h-3.5 ${item.favorite ? 'fill-white' : ''}`} />
                          </button>

                          {/* Category Badge */}
                          <span className="absolute bottom-1.5 left-1.5 bg-[#111111] text-white text-[9px] font-black px-1.5 py-0.2 uppercase border border-white">
                            {item.category}
                          </span>
                        </div>

                        {/* Title & tags */}
                        <h4 className="font-black text-xs text-[#111111] truncate mb-1 uppercase tracking-tight" title={item.name}>
                          {item.name}
                        </h4>

                        <div className="flex items-center justify-between text-[10px] font-mono text-gray-600 mb-1">
                          <div className="flex items-center gap-1">
                            <span 
                              className="w-2.5 h-2.5 rounded-full border border-[#111111] inline-block"
                              style={{ backgroundColor: item.colorHex }}
                            />
                            <span className="truncate max-w-[70px]">{item.color}</span>
                          </div>
                          <span className="bg-[#FFF500] text-[#111111] px-1 py-0.2 border border-[#111111] font-bold text-[9px]">
                            {item.season}
                          </span>
                        </div>

                        {item.vibeTag && (
                          <p className="scribble text-xs text-[#FF007A] font-bold truncate">
                            #{item.vibeTag}
                          </p>
                        )}
                      </div>

                      {/* Actions Footer */}
                      <div className="pt-2 mt-2 border-t border-dashed border-gray-300 flex items-center justify-between gap-1">
                        <span className="text-[9px] font-black uppercase bg-[#00D1FF] text-[#111111] px-1.5 py-0.2 border border-[#111111] truncate max-w-[90px]">
                          {item.styleClass}
                        </span>

                        <div className="flex items-center gap-1">
                          <button
                            onClick={() => onEditItem(item)}
                            className="p-1 text-gray-700 hover:text-black hover:bg-gray-100"
                            title="Edit Item Details"
                          >
                            <Edit3 className="w-3.5 h-3.5" />
                          </button>
                          <button
                            onClick={() => onDeleteItem(item.id)}
                            className="p-1 text-gray-400 hover:text-red-600"
                            title="Delete from Closet"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <div className="bg-white border-3 border-[#111111] p-12 text-center chunky-shadow space-y-3">
                <span className="text-4xl">🧺</span>
                <h3 className="big-display text-2xl text-[#111111]">No garments matched your filters</h3>
                <p className="scribble text-sm text-gray-600 max-w-md mx-auto">
                  Try clearing search filters or switch to the Editorial Mood Board to explore your loved favorites.
                </p>
                <div className="flex items-center justify-center gap-2 pt-2">
                  <button
                    onClick={() => {
                      setSelectedCategory('all');
                      setSelectedStyle('all');
                      setSelectedSeason('all');
                      setSelectedColorHex(null);
                      setSearchQuery('');
                      setOnlyFavorites(false);
                    }}
                    className="bg-[#FFF500] text-[#111111] border-2 border-[#111111] px-4 py-2 font-black text-xs uppercase tracking-wider chunky-shadow hover:bg-yellow-300"
                  >
                    Reset Filters
                  </button>

                  <button
                    onClick={() => setClosetTab('moodboard')}
                    className="bg-[#FF007A] text-white border-2 border-[#111111] px-4 py-2 font-black text-xs uppercase tracking-wider chunky-shadow hover:bg-pink-600"
                  >
                    Open Mood Board
                  </button>
                </div>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
};
