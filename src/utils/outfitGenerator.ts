import { ClothingItem, DailyOutfit, DayOfWeek, DayType, DayWeather, WeeklyPlan } from '../types';

export const DAYS_OF_WEEK: DayOfWeek[] = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export const DAY_FULL_NAMES: Record<DayOfWeek, string> = {
  Mon: 'Monday',
  Tue: 'Tuesday',
  Wed: 'Wednesday',
  Thu: 'Thursday',
  Fri: 'Friday',
  Sat: 'Saturday',
  Sun: 'Sunday',
};

// Filter clothing items by style match and weather compatibility
export function filterItemsForDay(
  items: ClothingItem[],
  dayType: DayType,
  weather: DayWeather
): ClothingItem[] {
  return items.filter(item => {
    // 1. Style class match
    const styleMatches = 
      item.styleClass === dayType || 
      item.styleClass === 'Accessories & Add-ons' ||
      // Casual items can sometimes bridge to Weekend or relaxed Office
      (dayType === 'Casual' && (item.styleClass === 'Office Wear' || item.styleClass === 'Weekend Wear')) ||
      (dayType === 'Weekend Wear' && item.styleClass === 'Casual');

    // 2. Season/Weather match
    let weatherMatches = true;
    if (weather.condition === 'hot' || weather.tempC >= 26) {
      if (item.season === 'winter') weatherMatches = false;
    } else if (weather.condition === 'chilly' || weather.tempC <= 14) {
      if (item.season === 'summer' && item.category !== 'accessory') weatherMatches = false;
    }

    return styleMatches && weatherMatches;
  });
}

// Generate an outfit for a single day respecting past days' used combinations
export function generateDayOutfit(
  wardrobe: ClothingItem[],
  day: DayOfWeek,
  dayIndex: number,
  dayType: DayType,
  weather: DayWeather,
  usedCombos: Set<string>,
  usedDresses: Set<string>,
  usedTops: Set<string>,
  usedBottoms: Set<string>,
  usedShoes: Set<string>,
  usedAccessories: Set<string>
): Partial<DailyOutfit> {
  const matchingItems = filterItemsForDay(wardrobe, dayType, weather);
  const fallbackPool = wardrobe; // Use whole wardrobe if filtered pool is too narrow

  const getCandidates = (category: ClothingItem['category']) => {
    const list = matchingItems.filter(i => i.category === category);
    return list.length > 0 ? list : fallbackPool.filter(i => i.category === category);
  };

  const tops = getCandidates('top');
  const bottoms = getCandidates('bottom');
  const dresses = getCandidates('dress');
  const outerwears = getCandidates('outerwear');
  const shoes = getCandidates('shoes');
  const accessories = getCandidates('accessory');

  // Decide whether to wear a Dress vs Top+Bottom
  // E.g., 30% chance for dress if dresses exist, especially on Friday/Weekend/Office wrap dress
  const preferDress = dresses.length > 0 && (dayIndex === 4 || dayIndex === 5 || dayIndex === 1) && Math.random() > 0.45;

  let selectedDressId: string | undefined = undefined;
  let selectedTopId: string | undefined = undefined;
  let selectedBottomId: string | undefined = undefined;

  if (preferDress && dresses.length > 0) {
    // Pick dress not used recently
    const unusedDresses = dresses.filter(d => !usedDresses.has(d.id));
    const dressPool = unusedDresses.length > 0 ? unusedDresses : dresses;
    const picked = dressPool[Math.floor(Math.random() * dressPool.length)];
    selectedDressId = picked?.id;
    if (picked) usedDresses.add(picked.id);
  } else if (tops.length > 0 && bottoms.length > 0) {
    // Find a top + bottom combo that hasn't been used this week
    let foundCombo = false;
    // Shuffle tops and bottoms to avoid repetitive first-item selection
    const shuffledTops = [...tops].sort(() => Math.random() - 0.5);
    const shuffledBottoms = [...bottoms].sort(() => Math.random() - 0.5);

    // 1st pass: Pick unused top AND unused bottom
    for (const t of shuffledTops) {
      for (const b of shuffledBottoms) {
        const comboKey = `${t.id}__${b.id}`;
        if (!usedCombos.has(comboKey) && !usedTops.has(t.id) && !usedBottoms.has(b.id)) {
          selectedTopId = t.id;
          selectedBottomId = b.id;
          usedCombos.add(comboKey);
          usedTops.add(t.id);
          usedBottoms.add(b.id);
          foundCombo = true;
          break;
        }
      }
      if (foundCombo) break;
    }

    // 2nd pass: At least combo key hasn't been used
    if (!foundCombo) {
      for (const t of shuffledTops) {
        for (const b of shuffledBottoms) {
          const comboKey = `${t.id}__${b.id}`;
          if (!usedCombos.has(comboKey)) {
            selectedTopId = t.id;
            selectedBottomId = b.id;
            usedCombos.add(comboKey);
            usedTops.add(t.id);
            usedBottoms.add(b.id);
            foundCombo = true;
            break;
          }
        }
        if (foundCombo) break;
      }
    }

    // Fallback: pick any top and bottom
    if (!foundCombo && shuffledTops.length > 0 && shuffledBottoms.length > 0) {
      selectedTopId = shuffledTops[0].id;
      selectedBottomId = shuffledBottoms[0].id;
    }
  } else if (dresses.length > 0) {
    // If no tops or bottoms available, use a dress
    selectedDressId = dresses[Math.floor(Math.random() * dresses.length)].id;
  }

  // Outerwear decision: if chilly (<18C), rainy, or office blazer look
  let selectedOuterwearId: string | undefined = undefined;
  const needsOuterwear = 
    weather.condition === 'chilly' || 
    weather.condition === 'rainy' || 
    weather.tempC <= 18 || 
    (dayType === 'Office Wear' && Math.random() > 0.4);

  if (needsOuterwear && outerwears.length > 0) {
    // If rainy, prefer rain mac / trench
    let pool = outerwears;
    if (weather.condition === 'rainy') {
      const rainCoats = outerwears.filter(o => o.subCategory?.toLowerCase().includes('rain') || o.notes?.toLowerCase().includes('waterproof'));
      if (rainCoats.length > 0) pool = rainCoats;
    }
    selectedOuterwearId = pool[Math.floor(Math.random() * pool.length)]?.id;
  }

  // Shoes decision
  let selectedShoesId: string | undefined = undefined;
  if (shoes.length > 0) {
    const unusedShoes = shoes.filter(s => !usedShoes.has(s.id));
    const shoePool = unusedShoes.length > 0 ? unusedShoes : shoes;
    const picked = shoePool[Math.floor(Math.random() * shoePool.length)];
    if (picked) {
      selectedShoesId = picked.id;
      usedShoes.add(picked.id);
    }
  }

  // Accessories decision: pick 1-2 distinct accessories
  const selectedAccessoryIds: string[] = [];
  if (accessories.length > 0) {
    const shuffledAcc = [...accessories].sort(() => Math.random() - 0.5);
    const unusedAcc = shuffledAcc.filter(a => !usedAccessories.has(a.id));
    const accPool = unusedAcc.length >= 2 ? unusedAcc : shuffledAcc;

    // Pick 1st accessory (e.g. bag or jewelry)
    if (accPool.length > 0) {
      selectedAccessoryIds.push(accPool[0].id);
      usedAccessories.add(accPool[0].id);
    }
    // Pick 2nd accessory with different subcategory if possible
    if (accPool.length > 1) {
      const firstSub = accPool[0].subCategory;
      const secondCandidate = accPool.slice(1).find(a => a.subCategory !== firstSub) || accPool[1];
      if (secondCandidate && secondCandidate.id !== accPool[0].id) {
        selectedAccessoryIds.push(secondCandidate.id);
        usedAccessories.add(secondCandidate.id);
      }
    }
  }

  return {
    topId: selectedTopId,
    bottomId: selectedBottomId,
    dressId: selectedDressId,
    outerwearId: selectedOuterwearId,
    shoesId: selectedShoesId,
    accessoryIds: selectedAccessoryIds,
  };
}

