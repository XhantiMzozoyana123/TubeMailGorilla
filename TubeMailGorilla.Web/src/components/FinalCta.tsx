import React from 'react';
import { ArrowRight, Sparkles, CheckCircle2, ShieldCheck, Zap } from 'lucide-react';

interface FinalCtaProps {
  onOpenTrial: () => void;
}

export const FinalCta: React.FC<FinalCtaProps> = ({ onOpenTrial }) => {
  return (
    <div className="relative overflow-hidden bg-[#07080b]">
      {/* Mid CTA Section */}
      <section className="py-24 relative border-t border-slate-800/80" id="cta-section">
        {/* Ambient Glow */}
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[500px] h-[300px] bg-[#ff003b]/10 blur-[140px] rounded-full pointer-events-none" />

        <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 text-center relative z-10">
          <div className="bg-gradient-to-b from-[#160f14] via-[#0f1118] to-[#07080b] border border-[#ff003b]/40 rounded-3xl p-8 sm:p-14 shadow-2xl">
            <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
              Your Next Client Could Be <br />
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#ff003b] via-[#ff4d73] to-white">
                One Search Away.
              </span>
            </h2>
            <p className="text-slate-300 text-lg sm:text-xl max-w-2xl mx-auto mb-3">
              You already have the skill. Now you need the direct opportunities.
            </p>
            <p className="text-[#ff003b] font-bold text-xl sm:text-2xl mb-8 font-['Rajdhani',sans-serif] tracking-wider uppercase">
              Start Finding YouTubers Today.
            </p>

            <p className="text-slate-400 text-sm sm:text-base max-w-xl mx-auto mb-10 font-mono">
              Start your free TubeMail Gorilla trial and discover how many high-potential creator clients you can uncover.
            </p>

            <button
              onClick={onOpenTrial}
              className="px-10 py-5 rounded-xl bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-lg uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_30px_rgba(255,0,59,0.4)] hover:shadow-[0_0_40px_rgba(255,0,59,0.6)] hover:scale-105 transition-all duration-200 inline-flex items-center gap-3 cursor-pointer group border border-[#ff4d73]"
              id="mid-cta-btn"
            >
              <span>START YOUR FREE TRIAL</span>
              <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </button>

            <div className="mt-8 flex flex-wrap items-center justify-center gap-4 sm:gap-8 text-xs sm:text-sm font-semibold text-slate-300 font-mono">
              <span className="flex items-center gap-1.5">
                <CheckCircle2 className="w-4 h-4 text-[#ff003b]" />
                <span className="font-sans">Find Active YouTubers</span>
              </span>
              <span className="text-slate-600">•</span>
              <span className="flex items-center gap-1.5">
                <CheckCircle2 className="w-4 h-4 text-[#ff003b]" />
                <span className="font-sans">Find High-Yield Clients</span>
              </span>
              <span className="text-slate-600">•</span>
              <span className="flex items-center gap-1.5">
                <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                <span className="font-sans">Scale Your Retainers</span>
              </span>
            </div>
          </div>
        </div>
      </section>

      {/* FINAL CTA SECTION (The Climax of the Copy) */}
      <section className="py-28 relative bg-gradient-to-b from-[#08090d] via-[#050608] to-[#000000] border-t border-slate-800" id="final-cta">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <div className="inline-block p-4 rounded-2xl bg-[#1b0e14] border border-[#ff003b]/40 text-5xl mb-8 shadow-[0_0_30px_rgba(255,0,59,0.25)] animate-pulse">
                        <img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-10 h-10 object-contain" />
          </div>

          <h2 className="text-3xl sm:text-5xl lg:text-6xl font-black text-white font-['Outfit',sans-serif] tracking-tight mb-8 leading-tight">
            Stop Hunting for Clients <br />
            <span className="text-[#ff003b]">the Hard Way.</span>
          </h2>

          {/* 3 Core Truths Stack from copy */}
          <div className="max-w-md mx-auto space-y-3 mb-10 font-mono">
            <div className="py-3 px-5 rounded-xl bg-[#0f1118] border border-slate-800 text-base font-bold text-slate-200">
              ⚡ YouTube is full of creators.
            </div>
            <div className="py-3 px-5 rounded-xl bg-[#180f14] border border-[#ff003b]/30 text-base font-bold text-[#ff4d73]">
              🔥 Creators need consistent content.
            </div>
            <div className="py-3 px-5 rounded-xl bg-[#0f1118] border border-slate-800 text-base font-bold text-emerald-300">
              🎬 Great content needs skilled editors.
            </div>
          </div>

          <p className="text-xl sm:text-2xl text-slate-200 font-semibold mb-8">
            Go find your next opportunity.
          </p>

          <div className="mb-10">
                        <h3 className="text-3xl sm:text-4xl font-extrabold text-white font-['Outfit',sans-serif] mb-2 flex items-center justify-center gap-3">
                          <img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-8 h-8 object-contain" />
              <span>TubeMail Gorilla</span>
            </h3>
            <p className="text-[#ff003b] font-bold text-lg font-mono tracking-wider">YOUR CLIENT-FINDING MACHINE.</p>
          </div>

          <button
            onClick={onOpenTrial}
            className="w-full sm:w-auto px-10 py-5 rounded-xl bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xl uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_35px_rgba(255,0,59,0.5)] hover:shadow-[0_0_50px_rgba(255,0,59,0.7)] hover:-translate-y-1 transition-all duration-200 inline-flex items-center justify-center gap-3 cursor-pointer border border-[#ff4d73]"
            id="final-cta-btn"
          >
            <span>START YOUR FREE TRIAL →</span>
          </button>
        </div>
      </section>
    </div>
  );
};
