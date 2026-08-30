import React from 'react';
import { Youtube, Target, Zap, ArrowRight, TrendingUp, Users, Video } from 'lucide-react';

interface TheSolutionProps {
  onOpenTrial: () => void;
}

export const TheSolution: React.FC<TheSolutionProps> = ({ onOpenTrial }) => {
  return (
    <section className="py-24 bg-[#07080b] relative overflow-hidden" id="the-solution">
      {/* Background glow */}
      <div className="absolute top-1/2 right-0 w-96 h-96 bg-[#ff003b]/10 blur-[150px] rounded-full pointer-events-none" />

      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>MEET YOUR SHORTCUT</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-6 leading-tight">
            Turn YouTube Into Your <br />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#ff003b] via-[#ff4d73] to-white">
              Client-Finding Machine.
            </span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl leading-relaxed">
            YouTube isn't just a place to watch videos. It's the highest concentration of video creators on the planet who desperately need workflow speed.
          </p>
        </div>

        {/* Opportunity Breakdown Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-14">
          <div className="bg-[#0f1118] border border-slate-800 hover:border-[#ff003b]/50 rounded-2xl p-7 relative group transition-all duration-300 shadow-xl hover:shadow-[0_0_20px_rgba(255,0,59,0.15)]">
            <div className="w-12 h-12 rounded-xl bg-[#ff003b]/10 border border-[#ff003b]/30 text-[#ff003b] flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
              <Youtube className="w-6 h-6" />
            </div>
            <h3 className="text-xl font-bold text-white mb-2 font-['Outfit',sans-serif]">
              Massive Creator Ecosystem
            </h3>
            <p className="text-slate-400 text-sm leading-relaxed">
              Millions of active channels publishing weekly long-form videos and daily Shorts across hundreds of high-value niches.
            </p>
          </div>

          <div className="bg-[#0f1118] border border-slate-800 hover:border-[#ff003b]/50 rounded-2xl p-7 relative group transition-all duration-300 shadow-xl hover:shadow-[0_0_20px_rgba(255,0,59,0.15)]">
            <div className="w-12 h-12 rounded-xl bg-[#ff003b]/10 border border-[#ff003b]/30 text-[#ff003b] flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
              <TrendingUp className="w-6 h-6" />
            </div>
            <h3 className="text-xl font-bold text-white mb-2 font-['Outfit',sans-serif]">
              Publishing = Opportunity
            </h3>
            <p className="text-slate-400 text-sm leading-relaxed">
              And every creator who consistently publishes content is a potential recurring client for an editor who can save them hours of export time.
            </p>
          </div>

          <div className="bg-[#0f1118] border border-slate-800 hover:border-[#ff003b]/50 rounded-2xl p-7 relative group transition-all duration-300 shadow-xl hover:shadow-[0_0_20px_rgba(255,0,59,0.15)]">
            <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
              <Target className="w-6 h-6" />
            </div>
            <h3 className="text-xl font-bold text-white mb-2 font-['Outfit',sans-serif]">
              Predictable Pipeline
            </h3>
            <p className="text-slate-400 text-sm leading-relaxed">
              <strong className="text-slate-200">TubeMail Gorilla</strong> extracts real YouTuber leads with real-time email addresses, then helps you send personalized emails with AI-written icebreaker first lines â€” a proactive prospecting pipeline you control.
            </p>
          </div>
        </div>

        {/* Visual Engine Showcase Banner */}
        <div className="bg-gradient-to-r from-[#0d0f17] via-[#141726] to-[#0d0f17] border border-[#ff003b]/40 rounded-2xl p-8 sm:p-12 text-center shadow-2xl relative overflow-hidden">
          <div className="max-w-3xl mx-auto relative z-10">
            <span className="text-4xl sm:text-5xl mb-4 inline-block">ðŸ¦</span>
            <h3 className="text-2xl sm:text-4xl font-extrabold text-white font-['Outfit',sans-serif] mb-4">
              Stop waiting for clients to post job listings.
            </h3>
            <p className="text-slate-300 text-base sm:text-lg mb-8 leading-relaxed">
              Find creators in your favorite niches (tech, gaming, finance, vlogs) who are already publishing, and pitch them directly with customized sample edits.
            </p>
            <button
              onClick={onOpenTrial}
              className="px-8 py-4 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-base uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_25px_rgba(255,0,59,0.4)] transition-all flex items-center justify-center gap-2 mx-auto cursor-pointer border border-[#ff4d73]/50"
            >
              <span>BUILD YOUR PROSPECT LIST NOW</span>
              <ArrowRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </section>
  );
};
