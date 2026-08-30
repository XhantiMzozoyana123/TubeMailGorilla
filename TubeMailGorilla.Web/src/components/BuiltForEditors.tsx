import React from 'react';
import { Sliders, ShieldX, CheckCircle2, ArrowRight, Zap, RefreshCw } from 'lucide-react';

interface BuiltForEditorsProps {
  onOpenTrial: () => void;
}

export const BuiltForEditors: React.FC<BuiltForEditorsProps> = ({ onOpenTrial }) => {
  return (
    <section className="py-24 bg-[#08090d] relative border-t border-slate-800/80" id="built-for-editors">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Sliders className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>SKIP THE JOB BOARDS</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            Made for Editors Who Want More <br />
            <span className="text-[#ff003b]">Than Saturated Freelance Platforms.</span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            You don't have to compete with hundreds of editors on a job board. You don't have to sit around refreshing freelance marketplaces.
          </p>
        </div>

        {/* Freelance Job Board vs Direct Creator Outreach Comparison */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-14">
          {/* Marketplace Card */}
          <div className="bg-[#0f1118] border border-slate-800 rounded-2xl p-7 flex flex-col justify-between shadow-xl">
            <div>
              <div className="flex items-center justify-between mb-4">
                <span className="text-xs font-mono font-bold uppercase tracking-wider text-rose-400">
                  Freelance Marketplaces (Upwork / Fiverr)
                </span>
                <ShieldX className="w-5 h-5 text-rose-500" />
              </div>
              <h3 className="text-xl font-bold text-slate-200 mb-4 font-['Outfit',sans-serif]">
                The Low-Rate Race to the Bottom
              </h3>
              <ul className="space-y-3 text-xs sm:text-sm text-slate-400 font-mono">
                <li className="flex items-start gap-2.5">
                  <span className="text-[#ff003b] font-bold">âœ•</span>
                  <span className="font-sans text-slate-400">Compete with 50+ low-bid editors within 10 minutes of a post</span>
                </li>
                <li className="flex items-start gap-2.5">
                  <span className="text-[#ff003b] font-bold">âœ•</span>
                  <span className="font-sans text-slate-400">Pay 20% platform commission cuts on your hard-earned cash</span>
                </li>
                <li className="flex items-start gap-2.5">
                  <span className="text-[#ff003b] font-bold">âœ•</span>
                  <span className="font-sans text-slate-400">Clients treating editing as a cheap commodity rather than a growth partner</span>
                </li>
                <li className="flex items-start gap-2.5">
                  <span className="text-[#ff003b] font-bold">âœ•</span>
                  <span className="font-sans text-slate-400">Endless refresh loops waiting for someone to post a job</span>
                </li>
              </ul>
            </div>
            <div className="mt-6 pt-4 border-t border-slate-800 text-xs text-slate-500 font-mono">
              STATUS: Trapped by platform algorithms and arbitrary commission cuts.
            </div>
          </div>

          {/* TubeMail Gorilla Card */}
          <div className="bg-gradient-to-b from-[#160f14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 rounded-2xl p-7 flex flex-col justify-between shadow-[0_0_25px_rgba(255,0,59,0.15)] ring-1 ring-[#ff003b]/30">
            <div>
              <div className="flex items-center justify-between mb-4">
                <span className="text-xs font-mono font-bold uppercase tracking-wider text-[#ff4d73]">
                  Direct Creator Prospecting Engine
                </span>
                <span className="text-xl">ðŸ¦</span>
              </div>
              <h3 className="text-xl font-bold text-white mb-4 font-['Outfit',sans-serif]">
                You Choose Who You Want to Work With
              </h3>
              <ul className="space-y-3 text-xs sm:text-sm text-slate-200 font-mono">
                <li className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Proactively target creators in niches you genuinely enjoy editing</span>
                </li>
                <li className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Zero platform fees â€” you keep 100% of your retainer earnings</span>
                </li>
                <li className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Reach out before a creator ever thinks about posting a public job ad</span>
                </li>
                <li className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Establish high-trust, direct relationships that renew every month</span>
                </li>
              </ul>
            </div>
            <div className="mt-6 pt-4 border-t border-[#ff003b]/30 text-xs font-mono font-bold text-[#ff4d73]">
              âš¡ TubeMail Gorilla gives you the prospecting engine to make that happen.
            </div>
          </div>
        </div>

        {/* Bottom Callout */}
        <div className="text-center">
          <button
            onClick={onOpenTrial}
            className="px-8 py-4 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.3)] transition-all inline-flex items-center gap-2 cursor-pointer border border-[#ff4d73]/40"
          >
            <span>Take Control of Your Client Pipeline</span>
            <ArrowRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </section>
  );
};
