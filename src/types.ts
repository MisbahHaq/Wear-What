export type ClothingCategory = 'top' | 'bottom' | 'dress' | 'outerwear' | 'shoes' | 'accessory';

export type StyleClass = 
  | 'Office Wear' 
  | 'Casual' 
  | 'Weekend Wear' 
  | 'Accessories & Add-ons';

export type SeasonTag = 'summer' | 'winter' | 'all-season';

export type AccessorySubtype = 
  | 'bag' 
  | 'jewelry' 
  | 'scarf' 
  | 'belt' 
  | 'sunglasses' 
  | 'watch' 
  | 'hat' 
  | 'other';

export interface ClothingItem {
  id: string;
  name: string;
  imageUrl: string;
  category: ClothingCategory;
  styleClass: StyleClass;
  season: SeasonTag;
  color: string;
  colorHex: string;
  subCategory?: string;
  notes?: string;
  vibeTag?: string;
  favorite?: boolean;
  isCustom?: boolean;
  dateAdded: string;
}

export type DayOfWeek = 'Mon' | 'Tue' | 'Wed' | 'Thu' | 'Fri' | 'Sat' | 'Sun';

export type DayType = 'Office Wear' | 'Casual' | 'Weekend Wear';

export type WeatherCondition = 'sunny' | 'rainy' | 'chilly' | 'hot' | 'windy' | 'cloudy';

export interface DayWeather {
  tempC: number;
  condition: WeatherCondition;
  label: string;
  city: string;
  icon: string;
  rainChance?: number;
}

export interface DailyOutfit {
  day: DayOfWeek;
  dayName: string;
  dateLabel: string;
  dayType: DayType;
  weather: DayWeather;
  topId?: string;
  bottomId?: string;
  dressId?: string;
  outerwearId?: string;
  shoesId?: string;
  accessoryIds: string[];
  notes?: string;
  isLocked?: boolean;
  status?: 'planned' | 'worn' | 'loved';
}

export type WeeklyPlan = Record<DayOfWeek, DailyOutfit>;

export type ActiveView = 'planner' | 'closet' | 'upload';

export interface CollageItemTransform {
  itemId: string;
  x: number;
  y: number;
  rotation: number;
  scale: number;
  zIndex: number;
}

export interface FlatLayCanvasState {
  day: DayOfWeek;
  items: CollageItemTransform[];
  stickers: {
    id: string;
    text: string;
    type: 'tape' | 'badge' | 'doodle' | 'memo';
    x: number;
    y: number;
    rotation: number;
    bg?: string;
  }[];
  bgColor: string;
}
