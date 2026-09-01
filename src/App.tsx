import React, { useState, useEffect } from 'react';
import { ActiveView, ClothingCategory, ClothingItem, DailyOutfit, DayOfWeek, DayType, DayWeather, WeeklyPlan } from './types';
import { INITIAL_WARDROBE } from './data/initialWardrobe';
import { DEFAULT_WEEK_WEATHER, fetchCityWeather } from './utils/weather';
import { DAYS_OF_WEEK, generateDayOutfit, generateFullWeekPlan } from './utils/outfitGenerator';
import { Navbar } from './components/Navbar';
import { WeeklyPlanner } from './components/WeeklyPlanner';
import { ClosetView } from './components/ClosetView';
import { FlatLayCollageModal } from './components/FlatLayCollageModal';
import { QuickSwapModal } from './components/QuickSwapModal';
import { UploadModal } from './components/UploadModal';
import { WeatherModal } from './components/WeatherModal';
import { StyleVibeCheckModal } from './components/StyleVibeCheckModal';
import { AccessoriesDrawer } from './components/AccessoriesDrawer';
import { AuthModal } from './components/AuthModal';
import { DoodleSparkle, DoodleSquiggle, WashiTape } from './components/DoodleDecorations';
import { useAuth } from './contexts/AuthContext';
import {
  subscribeToUserWardrobe,
  subscribeToUserWeeklyPlan,
  initializeUserWardrobeIfEmpty,
  saveUserClothingItem,
  deleteUserClothingItem,
  saveUserWeeklyPlan,
  saveUserDailyOutfit,
  resetUserToDemoWardrobe,
} from './services/wardrobeFirestore';
import { Sparkles, UserPlus, Cloud } from 'lucide-react';

const STORAGE_KEY_WARDROBE = 'wear_what_wardrobe_v2';
const STORAGE_KEY_PLAN = 'wear_what_plan_v2';
const STORAGE_KEY_CITY = 'wear_what_city_v2';

