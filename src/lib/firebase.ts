/// <reference types="vite/client" />
import { initializeApp, getApps, getApp } from 'firebase/app';
import { getAuth, GoogleAuthProvider, connectAuthEmulator } from 'firebase/auth';
import { getFirestore, connectFirestoreEmulator } from 'firebase/firestore';
import firebaseConfigJson from '../../firebase-applet-config.json';

export const firebaseConfig = {
  apiKey: firebaseConfigJson.apiKey,
  authDomain: firebaseConfigJson.authDomain,
  projectId: firebaseConfigJson.projectId,
  storageBucket: firebaseConfigJson.storageBucket,
  messagingSenderId: firebaseConfigJson.messagingSenderId,
  appId: firebaseConfigJson.appId,
};

// Initialize Firebase App
export const app = !getApps().length ? initializeApp(firebaseConfig) : getApp();

// Initialize Auth
export const auth = getAuth(app);
export const googleProvider = new GoogleAuthProvider();

// Firestore uses an App Host "named" database in production. The local emulator
// only serves the default database, so we point at that while emulating.
const prodDatabaseId =
  (firebaseConfigJson as { firestoreDatabaseId?: string }).firestoreDatabaseId || '(default)';

// During local development the Firebase emulators (Auth + Firestore) let Google
// sign-in run without requiring the page domain to be whitelisted in the
// project's authorized domains — the source of `auth/unauthorized-domain`
// against the real backend. The Auth emulator issues tokens the Firestore
// emulator trusts, so both must run together (see `npm run emulators`).
// Off by default so a plain `npm run dev` on localhost still uses the real
// project (where `localhost` is authorized by default); set
// VITE_USE_FIREBASE_EMULATOR=true in .env.local to opt in.
const isLocal =
  typeof location !== 'undefined' &&
  (location.hostname === 'localhost' ||
    location.hostname === '127.0.0.1' ||
    location.hostname === '0.0.0.0');
const useEmulator = isLocal && import.meta.env.VITE_USE_FIREBASE_EMULATOR === 'true';

export const db = useEmulator
  ? getFirestore(app, '(default)')
  : getFirestore(app, prodDatabaseId);

if (useEmulator) {
  connectAuthEmulator(auth, 'http://localhost:9099', { disableWarnings: true });
  connectFirestoreEmulator(db, 'localhost', 8080);
}
