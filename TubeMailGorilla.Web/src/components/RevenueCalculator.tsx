import React, { useState } from 'react';
import { DollarSign, TrendingUp, Users, Sparkles, ArrowRight, CheckCircle2, ShieldCheck, Flame, Zap } from 'lucide-react';
import confetti from 'canvas-confetti';

interface RevenueCalculatorProps {
  onOpenTrial: () => void;
}

export const RevenueCalculator: React.FC<RevenueCalculatorProps> = ({ onOpenTrial }) => {
  const [clientCount, setClientCount] = useState<number>(1);
  const [retainerAmount, setRetainerAmount] = useState<number>(500);

  const monthlyTotal = clientCount * retainerAmount;

  const handleClientChange = (count: number) => {
    setClientCount(count);
    if (count >= 4) {
      confetti({
        particleCount: 35,
        spread: 60,
        origin: { y: 0.7 },
        colors: ['#ff003b', '#ff4d73', '#ffffff']
      });
    }
  };

  return (
    <section className="py-24 bg-[#07080b] border-y border-slate-800/80 relative overflow-hidden" id="calculator">
      {/* Glow */}
      <div className="absolute top-1/3 left-1/2 -translate-x-1/2 w-96 h-96 bg-[#ff003b]/10 blur-[140px] rounded-full pointer-events-none" />

      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        {/* Section Header from Copy */}
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>REVENUE SIMULATOR</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            What Could One Client Be Worth?
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            One editing client can be worth $500+/month. TubeMail Gorilla helps you find more of them.
          </p>
        </div>

        {/* Copy Breakdown Visual Cards: 1 Client = $500 */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-12 font-mono">
          <div className="bg-[#0f1118] border border-slate-800 rounded-xl p-5 text-center flex flex-col justify-center shadow-lg">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">Client #1</span>
            <p className="text-2xl sm:text-3xl font-extrabold text-white font-['Rajdhani',sans-serif]">$500<span className="text-xs font-normal text-slate-400">/mo</span></p>
            <p className="text-[11px] text-slate-400 mt-1">4-6 edits / month</p>
          </div>

          <div className="bg-[#0f1118] border border-slate-800 rounded-xl p-5 text-center flex flex-col justify-center shadow-lg">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">Client #2</span>
            <p className="text-2xl sm:text-3xl font-extrabold text-white font-['Rajdhani',sans-serif]">$500<span className="text-xs font-normal text-slate-400">/mo</span></p>
            <p className="text-[11px] text-slate-400 mt-1">4-6 edits / month</p>
          </div>

          <div className="bg-[#1b0e14] border border-[#ff003b]/50 rounded-xl p-5 text-center flex flex-col justify-center shadow-[0_0_20px_rgba(255,0,59,0.15)] ring-1 ring-[#ff003b]/30">
            <span className="text-xs font-bold uppercase tracking-wider text-[#ff4d73] mb-1">Baseline Yield</span>
            <p className="text-3xl sm:text-4xl font-extrabold text-white font-['Rajdhani',sans-serif]">$500<span className="text-xs font-normal text-slate-300">/mo</span></p>
            <p className="text-[11px] text-[#ff708f] mt-1">Just ONE creator</p>
          </div>

          <div className="bg-[#0f1118] border border-slate-800 rounded-xl p-5 text-center flex flex-col justify-center shadow-lg">
            <span className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">Scale Target</span>
            <p className="text-2xl sm:text-3xl font-extrabold text-[#ff4d73] font-['Rajdhani',sans-serif]">$1,500 – $2,000<span className="text-xs font-normal text-slate-400">/mo</span></p>
            <p className="text-[11px] text-slate-400 mt-1">3 to 4 clients</p>
          </div>
        </div>

        {/* Interactive Dynamic Retainer Calculator */}
        <div className="bg-[#0b0d14] border border-[#ff003b]/30 rounded-2xl p-6 sm:p-10 shadow-2xl mb-12">
          <div className="flex flex-col lg:flex-row gap-10 items-center justify-between">
            {/* Left Sliders & Controls */}
            <div className="w-full lg:w-3/5 space-y-8">
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-bold text-slate-200 flex items-center gap-2 font-mono">
                    <Users className="w-4 h-4 text-[#ff003b]" />
                    <span>Number of YouTube Clients:</span>
                  </label>
                  <span className="text-2xl font-extrabold text-[#ff4d73] font-['Rajdhani',sans-serif] tracking-wider">
                    {clientCount} {clientCount === 1 ? 'Creator' : 'Creators'}
                  </span>
                </div>
                <input
                  type="range"
                  min="1"
                  max="8"
                  step="1"
                  value={clientCount}
                  onChange={(e) => handleClientChange(parseInt(e.target.value))}
                  className="w-full h-2.5 bg-slate-800 rounded-lg appearance-none cursor-pointer accent-[#ff003b]"
                />
                <div className="flex justify-between text-[11px] text-slate-400 mt-1.5 font-mono">
                  <span>1 Client</span>
                  <span>1 ($500 base)</span>
                  <span>2 ($1k mark)</span>
                  <span>4 (Scale)</span>
                </div>
              </div>

              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-bold text-slate-200 flex items-center gap-2 font-mono">
                    <DollarSign className="w-4 h-4 text-[#ff003b]" />
                    <span>Average Monthly Retainer / Client:</span>
                  </label>
                  <span className="text-2xl font-extrabold text-[#ff4d73] font-['Rajdhani',sans-serif] tracking-wider">
                    ${retainerAmount}<span className="text-xs font-normal text-slate-400">/mo</span>
                  </span>
                </div>
                
                {/* Retainer Preset Buttons */}
                <div className="grid grid-cols-4 gap-2 font-mono">
                  {[250, 500, 750, 1000].map((preset) => (
                    <button
                      key={preset}
                      onClick={() => setRetainerAmount(preset)}
                      className={`py-2 px-3 rounded-lg text-xs font-bold transition-all cursor-pointer ${
                        retainerAmount === preset
                          ? 'bg-[#ff003b] text-white shadow-md shadow-[#ff003b]/30 border border-[#ff4d73]'
                          : 'bg-[#151824] border border-slate-800 text-slate-300 hover:bg-[#1e2338]'
                      }`}
                    >
                      ${preset}/mo
                    </button>
                  ))}
                </div>
              </div>

              {/* Realistic Outreach Math */}
              <div className="p-4 rounded-xl bg-[#07080b] border border-slate-800 space-y-2 text-xs text-slate-300 font-mono">
                <div className="flex items-center justify-between">
                  <span className="text-slate-400">Prospects needed in pipeline (at ~5% close rate):</span>
                  <span className="font-bold text-[#ff4d73]">~{clientCount * 20} YouTube Leads</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-slate-400">Time to generate lead list with TubeMail Gorilla:</span>
                  <span className="font-bold text-emerald-400">&lt; 15 Minutes</span>
                </div>
              </div>
            </div>

            {/* Right Revenue Summary Display */}
            <div className="w-full lg:w-2/5 bg-gradient-to-br from-[#180f14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 rounded-2xl p-6 sm:p-8 text-center flex flex-col justify-between shadow-2xl">
              <div>
                <span className="text-xs font-mono font-bold uppercase tracking-wider text-[#ff4d73] mb-2 inline-block">
                  POTENTIAL MONTHLY RECURRING REVENUE
                </span>
                <p className="text-5xl sm:text-6xl font-extrabold text-white font-['Rajdhani',sans-serif] tracking-tight mb-2">
                  ${monthlyTotal.toLocaleString()}
                  <span className="text-base font-normal text-slate-400 font-sans">/month</span>
                </p>
                <p className="text-sm font-semibold text-[#ff708f] mb-6 font-mono">
                  if you close {clientCount} client{clientCount !== 1 ? 's' : ''} at ${retainerAmount}/mo
                </p>

                <div className="space-y-2 text-xs text-slate-300 text-left border-t border-slate-800 pt-4 mb-6 font-mono">
                  <div className="flex items-center gap-2">
                    <CheckCircle2 className="w-4 h-4 text-[#ff003b] shrink-0" />
                    <span className="font-sans">Predictable monthly cash flow</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <CheckCircle2 className="w-4 h-4 text-[#ff003b] shrink-0" />
                    <span className="font-sans">Zero 20% platform commission cuts</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <CheckCircle2 className="w-4 h-4 text-[#ff003b] shrink-0" />
                    <span className="font-sans">Long-term creator partnerships</span>
                  </div>
                </div>
              </div>

              <button
                onClick={onOpenTrial}
                className="w-full py-3.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.3)] transition-all flex items-center justify-center gap-2 cursor-pointer border border-[#ff4d73]/40"
              >
                <span>Find Your First 2 Clients Now</span>
                <ArrowRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        {/* Copy Disclaimer Note */}
        <p className="text-xs text-slate-400 text-center max-w-2xl mx-auto leading-relaxed">
          Your earning potential ultimately depends on your pricing, your offer, your outreach and your ability to close clients. <strong>TubeMail Gorilla gives you more opportunities to make those connections.</strong>
        </p>
      </div>
    </section>
  );
};
