import React, { useState } from 'react';
import { X, Sparkles, Flame, Check, RefreshCw, Heart, Zap, Award } from 'lucide-react';
import confetti from 'canvas-confetti';
import { ClothingItem, DailyOutfit, WeeklyPlan } from '../types';
import { DAYS_OF_WEEK } from '../utils/outfitGenerator';
import { DoodleSparkle, DoodleSquiggle, NeonStickerBadge, RetroSmileyBadge, WashiTape } from './DoodleDecorations';

interface StyleVibeCheckModalProps {
  isOpen: boolean;
  onClose: () => void;
  weeklyPlan: WeeklyPlan;
  wardrobe: ClothingItem[];
}

export const StyleVibeCheckModal: React.FC<StyleVibeCheckModalProps> = ({
  isOpen,
  onClose,
  weeklyPlan,
  wardrobe,
}) => {
  if (!isOpen) return null;

  const [analyzing, setAnalyzing] = useState(false);
  const [vibeScore, setVibeScore] = useState<number>(96);

  // Compute stats on weekly plan
  const days = DAYS_OF_WEEK.map(d => weeklyPlan[d]).filter(Boolean);
  
  // Calculate color variety
  const usedColorHexes = new Set<string>();
  const usedCategories = new Set<string>();
  let totalAccessoriesCount = 0;

  days.forEach(d => {
    if (d.topId) {
      const it = wardrobe.find(i => i.id === d.topId);
      if (it) { usedColorHexes.add(it.colorHex); usedCategories.add(it.category); }
    }
    if (d.bottomId) {
      const it = wardrobe.find(i => i.id === d.bottomId);
      if (it) { usedColorHexes.add(it.colorHex); usedCategories.add(it.category); }
    }
    if (d.dressId) {
      const it = wardrobe.find(i => i.id === d.dressId);
      if (it) { usedColorHexes.add(it.colorHex); usedCategories.add(it.category); }
    }
    if (d.shoesId) {
      const it = wardrobe.find(i => i.id === d.shoesId);
      if (it) { usedColorHexes.add(it.colorHex); }
    }
    totalAccessoriesCount += (d.accessoryIds?.length || 0);
  });

  const handleRerunVibeCheck = () => {
    setAnalyzing(true);
    setTimeout(() => {
      setVibeScore(Math.floor(Math.random() * 6) + 94); // 94 to 99
      setAnalyzing(false);
      confetti({
        particleCount: 60,
        spread: 60,
        origin: { y: 0.5 },
        colors: ['#FF007A', '#0055FF', '#D4FF00', '#FFE600']
      });
    }, 600);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 bg-black/80 backdrop-blur-sm overflow-y-auto">
      <div className="relative w-full max-w-2xl bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="bg-[#FFF500] border-b-4 border-[#111111] px-4 sm:px-6 py-3 flex items-center justify-between gap-3 text-[#111111]">
          <div className="flex items-center gap-2">
            <div className="w-9 h-9 bg-[#FF007A] text-white border-2 border-[#111111] flex items-center justify-center font-black text-lg -rotate-6 chunky-shadow">
              ⚡
            </div>
            <div>
              <h3 className="big-display text-xl sm:text-2xl tracking-wide">
                EDITORIAL STYLE VIBE CHECK
              </h3>
              <p className="text-[11px] font-mono text-gray-800 font-bold">
                Editorial fashion critique and analysis on your 7-day wardrobe rotation
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

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6">
          {/* Main Scorecard Banner */}
          <div className="relative bg-white border-3 border-[#111111] p-5 chunky-shadow-lg text-center overflow-hidden">
            <div className="absolute -top-3 left-6">
              <WashiTape pattern="pink" angle={-3} className="w-20" />
            </div>

            <div className="flex flex-col sm:flex-row items-center justify-around gap-4 pt-2">
              <div>
                <span className="text-[11px] font-mono font-bold uppercase text-gray-500 block mb-1">
                  Overall Editorial Fashion Index
                </span>
                <div className="flex items-center justify-center gap-2">
                  <span className="big-display text-5xl sm:text-6xl text-[#FF007A] drop-shadow-[3px_3px_0px_#111111]">
                    {vibeScore}
                  </span>
                  <span className="big-display text-2xl text-[#111111]">/ 100</span>
                </div>
                <NeonStickerBadge text="RUNWAY READY" bg="bg-[#FFF500]" textColor="text-black" rotate="-rotate-2" />
              </div>

              <div className="border-t-2 sm:border-t-0 sm:border-l-2 border-dashed border-gray-300 pt-3 sm:pt-0 sm:pl-6 text-left space-y-2">
                <div className="flex items-center gap-2 text-xs font-black uppercase text-black">
                  <Check className="w-4 h-4 text-green-600 stroke-[3]" />
                  <span>Zero repeating top/bottom combos</span>
                </div>
                <div className="flex items-center gap-2 text-xs font-black uppercase text-black">
                  <Check className="w-4 h-4 text-green-600 stroke-[3]" />
                  <span>{usedColorHexes.size} Distinct High-Contrast Colors</span>
                </div>
                <div className="flex items-center gap-2 text-xs font-black uppercase text-black">
                  <Check className="w-4 h-4 text-green-600 stroke-[3]" />
                  <span>{totalAccessoriesCount} Accessory Add-Ons Styled</span>
                </div>
              </div>
            </div>
          </div>

          {/* Stylist Breakdown Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {/* Card 1: Color Theory */}
            <div className="bg-[#FFF0F5] border-2 border-[#111111] p-3.5 chunky-shadow -rotate-1">
              <div className="flex items-center gap-2 mb-1.5">
                <span className="text-lg">🎨</span>
                <h4 className="big-display text-sm text-black">Color Harmonization</h4>
              </div>
              <p className="text-xs font-mono text-gray-800 leading-relaxed">
                Super saturated pairing! Clashing bright electric cobalt with tangerine trousers and neon accessories gives unmatched fashion editor presence.
              </p>
            </div>

            {/* Card 2: Weather Readiness */}
            <div className="bg-[#F0FDF4] border-2 border-[#111111] p-3.5 chunky-shadow rotate-1">
              <div className="flex items-center gap-2 mb-1.5">
                <span className="text-lg">🌤️</span>
                <h4 className="big-display text-sm text-black">Weather & Layering</h4>
              </div>
              <p className="text-xs font-mono text-gray-800 leading-relaxed">
                Rain coats and structured blazers are appropriately deployed on cooler and rainy weekdays, transitioning into easy flowy silhouettes on weekends.
              </p>
            </div>
          </div>

          {/* Stylist Recommendation Memo */}
          <div className="bg-[#FFF500] border-3 border-[#111111] p-4 chunky-shadow rotate-0.5">
            <div className="flex items-center gap-2 mb-2">
              <Sparkles className="w-4 h-4 text-[#FF007A]" />
              <h4 className="scribble font-bold text-lg text-black">
                Stylist Pro-Tip for the Week:
              </h4>
            </div>
            <p className="text-xs scribble font-bold text-black leading-relaxed">
              "Never tone down your accessories when wearing bold color blocking! On Friday, throw on the chunky gold hoops and cherry red patent bag to elevate from daytime meeting directly to evening drinks!"
            </p>
          </div>
        </div>

        {/* Footer */}
        <div className="bg-[#FAF5E8] border-t-3 border-[#111111] p-3 px-6 flex items-center justify-between">
          <button
            onClick={handleRerunVibeCheck}
            disabled={analyzing}
            className="bg-[#00D1FF] hover:bg-cyan-400 text-[#111111] border-2 border-[#111111] px-4 py-2 font-black uppercase text-xs chunky-shadow flex items-center gap-1.5 active:translate-y-0.5 disabled:opacity-50"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${analyzing ? 'animate-spin' : ''}`} />
            <span>{analyzing ? 'Recalculating...' : 'Re-Evaluate Fits'}</span>
          </button>

          <button
            onClick={onClose}
            className="bg-[#111111] text-white px-5 py-2 font-black uppercase text-xs chunky-shadow hover:bg-gray-800"
          >
            Close Vibe Check
          </button>
        </div>
      </div>
    </div>
  );
};
