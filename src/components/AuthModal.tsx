import React, { useState } from 'react';
import { X, Lock, Mail, User as UserIcon, Sparkles, ArrowRight, Eye, EyeOff, AlertCircle, CheckCircle2 } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { WashiTape, NeonStickerBadge, RetroSmileyBadge } from './DoodleDecorations';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialMode?: 'signin' | 'signup';
}

export const AuthModal: React.FC<AuthModalProps> = ({
  isOpen,
  onClose,
  initialMode = 'signin',
}) => {
  const { signInWithEmail, signUpWithEmail, signInWithGoogle, sendPasswordReset } = useAuth();

  const [mode, setMode] = useState<'signin' | 'signup' | 'forgot'>(initialMode);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  if (!isOpen) return null;

  const getFriendlyError = (err: any): string => {
    const code = err?.code || '';
    if (code === 'auth/email-already-in-use') {
      return 'An account already exists with this email. Please sign in instead.';
    }
    if (code === 'auth/invalid-credential' || code === 'auth/user-not-found' || code === 'auth/wrong-password') {
      return 'Incorrect email or password. Please verify your credentials.';
    }
    if (code === 'auth/weak-password') {
      return 'Password must be at least 6 characters long.';
    }
    if (code === 'auth/invalid-email') {
      return 'Please enter a valid email address.';
    }
    if (code === 'auth/popup-closed-by-user') {
      return 'Google sign-in popup was closed before completing.';
    }
    return err?.message || 'Authentication failed. Please try again.';
  };

  const handleEmailSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);
    setSuccessMsg(null);

    if (!email.trim()) {
      setErrorMsg('Please enter your email address.');
      return;
    }

    if (mode === 'forgot') {
      try {
        setLoading(true);
        await sendPasswordReset(email);
        setSuccessMsg('Password reset link sent to your email! Check your inbox.');
      } catch (err) {
        setErrorMsg(getFriendlyError(err));
      } finally {
        setLoading(false);
      }
      return;
    }

    if (!password) {
      setErrorMsg('Please enter your password.');
      return;
    }

    try {
      setLoading(true);
      if (mode === 'signup') {
        await signUpWithEmail(email, password, displayName.trim() || 'Style Enthusiast');
      } else {
        await signInWithEmail(email, password);
      }
      onClose();
    } catch (err) {
      setErrorMsg(getFriendlyError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleSignIn = async () => {
    setErrorMsg(null);
    setSuccessMsg(null);
    try {
      setLoading(true);
      await signInWithGoogle();
      onClose();
    } catch (err) {
      setErrorMsg(getFriendlyError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 bg-black/80 backdrop-blur-sm overflow-y-auto">
      <div className="relative w-full max-w-md bg-[#FFF9ED] border-4 border-[#111111] chunky-shadow-xl overflow-hidden my-auto flex flex-col">
        {/* Editorial Masthead Bar */}
        <div className="bg-[#FF007A] text-white border-b-4 border-[#111111] px-5 py-3.5 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-[#FFF500] text-[#111111] border-2 border-[#111111] flex items-center justify-center font-black text-sm -rotate-3 chunky-shadow">
              ★
            </div>
            <div>
              <h3 className="big-display text-xl tracking-wide leading-none">
                {mode === 'signup' ? 'CREATE CLOSET' : mode === 'forgot' ? 'RESET KEY' : 'MEMBER LOGIN'}
              </h3>
              <p className="text-[10px] font-mono uppercase tracking-widest text-pink-100 mt-0.5">
                Personal Wardrobe Vault
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="w-8 h-8 bg-[#111111] text-white border-2 border-white flex items-center justify-center hover:bg-gray-800 active:scale-95 transition-all"
          >
            <X className="w-5 h-5 stroke-[3]" />
          </button>
        </div>

        {/* Tab Switcher (Sign In vs Sign Up) */}
        {mode !== 'forgot' && (
          <div className="grid grid-cols-2 border-b-3 border-[#111111] bg-white text-xs font-black uppercase">
            <button
              type="button"
              onClick={() => {
                setMode('signin');
                setErrorMsg(null);
                setSuccessMsg(null);
              }}
              className={`py-3 text-center transition-all ${
                mode === 'signin'
                  ? 'bg-[#00D1FF] text-[#111111] border-r-3 border-[#111111] shadow-inner font-black'
                  : 'bg-white text-gray-600 hover:bg-[#FFF9ED] border-r-3 border-[#111111]'
              }`}
            >
              Log In
            </button>
            <button
              type="button"
              onClick={() => {
                setMode('signup');
                setErrorMsg(null);
                setSuccessMsg(null);
              }}
              className={`py-3 text-center transition-all ${
                mode === 'signup'
                  ? 'bg-[#FFF500] text-[#111111] shadow-inner font-black'
                  : 'bg-white text-gray-600 hover:bg-[#FFF9ED]'
              }`}
            >
              Create Account
            </button>
          </div>
        )}

        {/* Form Body */}
        <div className="p-5 sm:p-6 space-y-4">
          {/* Quick Intro Banner */}
          <div className="relative bg-white border-2 border-[#111111] p-3 chunky-shadow text-xs font-mono text-gray-800">
            <div className="absolute -top-3 right-4">
              <WashiTape pattern="yellow" angle={3} className="w-16" />
            </div>
            <p className="flex items-center gap-1.5 font-bold">
              <Sparkles className="w-3.5 h-3.5 text-[#FF007A]" />
              <span>
                {mode === 'signup'
                  ? 'Save your custom clothes, tags, and weekly outfit rotations forever.'
                  : mode === 'forgot'
                  ? 'Enter your account email to receive a recovery link.'
                  : 'Sign in to access your personal synchronized wardrobe library.'}
              </span>
            </p>
          </div>

          {/* Feedback Messages */}
          {errorMsg && (
            <div className="bg-red-50 border-2 border-red-600 p-2.5 flex items-start gap-2 text-xs font-mono text-red-800">
              <AlertCircle className="w-4 h-4 text-red-600 shrink-0 mt-0.5" />
              <span>{errorMsg}</span>
            </div>
          )}

          {successMsg && (
            <div className="bg-green-50 border-2 border-green-600 p-2.5 flex items-start gap-2 text-xs font-mono text-green-800">
              <CheckCircle2 className="w-4 h-4 text-green-600 shrink-0 mt-0.5" />
              <span>{successMsg}</span>
            </div>
          )}

          {/* Google One-Click Button */}
          {mode !== 'forgot' && (
            <>
              <button
                type="button"
                onClick={handleGoogleSignIn}
                disabled={loading}
                className="w-full bg-white hover:bg-gray-50 text-[#111111] border-3 border-[#111111] py-2.5 px-4 chunky-shadow text-xs font-black uppercase tracking-wider flex items-center justify-center gap-2.5 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all disabled:opacity-50"
              >
                <svg className="w-4 h-4" viewBox="0 0 24 24">
                  <path
                    fill="#4285F4"
                    d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                  />
                  <path
                    fill="#34A853"
                    d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                  />
                  <path
                    fill="#FBBC05"
                    d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"
                  />
                  <path
                    fill="#EA4335"
                    d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"
                  />
                </svg>
                <span>Continue with Google</span>
              </button>

              {/* Divider */}
              <div className="flex items-center gap-3 my-1">
                <div className="flex-1 border-t-2 border-dashed border-gray-400"></div>
                <span className="text-[10px] font-mono uppercase font-black text-gray-500 bg-[#FFF9ED] px-1">
                  OR WITH EMAIL
                </span>
                <div className="flex-1 border-t-2 border-dashed border-gray-400"></div>
              </div>
            </>
          )}

          {/* Form */}
          <form onSubmit={handleEmailSubmit} className="space-y-3">
            {/* Display Name (Sign up only) */}
            {mode === 'signup' && (
              <div>
                <label className="block text-[11px] font-black uppercase text-[#111111] mb-1">
                  Curator / Stylist Name
                </label>
                <div className="relative">
                  <UserIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
                  <input
                    type="text"
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    placeholder="e.g. Maya Chen"
                    className="w-full pl-9 pr-3 py-2 bg-white border-2 border-[#111111] text-xs font-mono font-bold focus:outline-none focus:bg-yellow-50"
                  />
                </div>
              </div>
            )}

            {/* Email */}
            <div>
              <label className="block text-[11px] font-black uppercase text-[#111111] mb-1">
                Email Address
              </label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="curator@editorial.fashion"
                  className="w-full pl-9 pr-3 py-2 bg-white border-2 border-[#111111] text-xs font-mono font-bold focus:outline-none focus:bg-yellow-50"
                />
              </div>
            </div>

            {/* Password */}
            {mode !== 'forgot' && (
              <div>
                <div className="flex items-center justify-between mb-1">
                  <label className="block text-[11px] font-black uppercase text-[#111111]">
                    Password
                  </label>
                  {mode === 'signin' && (
                    <button
                      type="button"
                      onClick={() => {
                        setMode('forgot');
                        setErrorMsg(null);
                        setSuccessMsg(null);
                      }}
                      className="text-[10px] font-mono text-[#FF007A] hover:underline font-bold"
                    >
                      Forgot password?
                    </button>
                  )}
                </div>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
                  <input
                    type={showPassword ? 'text' : 'password'}
                    required
                    minLength={6}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="••••••••"
                    className="w-full pl-9 pr-10 py-2 bg-white border-2 border-[#111111] text-xs font-mono font-bold focus:outline-none focus:bg-yellow-50"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black"
                  >
                    {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
              </div>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              disabled={loading}
              className="w-full bg-[#FFF500] hover:bg-yellow-300 text-[#111111] border-3 border-[#111111] py-2.5 px-4 chunky-shadow text-xs font-black uppercase tracking-wider flex items-center justify-center gap-2 active:translate-x-0.5 active:translate-y-0.5 active:shadow-none transition-all disabled:opacity-50"
            >
              {loading ? (
                <span className="animate-spin text-sm">🌀</span>
              ) : (
                <>
                  <span>
                    {mode === 'signup'
                      ? 'Launch My Wardrobe'
                      : mode === 'forgot'
                      ? 'Send Recovery Link'
                      : 'Open Wardrobe Vault'}
                  </span>
                  <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          {/* Toggle back from Forgot Password */}
          {mode === 'forgot' && (
            <div className="text-center pt-2">
              <button
                type="button"
                onClick={() => {
                  setMode('signin');
                  setErrorMsg(null);
                  setSuccessMsg(null);
                }}
                className="text-xs font-black uppercase text-[#111111] hover:underline"
              >
                ← Back to Log In
              </button>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="bg-[#FAF5E8] border-t-3 border-[#111111] p-3 px-6 flex items-center justify-between text-[11px] font-mono">
          <span className="text-gray-600">Want to explore first?</span>
          <button
            type="button"
            onClick={onClose}
            className="text-[#FF007A] font-bold hover:underline uppercase"
          >
            Continue as Guest →
          </button>
        </div>
      </div>
    </div>
  );
};