export default function App() {
  const { user, userProfile, updateUserCity } = useAuth();

  // --- STATE ---
  const [activeView, setActiveView] = useState<ActiveView>('planner');

  // Wardrobe items state
  const [wardrobe, setWardrobe] = useState<ClothingItem[]>(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY_WARDROBE);
      if (saved) return JSON.parse(saved);
    } catch (e) {
      console.warn('Failed to parse saved wardrobe', e);
    }
    return INITIAL_WARDROBE;
  });

  // Current City & 7-Day Weather
  const [currentCity, setCurrentCity] = useState<string>(() => {
    return localStorage.getItem(STORAGE_KEY_CITY) || 'Karachi, Pakistan';
  });

  const [weatherForecast, setWeatherForecast] = useState<DayWeather[]>(DEFAULT_WEEK_WEATHER);

  // Weekly Plan state
  const [weeklyPlan, setWeeklyPlan] = useState<WeeklyPlan>(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY_PLAN);
      if (saved) return JSON.parse(saved);
    } catch (e) {
      console.warn('Failed to parse saved plan', e);
    }
    return generateFullWeekPlan(INITIAL_WARDROBE, undefined, DEFAULT_WEEK_WEATHER);
  });

  // Modals state
  const [flatLayDay, setFlatLayDay] = useState<DayOfWeek | null>(null);
  const [swapModalState, setSwapModalState] = useState<{
    isOpen: boolean;
    day: DayOfWeek;
    slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory';
    currentItemId?: string;
  }>({
    isOpen: false,
    day: 'Mon',
    slot: 'top',
  });

  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<ClothingItem | null>(null);
  const [isWeatherModalOpen, setIsWeatherModalOpen] = useState(false);
  const [weatherTargetDay, setWeatherTargetDay] = useState<DayOfWeek | null>(null);
  const [isVibeCheckModalOpen, setIsVibeCheckModalOpen] = useState(false);

  // Auth Modal state
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
  const [authModalMode, setAuthModalMode] = useState<'signin' | 'signup'>('signin');

  // --- FIRESTORE USER SYNC ---
  useEffect(() => {
    if (user) {
      // 1. Initial starter check
      initializeUserWardrobeIfEmpty(user.uid).then((items) => {
        if (items) {
          setWardrobe(items);
        }
      });

      // 2. Realtime listener for wardrobe
      const unsubWardrobe = subscribeToUserWardrobe(user.uid, (items) => {
        setWardrobe(items);
      });

      // 3. Realtime listener for weekly plan
      const unsubPlan = subscribeToUserWeeklyPlan(user.uid, (cloudPlan) => {
        if (cloudPlan) {
          setWeeklyPlan(cloudPlan);
        }
      });

      // 4. Sync city if in profile
      if (userProfile?.currentCity) {
        setCurrentCity(userProfile.currentCity);
      }

      return () => {
        unsubWardrobe();
        unsubPlan();
      };
    } else {
      // Guest mode: fallback to local storage
      try {
        const saved = localStorage.getItem(STORAGE_KEY_WARDROBE);
        if (saved) setWardrobe(JSON.parse(saved));
        const savedPlan = localStorage.getItem(STORAGE_KEY_PLAN);
        if (savedPlan) setWeeklyPlan(JSON.parse(savedPlan));
      } catch (e) {
        console.warn('Guest localStorage sync fallback error', e);
      }
    }
  }, [user]);

  // Sync to local storage for guest session
  useEffect(() => {
    if (!user) {
      localStorage.setItem(STORAGE_KEY_WARDROBE, JSON.stringify(wardrobe));
    }
  }, [wardrobe, user]);

  useEffect(() => {
    if (!user) {
      localStorage.setItem(STORAGE_KEY_PLAN, JSON.stringify(weeklyPlan));
    }
  }, [weeklyPlan, user]);

  useEffect(() => {
    if (!user) {
      localStorage.setItem(STORAGE_KEY_CITY, currentCity);
    }
  }, [currentCity, user]);

  // --- ACTIONS ---

  // 1. Regenerate entire 7-day week (zero duplicates)
  const handleShuffleWholeWeek = () => {
    const newPlan = generateFullWeekPlan(wardrobe, weeklyPlan, weatherForecast);
    setWeeklyPlan(newPlan);
    if (user) {
      saveUserWeeklyPlan(user.uid, newPlan).catch(console.error);
    }
  };

  // 2. Shuffle single day
  const handleShuffleDay = (day: DayOfWeek) => {
    const prevDay = weeklyPlan[day];
    if (!prevDay || prevDay.isLocked) return;

    const dayIndex = DAYS_OF_WEEK.indexOf(day);
    const usedCombos = new Set<string>();
    const usedDresses = new Set<string>();
    const usedTops = new Set<string>();
    const usedBottoms = new Set<string>();
    const usedShoes = new Set<string>();
    const usedAccessories = new Set<string>();

    // Register other days' used pieces
    DAYS_OF_WEEK.forEach(otherDay => {
      if (otherDay !== day && weeklyPlan[otherDay]) {
        const d = weeklyPlan[otherDay];
        if (d.topId && d.bottomId) {
          usedCombos.add(`${d.topId}__${d.bottomId}`);
          usedTops.add(d.topId);
          usedBottoms.add(d.bottomId);
        }
        if (d.dressId) usedDresses.add(d.dressId);
        if (d.shoesId) usedShoes.add(d.shoesId);
        d.accessoryIds?.forEach(a => usedAccessories.add(a));
      }
    });

    const generated = generateDayOutfit(
      wardrobe,
      day,
      dayIndex,
      prevDay.dayType,
      prevDay.weather,
      usedCombos,
      usedDresses,
      usedTops,
      usedBottoms,
      usedShoes,
      usedAccessories
    );

    const updatedOutfit: DailyOutfit = {
      ...prevDay,
      topId: generated.topId,
      bottomId: generated.bottomId,
      dressId: generated.dressId,
      outerwearId: generated.outerwearId,
      shoesId: generated.shoesId,
      accessoryIds: generated.accessoryIds || [],
    };

    setWeeklyPlan(prev => ({
      ...prev,
      [day]: updatedOutfit
    }));

    if (user) {
      saveUserDailyOutfit(user.uid, updatedOutfit).catch(console.error);
    }
  };

  // 3. Toggle Day Lock
  const handleToggleLock = (day: DayOfWeek) => {
    const updatedOutfit = {
      ...weeklyPlan[day],
      isLocked: !weeklyPlan[day].isLocked
    };
    setWeeklyPlan(prev => ({
      ...prev,
      [day]: updatedOutfit
    }));
    if (user) {
      saveUserDailyOutfit(user.uid, updatedOutfit).catch(console.error);
    }
  };

  // 4. Change Day Type (Office / Casual / Weekend)
  const handleChangeDayType = (day: DayOfWeek, newType: DayType) => {
    const updatedDay: DailyOutfit = {
      ...weeklyPlan[day],
      dayType: newType,
    };
    setWeeklyPlan(prev => ({
      ...prev,
      [day]: updatedDay
    }));
    if (user) {
      saveUserDailyOutfit(user.uid, updatedDay).catch(console.error);
    }
  };

  // 5. Open Quick Swap modal for a slot
  const handleOpenSwapModal = (
    day: DayOfWeek,
    slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory',
    currentItemId?: string
  ) => {
    setSwapModalState({
      isOpen: true,
      day,
      slot,
      currentItemId,
    });
  };

  // 6. Select item for slot
  const handleSelectItemForSlot = (
    day: DayOfWeek,
    slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory',
    item: ClothingItem
  ) => {
    const dayOutfit = { ...weeklyPlan[day] };
    if (slot === 'dress') {
      dayOutfit.dressId = item.id;
      dayOutfit.topId = undefined;
      dayOutfit.bottomId = undefined;
    } else if (slot === 'top') {
      dayOutfit.topId = item.id;
      dayOutfit.dressId = undefined;
    } else if (slot === 'bottom') {
      dayOutfit.bottomId = item.id;
      dayOutfit.dressId = undefined;
    } else if (slot === 'outerwear') {
      dayOutfit.outerwearId = item.id;
    } else if (slot === 'shoes') {
      dayOutfit.shoesId = item.id;
    } else if (slot === 'accessory') {
      if (!dayOutfit.accessoryIds.includes(item.id)) {
        dayOutfit.accessoryIds = [...dayOutfit.accessoryIds, item.id];
      }
    }

    setWeeklyPlan(prev => ({
      ...prev,
      [day]: dayOutfit
    }));

    if (user) {
      saveUserDailyOutfit(user.uid, dayOutfit).catch(console.error);
    }
  };

  // 7. Clear slot
  const handleClearSlot = (
    day: DayOfWeek,
    slot: 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory'
  ) => {
    const dayOutfit = { ...weeklyPlan[day] };
    if (slot === 'dress') dayOutfit.dressId = undefined;
    if (slot === 'top') dayOutfit.topId = undefined;
    if (slot === 'bottom') dayOutfit.bottomId = undefined;
    if (slot === 'outerwear') dayOutfit.outerwearId = undefined;
    if (slot === 'shoes') dayOutfit.shoesId = undefined;

    setWeeklyPlan(prev => ({
      ...prev,
      [day]: dayOutfit
    }));

    if (user) {
      saveUserDailyOutfit(user.uid, dayOutfit).catch(console.error);
    }
  };

  // 8. Add accessory to day (from drawer)
  const handleAddAccessoryToDay = (day: DayOfWeek, accessory: ClothingItem) => {
    const dayOutfit = { ...weeklyPlan[day] };
    if (!dayOutfit.accessoryIds.includes(accessory.id)) {
      dayOutfit.accessoryIds = [...dayOutfit.accessoryIds, accessory.id];
      setWeeklyPlan(prev => ({
        ...prev,
        [day]: dayOutfit
      }));
      if (user) {
        saveUserDailyOutfit(user.uid, dayOutfit).catch(console.error);
      }
    }
  };

  // 9. Remove accessory from day
  const handleRemoveAccessory = (day: DayOfWeek, accessoryId: string) => {
    const dayOutfit = { ...weeklyPlan[day] };
    dayOutfit.accessoryIds = dayOutfit.accessoryIds.filter(id => id !== accessoryId);
    setWeeklyPlan(prev => ({
      ...prev,
      [day]: dayOutfit
    }));
    if (user) {
      saveUserDailyOutfit(user.uid, dayOutfit).catch(console.error);
    }
  };

  // 10. Toggle status (planned -> worn -> loved)
  const handleToggleStatus = (day: DayOfWeek) => {
    const current = weeklyPlan[day].status || 'planned';
    const nextStatus = current === 'planned' ? 'worn' : current === 'worn' ? 'loved' : 'planned';
    const updatedOutfit = {
      ...weeklyPlan[day],
      status: nextStatus
    };
    setWeeklyPlan(prev => ({
      ...prev,
      [day]: updatedOutfit
    }));
    if (user) {
      saveUserDailyOutfit(user.uid, updatedOutfit).catch(console.error);
    }
  };

  // 11. Save styling note for day
  const handleUpdateNotes = (day: DayOfWeek, notes: string) => {
    const updatedOutfit = {
      ...weeklyPlan[day],
      notes
    };
    setWeeklyPlan(prev => ({
      ...prev,
      [day]: updatedOutfit
    }));
    if (user) {
      saveUserDailyOutfit(user.uid, updatedOutfit).catch(console.error);
    }
  };

  // 12. Save uploaded / edited clothing items
  const handleSaveUploadedItems = (newItems: ClothingItem[]) => {
    if (editingItem) {
      // Update existing item
      const updatedItem = newItems[0];
      setWardrobe(prev => prev.map(i => i.id === editingItem.id ? updatedItem : i));
      if (user) {
        saveUserClothingItem(user.uid, updatedItem).catch(console.error);
      }
      setEditingItem(null);
    } else {
      // Prepend new items
      setWardrobe(prev => [...newItems, ...prev]);
      if (user) {
        newItems.forEach(item => {
          saveUserClothingItem(user.uid, item).catch(console.error);
        });
      }
    }
  };

  // 13. Delete wardrobe item
  const handleDeleteItem = (id: string) => {
    if (confirm('Remove this piece from your personal wardrobe?')) {
      setWardrobe(prev => prev.filter(i => i.id !== id));
      if (user) {
        deleteUserClothingItem(user.uid, id).catch(console.error);
      }

      // Also clean up from plan
      const nextPlan = { ...weeklyPlan };
      DAYS_OF_WEEK.forEach(d => {
        if (nextPlan[d].topId === id) nextPlan[d].topId = undefined;
        if (nextPlan[d].bottomId === id) nextPlan[d].bottomId = undefined;
        if (nextPlan[d].dressId === id) nextPlan[d].dressId = undefined;
        if (nextPlan[d].outerwearId === id) nextPlan[d].outerwearId = undefined;
        if (nextPlan[d].shoesId === id) nextPlan[d].shoesId = undefined;
        nextPlan[d].accessoryIds = nextPlan[d].accessoryIds.filter(a => a !== id);
      });
      setWeeklyPlan(nextPlan);
      if (user) {
        saveUserWeeklyPlan(user.uid, nextPlan).catch(console.error);
      }
    }
  };

  // 14. Toggle favorite
  const handleToggleFavorite = (id: string) => {
    const updated = wardrobe.map(i => {
      if (i.id === id) {
        const itemUpdated = { ...i, favorite: !i.favorite };
        if (user) {
          saveUserClothingItem(user.uid, itemUpdated).catch(console.error);
        }
        return itemUpdated;
      }
      return i;
    });
    setWardrobe(updated);
  };

  // 15. Edit item modal trigger
  const handleEditItem = (item: ClothingItem) => {
    setEditingItem(item);
    setIsUploadModalOpen(true);
  };

  // 16. Reset demo
  const handleResetToDemo = () => {
    if (confirm('Reset your personal wardrobe to curated high-fashion starter pieces?')) {
      if (user) {
        resetUserToDemoWardrobe(user.uid).then(() => {
          setWardrobe(INITIAL_WARDROBE);
          const freshPlan = generateFullWeekPlan(INITIAL_WARDROBE, undefined, weatherForecast);
          setWeeklyPlan(freshPlan);
        }).catch(console.error);
      } else {
        setWardrobe(INITIAL_WARDROBE);
        const freshPlan = generateFullWeekPlan(INITIAL_WARDROBE, undefined, weatherForecast);
        setWeeklyPlan(freshPlan);
      }
    }
  };

  // 17. Update city weather
  const handleUpdateCityWeather = (city: string, forecast: DayWeather[]) => {
    setCurrentCity(city);
    setWeatherForecast(forecast);
    if (user) {
      updateUserCity(city).catch(console.error);
    }
    // Update daily plan weather
    setWeeklyPlan(prev => {
      const next = { ...prev };
      DAYS_OF_WEEK.forEach((d, idx) => {
        if (next[d] && forecast[idx]) {
          next[d].weather = forecast[idx];
        }
      });
      if (user) {
        saveUserWeeklyPlan(user.uid, next).catch(console.error);
      }
      return next;
    });
  };

  const handleUpdateSingleDayWeather = (day: DayOfWeek, dayWeather: DayWeather) => {
    const updatedOutfit = {
      ...weeklyPlan[day],
      weather: dayWeather
    };
    setWeeklyPlan(prev => ({
      ...prev,
      [day]: updatedOutfit
    }));
    if (user) {
      saveUserDailyOutfit(user.uid, updatedOutfit).catch(console.error);
    }
  };

  return (
    <div className="min-h-screen bg-[#FFF9ED] text-[#1A1A1A] flex flex-col font-sans">
      {/* Top Navbar with Marquee, Controls & User Auth */}
      <Navbar
        activeView={activeView}
        setActiveView={setActiveView}
        wardrobeCount={wardrobe.length}
        currentCity={currentCity}
        onOpenWeatherModal={() => {
          setWeatherTargetDay(null);
          setIsWeatherModalOpen(true);
        }}
        onOpenUploadModal={() => {
          setEditingItem(null);
          setIsUploadModalOpen(true);
        }}
        onShuffleWeek={handleShuffleWholeWeek}
        onOpenVibeCheckModal={() => setIsVibeCheckModalOpen(true)}
        onOpenAuthModal={(mode) => {
          setAuthModalMode(mode || 'signin');
          setIsAuthModalOpen(true);
        }}
      />

      {/* Guest Mode Callout Bar if not signed in */}
      {!user && (
        <div className="bg-[#FFF500] border-b-3 border-[#111111] px-4 py-2 text-xs font-mono">
          <div className="max-w-7xl mx-auto flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2 font-bold text-[#111111]">
              <Sparkles className="w-4 h-4 text-[#FF007A]" />
              <span>
                Personal Library: <span className="font-normal text-gray-800">You are browsing in guest mode. Sign up to sync your custom closet across devices.</span>
              </span>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => {
                  setAuthModalMode('signup');
                  setIsAuthModalOpen(true);
                }}
                className="bg-[#FF007A] hover:bg-[#e0006c] text-white px-2.5 py-1 border-2 border-[#111111] font-black uppercase text-[10px] shadow-[2px_2px_0px_#111111] flex items-center gap-1 active:translate-x-0.5 active:translate-y-0.5"
              >
                <UserPlus className="w-3 h-3" />
                <span>Create Personal Closet</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Main Viewport Container */}
      <main className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 pt-6">
        {activeView === 'planner' && (
          <WeeklyPlanner
            weeklyPlan={weeklyPlan}
            wardrobe={wardrobe}
            currentCity={currentCity}
            onShuffleDay={handleShuffleDay}
            onShuffleWholeWeek={handleShuffleWholeWeek}
            onToggleLock={handleToggleLock}
            onOpenSwapModal={handleOpenSwapModal}
            onOpenFlatLay={(day) => setFlatLayDay(day)}
            onChangeDayType={handleChangeDayType}
            onChangeWeather={(day) => {
              setWeatherTargetDay(day);
              setIsWeatherModalOpen(true);
            }}
            onRemoveAccessory={handleRemoveAccessory}
            onToggleStatus={handleToggleStatus}
            onOpenWeatherModal={() => {
              setWeatherTargetDay(null);
              setIsWeatherModalOpen(true);
            }}
            onOpenVibeCheckModal={() => setIsVibeCheckModalOpen(true)}
          />
        )}

        {activeView === 'closet' && (
          <ClosetView
            wardrobe={wardrobe}
            onOpenUploadModal={() => {
              setEditingItem(null);
              setIsUploadModalOpen(true);
            }}
            onDeleteItem={handleDeleteItem}
            onToggleFavorite={handleToggleFavorite}
            onEditItem={handleEditItem}
            onResetToDemo={handleResetToDemo}
          />
        )}
      </main>

      {/* Floating Accessories Rail (Available across both views) */}
      <AccessoriesDrawer
        wardrobe={wardrobe}
        onAddAccessoryToDay={handleAddAccessoryToDay}
        onOpenUploadModal={() => {
          setEditingItem(null);
          setIsUploadModalOpen(true);
        }}
      />

      {/* --- MODALS --- */}

      {/* 1. Flat-Lay Scrapbook Collage Modal */}
      {flatLayDay && weeklyPlan[flatLayDay] && (
        <FlatLayCollageModal
          isOpen={Boolean(flatLayDay)}
          onClose={() => setFlatLayDay(null)}
          day={flatLayDay}
          outfit={weeklyPlan[flatLayDay]}
          wardrobe={wardrobe}
          onOpenSwapModal={handleOpenSwapModal}
          onUpdateNotes={handleUpdateNotes}
        />
      )}

      {/* 2. Quick Swap Modal (Manual Override) */}
      <QuickSwapModal
        isOpen={swapModalState.isOpen}
        onClose={() => setSwapModalState(prev => ({ ...prev, isOpen: false }))}
        day={swapModalState.day}
        slot={swapModalState.slot}
        currentItemId={swapModalState.currentItemId}
        wardrobe={wardrobe}
        onSelectItem={handleSelectItemForSlot}
        onClearSlot={handleClearSlot}
      />

      {/* 3. Upload & Tagging Modal */}
      <UploadModal
        isOpen={isUploadModalOpen}
        onClose={() => {
          setIsUploadModalOpen(false);
          setEditingItem(null);
        }}
        onSaveItems={handleSaveUploadedItems}
        editingItem={editingItem}
      />

      {/* 4. Weather Adjuster Modal */}
      <WeatherModal
        isOpen={isWeatherModalOpen}
        onClose={() => setIsWeatherModalOpen(false)}
        currentCity={currentCity}
        weatherForecast={weatherForecast}
        onUpdateCityWeather={handleUpdateCityWeather}
        onUpdateSingleDayWeather={handleUpdateSingleDayWeather}
        targetDay={weatherTargetDay}
      />

      {/* 5. AI Style Vibe Check Modal */}
      <StyleVibeCheckModal
        isOpen={isVibeCheckModalOpen}
        onClose={() => setIsVibeCheckModalOpen(false)}
        weeklyPlan={weeklyPlan}
        wardrobe={wardrobe}
      />

      {/* 6. Auth Modal (Sign In / Sign Up) */}
      <AuthModal
        isOpen={isAuthModalOpen}
        onClose={() => setIsAuthModalOpen(false)}
        initialMode={authModalMode}
      />
    </div>
  );
}
