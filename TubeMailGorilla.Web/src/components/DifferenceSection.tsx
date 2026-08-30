import React from 'react';
import { ArrowRight, Check, X, Sparkles, RefreshCw, Zap } from 'lucide-react';

interface DifferenceSectionProps {
  onOpenTrial: () => void;
}

export const DifferenceSection: React.FC<DifferenceSectionProps> = ({ onOpenTrial }) => {
  return (
    <section className="py-24 bg-[#07080b] relative border-t border-slate-800/80" id="the-difference">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>TWO PATHS. ONE CHOICE.</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            Don't Wait for Clients. <br />
            <span className="text-[#ff003b]">Go Find Them.</span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            The difference between struggling for freelance gigs and building a stable editing business comes down to your operational pipeline.
          </p>
        </div>

        {/* Reactive vs Proactive Comparison Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-14">
          {/* Reactive Card */}
          <div className="bg-[#0f1118] border border-slate-800 rounded-3xl p-8 flex flex-col justify-between shadow-xl">
            <div>
              <div className="flex items-center gap-2 text-rose-400 text-xs font-mono font-bold uppercase tracking-wider mb-4">
                <RefreshCw className="w-4 h-4 text-rose-500" />
                <span>The Reactive Trap (Most Freelancers)</span>
              </div>
              <h3 className="text-2xl font-bold text-slate-200 mb-6 font-['Outfit',sans-serif]">
                Waiting for opportunities to appear
              </h3>

              <div className="space-y-4 text-sm text-slate-400 font-mono">
                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-slate-800">
                  <span className="w-6 h-6 rounded-md bg-rose-500/20 text-[#ff003b] font-bold flex items-center justify-center text-xs shrink-0">
                    âœ•
                  </span>
                  <span className="font-sans">A job gets posted <span className="text-slate-200 font-medium">â†’ they apply late</span></span>
                </div>

                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-slate-800">
                  <span className="w-6 h-6 rounded-md bg-rose-500/20 text-[#ff003b] font-bold flex items-center justify-center text-xs shrink-0">
                    âœ•
                  </span>
                  <span className="font-sans">Someone asks for an editor <span className="text-slate-200 font-medium">â†’ compete with 40 people</span></span>
                </div>

                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-slate-800">
                  <span className="w-6 h-6 rounded-md bg-rose-500/20 text-[#ff003b] font-bold flex items-center justify-center text-xs shrink-0">
                    âœ•
                  </span>
                  <span className="font-sans">A friend refers someone <span className="text-slate-200 font-medium">â†’ take whatever low rate offered</span></span>
                </div>
              </div>
            </div>

            <div className="mt-8 pt-4 border-t border-slate-800 text-xs text-slate-500 font-mono">
              STATUS: Inconsistent income, constant anxiety, zero control over creator roster.
            </div>
          </div>

          {/* Proactive Card */}
          <div className="bg-gradient-to-br from-[#1b0e14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 rounded-3xl p-8 flex flex-col justify-between shadow-[0_0_25px_rgba(255,0,59,0.15)] ring-1 ring-[#ff003b]/30">
            <div>
              <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-4">
                <span>ðŸ¦</span>
                <span>The TubeMail Gorilla Protocol (Proactive)</span>
              </div>
              <h3 className="text-2xl font-bold text-white mb-6 font-['Outfit',sans-serif]">
                Creating your own opportunities
              </h3>

              <div className="space-y-4 text-sm text-slate-200 font-mono">
                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-[#ff003b]/30">
                  <span className="w-6 h-6 rounded-md bg-[#ff003b]/20 text-[#ff003b] font-bold flex items-center justify-center text-xs shrink-0">
                    âœ“
                  </span>
                  <span className="font-semibold text-white font-sans">Find the exact niche creators.</span>
                </div>

                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-[#ff003b]/30">
                  <span className="w-6 h-6 rounded-md bg-[#ff003b]/20 text-[#ff003b] font-bold flex items-center justify-center text-xs shrink-0">
                    âœ“
                  </span>
                  <span className="font-semibold text-white font-sans">Build an organized prospect target list.</span>
                </div>

                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-[#ff003b]/30">
                  <span className="w-6 h-6 rounded-md bg-[#ff003b]/20 text-[#ff003b] font-bold flex items-center justify-center text-xs shrink-0">
                    âœ“
                  </span>
                  <span className="font-semibold text-white font-sans">Start the direct value conversation.</span>
                </div>

                <div className="flex items-center gap-3 p-3.5 rounded-xl bg-[#08090d] border border-[#ff4d73]/40 text-[#ff708f]">
                  <span className="w-6 h-6 rounded-md bg-[#ff003b]/30 text-white font-bold flex items-center justify-center text-xs shrink-0">
                    âœ“
                  </span>
                  <span className="font-bold text-white font-sans">Create your own high-ticket recurring retainers.</span>
                </div>
              </div>
            </div>

            <div className="mt-8 pt-4 border-t border-[#ff003b]/30 text-xs font-mono font-bold text-[#ff4d73]">
              RESULT: Predictable client pipeline, higher retainer rates, complete schedule control.
            </div>
          </div>
        </div>

        {/* CTA Bar */}
        <div className="text-center">
          <button
            onClick={onOpenTrial}
            className="px-8 py-4 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.3)] transition-all inline-flex items-center gap-2 cursor-pointer border border-[#ff4d73]/40"
          >
            <span>Start Finding Creators Now</span>
            <ArrowRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </section>
  );
};
