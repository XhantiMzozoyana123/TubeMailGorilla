import React from 'react';
import { Clock, AlertTriangle, Layers, Film, Sparkles, XCircle, ArrowRight, Zap, ShieldAlert } from 'lucide-react';

interface TheProblemProps {
  onOpenTrial: () => void;
}

export const TheProblem: React.FC<TheProblemProps> = ({ onOpenTrial }) => {
  const manualSteps = [
    'Scrolling through YouTube endlessly without clear metrics',
    'Finding creators with inconsistent upload cadence & weak pacing',
    'Checking obscure "About" pages and broken dead links',
    'Scraping hard-to-find business contact info manually',
    'Building messy, unorganized spreadsheets that waste hours',
    'Sending generic cold messages that land in spam folders',
    'Manually tracking follow-ups across 20 open browser tabs',
  ];

  return (
    <section className="py-24 bg-[#08090d] relative overflow-hidden" id="the-problem">
      {/* Background ambient accent */}
      <div className="absolute top-1/2 left-0 w-80 h-80 bg-[#ff003b]/10 blur-[140px] rounded-full pointer-events-none" />

      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <AlertTriangle className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>LET'S BE HONEST FOR A SECOND</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            Your Editing Skills Aren't the Problem. <br />
            <span className="text-[#ff003b] underline decoration-[#ff003b]/40 decoration-wavy">
              Finding clients is.
            </span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl leading-relaxed">
            You already know how to edit. You know how to turn raw footage into content people actually want to watch. But there's one problem: <span className="text-white font-semibold">You need consistent creators to pay you.</span>
          </p>
        </div>

        {/* The Editing Stack & The Frustration Split */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-stretch">
          {/* Left Column: Your Superpower */}
          <div className="lg:col-span-5 bg-[#0f1118] border border-slate-800 hover:border-slate-700 rounded-2xl p-6 sm:p-8 flex flex-col justify-between shadow-2xl relative">
            <div className="absolute top-0 right-8 -translate-y-1/2 px-2.5 py-0.5 rounded bg-[#1c2033] border border-slate-700 text-[10px] font-mono text-slate-300 uppercase tracking-widest">
              YOU ALREADY DID THE HARD PART
            </div>
            <div>
              <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-4">
                <Film className="w-4 h-4" />
                <span>What You Already Mastered</span>
              </div>
              <h3 className="text-2xl font-extrabold text-white mb-4 font-['Outfit',sans-serif]">
                You Know Your Craft Inside Out.
              </h3>
              <p className="text-slate-300 text-sm leading-relaxed mb-6">
                You've put in the countless hours learning keyframing, audio mixing, pacing, and visual storytelling.
              </p>

              {/* Tool Badges with high-tech frames */}
              <div className="space-y-3 mb-6">
                <div className="flex items-center gap-3 p-3 rounded-xl bg-[#141724] border border-purple-500/30 text-slate-200">
                  <div className="w-9 h-9 rounded-lg bg-purple-900/60 border border-purple-500/50 flex items-center justify-center font-bold text-purple-300 text-sm shrink-0 font-mono">
                    Pr
                  </div>
                  <div>
                    <p className="text-sm font-bold text-white">Adobe Premiere Pro</p>
                    <p className="text-xs text-slate-400">Timeline mastering & multi-cam syncing</p>
                  </div>
                </div>

                <div className="flex items-center gap-3 p-3 rounded-xl bg-[#141724] border border-pink-500/30 text-slate-200">
                  <div className="w-9 h-9 rounded-lg bg-pink-900/60 border border-pink-500/50 flex items-center justify-center font-bold text-pink-300 text-sm shrink-0 font-mono">
                    Da
                  </div>
                  <div>
                    <p className="text-sm font-bold text-white">DaVinci Resolve</p>
                    <p className="text-xs text-slate-400">Color grading & Fairlight audio perfection</p>
                  </div>
                </div>

                <div className="flex items-center gap-3 p-3 rounded-xl bg-[#141724] border border-blue-500/30 text-slate-200">
                  <div className="w-9 h-9 rounded-lg bg-blue-900/60 border border-blue-500/50 flex items-center justify-center font-bold text-blue-300 text-sm shrink-0 font-mono">
                    Ae
                  </div>
                  <div>
                    <p className="text-sm font-bold text-white">After Effects</p>
                    <p className="text-xs text-slate-400">Kinetic typography & visual motion effects</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="p-4 rounded-xl bg-[#ff003b]/10 border border-[#ff003b]/30 text-[#ff708f] text-xs font-semibold font-mono">
              âš¡ You have the skills. Now you just need consistent creators to put them in front of.
            </div>
          </div>

          {/* Right Column: The Manual Prospecting Grind */}
          <div className="lg:col-span-7 bg-gradient-to-b from-[#1b0e14] via-[#0f1118] to-[#0b0d13] border border-[#ff003b]/40 rounded-2xl p-6 sm:p-8 flex flex-col justify-between shadow-2xl relative">
            <div>
              <div className="flex items-center justify-between gap-4 mb-4">
                <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider">
                  <Clock className="w-4 h-4" />
                  <span>The Manual Prospecting Drain</span>
                </div>
                <span className="px-2.5 py-1 rounded-md bg-[#ff003b]/20 border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold">
                  DRAINS 10+ HOURS / WEEK
                </span>
              </div>

              <h3 className="text-2xl font-extrabold text-white mb-2 font-['Outfit',sans-serif]">
                Searching for clients manually kills editing flow.
              </h3>
              <p className="text-slate-300 text-sm mb-6">
                By the time you're finished hunting for emails and updating spreadsheets, you've barely had time to edit.
              </p>

              <div className="space-y-2.5 mb-8">
                {manualSteps.map((step, index) => (
                  <div
                    key={index}
                    className="flex items-center gap-3 p-2.5 rounded-lg bg-[#07080b]/80 border border-slate-800 hover:border-slate-700 text-slate-300 text-xs sm:text-sm"
                  >
                    <XCircle className="w-4 h-4 text-[#ff003b] shrink-0" />
                    <span>{step}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Transition callout with Crimson styling */}
            <div className="p-5 rounded-xl bg-gradient-to-r from-[#ff003b]/20 via-[#990024]/15 to-[#ff003b]/20 border border-[#ff003b]/50 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-[0_0_20px_rgba(255,0,59,0.2)]">
              <div className="flex-1">
                <p className="text-lg font-extrabold font-['Outfit',sans-serif]">
                  <span className="text-white">Stop scrolling.</span>{' '}
                  <span className="text-[#ff003b]">Start sending.</span>
                </p>
                <p className="text-xs text-slate-300 mt-0.5">Lead lists in seconds.</p>
              </div>
              <button
                onClick={onOpenTrial}
                className="w-full sm:w-auto px-5 py-2.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-sm uppercase tracking-wider font-['Rajdhani',sans-serif] whitespace-nowrap transition-all flex items-center justify-center gap-2 cursor-pointer shadow-lg shadow-[#ff003b]/40 border border-[#ff4d73]/50"
              >
                <span>Automate Your Search</span>
                <ArrowRight className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

