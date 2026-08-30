import React, { useState } from 'react';
import { ChevronDown, HelpCircle, Sparkles, MessageSquare, Zap } from 'lucide-react';
import { FAQ_DATA } from '../data/pitchTemplates';

interface FaqSectionProps {
  onOpenTrial: () => void;
}

export const FaqSection: React.FC<FaqSectionProps> = ({ onOpenTrial }) => {
  const [openIndices, setOpenIndices] = useState<number[]>([0, 1]);

  const toggleIndex = (index: number) => {
    if (openIndices.includes(index)) {
      setOpenIndices(openIndices.filter((i) => i !== index));
    } else {
      setOpenIndices([...openIndices, index]);
    }
  };

  return (
    <section className="py-24 bg-[#08090d] border-t border-slate-800/80 relative" id="faq">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        {/* Section Header */}
        <div className="text-center mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Zap className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>QUESTIONS EDITORS ACTUALLY ASK</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4">
            Got Questions? We've Got Answers.
          </h2>
          <p className="text-slate-300 text-base sm:text-lg">
            Everything you need to know about using TubeMail Gorilla to land video editing clients.
          </p>
        </div>

        {/* FAQ Accordions */}
        <div className="space-y-4 mb-14">
          {FAQ_DATA.map((faq, index) => {
            const isOpen = openIndices.includes(index);
            return (
              <div
                key={index}
                className={`bg-[#0f1118] border rounded-xl transition-all duration-200 overflow-hidden shadow-lg ${
                  isOpen ? 'border-[#ff003b]/60 shadow-[0_0_20px_rgba(255,0,59,0.15)]' : 'border-slate-800 hover:border-slate-700'
                }`}
              >
                <button
                  onClick={() => toggleIndex(index)}
                  className="w-full p-5 sm:p-6 text-left flex items-center justify-between gap-4 cursor-pointer"
                  aria-expanded={isOpen}
                >
                  <span className="font-bold text-base sm:text-lg text-white font-['Outfit',sans-serif]">
                    {faq.question}
                  </span>
                  <div
                    className={`w-8 h-8 rounded-lg bg-[#151824] flex items-center justify-center text-slate-400 shrink-0 transition-transform duration-200 ${
                      isOpen ? 'rotate-180 text-[#ff003b] bg-[#2a131b] border border-[#ff003b]/40' : ''
                    }`}
                  >
                    <ChevronDown className="w-4 h-4" />
                  </div>
                </button>

                {isOpen && (
                  <div className="px-5 pb-6 sm:px-6 text-slate-300 text-sm sm:text-base leading-relaxed border-t border-slate-800/80 pt-4 font-mono text-xs sm:text-sm">
                    <p className="font-sans text-slate-300 leading-relaxed">{faq.answer}</p>
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {/* FAQ Help Callout */}
        <div className="p-6 rounded-xl bg-gradient-to-r from-[#180f14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/40 text-center flex flex-col sm:flex-row items-center justify-between gap-4 shadow-xl">
          <div className="text-left">
            <p className="font-bold text-white text-base">Ready to uncover your first creator leads?</p>
            <p className="text-xs text-slate-400 font-mono">No credit card required. Instant access in under 2 minutes.</p>
          </div>
          <button
            onClick={onOpenTrial}
            className="px-6 py-3 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] whitespace-nowrap shadow-[0_0_15px_rgba(255,0,59,0.3)] cursor-pointer transition-colors border border-[#ff4d73]/40"
          >
            Start Your Free Trial â†’
          </button>
        </div>
      </div>
    </section>
  );
};
