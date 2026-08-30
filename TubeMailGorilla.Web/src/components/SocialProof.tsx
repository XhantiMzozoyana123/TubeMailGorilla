import React from 'react';
import { ArrowRight, Youtube, Mail, Users, TrendingUp } from 'lucide-react';

interface SocialProofProps {
  onOpenTrial: () => void;
}

const BETA_RESULTS = [
  {
    initial: 'M',
    name: 'Marcus R., Gaming Editor',
    timeframe: 'Beta tester â€” 3 days in',
    found: '42', extracted: '31', sent: '28', replies: '9',
    quote: '"Found 31 emails in under 20 minutes. Got 3 editing consultations scheduled. One client at $400/mo."',
  },
  {
    initial: 'J',
    name: 'Jenna K., Finance Editor',
    timeframe: 'Beta tester â€” 1 week in',
    found: '87', extracted: '64', sent: '52', replies: '11',
    quote: '"The AI pitch referencing my target\'s actual videos got replies I never got with generic templates. Two calls booked, one retainer signed."',
  },
  {
    initial: 'D',
    name: 'Derek L., Vlog Editor',
    timeframe: 'Beta tester â€” 2 weeks in',
    found: '156', extracted: '93', sent: '78', replies: '22',
    quote: '"Batch-extracted 93 emails across 3 niches. 3 retainer offers accepted so far. Pipeline is growing every week."',
  },
];

const FLOW = [
  { icon: <Youtube className="w-5 h-5 text-[#ff003b]" />, num: '1', label: 'Enter keyword', sub: 'e.g. "video editor needed"' },
  { icon: <Users className="w-5 h-5 text-[#ff003b]" />, num: '538', label: 'Creators found', sub: 'Across 5 pages of results' },
  { icon: <Mail className="w-5 h-5 text-[#ff003b]" />, num: '217', label: 'Emails extracted', sub: 'Business contact verified' },
];

export const SocialProof: React.FC<SocialProofProps> = ({ onOpenTrial }) => {
  return (
    <section className="py-24 bg-[#08090d] relative border-t border-slate-800/80" id="results-and-proof">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <TrendingUp className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>BETA RESULTS FROM REAL EDITORS</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            Real editors. Real YouTube data.
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            These are the results from editors in our private beta who searched, extracted, and pitched creator channels in their first few sessions.
          </p>
        </div>

        {/* Process Flow */}
        <div className="mb-16 grid grid-cols-1 md:grid-cols-5 gap-4 items-stretch text-center">
          {FLOW.map((step, i) => (
            <React.Fragment key={step.label}>
              <div className="bg-[#0f1118] border border-slate-800 rounded-xl p-6 flex flex-col">
                <div className="w-12 h-12 rounded-full bg-[#ff003b]/20 border border-[#ff003b]/40 flex items-center justify-center mx-auto mb-4">
                  {step.icon}
                </div>
                <p className="text-3xl font-extrabold text-white font-['Rajdhani',sans-serif] mb-1">{step.num}</p>
                <p className="text-xs text-slate-400 font-mono uppercase tracking-wider">{step.label}</p>
                <p className="text-sm text-slate-300 mt-2 font-sans">{step.sub}</p>
              </div>
              {i < FLOW.length - 1 && (
                <div className="flex items-center justify-center text-[#ff003b] rotate-90 md:rotate-0">
                  <ArrowRight className="w-6 h-6" />
                </div>
              )}
            </React.Fragment>
          ))}
        </div>
        {/* Beta Results Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-16">
          {BETA_RESULTS.map((r) => (
            <div key={r.name} className="bg-gradient-to-br from-[#160f14] via-[#0f1118] to-[#0f1118] border border-[#ff003b]/30 rounded-2xl p-6 shadow-[0_0_25px_rgba(255,0,59,0.1)] flex flex-col">
              <div className="flex items-center gap-3 mb-4">
                <div className="w-10 h-10 rounded-full bg-slate-700 flex items-center justify-center text-white font-bold text-sm">{r.initial}</div>
                <div>
                  <p className="font-bold text-white font-['Outfit',sans-serif]">{r.name}</p>
                  <p className="text-xs text-slate-500">{r.timeframe}</p>
                </div>
              </div>
              <div className="space-y-3 text-xs font-mono mb-4">
                <div className="flex justify-between"><span className="text-slate-400">Creators found</span><span className="text-white font-bold">{r.found}</span></div>
                <div className="flex justify-between"><span className="text-slate-400">Emails extracted</span><span className="text-white font-bold">{r.extracted}</span></div>
                <div className="flex justify-between"><span className="text-slate-400">Pitches sent</span><span className="text-white font-bold">{r.sent}</span></div>
                <div className="flex justify-between border-t border-slate-800 pt-2"><span className="text-slate-400">Replies received</span><span className="text-emerald-400 font-bold">{r.replies}</span></div>
              </div>
              <p className="text-xs text-slate-300 font-sans italic mt-auto">{r.quote}</p>
            </div>
          ))}
        </div>

        {/* CTA */}
        <div className="text-center">
          <button
            onClick={onOpenTrial}
            className="px-10 py-4 rounded-xl bg-gradient-to-r from-[#ff003b] via-[#e60028] to-[#b3001f] hover:from-[#ff1a4b] hover:to-[#e60028] text-white font-extrabold text-lg uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_30px_rgba(255,0,59,0.4)] hover:shadow-[0_0_40px_rgba(255,0,59,0.6)] hover:-translate-y-0.5 transition-all duration-200 inline-flex items-center gap-3 cursor-pointer border border-[#ff4d73]/60"
            id="socialproof-cta"
          >
            <span>Find My First Leads Free</span>
            <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
          </button>
          <p className="mt-3 text-xs text-slate-400 font-mono">
            No credit card. No GPU required. Get your first creator leads in minutes.
          </p>
        </div>

      </div>
    </section>
  );
};
