import React, { useState } from 'react';
import { X, Search, CloudRain, Sun, Wind, Thermometer, MapPin, Check, Sparkles } from 'lucide-react';
import { DayOfWeek, DayWeather, WeatherCondition } from '../types';
import { fetchCityWeather, WEATHER_PRESETS } from '../utils/weather';
import { DAYS_OF_WEEK } from '../utils/outfitGenerator';
import { NeonStickerBadge, WashiTape } from './DoodleDecorations';

interface WeatherModalProps {
  isOpen: boolean;
  onClose: () => void;
  currentCity: string;
  weatherForecast: DayWeather[];
  onUpdateCityWeather: (city: string, forecast: DayWeather[]) => void;
  onUpdateSingleDayWeather: (day: DayOfWeek, weather: DayWeather) => void;
  targetDay?: DayOfWeek | null;
}

export const WeatherModal: React.FC<WeatherModalProps> = ({
  isOpen,
  onClose,
  currentCity,
  weatherForecast,
  onUpdateCityWeather,
  onUpdateSingleDayWeather,
  targetDay,
}) => {
  if (!isOpen) return null;

  const [cityInput, setCityInput] = useState(currentCity || '');
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');

  const handleSearchCity = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!cityInput.trim()) return;

    setLoading(true);
    setErrorMsg('');
    try {
      const forecast = await fetchCityWeather(cityInput.trim());
      onUpdateCityWeather(cityInput.trim(), forecast);
      onClose();
    } catch (err) {
      setErrorMsg('Could not fetch weather. Using simulated forecast.');
    } finally {
      setLoading(false);
    }
  };

  const handleApplyPreset = (preset: typeof WEATHER_PRESETS[0]) => {
    const updated = weatherForecast.map(w => ({
      ...w,
      tempC: preset.tempC,
      condition: preset.condition,
      label: preset.label,
      icon: preset.icon,
    }));
    onUpdateCityWeather(cityInput || preset.label, updated);
    onClose();
  };

  const handleSingleDayCondition = (day: DayOfWeek, cond: WeatherCondition, temp: number, icon: string, label: string) => {
    const dayWeather: DayWeather = {
      city: currentCity,
      tempC: temp,
      condition: cond,
      icon,
      label,
      rainChance: cond === 'rainy' ? 85 : cond === 'hot' ? 0 : 15,
    };
    onUpdateSingleDayWeather(day, dayWeather);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 bg-black/75 backdrop-blur-sm overflow-y-auto">
      <div className="relative w-full max-w-2xl bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="bg-[#00D1FF] border-b-4 border-[#111111] px-4 sm:px-6 py-3 flex items-center justify-between gap-3 text-[#111111]">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-[#FFF500] text-black border-2 border-[#111111] flex items-center justify-center font-black text-lg -rotate-3 chunky-shadow">
              🌤️
            </div>
            <div>
              <h3 className="big-display text-xl sm:text-2xl tracking-wide">
                WEATHER & CLIMATE ADJUSTER
              </h3>
              <p className="text-[11px] font-mono text-gray-800">
                Adjust 7-day temperature and rain to auto-layer outfits
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
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-5">
          {/* City Search Bar */}
          <form onSubmit={handleSearchCity} className="space-y-2">
            <label className="block text-xs font-black uppercase text-black">
              1. Enter Your City (Live Forecast API)
            </label>
            <div className="flex items-center gap-2">
              <div className="relative flex-1">
                <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[#FF007A]" />
                <input
                  type="text"
                  value={cityInput}
                  onChange={(e) => setCityInput(e.target.value)}
                  placeholder="e.g. New York, London, Tokyo, Paris, Sydney..."
                  className="w-full pl-9 pr-3 py-2 bg-white border-2 border-[#111111] text-xs font-mono font-bold focus:outline-none"
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="bg-[#FFF500] hover:bg-yellow-400 text-black border-2 border-[#111111] px-4 py-2 font-black uppercase text-xs chunky-shadow disabled:opacity-50 flex items-center gap-1.5"
              >
                {loading ? <span className="animate-spin">🌀</span> : <Search className="w-3.5 h-3.5" />}
                <span>Fetch Weather</span>
              </button>
            </div>

            {errorMsg && (
              <p className="text-xs font-mono text-red-600 font-bold">{errorMsg}</p>
            )}
          </form>

          {/* Quick Weather Presets */}
          <div>
            <label className="block text-xs font-black uppercase text-black mb-2">
              2. Or Quick Weather Vibe Presets:
            </label>
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
              {WEATHER_PRESETS.map((preset) => (
                <button
                  key={preset.label}
                  type="button"
                  onClick={() => handleApplyPreset(preset)}
                  className="bg-white hover:bg-[#FFF0F5] border-2 border-[#111111] p-2.5 text-left chunky-shadow hover:-translate-y-0.5 transition-all group"
                >
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xl">{preset.icon}</span>
                    <span className="font-mono text-xs font-bold text-gray-700">{preset.tempC}°C</span>
                  </div>
                  <p className="font-black uppercase text-xs text-[#111111] group-hover:text-[#FF007A]">
                    {preset.label}
                  </p>
                </button>
              ))}
            </div>
          </div>

          {/* 7-Day Quick Weather Table */}
          <div className="bg-white border-2 border-[#111111] p-3.5 chunky-shadow">
            <h4 className="font-black uppercase text-xs text-black mb-2 flex items-center justify-between">
              <span>7-Day Active Forecast Overview:</span>
              <span className="text-[10px] text-gray-500 font-mono">Live dynamic atmospheric layer</span>
            </h4>

            <div className="grid grid-cols-7 gap-1.5 text-center">
              {DAYS_OF_WEEK.map((day, idx) => {
                const w = weatherForecast[idx] || { tempC: 22, icon: '☀️', condition: 'sunny', label: 'Sunny' };
                return (
                  <div 
                    key={day} 
                    className="bg-[#FFF9ED] border border-[#111111] p-1.5 shadow-[1px_1px_0px_#111111]"
                  >
                    <p className="font-black uppercase text-[11px] text-black">{day}</p>
                    <span className="text-lg block my-0.5">{w.icon}</span>
                    <p className="font-mono text-[10px] font-bold text-gray-700">{w.tempC}°C</p>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="bg-[#FAF5E8] border-t-3 border-[#111111] p-3 px-6 flex items-center justify-end">
          <button
            onClick={onClose}
            className="bg-[#111111] text-white px-5 py-2 font-black uppercase text-xs chunky-shadow hover:bg-gray-800"
          >
            Apply & Close
          </button>
        </div>
      </div>
    </div>
  );
};
