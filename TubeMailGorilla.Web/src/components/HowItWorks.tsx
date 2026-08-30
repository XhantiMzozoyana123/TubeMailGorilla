import React, { useState } from 'react';
import { Search, Database, Send, DollarSign, ArrowRight, CheckCircle2, Sparkles, Filter, Mail, Flame, Zap } from 'lucide-react';

interface HowItWorksProps {
  onOpenTrial: () => void;
  onOpenPitchDrawer: () => void;
}

export const HowItWorks: React.FC<HowItWorksProps> = ({ onOpenTrial, onOpenPitchDrawer }) => {
  const [activeStep, setActiveStep] = useState<number>(1);

  const steps = [
    {
      num: '01',
      title: 'Search',
      subtitle: 'Find the creators you want to work with.',
      description:
        "Search YouTube for creators and channels that match your ideal client. Find creators based on the type of content you're interested in editing â€” gaming, tech, finance, lifestyle vlogs, or documentaries.",
      icon: <Search className="w-6 h-6 text-[#ff003b]" />,
      badge: 'Targeted Niche Discovery',
      preview: {
        type: 'search',
        query: 'Personal Finance & Crypto',
        resultsCount: '1,420 Active Channels Found',
        sampleNiche: 'Finance & Investing',
      },
    },
    {
      num: '02',
      title: 'Extract',
      subtitle: 'Build your lead list.',
      description:
        'TubeMail Gorilla extracts YouTube creators in real time â€” their channel data and direct email address â€” so you build a targeted prospect list in minutes instead of collecting leads one by one.',
      highlight: 'Real-time emails. Verified contacts.',
      icon: <Database className="w-6 h-6 text-[#ff003b]" />,
      badge: 'Real-Time Email Extraction',
      preview: {
        type: 'extract',
        extractedInfo: [
          'Real-Time Business Contact Email',
          'Average Monthly Video Output',
          'Channel Subscriber Tier (10k-500k)',
          'Editing Bottleneck & Pitch Angle',
        ],
      },
    },
    {
      num: '03',
      title: 'Reach Out',
      subtitle: 'Put your editing skills in front of potential clients.',
      description:
        "Once you've built your list, send personalized emails with AI-written icebreaker first lines that reference each creator's actual content. No generic templates â€” every pitch feels hand-written.",
      highlight: 'Personalized first lines. Real replies.',
      icon: <Send className="w-6 h-6 text-[#ff003b]" />,
      badge: 'AI Icebreakers & Personalized Pitches',
      preview: {
        type: 'reachout',
        subject: 'Quick retention edit idea for your latest video',
        strategy: '30-second sample intro hook + custom motion graphics offer',
      },
    },
    {
      num: '04',
      title: 'Close',
      subtitle: 'Turn conversations into clients.',
      description:
        "One client can be worth $500+/month. You bring the editing skills — TubeMail Gorilla brings you a steady stream of creators worth pitching, so your pipeline never runs dry.",
      highlight: "You bring the editing skills — TubeMail Gorilla brings the leads.",
      icon: <DollarSign className="w-6 h-6 text-emerald-400" />,
      badge: 'Recurring Retainer Potential',
      preview: {
        type: 'close',
        math: '1 Client @ $500/mo = $500/month potential',
        scaling: 'Your results depend on your outreach & offer',
      },
    },
  ];

  return (
    <section className="py-24 bg-[#08090d] relative border-b border-slate-800/80" id="how-it-works">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>FROM EMPTY INBOX TO PAID RETAINER</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            From Blank Spreadsheet to <br />
            <span className="text-[#ff003b]">Paid Video Editing Retainers.</span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            A high-performance 4-step framework designed to eliminate guesswork from creator prospecting.
          </p>
        </div>

        {/* 4 Cards Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-16">
          {steps.map((step, idx) => (
            <div
              key={step.num}
              className={`bg-[#0f1118] border rounded-2xl p-6 flex flex-col justify-between transition-all duration-300 relative group hover:border-[#ff003b]/60 hover:shadow-[0_0_20px_rgba(255,0,59,0.15)] ${
                activeStep === idx + 1 ? 'border-[#ff003b]/60 ring-1 ring-[#ff003b]/30 shadow-lg' : 'border-slate-800'
              }`}
            >
              <div>
                {/* Step number badge & icon */}
                <div className="flex items-center justify-between gap-2 mb-4">
                  <span className="text-3xl font-extrabold text-slate-600 group-hover:text-[#ff003b] font-['Rajdhani',sans-serif] tracking-wider transition-colors">
                    {step.num}
                  </span>
                  <div className="w-10 h-10 rounded-xl bg-[#151824] border border-slate-700/80 group-hover:border-[#ff003b]/50 flex items-center justify-center transition-colors">
                    {step.icon}
                  </div>
                </div>

                <span className="inline-block px-2.5 py-0.5 rounded bg-[#ff003b]/10 border border-[#ff003b]/30 text-[#ff4d73] text-[11px] font-mono font-bold mb-2">
                  {step.badge}
                </span>

                <h3 className="text-xl font-bold text-white mb-2 font-['Outfit',sans-serif]">
                  {step.title}
                </h3>
                <h4 className="text-sm font-semibold text-slate-300 mb-3">
                  {step.subtitle}
                </h4>
                <p className="text-xs text-slate-400 leading-relaxed mb-4">
                  {step.description}
                </p>

                {step.highlight && (
                  <div className="p-2.5 rounded-lg bg-[#ff003b]/10 border border-[#ff003b]/30 text-[#ff708f] text-xs font-mono font-bold mb-4">
                    {step.highlight}
                  </div>
                )}
              </div>

              {/* Step Mini Mockup Visual */}
              <div className="mt-4 pt-4 border-t border-slate-800/80">
                {step.preview.type === 'search' && (
                  <div className="p-3 rounded-lg bg-[#07080b] border border-slate-800 text-xs space-y-1.5 font-mono">
                    <div className="flex items-center gap-1.5 text-slate-400">
                      <Search className="w-3 h-3 text-[#ff003b]" />
                      <span className="text-slate-200">"{step.preview.query}"</span>
                    </div>
                    <p className="text-[11px] text-emerald-400 font-semibold">{step.preview.resultsCount}</p>
                  </div>
                )}

                {step.preview.type === 'extract' && (
                  <div className="p-3 rounded-lg bg-[#07080b] border border-slate-800 text-xs space-y-1 font-mono">
                    {step.preview.extractedInfo?.map((info, i) => (
                      <div key={i} className="flex items-center gap-1.5 text-[11px] text-slate-300">
                        <CheckCircle2 className="w-3 h-3 text-emerald-400 shrink-0" />
                        <span>{info}</span>
                      </div>
                    ))}
                  </div>
                )}

                {step.preview.type === 'reachout' && (
                  <div className="p-3 rounded-lg bg-[#07080b] border border-slate-800 text-xs space-y-1.5 font-mono">
                    <p className="text-[11px] text-slate-400">Subject: <span className="text-slate-200 font-medium">{step.preview.subject}</span></p>
                    <p className="text-[10px] text-[#ff4d73] font-semibold">{step.preview.strategy}</p>
                  </div>
                )}

                {step.preview.type === 'close' && (
                  <div className="p-3 rounded-lg bg-emerald-950/30 border border-emerald-500/40 text-xs space-y-1 text-center font-mono">
                    <p className="text-emerald-400 font-bold text-sm font-['Outfit',sans-serif]">
                      {step.preview.math}
                    </p>
                    <p className="text-[10px] text-slate-300">{step.preview.scaling}</p>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>

        {/* Step Action Bar */}
        <div className="bg-[#0f1118] border border-[#ff003b]/30 rounded-2xl p-6 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-xl">
          <div className="flex items-center gap-3">
            <span className="text-2xl">ðŸ¦</span>
            <p className="text-sm text-slate-300">
              <strong className="text-white">Ready to activate step 01?</strong> Generate your first creator lead list in under 2 minutes.
            </p>
          </div>
          <div className="flex items-center gap-3 w-full sm:w-auto">
            <button
              onClick={onOpenPitchDrawer}
              className="w-full sm:w-auto px-4 py-2.5 rounded-lg bg-[#151824] hover:bg-[#1f2438] border border-slate-700 text-xs font-mono font-bold text-slate-200 transition-colors cursor-pointer uppercase"
            >
              Email Pitch Library âœ‰ï¸
            </button>
            <button
              onClick={onOpenTrial}
              className="w-full sm:w-auto px-5 py-2.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.3)] transition-all flex items-center justify-center gap-1.5 cursor-pointer whitespace-nowrap border border-[#ff4d73]/40"
            >
              <span>Start Free Trial</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>
    </section>
  );
};
