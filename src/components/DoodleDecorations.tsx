import React from 'react';

export function WashiTape({ 
  className = '', 
  pattern = 'yellow',
  angle = -3
}: { 
  className?: string; 
  pattern?: 'pink' | 'yellow' | 'cyan' | 'leopard' | 'solid-yellow' | 'black-dots';
  angle?: number;
}) {
  const patternClasses = {
    pink: 'washi-tape-pink',
    yellow: 'washi-tape-yellow',
    cyan: 'washi-tape-cyan',
    leopard: 'washi-tape-leopard',
    'solid-yellow': 'washi-tape-solid-yellow',
    'black-dots': 'bg-white border-y-2 border-dashed border-black text-black'
  }[pattern] || 'washi-tape-yellow';

  return (
    <div 
      className={`h-5 px-3 py-0.5 text-[10px] font-mono tracking-widest uppercase font-black select-none pointer-events-none flex items-center justify-center opacity-95 ${patternClasses} ${className}`}
      style={{
        transform: `rotate(${angle}deg)`,
        clipPath: 'polygon(0% 10%, 4% 0%, 96% 0%, 100% 10%, 97% 90%, 93% 100%, 7% 100%, 0% 90%)'
      }}
    />
  );
}

export function CircularSticker({
  text = 'HOT OUTFIT',
  subtext = '03',
  bg = 'bg-[#FFF500]',
  textColor = 'text-[#111111]',
  className = '',
  rotate = 'rotate-6'
}: {
  text?: string;
  subtext?: string;
  bg?: string;
  textColor?: string;
  className?: string;
  rotate?: string;
}) {
  return (
    <div
      className={`inline-flex flex-col items-center justify-center rounded-full border-3 border-[#111111] ${bg} ${textColor} ${rotate} shadow-[4px_4px_0px_#111111] p-2 min-w-[56px] min-h-[56px] select-none transition-transform hover:scale-110 ${className}`}
    >
      <span className="text-[9px] font-black uppercase tracking-tighter leading-tight text-center">{text}</span>
      {subtext && <span className="text-[12px] font-display font-black leading-none">{subtext}</span>}
    </div>
  );
}

export function DoodleSparkle({ className = '', color = '#FFF500', size = 28 }: { className?: string; color?: string; size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" className={`pointer-events-none ${className}`}>
      <path
        d="M12 0L14.5 9.5L24 12L14.5 14.5L12 24L9.5 14.5L0 12L9.5 9.5L12 0Z"
        fill={color}
        stroke="#111111"
        strokeWidth="1.8"
      />
    </svg>
  );
}

export function DoodleSquiggle({ className = '', color = '#FF007A' }: { className?: string; color?: string }) {
  return (
    <svg width="48" height="18" viewBox="0 0 60 20" fill="none" className={`pointer-events-none ${className}`}>
      <path
        d="M3 10 C 10 2, 15 18, 22 10 C 29 2, 35 18, 42 10 C 49 2, 54 18, 58 10"
        stroke={color}
        strokeWidth="3.5"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function DoodleArrow({ className = '', color = '#111111' }: { className?: string; color?: string }) {
  return (
    <svg width="36" height="24" viewBox="0 0 36 24" fill="none" className={`pointer-events-none ${className}`}>
      <path
        d="M2 18 C 10 8, 22 5, 32 10 M 24 4 L 32 10 L 26 18"
        stroke={color}
        strokeWidth="3"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export function RetroSmileyBadge({ className = '', text = 'OOTD' }: { className?: string; text?: string }) {
  return (
    <div className={`inline-flex items-center gap-1.5 bg-[#FFF500] text-[#111111] border-2 border-[#111111] font-display text-xs px-2.5 py-1 rounded-full chunky-shadow uppercase tracking-wider ${className}`}>
      <span className="text-sm">★</span>
      <span>{text}</span>
    </div>
  );
}

export const NeonStickerBadge: React.FC<{
  text: string;
  bg?: string;
  textColor?: string;
  rotate?: string;
  className?: string;
}> = ({
  text,
  bg = 'bg-[#FF007A]',
  textColor = 'text-white',
  rotate = 'rotate-3',
  className = ''
}) => {
  return (
    <div
      className={`inline-block font-syne font-black text-[11px] uppercase tracking-wider px-2 py-0.5 border-2 border-[#111111] shadow-[3px_3px_0px_#111111] select-none ${bg} ${textColor} ${rotate} sticker-badge ${className}`}
    >
      {text}
    </div>
  );
};

export function SafetyPinIcon({ className = '' }: { className?: string }) {
  return (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" className={`pointer-events-none ${className}`}>
      <path
        d="M6 18 L18 6 C19.5 4.5 22 7 20.5 8.5 L9 20 C7.5 21.5 4.5 19.5 5 17.5 L15 7.5"
        stroke="#111111"
        strokeWidth="2.5"
        strokeLinecap="round"
      />
      <circle cx="19" cy="5" r="2" fill="#D1D5DB" stroke="#111111" strokeWidth="1.5" />
    </svg>
  );
}

