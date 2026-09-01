import React, { useState, useRef, useEffect } from 'react';
import { Shirt, Calendar, PlusCircle, MapPin, Sparkles, User, LogOut, LogIn, ChevronDown, Check, Cloud } from 'lucide-react';
import { ActiveView } from '../types';
import { DoodleSquiggle, NeonStickerBadge, CircularSticker } from './DoodleDecorations';
import { useAuth } from '../contexts/AuthContext';

interface NavbarProps {
  activeView: ActiveView;
  setActiveView: (view: ActiveView) => void;
  wardrobeCount: number;
  currentCity: string;
  onOpenWeatherModal: () => void;
  onOpenUploadModal: () => void;
  onShuffleWeek: () => void;
  onOpenVibeCheckModal: () => void;
  onOpenAuthModal: (mode?: 'signin' | 'signup') => void;
}

export const Navbar: React.FC<NavbarProps> = ({
  activeView,
  setActiveView,
  wardrobeCount,
  currentCity,
  onOpenWeatherModal,
  onOpenUploadModal,
  onShuffleWeek,
  onOpenVibeCheckModal,
  onOpenAuthModal,
}) => {
  const { user, userProfile, signOut } = useAuth();
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // Close menu on outside click
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsUserMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const displayName = userProfile?.displayName || user?.displayName || user?.email?.split('@')[0] || 'Fashion Curator';
  const initial = (displayName[0] || 'U').toUpperCase();

  return (
    <header className="sticky top-0 z-40 w-full bg-[#FFF9ED] border-b-4 border-[#111111]">
      {/* Top Editorial Ticker Bar */}
      <div className="bg-[#FF007A] text-white border-b-2 border-[#111111] overflow-hidden py-1 select-none">
        <div className="animate-marquee whitespace-nowrap text-xs font-black tracking-widest uppercase flex items-center gap-8">
          <span>⚡ WEAR WHAT: EDITORIAL WARDROBE LAB</span>
          <span>✦ NO BORING OUTFITS ALLOWED</span>
          <span>★ 7-DAY ZERO-REPEAT GENERATOR</span>
          <span>✿ WEATHER-ADAPTIVE FLAT-LAY BOARDS</span>
          <span>⚡ THE CAPSULE EDIT</span>
          <span>✦ GENERATED FOR YOUR BEST SELF</span>
          <span>★ EDITORIAL ROTATION</span>
          <span>⚡ WEAR WHAT: EDITORIAL WARDROBE LAB</span>
          <span>✦ NO BORING OUTFITS ALLOWED</span>
          <span>★ 7-DAY ZERO-REPEAT GENERATOR</span>
        </div>
      </div>

      {/* Main Editorial Masthead */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 py-3 flex flex-wrap items-center justify-between gap-3 relative">
        {/* Brand Masthead with Skewed Big Display & Pinned Beta Badge */}
        <div className="flex items-center gap-3">
          <div 
            onClick={() => setActiveView('planner')} 
            className="cursor-pointer group flex items-center gap-3 relative"
            id="brand-logo-btn"
          >
            <div className="w-11 h-11 bg-[#FFF500] border-3 border-[#111111] rounded-none chunky-shadow flex items-center justify-center -rotate-3 group-hover:rotate-6 transition-transform">
              <span className="font-display text-2xl font-black">★</span>
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h1 className="big-display text-2xl sm:text-3xl text-[#111111] leading-none drop-shadow-[2px_2px_0px_#FF007A]">
                  WEAR WHAT
                </h1>
                <span className="inline-block bg-[#00D1FF] text-[#111111] border-2 border-[#111111] text-[10px] font-black uppercase px-1.5 py-0.2 -rotate-3 shadow-[2px_2px_0px_#111111]">
                  BETA v.02
                </span>
              </div>
              <p className="scribble text-xs text-gray-700 -mt-0.5 tracking-tight flex items-center gap-1.5">
                The Weekly Outfit Planner <span className="font-sans font-black text-[#FF5C00] not-italic text-[10px]">● NO-REPEAT ALGORITHM</span>
              </p>
            </div>
          </div>
        </div>

        {/* View Switchers & Controls */}
        <div className="flex items-center flex-wrap gap-2 sm:gap-3">
          {/* Weather City Pill */}
          <button
            id="nav-weather-btn"
            onClick={onOpenWeatherModal}
            className="flex items-center gap-1.5 bg-white px-3 py-1.5 border-2 border-[#111111] chunky-shadow text-xs font-funky font-bold hover:bg-[#FFF500] active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
            title="Change City or Adjust Forecast"
          >
            <MapPin className="w-3.5 h-3.5 text-[#FF007A]" />
            <span className="max-w-[110px] truncate">{currentCity || 'Weather'}</span>
            <span className="bg-[#FFF500] px-1 py-0.2 border border-[#111111] text-[10px] font-black">🌤️</span>
          </button>

          {/* Planner View Tab */}
          <button
            id="nav-tab-planner"
            onClick={() => setActiveView('planner')}
            className={`flex items-center gap-2 px-3.5 py-1.5 border-3 border-[#111111] text-xs font-black uppercase tracking-wider transition-all ${
              activeView === 'planner'
                ? 'bg-[#00D1FF] text-[#111111] chunky-shadow -translate-y-0.5'
                : 'bg-white text-[#111111] chunky-shadow hover:bg-gray-100 active:translate-y-0.5'
            }`}
          >
            <Calendar className="w-3.5 h-3.5" />
            <span>7-Day Planner</span>
          </button>

          {/* Closet View Tab */}
          <button
            id="nav-tab-closet"
            onClick={() => setActiveView('closet')}
            className={`flex items-center gap-2 px-3.5 py-1.5 border-3 border-[#111111] text-xs font-black uppercase tracking-wider transition-all ${
              activeView === 'closet'
                ? 'bg-[#FF5C00] text-white chunky-shadow -translate-y-0.5'
                : 'bg-white text-[#111111] chunky-shadow hover:bg-gray-100 active:translate-y-0.5'
            }`}
          >
            <Shirt className="w-3.5 h-3.5" />
            <span>Closet Vault</span>
            <span className="bg-[#FFF500] text-[#111111] text-[10px] font-mono px-1.5 py-0.2 border border-[#111111] font-black">
              {wardrobeCount}
            </span>
          </button>

          {/* AI Style Vibe Check Button */}
          <button
            id="nav-btn-vibe-check"
            onClick={onOpenVibeCheckModal}
            className="hidden sm:flex items-center gap-1.5 bg-[#FFF500] text-[#111111] px-3 py-1.5 border-2 border-[#111111] chunky-shadow text-xs font-black uppercase tracking-wider hover:bg-yellow-300 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
          >
            <Sparkles className="w-3.5 h-3.5 text-[#FF007A]" />
            <span>Vibe Check</span>
          </button>

          {/* Add Item Button */}
          <button
            id="nav-btn-upload"
            onClick={onOpenUploadModal}
            className="flex items-center gap-1.5 bg-[#FF007A] text-white px-3.5 py-1.5 border-3 border-[#111111] chunky-shadow text-xs font-black uppercase tracking-wider hover:bg-[#e0006c] active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
          >
            <PlusCircle className="w-4 h-4 stroke-[2.5]" />
            <span>Upload</span>
          </button>

          {/* User Account / Sign In Controls */}
          {user ? (
            <div className="relative" ref={menuRef}>
              <button
                id="nav-user-menu-btn"
                onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                className="flex items-center gap-1.5 bg-white pl-1.5 pr-2 py-1 border-3 border-[#111111] chunky-shadow hover:bg-yellow-50 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
                title={user.email || 'User Account'}
              >
                {user.photoURL ? (
                  <img
                    src={user.photoURL}
                    alt={displayName}
                    className="w-6 h-6 rounded-full border border-[#111111] object-cover"
                  />
                ) : (
                  <div className="w-6 h-6 bg-[#FF007A] text-white border border-[#111111] flex items-center justify-center text-xs font-black">
                    {initial}
                  </div>
                )}
                <span className="text-xs font-black uppercase max-w-[80px] sm:max-w-[100px] truncate">
                  {displayName}
                </span>
                <ChevronDown className="w-3.5 h-3.5 text-gray-700" />
              </button>

              {/* Dropdown Menu */}
              {isUserMenuOpen && (
                <div className="absolute right-0 mt-2 w-64 bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-lg z-50 p-3 space-y-3">
                  <div className="border-b-2 border-dashed border-gray-400 pb-2.5">
                    <div className="flex items-center gap-2 mb-1">
                      <div className="w-7 h-7 bg-[#00D1FF] text-[#111111] border-2 border-[#111111] flex items-center justify-center text-xs font-black">
                        {initial}
                      </div>
                      <div className="min-w-0">
                        <p className="font-black text-xs uppercase text-[#111111] truncate">{displayName}</p>
                        <p className="font-mono text-[10px] text-gray-600 truncate">{user.email}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-1 text-[10px] font-mono font-bold text-green-700 mt-1">
                      <Cloud className="w-3 h-3 text-green-600" />
                      <span>Personal Library Synced</span>
                    </div>
                  </div>

                  {/* Wardrobe stats */}
                  <div className="bg-white border-2 border-[#111111] p-2 text-xs font-mono">
                    <div className="flex justify-between font-bold text-gray-800">
                      <span>Vault Pieces:</span>
                      <span className="text-[#FF007A] font-black">{wardrobeCount} items</span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="space-y-1.5 pt-1">
                    <button
                      onClick={() => {
                        setIsUserMenuOpen(false);
                        onOpenUploadModal();
                      }}
                      className="w-full text-left bg-white hover:bg-yellow-100 text-[#111111] border-2 border-[#111111] px-2.5 py-1.5 text-xs font-black uppercase flex items-center gap-2"
                    >
                      <PlusCircle className="w-3.5 h-3.5 text-[#FF007A]" />
                      <span>Upload New Clothes</span>
                    </button>

                    <button
                      onClick={async () => {
                        setIsUserMenuOpen(false);
                        await signOut();
                      }}
                      className="w-full text-left bg-[#FFF0F5] hover:bg-pink-100 text-red-700 border-2 border-[#111111] px-2.5 py-1.5 text-xs font-black uppercase flex items-center gap-2"
                    >
                      <LogOut className="w-3.5 h-3.5 text-red-600" />
                      <span>Sign Out</span>
                    </button>
                  </div>
                </div>
              )}
            </div>
          ) : (
            <button
              id="nav-btn-signin"
              onClick={() => onOpenAuthModal('signin')}
              className="flex items-center gap-1.5 bg-[#FFF500] hover:bg-yellow-300 text-[#111111] px-3 py-1.5 border-3 border-[#111111] chunky-shadow text-xs font-black uppercase tracking-wider active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all"
            >
              <LogIn className="w-3.5 h-3.5 stroke-[2.5]" />
              <span>Log In</span>
            </button>
          )}
        </div>
      </div>
    </header>
  );
};

