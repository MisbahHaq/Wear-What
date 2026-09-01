import React, { useState } from 'react';
import { 
  Sparkles, 
  RotateCw, 
  Calendar, 
  CloudSun, 
  Filter, 
  Zap,
  ArrowRight
} from 'lucide-react';
import confetti from 'canvas-confetti';
import { ClothingItem, DayOfWeek, DayType, WeeklyPlan } from '../types';
import { DAYS_OF_WEEK } from '../utils/outfitGenerator';
import { DayCard } from './DayCard';
import { CircularSticker, DoodleSparkle, DoodleSquiggle, NeonStickerBadge, RetroSmileyBadge } from './DoodleDecorations';

interface WeeklyPlannerProps {
  weeklyPlan: WeeklyPlan;
  wardrobe: ClothingItem[];
  currentCity: string;
  onShuffleDay: (day: DayOfWeek) => void;
  onShuffleWholeWeek: () => void;
  onToggleLock: (day: DayOfWeek) => void;
  onOpenSwapModal: (day: DayOfWeek, slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory', currentItemId?: string) => void;
  onOpenFlatLay: (day: DayOfWeek) => void;
  onChangeDayType: (day: DayOfWeek, newType: DayType) => void;
  onChangeWeather: (day: DayOfWeek) => void;
  onRemoveAccessory: (day: DayOfWeek, accessoryId: string) => void;
  onToggleStatus: (day: DayOfWeek) => void;
  onOpenWeatherModal: () => void;
  onOpenVibeCheckModal: () => void;
}

export const WeeklyPlanner: React.FC<WeeklyPlannerProps> = ({
  weeklyPlan,
  wardrobe,
  currentCity,
  onShuffleDay,
  onShuffleWholeWeek,
  onToggleLock,
  onOpenSwapModal,
  onOpenFlatLay,
  onChangeDayType,
  onChangeWeather,
  onRemoveAccessory,
  onToggleStatus,
  onOpenWeatherModal,
  onOpenVibeCheckModal,
}) => {
  const [filterType, setFilterType] = useState<string>('all');

  const handleMagicWeekGenerate = () => {
    onShuffleWholeWeek();
    confetti({
      particleCount: 100,
      spread: 80,
      origin: { y: 0.5 },
      colors: ['#FF007A', '#00D1FF', '#FFF500', '#FF5C00', '#111111']
    });
  };

  const displayedDays = DAYS_OF_WEEK.filter(day => {
    if (filterType === 'all') return true;
    return weeklyPlan[day]?.dayType === filterType;
  });

  return (
    <div className="space-y-6 pb-28">
      {/* Planner Hero Editorial Banner */}
      <div className="relative bg-[#FF007A] border-4 border-[#111111] p-5 sm:p-7 chunky-shadow-lg text-white overflow-hidden">
        {/* Halftone texture */}
        <div className="halftone opacity-15"></div>

        {/* Playful Stickers and Doodles */}
        <div className="absolute top-2 right-4 pointer-events-none opacity-90 hidden sm:block">
          <CircularSticker text="HOT OUTFIT" subtext="03" bg="bg-[#FFF500]" textColor="text-[#111111]" rotate="rotate-12" />
        </div>
        <div className="absolute bottom-2 left-1/3 pointer-events-none opacity-40">
          <DoodleSquiggle color="#FFFFFF" />
        </div>

        <div className="flex flex-col md:flex-row md:items-center justify-between gap-5 relative z-10">
          <div>
            <div className="flex items-center gap-2 mb-1 flex-wrap">
              <h2 className="big-display text-3xl sm:text-4xl text-white drop-shadow-[3px_3px_0px_#111111]">
                WEEKLY OUTFIT ROTATION
              </h2>
              <span className="bg-[#FFF500] text-[#111111] text-xs font-black uppercase px-2 py-0.5 border-2 border-[#111111] shadow-[2px_2px_0px_#111111] rotate-2">
                MON → SUN
              </span>
            </div>
            <p className="scribble text-base text-pink-100 max-w-xl">
              Zero outfit repeats, weather-synchronized layers, and curated accessories for every day of your week.
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex items-center gap-2.5 flex-wrap">
            <button
              onClick={onOpenWeatherModal}
              className="bg-white hover:bg-gray-100 text-[#111111] border-2 border-[#111111] px-3.5 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-1.5 active:translate-y-0.5 transition-all"
            >
              <CloudSun className="w-4 h-4 text-[#00D1FF]" />
              <span>Weather: {currentCity || 'Set City'}</span>
            </button>

            <button
              id="magic-auto-suggest-btn"
              onClick={handleMagicWeekGenerate}
              className="bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-3 border-[#111111] px-4 py-2 text-xs font-black uppercase tracking-wider chunky-shadow flex items-center gap-2 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
              title="Re-generate 7-day combinations avoiding duplicates"
            >
              <Sparkles className="w-4 h-4 text-[#FF007A]" />
              <span>Auto-Suggest Week ⚡</span>
            </button>
          </div>
        </div>

        {/* Quick Day Type filter bar & Vibe Check CTA */}
        <div className="pt-4 mt-5 border-t-2 border-dashed border-pink-300/60 flex flex-wrap items-center justify-between gap-3 text-xs relative z-10">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="font-black uppercase text-white tracking-wider flex items-center gap-1">
              <Filter className="w-3.5 h-3.5" /> Filter Days:
            </span>
            {[
              { id: 'all', label: 'All 7 Days' },
              { id: 'Office Wear', label: '💼 Office Edit' },
              { id: 'Casual', label: '🧢 Casual Edit' },
              { id: 'Weekend Wear', label: '🍸 Weekend Edit' },
            ].map(f => (
              <button
                key={f.id}
                onClick={() => setFilterType(f.id)}
                className={`px-3 py-1 text-xs font-black uppercase tracking-wider border-2 border-[#111111] transition-all ${
                  filterType === f.id
                    ? 'bg-[#FFF500] text-[#111111] chunky-shadow -translate-y-0.5'
                    : 'bg-white/95 text-[#111111] hover:bg-white'
                }`}
              >
                {f.label}
              </button>
            ))}
          </div>

          <button
            onClick={onOpenVibeCheckModal}
            className="bg-[#00D1FF] text-[#111111] border-2 border-[#111111] px-3.5 py-1.5 font-black text-xs uppercase tracking-wider hover:bg-cyan-300 chunky-shadow flex items-center gap-1.5 transition-all"
          >
            <Zap className="w-3.5 h-3.5 text-[#FF007A]" />
            <span>AI Stylist Vibe Check</span>
          </button>
        </div>
      </div>

      {/* 7-Day Asymmetrical Grid */}
      <div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 items-start">
          {displayedDays.map((day, idx) => {
            const outfit = weeklyPlan[day];
            if (!outfit) return null;

            return (
              <DayCard
                key={day}
                outfit={outfit}
                wardrobe={wardrobe}
                dayIndex={idx}
                onShuffleDay={onShuffleDay}
                onToggleLock={onToggleLock}
                onOpenSwapModal={onOpenSwapModal}
                onOpenFlatLay={onOpenFlatLay}
                onChangeDayType={onChangeDayType}
                onChangeWeather={onChangeWeather}
                onRemoveAccessory={onRemoveAccessory}
                onToggleStatus={onToggleStatus}
              />
            );
          })}
        </div>
      </div>
    </div>
  );
};