// Generate the complete 7-day Weekly Plan
export function generateFullWeekPlan(
  wardrobe: ClothingItem[],
  existingPlan?: WeeklyPlan,
  weatherForecast?: DayWeather[]
): WeeklyPlan {
  const result: Partial<WeeklyPlan> = {};
  const usedCombos = new Set<string>();
  const usedDresses = new Set<string>();
  const usedTops = new Set<string>();
  const usedBottoms = new Set<string>();
  const usedShoes = new Set<string>();
  const usedAccessories = new Set<string>();

  DAYS_OF_WEEK.forEach((day, index) => {
    const prevDay = existingPlan?.[day];
    
    // Default day types: Mon-Wed Office, Thu-Fri Casual (or preserve user selection), Sat-Sun Weekend
    const defaultDayType: DayType = 
      index < 3 ? 'Office Wear' : 
      index < 5 ? 'Casual' : 
      'Weekend Wear';

    const dayType: DayType = prevDay?.dayType || defaultDayType;
    
    const weather: DayWeather = 
      weatherForecast?.[index] || 
      prevDay?.weather || {
        city: 'Local Area',
        tempC: 22 + (index % 3) * 2,
        condition: index === 2 ? 'rainy' : index === 5 ? 'hot' : 'sunny',
        label: index === 2 ? 'Rainy & Fresh' : index === 5 ? 'Bright & Warm' : 'Sunny & Mild',
        icon: index === 2 ? '🌧️' : index === 5 ? '☀️' : '🌤️'
      };

    // If day is locked by user, preserve it and register its used items
    if (prevDay?.isLocked && (prevDay.topId || prevDay.dressId)) {
      if (prevDay.topId && prevDay.bottomId) {
        usedCombos.add(`${prevDay.topId}__${prevDay.bottomId}`);
        usedTops.add(prevDay.topId);
        usedBottoms.add(prevDay.bottomId);
      }
      if (prevDay.dressId) usedDresses.add(prevDay.dressId);
      if (prevDay.shoesId) usedShoes.add(prevDay.shoesId);
      prevDay.accessoryIds.forEach(id => usedAccessories.add(id));

      result[day] = prevDay;
      return;
    }

    const generated = generateDayOutfit(
      wardrobe,
      day,
      index,
      dayType,
      weather,
      usedCombos,
      usedDresses,
      usedTops,
      usedBottoms,
      usedShoes,
      usedAccessories
    );

    result[day] = {
      day,
      dayName: DAY_FULL_NAMES[day],
      dateLabel: `Day ${index + 1}`,
      dayType,
      weather,
      topId: generated.topId,
      bottomId: generated.bottomId,
      dressId: generated.dressId,
      outerwearId: generated.outerwearId,
      shoesId: generated.shoesId,
      accessoryIds: generated.accessoryIds || [],
      isLocked: false,
      status: 'planned'
    };
  });

  return result as WeeklyPlan;
}
