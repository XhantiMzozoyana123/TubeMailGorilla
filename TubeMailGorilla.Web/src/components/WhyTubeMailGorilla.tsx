import React from 'react';
import { Zap, Target, TrendingUp, Laptop, Users, CheckCircle2, ArrowRight } from 'lucide-react';

interface WhyProps {
  onOpenTrial: () => void;
}

export const WhyTubeMailGorilla: React.FC<WhyProps> = ({ onOpenTrial }) => {
  const benefits = [
    {
      emoji: 'ðŸ¦',
      title: 'Find More Potential Clients',
      description: 'Stop relying entirely on referrals, freelance platforms and waiting for opportunities.',
      color: 'from-[#ff003b]/20 to-transparent',
      borderColor: 'group-hover:border-[#ff003b]/60',
    },
    {
      emoji: 'âš¡',
      title: 'Prospect at Warp Speed',
      description: 'Spend less time manually searching for creators and building prospect lists.',
      color: 'from-[#ff003b]/20 to-transparent',
      borderColor: 'group-hover:border-[#ff003b]/60',
    },
    {
      emoji: 'ðŸŽ¯',
      title: 'Target Your Ideal Clients',
      description: 'Look for creators in the niches and categories you actually want to work with.',
      color: 'from-[#ff003b]/20 to-transparent',
      borderColor: 'group-hover:border-[#ff003b]/60',
    },
    {
      emoji: 'ðŸ“ˆ',
      title: 'Build a Scalable Pipeline',
      description: "Don't depend on one client. Build a list of potential opportunities you can continuously work through.",
      color: 'from-[#ff003b]/20 to-transparent',
      borderColor: 'group-hover:border-[#ff003b]/60',
    },
    {
      emoji: 'ðŸ’»',
      title: 'Work From Anywhere',
      description: 'Your client acquisition system runs wherever your workstation is.',
      color: 'from-[#ff003b]/20 to-transparent',
      borderColor: 'group-hover:border-[#ff003b]/60',
    },
  ];

  return (
    <section className="py-24 bg-[#08090d] relative" id="why-tubemail-gorilla">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>BUILT AROUND ONE PROMISE</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            Built from the ground up for <br />
            <span className="text-[#ff003b]">proactive video editors.</span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            Everything you need to turn YouTube into your private client prospecting engine.
          </p>
        </div>

        {/* 5 Core Pillars Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-14">
          {benefits.map((benefit, i) => (
            <div
              key={i}
              className={`bg-[#0f1118] border border-slate-800 rounded-2xl p-7 flex flex-col justify-between group transition-all duration-300 shadow-xl ${benefit.borderColor} hover:-translate-y-1`}
            >
              <div>
                <div className="w-12 h-12 rounded-xl bg-[#151824] border border-slate-700 flex items-center justify-center text-2xl mb-5 shadow-inner">
                  {benefit.emoji}
                </div>
                <h3 className="text-xl font-bold text-white mb-2 font-['Outfit',sans-serif] group-hover:text-[#ff4d73] transition-colors">
                  {benefit.title}
                </h3>
                <p className="text-slate-300 text-sm leading-relaxed font-mono text-xs">
                  {benefit.description}
                </p>
              </div>
            </div>
          ))}

          {/* Quick Trial Card */}
          <div className="bg-gradient-to-br from-[#1b0e14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 rounded-2xl p-7 flex flex-col justify-between shadow-[0_0_25px_rgba(255,0,59,0.15)]">
            <div>
              <span className="text-xs font-mono font-bold uppercase tracking-wider text-[#ff4d73] block mb-2">
                READY WHEN YOU ARE
              </span>
              <h3 className="text-xl font-bold text-white mb-2 font-['Outfit',sans-serif]">
                Launch Your First Campaign
              </h3>
              <p className="text-slate-300 text-xs leading-relaxed mb-4 font-mono">
                Join video editors scaling to full client capacity without relying on saturated job board bidding wars.
              </p>
            </div>
            <button
              onClick={onOpenTrial}
              className="w-full py-3 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.3)] flex items-center justify-center gap-2 transition-colors cursor-pointer border border-[#ff4d73]/40"
            >
              <span>Start Free Trial â†’</span>
            </button>
          </div>
        </div>
      </div>
    </section>
  );
};
