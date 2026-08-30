import React from 'react';
import { PlayCircle, Award, Target, ArrowRight, CheckCircle2, Sparkles, XCircle, Zap } from 'lucide-react';

interface TheBigPromiseProps {
  onOpenTrial: () => void;
}

export const TheBigPromise: React.FC<TheBigPromiseProps> = ({ onOpenTrial }) => {
  return (
    <section className="py-24 bg-[#08090d] relative overflow-hidden" id="the-big-promise">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto mb-14">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>STOP LEARNING. START EARNING.</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            You Don't Need More Editing Tutorials. <br />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#ff003b] via-[#ff4d73] to-white">
              You Need More Paid Opportunities.
            </span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl leading-relaxed">
            You've probably watched enough tutorials. You've learned enough timeline techniques. You've spent enough time improving your portfolio.
          </p>
        </div>

        {/* Tutorial Trap vs Real Action Contrast */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-14">
          {/* The Tutorial Hamster Wheel */}
          <div className="bg-[#0f1118] border border-slate-800 rounded-2xl p-7 flex flex-col justify-between shadow-xl">
            <div>
              <div className="flex items-center gap-2 text-slate-400 text-xs font-mono font-bold uppercase tracking-wider mb-4">
                <PlayCircle className="w-4 h-4 text-rose-500" />
                <span>The Tutorial Loop (Zero Income)</span>
              </div>
              <h3 className="text-xl font-bold text-slate-200 mb-4 font-['Outfit',sans-serif]">
                Watching 50+ hours of timeline tricks
              </h3>
              <div className="space-y-3 text-xs sm:text-sm text-slate-400 font-mono">
                <div className="flex items-start gap-2.5">
                  <XCircle className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-300">Learning another niche plugin you might never use in client work</span>
                </div>
                <div className="flex items-start gap-2.5">
                  <XCircle className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-300">Re-tweaking your showreel for the 14th time with zero viewers</span>
                </div>
                <div className="flex items-start gap-2.5">
                  <XCircle className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-300">Feeling like you're "still not ready" to pitch real creators</span>
                </div>
              </div>
            </div>

            <div className="mt-6 pt-4 border-t border-slate-800 text-xs text-slate-500 font-mono">
              STATUS: Great technical skills, but zero active client revenue.
            </div>
          </div>

          {/* The TubeMail Gorilla Outreach Engine */}
          <div className="bg-gradient-to-br from-[#1b0e14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 rounded-2xl p-7 flex flex-col justify-between shadow-[0_0_25px_rgba(255,0,59,0.15)]">
            <div>
              <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-4">
                <Target className="w-4 h-4" />
                <span>The Direct Pipeline (Client Revenue)</span>
              </div>
              <h3 className="text-xl font-bold text-white mb-4 font-['Outfit',sans-serif]">
                Putting your skills in front of paying creators
              </h3>
              <div className="space-y-3 text-xs sm:text-sm text-slate-200 font-mono">
                <div className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Targeting creators who are actively burning out on weekly video edits</span>
                </div>
                <div className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Sending tailored 30-second sample cuts that prove your value instantly</span>
                </div>
                <div className="flex items-start gap-2.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                  <span className="font-sans text-slate-200">Closing reliable $250 - $1,000 monthly recurring editing retainers</span>
                </div>
              </div>
            </div>

            <div className="mt-6 pt-4 border-t border-[#ff003b]/30 text-xs font-mono font-bold text-[#ff4d73]">
              RESULT: Predictable monthly recurring revenue and creative freedom.
            </div>
          </div>
        </div>

        {/* Big Punchline Banner */}
        <div className="bg-[#0f1118] border border-[#ff003b]/30 rounded-2xl p-8 text-center max-w-2xl mx-auto shadow-xl">
          <p className="text-xl sm:text-2xl font-extrabold text-white font-['Outfit',sans-serif] mb-3">
            Find creators. Start conversations. Land clients.
          </p>
          <p className="text-sm text-slate-400 mb-6">
            Now it's time to put those skills in front of people who might actually pay for them.
          </p>
          <button
            onClick={onOpenTrial}
            className="px-8 py-3.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.3)] inline-flex items-center gap-2 cursor-pointer border border-[#ff4d73]/40"
          >
            <span>Start Finding Creators Today</span>
            <ArrowRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </section>
  );
};
