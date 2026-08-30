import React from 'react';
import { ArrowRight, Clock, Search, Mail, Sparkles } from 'lucide-react';

interface BeforeAfterProps {
  onOpenTrial: () => void;
}

export const BeforeAfter: React.FC<BeforeAfterProps> = ({ onOpenTrial }) => {
  return (
    <section className="py-24 bg-[#07080b] relative border-t border-slate-800/80" id="before-after">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Clock className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>THE OLD WAY VS THE NEW WAY</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            40+ Hours of Prospecting. <br />
            <span className="text-[#ff003b]">Or 15 Minutes. Your Choice.</span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            Most editors spend their week hunting. TubeMail Gorilla automates it.
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-stretch">
          {/* BEFORE — The Manual Grind */}
          <div className="bg-[#0f1118] border border-slate-800 rounded-2xl p-7 flex flex-col justify-between shadow-xl">
            <div>
              <div className="flex items-center gap-3 text-rose-400 text-xs font-mono font-bold uppercase tracking-wider mb-5">
                <div className="w-3 h-3 rounded-full bg-rose-500 animate-pulse" />
                <span>BEFORE — Manual Prospecting</span>
              </div>
              <h3 className="text-2xl font-bold text-slate-200 mb-6 font-['Outfit',sans-serif]">
                Monday → Friday. All prospecting. Zero pitching.
              </h3>


              <div className="space-y-4 mb-6">
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-slate-800 text-slate-300 text-sm">
                  <Clock className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">2 hours scrolling through YouTube with no clear metrics</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-slate-800 text-slate-300 text-sm">
                  <Mail className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Manually checking "About" pages and dead links for emails</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-slate-800 text-slate-300 text-sm">
                  <Mail className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Building messy spreadsheets with no verification</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-slate-800 text-slate-300 text-sm">
                  <Search className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Sending generic cold emails that mostly go unanswered</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-slate-800 text-slate-300 text-sm">
                  <Clock className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Manually tracking 20+ browser tabs for follow-ups</span>
                </div>
              </div>
            </div>
            <div className="pt-4 border-t border-slate-800 text-xs text-slate-500 font-mono">
              STATUS: Great editor. Zero consistent client pipeline. 10+ hours wasted per week.
            </div>
          </div>
          {/* AFTER — The TubeMail Gorilla Way */}
          <div className="bg-gradient-to-br from-[#1b0e14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 rounded-2xl p-7 flex flex-col justify-between shadow-[0_0_25px_rgba(255,0,59,0.15)]">
            <div>
              <div className="flex items-center gap-3 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-5">
                <div className="w-3 h-3 rounded-full bg-[#ff003b] animate-pulse" />
                <span>AFTER — TubeMail Gorilla</span>
              </div>
              <h3 className="text-2xl font-bold text-white mb-6 font-['Outfit',sans-serif]">
                Enter a keyword. <br />
                Get a client pipeline. 15 minutes.
              </h3>


              <div className="space-y-4 mb-6">
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-[#ff003b]/30 text-slate-200 text-sm">
                  <Search className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Search "video editor" → 538 creators found instantly</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-[#ff003b]/30 text-slate-200 text-sm">
                  <Mail className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Extract 217 verified business emails in one click</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-[#ff003b]/30 text-slate-200 text-sm">
                  <Sparkles className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">AI writes personalized pitches referencing each creator's actual content</span>
                </div>
                <div className="flex items-start gap-3 p-3 rounded-lg bg-[#07080b]/80 border border-[#ff003b]/30 text-slate-200 text-sm">
                  <ArrowRight className="w-4 h-4 text-[#ff003b] shrink-0 mt-0.5" />
                  <span className="font-sans">Send your campaign. Start getting replies. Get back to editing.</span>
                </div>
              </div>
            </div>
            <div className="pt-4 border-t border-[#ff003b]/30 text-xs font-mono font-bold text-[#ff4d73]">
              RESULT: From 10+ hours/week prospecting to 15 minutes of setup.
            </div>
          </div>
        </div>

        {/* CTA */}
        <div className="text-center mt-16">
          <button
            onClick={onOpenTrial}
            className="px-10 py-4 rounded-xl bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-lg uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_30px_rgba(255,0,59,0.4)] hover:shadow-[0_0_40px_rgba(255,0,59,0.6)] hover:-translate-y-0.5 transition-all duration-200 inline-flex items-center gap-3 cursor-pointer border border-[#ff4d73]"
            id="beforeafter-cta"
          >
            <span>Find My First Leads Free</span>
            <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
          </button>
          <p className="mt-3 text-xs text-slate-400 font-mono">
            No credit card. Get your first creator leads in minutes.
          </p>
        </div>
      </div>
    </section>
  );
};
