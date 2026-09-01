import React, { useState, useRef } from 'react';
import { 
  X, 
  Upload, 
  Camera, 
  Sparkles, 
  Check, 
  Plus, 
  Trash2, 
  Image as ImageIcon,
  Tag,
  Palette,
  Layers,
  HelpCircle
} from 'lucide-react';
import confetti from 'canvas-confetti';
import { ClothingCategory, ClothingItem, SeasonTag, StyleClass } from '../types';
import { COLOR_PALETTE_PRESETS, VIBE_TAG_PRESETS } from '../data/initialWardrobe';
import { NeonStickerBadge, WashiTape } from './DoodleDecorations';

interface UploadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSaveItems: (items: ClothingItem[]) => void;
  editingItem?: ClothingItem | null;
}

interface StagedPhoto {
  id: string;
  previewUrl: string;
  name: string;
  category: ClothingCategory;
  styleClass: StyleClass;
  season: SeasonTag;
  color: string;
  colorHex: string;
  subCategory: string;
  vibeTag: string;
  notes: string;
}

export const UploadModal: React.FC<UploadModalProps> = ({
  isOpen,
  onClose,
  onSaveItems,
  editingItem,
}) => {
  if (!isOpen) return null;

  const fileInputRef = useRef<HTMLInputElement>(null);
  const cameraInputRef = useRef<HTMLInputElement>(null);

  // Staged items for multi-upload
  const [stagedItems, setStagedItems] = useState<StagedPhoto[]>(() => {
    if (editingItem) {
      return [{
        id: editingItem.id,
        previewUrl: editingItem.imageUrl,
        name: editingItem.name,
        category: editingItem.category,
        styleClass: editingItem.styleClass,
        season: editingItem.season,
        color: editingItem.color,
        colorHex: editingItem.colorHex,
        subCategory: editingItem.subCategory || '',
        vibeTag: editingItem.vibeTag || 'Casual Chic',
        notes: editingItem.notes || '',
      }];
    }
    return [];
  });

  const [activeItemIndex, setActiveItemIndex] = useState<number>(0);
  const [isDragging, setIsDragging] = useState(false);
  const [sampleUrlInput, setSampleUrlInput] = useState('');

  const currentItem = stagedItems[activeItemIndex];

  const updateCurrentItem = (field: keyof StagedPhoto, value: any) => {
    if (!currentItem) return;
    const updated = [...stagedItems];
    updated[activeItemIndex] = {
      ...updated[activeItemIndex],
      [field]: value,
    };
    // Auto-adjust styleClass if category is accessory
    if (field === 'category' && value === 'accessory') {
      updated[activeItemIndex].styleClass = 'Accessories & Add-ons';
    }
    setStagedItems(updated);
  };

  const handleFiles = (files: FileList | null) => {
    if (!files || files.length === 0) return;

    const newStaged: StagedPhoto[] = [];
    Array.from(files).forEach((file, idx) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        const url = e.target?.result as string;
        const itemName = file.name.replace(/\.[^/.]+$/, '').replace(/[-_]/g, ' ');
        const cleanName = itemName.charAt(0).toUpperCase() + itemName.slice(1);

        setStagedItems(prev => [
          ...prev,
          {
            id: `custom-${Date.now()}-${idx}`,
            previewUrl: url,
            name: cleanName || `Piece #${prev.length + 1}`,
            category: 'top',
            styleClass: 'Casual',
            season: 'all-season',
            color: 'Hot Pink',
            colorHex: '#FF007A',
            subCategory: 'Top',
            vibeTag: 'Casual Chic',
            notes: '',
          }
        ]);
      };
      reader.readAsDataURL(file);
    });
  };

  const handleAddSampleUrl = () => {
    if (!sampleUrlInput.trim()) return;
    setStagedItems(prev => [
      ...prev,
      {
        id: `custom-url-${Date.now()}`,
        previewUrl: sampleUrlInput.trim(),
        name: `Wardrobe Item #${prev.length + 1}`,
        category: 'top',
        styleClass: 'Casual',
        season: 'all-season',
        color: 'Electric Blue',
        colorHex: '#0055FF',
        subCategory: 'Top',
        vibeTag: 'Power Move',
        notes: '',
      }
    ]);
    setSampleUrlInput('');
  };

  const handleRemoveStaged = (idx: number) => {
    const updated = stagedItems.filter((_, i) => i !== idx);
    setStagedItems(updated);
    if (activeItemIndex >= updated.length) {
      setActiveItemIndex(Math.max(0, updated.length - 1));
    }
  };

  const handleSaveAll = () => {
    if (stagedItems.length === 0) return;

    const finalClothingItems: ClothingItem[] = stagedItems.map(st => ({
      id: st.id,
      name: st.name.trim() || 'Wardrobe Piece',
      imageUrl: st.previewUrl,
      category: st.category,
      styleClass: st.styleClass,
      season: st.season,
      color: st.color,
      colorHex: st.colorHex,
      subCategory: st.subCategory.trim() || undefined,
      vibeTag: st.vibeTag.trim() || undefined,
      notes: st.notes.trim() || undefined,
      favorite: editingItem ? editingItem.favorite : false,
      isCustom: true,
      dateAdded: new Date().toISOString().split('T')[0],
    }));

    onSaveItems(finalClothingItems);
    confetti({
      particleCount: 70,
      spread: 60,
      origin: { y: 0.6 },
      colors: ['#FF007A', '#0055FF', '#D4FF00']
    });
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 bg-black/80 backdrop-blur-sm overflow-y-auto">
      <div className="relative w-full max-w-4xl bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto max-h-[92vh] flex flex-col">
        {/* Header */}
        <div className="bg-[#FF007A] border-b-4 border-[#111111] px-4 sm:px-6 py-3 flex items-center justify-between gap-3 text-white">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-[#FFF500] text-black border-2 border-[#111111] flex items-center justify-center font-black text-lg -rotate-6 chunky-shadow">
              📷
            </div>
            <div>
              <h3 className="big-display text-xl sm:text-2xl tracking-wide">
                {editingItem ? 'Edit Wardrobe Piece' : 'Upload & Tag Clothes'}
              </h3>
              <p className="text-[11px] font-mono text-pink-100">
                Drag-and-drop photos, snap from camera, and tag style & season!
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="w-8 h-8 bg-[#111111] text-white border-2 border-white flex items-center justify-center hover:bg-gray-800 transition-colors"
          >
            <X className="w-5 h-5 stroke-[3]" />
          </button>
        </div>

        {/* Modal Content */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6">
          {/* Upload Dropzone & Camera Controls */}
          {stagedItems.length === 0 ? (
            <div className="space-y-4">
              <div
                onDragOver={(e) => { e.preventDefault(); setIsDragging(true); }}
                onDragLeave={() => setIsDragging(false)}
                onDrop={(e) => {
                  e.preventDefault();
                  setIsDragging(false);
                  handleFiles(e.dataTransfer.files);
                }}
                className={`border-4 border-dashed p-8 sm:p-12 text-center transition-all cursor-pointer ${
                  isDragging
                    ? 'border-[#FF007A] bg-[#FFF0F5] scale-[1.01]'
                    : 'border-[#111111] bg-white hover:bg-[#FFF9ED]'
                }`}
                onClick={() => fileInputRef.current?.click()}
              >
                <input
                  type="file"
                  ref={fileInputRef}
                  onChange={(e) => handleFiles(e.target.files)}
                  multiple
                  accept="image/*"
                  className="hidden"
                />
                <input
                  type="file"
                  ref={cameraInputRef}
                  onChange={(e) => handleFiles(e.target.files)}
                  accept="image/*"
                  capture="environment"
                  className="hidden"
                />

                <div className="w-16 h-16 bg-[#FFF500] border-3 border-[#111111] chunky-shadow mx-auto flex items-center justify-center mb-4 -rotate-3">
                  <Upload className="w-8 h-8 text-black" />
                </div>

                <h4 className="big-display text-2xl text-[#111111] mb-1">
                  Drag & Drop Clothing Photos
                </h4>
                <p className="text-xs font-mono text-gray-600 mb-4 max-w-sm mx-auto">
                  Upload multiple photos at once. JPEG, PNG, or WebP.
                </p>

                <div className="flex items-center justify-center gap-3 flex-wrap">
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      fileInputRef.current?.click();
                    }}
                    className="bg-[#00D1FF] hover:bg-cyan-400 text-[#111111] border-2 border-[#111111] px-4 py-2 font-black uppercase text-xs chunky-shadow flex items-center gap-2 active:translate-y-0.5"
                  >
                    <ImageIcon className="w-4 h-4" />
                    <span>Browse Computer</span>
                  </button>

                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      cameraInputRef.current?.click();
                    }}
                    className="bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-2 border-[#111111] px-4 py-2 font-black uppercase text-xs chunky-shadow flex items-center gap-2 active:translate-y-0.5"
                  >
                    <Camera className="w-4 h-4 text-[#FF007A]" />
                    <span>Mobile Camera Snap</span>
                  </button>
                </div>
              </div>

              {/* Or paste image URL */}
              <div className="bg-white border-2 border-[#111111] p-3 chunky-shadow flex items-center gap-2">
                <span className="text-xs font-black uppercase text-gray-700 whitespace-nowrap">
                  Or paste Image URL:
                </span>
                <input
                  type="url"
                  value={sampleUrlInput}
                  onChange={(e) => setSampleUrlInput(e.target.value)}
                  placeholder="https://images.unsplash.com/..."
                  className="flex-1 bg-[#FFF9ED] border border-[#111111] px-2.5 py-1 text-xs font-mono focus:outline-none focus:bg-white"
                />
                <button
                  type="button"
                  onClick={handleAddSampleUrl}
                  className="bg-[#FF5C00] text-white px-3 py-1 border border-[#111111] text-xs font-black uppercase hover:bg-orange-600 shadow-[1px_1px_0px_#111111]"
                >
                  Add URL
                </button>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-12 gap-6 items-start">
              {/* Left Column: Staged Photos Strip & Current Preview */}
              <div className="md:col-span-5 space-y-4">
                {/* Active Photo Polaroid */}
                <div className="relative bg-white border-3 border-[#111111] p-3.5 chunky-shadow-lg rotate-1">
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <WashiTape pattern="yellow" angle={-2} className="w-24" />
                  </div>

                  <div className="aspect-[4/5] w-full border-2 border-[#111111] overflow-hidden bg-gray-100 mb-2 mt-1">
                    <img
                      src={currentItem?.previewUrl}
                      alt="Uploaded preview"
                      referrerPolicy="no-referrer"
                      className="w-full h-full object-cover"
                    />
                  </div>

                  <div className="text-center">
                    <p className="big-display text-base text-[#111111] truncate">
                      {currentItem?.name || 'Untitled Piece'}
                    </p>
                    <p className="text-[11px] font-mono text-gray-500">
                      {currentItem?.category.toUpperCase()} • {currentItem?.styleClass}
                    </p>
                  </div>
                </div>

                {/* Staged Items Selector Rail (if multiple photos) */}
                {stagedItems.length > 1 && (
                  <div>
                    <p className="text-xs font-black uppercase text-[#111111] mb-1.5 flex items-center justify-between">
                      <span>Batch Queue ({stagedItems.length} items):</span>
                      <span className="text-[10px] text-gray-600">Select to edit tags</span>
                    </p>
                    <div className="flex gap-2 overflow-x-auto pb-2">
                      {stagedItems.map((item, idx) => (
                        <div
                          key={item.id}
                          onClick={() => setActiveItemIndex(idx)}
                          className={`relative flex-shrink-0 w-14 h-16 border-2 border-[#111111] overflow-hidden cursor-pointer transition-all ${
                            activeItemIndex === idx
                              ? 'ring-3 ring-[#FF007A] scale-105 chunky-shadow'
                              : 'opacity-70 hover:opacity-100'
                          }`}
                        >
                          <img
                            src={item.previewUrl}
                            alt=""
                            referrerPolicy="no-referrer"
                            className="w-full h-full object-cover"
                          />
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              handleRemoveStaged(idx);
                            }}
                            className="absolute top-0.5 right-0.5 w-4 h-4 bg-red-600 text-white flex items-center justify-center text-[10px]"
                            title="Remove from batch"
                          >
                            ×
                          </button>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Add more photos button */}
                <div className="flex items-center gap-2">
                  <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    className="flex-1 bg-white hover:bg-gray-100 text-[#111111] border-2 border-[#111111] py-1.5 text-xs font-black uppercase flex items-center justify-center gap-1 shadow-[2px_2px_0px_#111111]"
                  >
                    <Plus className="w-3.5 h-3.5" /> Add Another Photo
                  </button>
                  <input
                    type="file"
                    ref={fileInputRef}
                    onChange={(e) => handleFiles(e.target.files)}
                    multiple
                    accept="image/*"
                    className="hidden"
                  />
                </div>
              </div>

              {/* Right Column: Tagging Form Controls */}
              <div className="md:col-span-7 bg-white border-3 border-[#111111] p-4 sm:p-5 chunky-shadow space-y-4">
                <div className="flex items-center justify-between border-b-2 border-[#111111] pb-2">
                  <h4 className="big-display text-lg text-[#111111]">
                    TAGGING DETAILS (ITEM {activeItemIndex + 1} OF {stagedItems.length})
                  </h4>
                  <NeonStickerBadge text="TAG LAB" bg="bg-[#FFF500]" textColor="text-[#111111]" />
                </div>

                {/* Item Name */}
                <div>
                  <label className="block text-xs font-black uppercase text-black mb-1">
                    Piece Name / Title
                  </label>
                  <input
                    type="text"
                    value={currentItem?.name || ''}
                    onChange={(e) => updateCurrentItem('name', e.target.value)}
                    placeholder="e.g. Cobalt Poplin Shirt, Vintage Biker Jacket..."
                    className="w-full px-3 py-1.5 bg-[#FFF9ED] border-2 border-[#111111] text-xs font-mono font-bold focus:outline-none focus:bg-white"
                  />
                </div>

                {/* 1. Category Tagging */}
                <div>
                  <label className="block text-xs font-black uppercase text-black mb-1.5">
                    1. Clothing Category
                  </label>
                  <div className="grid grid-cols-3 gap-1.5">
                    {[
                      { id: 'top', label: 'Top', icon: '👕' },
                      { id: 'bottom', label: 'Bottom', icon: '👖' },
                      { id: 'dress', label: 'Dress', icon: '👗' },
                      { id: 'outerwear', label: 'Outerwear', icon: '🧥' },
                      { id: 'shoes', label: 'Shoes', icon: '👟' },
                      { id: 'accessory', label: 'Accessory', icon: '💎' },
                    ].map(cat => (
                      <button
                        key={cat.id}
                        type="button"
                        onClick={() => updateCurrentItem('category', cat.id)}
                        className={`p-2 border-2 border-[#111111] text-xs font-black uppercase flex items-center justify-center gap-1.5 transition-all ${
                          currentItem?.category === cat.id
                            ? 'bg-[#00D1FF] text-[#111111] chunky-shadow -translate-y-0.5'
                            : 'bg-[#FFF9ED] text-black hover:bg-gray-100 shadow-[1px_1px_0px_#111111]'
                        }`}
                      >
                        <span>{cat.icon}</span>
                        <span>{cat.label}</span>
                      </button>
                    ))}
                  </div>
                </div>

                {/* 2. Style Class Tagging */}
                <div>
                  <label className="block text-xs font-black uppercase text-black mb-1.5">
                    2. Style Class (Outfit Planner Category)
                  </label>
                  <div className="grid grid-cols-2 sm:grid-cols-2 gap-2">
                    {[
                      { id: 'Office Wear', label: 'Office Wear', desc: 'Work, meetings, formal', color: 'bg-[#00D1FF]' },
                      { id: 'Casual', label: 'Casual', desc: 'Everyday, street, chill', color: 'bg-[#FF5C00]' },
                      { id: 'Weekend Wear', label: 'Weekend Wear', desc: 'Party, brunch, date night', color: 'bg-[#FFF500]' },
                      { id: 'Accessories & Add-ons', label: 'Accessories & Add-ons', desc: 'Bags, jewelry, belts', color: 'bg-[#FF007A]' },
                    ].map(st => (
                      <button
                        key={st.id}
                        type="button"
                        onClick={() => updateCurrentItem('styleClass', st.id)}
                        className={`p-2 border-2 border-[#111111] text-left transition-all ${
                          currentItem?.styleClass === st.id
                            ? `${st.color} ${st.id === 'Office Wear' || st.id === 'Weekend Wear' ? 'text-[#111111]' : 'text-white'} chunky-shadow -translate-y-0.5`
                            : 'bg-[#FFF9ED] text-black hover:bg-gray-100 shadow-[1px_1px_0px_#111111]'
                        }`}
                      >
                        <p className="font-black uppercase text-xs">{st.label}</p>
                        <p className="text-[10px] opacity-80">{st.desc}</p>
                      </button>
                    ))}
                  </div>
                </div>

                {/* 3. Season / Fabric */}
                <div>
                  <label className="block text-xs font-black uppercase text-black mb-1.5">
                    3. Fabric / Season Compatibility
                  </label>
                  <div className="grid grid-cols-3 gap-2">
                    {[
                      { id: 'summer', label: 'Summer (Warm)', icon: '☀️' },
                      { id: 'winter', label: 'Winter (Cold)', icon: '❄️' },
                      { id: 'all-season', label: 'All-Season', icon: '🌿' },
                    ].map(sea => (
                      <button
                        key={sea.id}
                        type="button"
                        onClick={() => updateCurrentItem('season', sea.id)}
                        className={`p-2 border-2 border-[#111111] text-xs font-black uppercase flex items-center justify-center gap-1 transition-all ${
                          currentItem?.season === sea.id
                            ? 'bg-[#FFF500] text-[#111111] chunky-shadow -translate-y-0.5'
                            : 'bg-[#FFF9ED] text-black hover:bg-gray-100 shadow-[1px_1px_0px_#111111]'
                        }`}
                      >
                        <span>{sea.icon}</span>
                        <span>{sea.id}</span>
                      </button>
                    ))}
                  </div>
                </div>

                {/* 4. Color & Swatch */}
                <div>
                  <label className="block text-xs font-black uppercase text-black mb-1.5 flex items-center justify-between">
                    <span>4. Color Tag</span>
                    <span className="font-mono text-[11px] text-gray-600 font-bold">{currentItem?.color}</span>
                  </label>
                  <div className="flex items-center gap-1.5 flex-wrap">
                    {COLOR_PALETTE_PRESETS.map(col => (
                      <button
                        key={col.hex}
                        type="button"
                        onClick={() => {
                          updateCurrentItem('color', col.name);
                          updateCurrentItem('colorHex', col.hex);
                        }}
                        className={`w-7 h-7 border-2 border-[#111111] transition-transform ${
                          currentItem?.colorHex === col.hex
                            ? 'scale-125 ring-2 ring-[#FF007A] chunky-shadow'
                            : 'hover:scale-110'
                        }`}
                        style={{ backgroundColor: col.hex }}
                        title={col.name}
                      />
                    ))}
                  </div>
                </div>

                {/* 5. Vibe Tag & Notes */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 pt-2 border-t border-dashed border-gray-300">
                  <div>
                    <label className="block text-[11px] font-black uppercase text-black mb-1">
                      Vibe / Aesthetic Tag
                    </label>
                    <select
                      value={currentItem?.vibeTag || 'Casual Chic'}
                      onChange={(e) => updateCurrentItem('vibeTag', e.target.value)}
                      className="w-full px-2.5 py-1.5 bg-[#FFF9ED] border-2 border-[#111111] text-xs font-mono font-bold"
                    >
                      {VIBE_TAG_PRESETS.map(v => (
                        <option key={v} value={v}>#{v}</option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-[11px] font-black uppercase text-black mb-1">
                      Subtype / Silouhette
                    </label>
                    <input
                      type="text"
                      value={currentItem?.subCategory || ''}
                      onChange={(e) => updateCurrentItem('subCategory', e.target.value)}
                      placeholder="e.g. Pleated Skirt, Blazer, Loafers"
                      className="w-full px-2.5 py-1.5 bg-[#FFF9ED] border-2 border-[#111111] text-xs font-mono focus:outline-none focus:bg-white"
                    />
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Modal Footer */}
        <div className="bg-[#FAF5E8] border-t-3 border-[#111111] p-3 px-6 flex items-center justify-between gap-3">
          <button
            type="button"
            onClick={onClose}
            className="bg-white hover:bg-gray-100 text-[#111111] border-2 border-[#111111] px-4 py-2 font-black uppercase text-xs shadow-[2px_2px_0px_#111111]"
          >
            Cancel
          </button>

          {stagedItems.length > 0 && (
            <button
              type="button"
              onClick={handleSaveAll}
              className="bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-3 border-[#111111] px-6 py-2.5 font-black uppercase text-sm chunky-shadow active:translate-x-0.5 active:translate-y-0.5 active:shadow-none flex items-center gap-2"
            >
              <Check className="w-4 h-4 stroke-[3]" />
              <span>
                {editingItem ? 'Save Updates' : `Save ${stagedItems.length} Piece${stagedItems.length > 1 ? 's' : ''} to Closet`}
              </span>
            </button>
          )}
        </div>
      </div>
    </div>
  );
};
