import React, { createContext, useContext, useEffect, useState } from 'react';
import {
  User,
  onAuthStateChanged,
  signInWithEmailAndPassword,
  createUserWithEmailAndPassword,
  signInWithPopup,
  signOut as fbSignOut,
  updateProfile,
  sendPasswordResetEmail,
} from 'firebase/auth';
import { doc, setDoc, getDoc, serverTimestamp } from 'firebase/firestore';
import { auth, googleProvider, db } from '../lib/firebase';

export interface UserProfileData {
  uid: string;
  email: string | null;
  displayName: string | null;
  photoURL: string | null;
  currentCity?: string;
  createdAt?: unknown;
  updatedAt?: unknown;
}

interface AuthContextType {
  user: User | null;
  userProfile: UserProfileData | null;
  loading: boolean;
  signInWithEmail: (email: string, pass: string) => Promise<void>;
  signUpWithEmail: (email: string, pass: string, displayName: string) => Promise<void>;
  signInWithGoogle: () => Promise<void>;
  signOut: () => Promise<void>;
  sendPasswordReset: (email: string) => Promise<void>;
  updateUserCity: (city: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [userProfile, setUserProfile] = useState<UserProfileData | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  // Sync user profile with Firestore
  const syncUserProfileDoc = async (authUser: User) => {
    try {
      const userRef = doc(db, 'users', authUser.uid);
      const snap = await getDoc(userRef);

      if (!snap.exists()) {
        const newProfile: Record<string, any> = {
          uid: authUser.uid,
          email: authUser.email || '',
          displayName: authUser.displayName || authUser.email?.split('@')[0] || 'Fashion Curator',
          photoURL: authUser.photoURL || '',
          currentCity: 'Karachi, Pakistan',
          createdAt: serverTimestamp(),
          updatedAt: serverTimestamp(),
        };
        await setDoc(userRef, newProfile);
        setUserProfile(newProfile as UserProfileData);
      } else {
        setUserProfile(snap.data() as UserProfileData);
      }
    } catch (err) {
      console.warn('Error syncing user profile with Firestore:', err);
    }
  };

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, async (currentUser) => {
      setUser(currentUser);
      if (currentUser) {
        await syncUserProfileDoc(currentUser);
      } else {
        setUserProfile(null);
      }
      setLoading(false);
    });

    return () => unsubscribe();
  }, []);

  const signInWithEmail = async (email: string, pass: string) => {
    const cred = await signInWithEmailAndPassword(auth, email.trim(), pass);
    await syncUserProfileDoc(cred.user);
  };

  const signUpWithEmail = async (email: string, pass: string, displayName: string) => {
    const cred = await createUserWithEmailAndPassword(auth, email.trim(), pass);
    if (displayName) {
      await updateProfile(cred.user, { displayName });
    }
    await syncUserProfileDoc(cred.user);
  };

  const signInWithGoogle = async () => {
    const result = await signInWithPopup(auth, googleProvider);
    await syncUserProfileDoc(result.user);
  };

  const signOut = async () => {
    await fbSignOut(auth);
    setUser(null);
    setUserProfile(null);
  };

  const sendPasswordReset = async (email: string) => {
    await sendPasswordResetEmail(auth, email.trim());
  };

  const updateUserCity = async (city: string) => {
    if (!user) return;
    try {
      const userRef = doc(db, 'users', user.uid);
      await setDoc(userRef, { currentCity: city, updatedAt: serverTimestamp() }, { merge: true });
      setUserProfile((prev) => (prev ? { ...prev, currentCity: city } : null));
    } catch (err) {
      console.warn('Failed to update city in profile:', err);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        userProfile,
        loading,
        signInWithEmail,
        signUpWithEmail,
        signInWithGoogle,
        signOut,
        sendPasswordReset,
        updateUserCity,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
