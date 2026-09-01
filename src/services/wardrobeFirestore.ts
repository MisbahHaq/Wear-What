import {
  collection,
  doc,
  setDoc,
  deleteDoc,
  getDoc,
  getDocs,
  onSnapshot,
  writeBatch,
  Unsubscribe,
} from 'firebase/firestore';
import { db } from '../lib/firebase';
import { ClothingItem, DailyOutfit, DayOfWeek, WeeklyPlan } from '../types';
import { INITIAL_WARDROBE } from '../data/initialWardrobe';
import { DEFAULT_WEEK_WEATHER } from '../utils/weather';
import { DAYS_OF_WEEK, generateFullWeekPlan } from '../utils/outfitGenerator';

/**
 * Remove undefined values recursively before passing payload to Firestore (which rejects undefined fields).
 */
function cleanForFirestore<T>(data: T): any {
  if (data === null || data === undefined) {
    return null;
  }
  if (Array.isArray(data)) {
    return data.map((item) => cleanForFirestore(item));
  }
  if (typeof data === 'object') {
    const cleaned: Record<string, any> = {};
    for (const [key, value] of Object.entries(data)) {
      if (value !== undefined) {
        cleaned[key] = cleanForFirestore(value);
      }
    }
    return cleaned;
  }
  return data;
}

/**
 * Realtime listener for user's personal wardrobe subcollection: users/{userId}/wardrobe
 */
export function subscribeToUserWardrobe(
  userId: string,
  onData: (items: ClothingItem[]) => void,
  onError?: (error: Error) => void
): Unsubscribe {
  const wardrobeRef = collection(db, 'users', userId, 'wardrobe');
  return onSnapshot(
    wardrobeRef,
    (snapshot) => {
      const items: ClothingItem[] = [];
      snapshot.forEach((docSnap) => {
        items.push({ ...(docSnap.data() as ClothingItem), id: docSnap.id });
      });
      // Sort newest first
      items.sort((a, b) => new Date(b.dateAdded || 0).getTime() - new Date(a.dateAdded || 0).getTime());
      onData(items);
    },
    (err) => {
      console.error('Error fetching wardrobe in realtime:', err);
      if (onError) onError(err);
    }
  );
}

/**
 * Realtime listener for user's weekly outfit plans: users/{userId}/weeklyPlan
 */
export function subscribeToUserWeeklyPlan(
  userId: string,
  onData: (plan: WeeklyPlan | null) => void,
  onError?: (error: Error) => void
): Unsubscribe {
  const planRef = collection(db, 'users', userId, 'weeklyPlan');
  return onSnapshot(
    planRef,
    (snapshot) => {
      if (snapshot.empty) {
        onData(null);
        return;
      }
      const plan: Partial<WeeklyPlan> = {};
      snapshot.forEach((docSnap) => {
        const outfit = docSnap.data() as DailyOutfit;
        if (outfit.day) {
          plan[outfit.day] = outfit;
        }
      });

      // Verify all 7 days exist
      const hasAllDays = DAYS_OF_WEEK.every((d) => Boolean(plan[d]));
      if (hasAllDays) {
        onData(plan as WeeklyPlan);
      } else {
        onData(null);
      }
    },
    (err) => {
      console.error('Error fetching weekly plan in realtime:', err);
      if (onError) onError(err);
    }
  );
}

/**
 * Initialize starter wardrobe ONLY for a brand-new user (no existing profile doc).
 * Returning users keep their closet exactly as-is — even if they deleted everything.
 * The profile document acts as the "has started before" marker.
 */
export async function initializeUserWardrobeIfEmpty(userId: string): Promise<ClothingItem[]> {
  try {
    const wardrobeRef = collection(db, 'users', userId, 'wardrobe');

    // A returning user always has a profile document. Only seed for first-timers.
    const profileRef = doc(db, 'users', userId);
    const profileDoc = await getDoc(profileRef);

    if (profileDoc.exists()) {
      // Returning user — never re-seed. Return whatever they currently have.
      const items: ClothingItem[] = [];
      const existingSnap = await getDocs(wardrobeRef);
      existingSnap.forEach((docSnap) => {
        items.push({ ...(docSnap.data() as ClothingItem), id: docSnap.id });
      });
      return items;
    }

    // Brand-new user: seed the starter wardrobe + weekly plan
    const batch = writeBatch(db);
    INITIAL_WARDROBE.forEach((item) => {
      const itemRef = doc(db, 'users', userId, 'wardrobe', item.id);
      batch.set(itemRef, cleanForFirestore({ ...item, isCustom: false }));
    });

    // Also initialize weekly plan
    const starterPlan = generateFullWeekPlan(INITIAL_WARDROBE, undefined, DEFAULT_WEEK_WEATHER);
    DAYS_OF_WEEK.forEach((day) => {
      const dayRef = doc(db, 'users', userId, 'weeklyPlan', day);
      batch.set(dayRef, cleanForFirestore(starterPlan[day]));
    });

    await batch.commit();
    return INITIAL_WARDROBE;
  } catch (err) {
    console.error('Failed to initialize user wardrobe:', err);
    return [];
  }
}

/**
 * Save single or batch clothing item to user's library
 */
export async function saveUserClothingItem(userId: string, item: ClothingItem): Promise<void> {
  const itemRef = doc(db, 'users', userId, 'wardrobe', item.id);
  await setDoc(itemRef, cleanForFirestore(item), { merge: true });
}

/**
 * Delete clothing item from user's library
 */
export async function deleteUserClothingItem(userId: string, itemId: string): Promise<void> {
  const itemRef = doc(db, 'users', userId, 'wardrobe', itemId);
  await deleteDoc(itemRef);
}

/**
 * Save full weekly plan for user
 */
export async function saveUserWeeklyPlan(userId: string, plan: WeeklyPlan): Promise<void> {
  const batch = writeBatch(db);
  DAYS_OF_WEEK.forEach((day) => {
    if (plan[day]) {
      const dayRef = doc(db, 'users', userId, 'weeklyPlan', day);
      batch.set(dayRef, cleanForFirestore(plan[day]), { merge: true });
    }
  });
  await batch.commit();
}

/**
 * Save single day outfit in plan
 */
export async function saveUserDailyOutfit(userId: string, outfit: DailyOutfit): Promise<void> {
  const dayRef = doc(db, 'users', userId, 'weeklyPlan', outfit.day);
  await setDoc(dayRef, cleanForFirestore(outfit), { merge: true });
}

/**
 * Reset personal wardrobe to default curated fashion capsule
 */
export async function resetUserToDemoWardrobe(userId: string): Promise<void> {
  const wardrobeRef = collection(db, 'users', userId, 'wardrobe');
  const snap = await getDocs(wardrobeRef);
  const batch = writeBatch(db);

  snap.forEach((docSnap) => {
    batch.delete(docSnap.ref);
  });

  INITIAL_WARDROBE.forEach((item) => {
    const itemRef = doc(db, 'users', userId, 'wardrobe', item.id);
    batch.set(itemRef, cleanForFirestore({ ...item, isCustom: false }));
  });

  const freshPlan = generateFullWeekPlan(INITIAL_WARDROBE, undefined, DEFAULT_WEEK_WEATHER);
  DAYS_OF_WEEK.forEach((day) => {
    const dayRef = doc(db, 'users', userId, 'weeklyPlan', day);
    batch.set(dayRef, cleanForFirestore(freshPlan[day]));
  });

  await batch.commit();
}
