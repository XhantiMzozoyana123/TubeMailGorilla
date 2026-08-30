import React, { useEffect, useRef, useState } from 'react';
import extractDemoVideo from '../videos/tubemailgorilla-app-demo-2.mp4';
import { Search, Mail, Bot, BarChart3, ArrowRight, ShieldCheck, Zap } from 'lucide-react';

/**
 * FeaturesShowcase
 * A visual, demo-driven "Features & Benefits" section inspired by the TubeMailGorilla MAUI app:
 *  - Extract tab    -> three.js 3D lead-network demo (creators orbiting into your pipeline)
 *  - Email Templates -> AI pitch typewriter demo
 *  - Send Emails     -> animated bulk-sending progress demo
 *  - Benefits        -> live chart.js graphs (replies over time + hours saved)
 */

/* ------------------------------------------------------------------ */
/* ------------------------------------------------------------------ */
/* Feature 01: The Extract Engine - live demo video                  */
/* ------------------------------------------------------------------ */
const ExtractHeroVideo: React.FC = () => {
  return (
    <video
      className="w-full h-[280px] sm:h-[340px] object-fill"
      src={extractDemoVideo}
      autoPlay
      muted
      loop
      playsInline
    >
      <p className="p-4 text-slate-300">Your browser does not support the video. <a className="text-[#ff4d73] underline" href={extractDemoVideo} download>Download the demo</a> instead.</p>
    </video>
  );
};

/* Demo 1: Extraction pipeline ticker                                  */
/* ------------------------------------------------------------------ */
const CHANNELS = [
  '@motionlab.studio', '@editforge', '@pixelrush', '@cutscenepro',
  '@framebyframe', '@renderhouse', '@loopcinema', '@keyframemedia',
];

const ExtractDemo: React.FC = () => {
  const [found, setFound] = useState<string[]>([]);
  const [count, setCount] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setFound((prev) => [...prev.slice(-4), CHANNELS[Math.floor(Math.random() * CHANNELS.length)]]);
      setCount((c) => c + Math.floor(Math.random() * 7) + 3);
    }, 900);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="bg-[#0a0c13] border border-slate-800 rounded-xl p-4 font-mono text-xs h-full flex flex-col">
      <div className="flex items-center gap-2 mb-3 text-slate-400 uppercase tracking-wider text-[10px]">
        <span className="h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
        extraction · keyword: “video editor” · pages: 5
      </div>
      <div className="flex-1 space-y-1 overflow-hidden">
        {found.map((c, i) => (
          <div key={`${c}-${i}`} className="flex items-center justify-between text-slate-300">
            <span className="flex items-center gap-2"><span className="text-[#ff003b]">▶</span> {c}</span>
            <span className="text-emerald-400">✓ email found</span>
          </div>
        ))}
      </div>
      <div className="mt-3 pt-3 border-t border-slate-800 flex items-center justify-between">
        <span className="text-slate-500 uppercase tracking-wider">contacts extracted</span>
        <span className="text-lg font-extrabold text-[#ff003b]">{count}</span>
      </div>
    </div>
  );
};

/* ------------------------------------------------------------------ */
/* Demo 2: AI pitch generator typewriter                               */
/* ------------------------------------------------------------------ */
const PITCH_TEXT =
  'Hey Alex — your recent DaVinci breakdown was insane. I edit for finance creators and think a collab could land you 2–3 retainers this quarter. Want my 90-sec showreel?';

const AiPitchDemo: React.FC = () => {
  const [chars, setChars] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setChars((c) => (c >= PITCH_TEXT.length ? 0 : c + 1));
    }, 34);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="bg-[#0a0c13] border border-slate-800 rounded-xl p-4 h-full flex flex-col">
      <div className="flex items-center gap-2 mb-3 text-[10px] font-mono uppercase tracking-wider text-slate-400">
        <Bot className="w-3.5 h-3.5 text-[#ff003b]" />
        AI pitch writer
        <span className="ml-auto px-1.5 py-0.5 rounded bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73]">generating…</span>
      </div>
      <p className="flex-1 text-sm text-slate-200 leading-relaxed font-mono min-h-[110px]">
        {PITCH_TEXT.slice(0, chars)}
        <span className="text-[#ff003b] animate-pulse">▌</span>
      </p>
      <div className="mt-3 pt-3 border-t border-slate-800 grid grid-cols-3 gap-2 text-center font-mono text-[10px] uppercase tracking-wider">
        <div><p className="text-emerald-400 font-bold text-sm">98%</p><p className="text-slate-500">personalized</p></div>
        <div><p className="text-white font-bold text-sm">1-click</p><p className="text-slate-500">variants</p></div>
        <div><p className="text-[#ff003b] font-bold text-sm">0s</p><p className="text-slate-500">to draft</p></div>
      </div>
    </div>
  );
};

/* ------------------------------------------------------------------ */
/* Demo 3: Bulk sending progress                                       */
/* ------------------------------------------------------------------ */
const SendDemo: React.FC = () => {
  const total = 48;
  const [sent, setSent] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setSent((s) => (s >= total ? 0 : s + 1));
    }, 220);
    return () => clearInterval(interval);
  }, []);

  const pct = Math.round((sent / total) * 100);

  return (
    <div className="bg-[#0a0c13] border border-slate-800 rounded-xl p-4 h-full flex flex-col">
      <div className="flex items-center gap-2 mb-3 text-[10px] font-mono uppercase tracking-wider text-slate-400">
        <Mail className="w-3.5 h-3.5 text-[#ff003b]" />
        bulk campaign · throttling on
      </div>
      <div className="flex-1 space-y-2.5">
        {[
          { label: 'Delivered', value: sent, color: '#34d399' },
          { label: 'Opened', value: Math.round(sent * 0.62), color: '#ff4d73' },
          { label: 'Replied', value: Math.round(sent * 0.21), color: '#8b5cf6' },
        ].map((row) => (
          <div key={row.label}>
            <div className="flex justify-between text-[11px] font-mono mb-1">
              <span className="text-slate-400">{row.label}</span>
              <span className="text-slate-200">{row.value}</span>
            </div>
            <div className="h-1.5 bg-[#141724] rounded-full overflow-hidden">
              <div
                className="h-full rounded-full transition-all duration-200"
                style={{ width: `${(row.value / total) * 100}%`, background: row.color }}
              />
            </div>
          </div>
        ))}
      </div>
      <div className="mt-3 pt-3 border-t border-slate-800 flex items-center justify-between font-mono text-[10px] uppercase tracking-wider">
        <span className="text-slate-500">{pct}% campaign complete</span>
        <Zap className="w-3.5 h-3.5 text-yellow-400" />
      </div>
    </div>
  );
};

/* ------------------------------------------------------------------ */
/* Chart.js benefit graphs                                             */
/* ------------------------------------------------------------------ */
const BenefitCharts: React.FC = () => {
  const lineRef = useRef<HTMLCanvasElement>(null);
  const doughnutRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    let disposed = false;
    let chartCleanup = () => {};

    (async () => {
      const { Chart, LineController, LineElement, PointElement, LinearScale, CategoryScale, Filler, Tooltip, DoughnutController, ArcElement } =
        await import('chart.js');
      if (disposed) return;

      Chart.register(LineController, LineElement, PointElement, LinearScale, CategoryScale, Filler, Tooltip, DoughnutController, ArcElement);

      const charts: any[] = [];

      if (lineRef.current) {
        charts.push(new Chart(lineRef.current, {
          type: 'line',
          data: {
            labels: ['Wk 1', 'Wk 2', 'Wk 3', 'Wk 4', 'Wk 5', 'Wk 6'],
            datasets: [
              {
                label: 'Pitches sent',
                data: [12, 30, 52, 78, 110, 148],
                borderColor: '#334155',
                backgroundColor: 'rgba(51,65,85,0.08)',
                fill: true,
                tension: 0.4,
                pointRadius: 0,
              },
              {
                label: 'Replies booked',
                data: [2, 6, 13, 22, 37, 54],
                borderColor: '#ff003b',
                backgroundColor: 'rgba(255,0,59,0.16)',
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                borderWidth: 2.5,
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { labels: { color: '#94a3b8', font: { family: 'monospace', size: 10 }, boxWidth: 12 } },
              tooltip: { backgroundColor: '#141724', borderColor: '#ff003b', borderWidth: 1 },
            },
            scales: {
              x: { ticks: { color: '#64748b', font: { family: 'monospace', size: 10 } }, grid: { color: 'rgba(51,65,85,0.3)' } },
              y: { ticks: { color: '#64748b', font: { family: 'monospace', size: 10 } }, grid: { color: 'rgba(51,65,85,0.3)' } },
            },
            animation: { duration: 1600, easing: 'easeOutQuart' as any },
          },
        }));
      }

      if (doughnutRef.current) {
        charts.push(new Chart(doughnutRef.current, {
          type: 'doughnut',
          data: {
            labels: ['Editing billable work', 'Lead hunting (before)', 'With TubeMail Gorilla'],
            datasets: [{
              data: [72, 22, 6],
              backgroundColor: ['#1e293b', '#7f1024', '#ff003b'],
              borderColor: '#0a0c13',
              borderWidth: 3,
              hoverOffset: 6,
            }],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '68%',
            plugins: {
              legend: { position: 'bottom', labels: { color: '#94a3b8', font: { family: 'monospace', size: 9 }, boxWidth: 10, padding: 8 } },
              tooltip: { backgroundColor: '#141724', borderColor: '#ff003b', borderWidth: 1 },
            },
            animation: { animateRotate: true, duration: 1600, easing: 'easeOutQuart' as any },
          },
        }));
      }

      chartCleanup = () => charts.forEach((c) => c.destroy());
    })();

    return () => {
      disposed = true;
      chartCleanup();
    };
  }, []);

  return (
    <div className="grid md:grid-cols-2 gap-4">
      <div className="bg-[#0a0c13] border border-slate-800 rounded-xl p-4">
        <div className="flex items-center gap-2 mb-3 text-[10px] font-mono uppercase tracking-wider text-slate-400">
          <BarChart3 className="w-3.5 h-3.5 text-[#ff003b]" />
          Pipeline growth · 6 weeks
        </div>
        <div className="h-[220px]"><canvas ref={lineRef} /></div>
      </div>
      <div className="bg-[#0a0c13] border border-slate-800 rounded-xl p-4">
        <div className="flex items-center gap-2 mb-3 text-[10px] font-mono uppercase tracking-wider text-slate-400">
          <BarChart3 className="w-3.5 h-3.5 text-[#ff003b]" />
          Where your week goes
        </div>
        <div className="h-[220px] relative">
          <canvas ref={doughnutRef} />
          <p className="absolute inset-x-0 top-[36%] text-center pointer-events-none">
            <span className="block text-2xl font-extrabold text-white font-['Outfit',sans-serif]">+22h</span>
            <span className="block text-[10px] font-mono uppercase tracking-wider text-slate-500">back to editing / week</span>
          </p>
        </div>
      </div>
    </div>
  );
};

/* ------------------------------------------------------------------ */
/* Section                                                             */
/* ------------------------------------------------------------------ */
interface FeaturesShowcaseProps {
  onOpenTrial: () => void;
}

export const FeaturesShowcase: React.FC<FeaturesShowcaseProps> = ({ onOpenTrial }) => {
  return (
    <section id="features-showcase" className="relative py-20 md:py-28 bg-[#07080b] overflow-hidden">
      {/* Ambient glows + grid */}
      <div className="absolute top-0 left-1/4 w-[500px] h-[350px] bg-[#ff003b]/10 blur-[150px] rounded-full pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-[450px] h-[300px] bg-[#990024]/15 blur-[140px] rounded-full pointer-events-none" />
      <div className="absolute inset-0 gorilla-grid-pattern opacity-40 pointer-events-none" />

      <div className="relative max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="text-center mb-14">
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold mb-6 shadow-[0_0_15px_rgba(255,0,59,0.2)]">
            <span className="flex h-2 w-2 rounded-full bg-[#ff003b] animate-ping" />
            <span className="uppercase tracking-wider">SEE IT WORK · NO SLIDES, JUST DEMOS</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold tracking-tight text-white font-['Outfit',sans-serif] leading-tight mb-4">
            Everything between{' '}
            <span className="text-[#ff003b]">“I need clients”</span>{' '}
            and <span className="text-[#ff003b]">“signed”</span>.
          </h2>
          <p className="max-w-2xl mx-auto text-slate-400 text-base sm:text-lg">
            Watch each part of the machine run — live.
          </p>
        </div>

        {/* Row 1: three.js hero visual */}
        <div className="grid lg:grid-cols-2 gap-8 items-center mb-14">
          <div className="order-2 lg:order-1">
            <div className="inline-flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-widest mb-3">
              <Search className="w-4 h-4" /> Feature 01 — The Extract Engine
            </div>
            <h3 className="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit',sans-serif] mb-3">
              YouTube’s creators, orbiting <span className="text-[#ff003b]">your</span> pipeline.
            </h3>
            <p className="text-slate-400 leading-relaxed mb-5">
              One keyword pulls channels, videos and contact details straight into your CRM. Every glowing node is a
              creator being captured — watch them stream into the hub in real time.
            </p>
            <ul className="space-y-2 text-sm text-slate-300">
              <li className="flex items-center gap-2"><ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" /> Verified emails & socials, deduped automatically</li>
              <li className="flex items-center gap-2"><ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" /> Channel-level filtering by niche & size</li>
              <li className="flex items-center gap-2"><ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" /> Saved straight to Contacts — no spreadsheets</li>
            </ul>
          </div>
          <div className="order-1 lg:order-2 rounded-2xl border border-slate-800 bg-gradient-to-b from-[#0f1118] to-[#0a0c13] shadow-[0_0_50px_rgba(255,0,59,0.08)] overflow-hidden">
            <ExtractHeroVideo />
          </div>
        </div>

        {/* Row 2: three live demos */}
        <div className="grid md:grid-cols-3 gap-5 mb-14">
          <div className="group flex flex-col">
            <div className="flex-1"><ExtractDemo /></div>
            <p className="mt-3 text-xs font-mono uppercase tracking-wider text-slate-500 group-hover:text-[#ff4d73] transition-colors">
              ▶ Live: scraping & extracting contacts
            </p>
          </div>
          <div className="group flex flex-col">
            <div className="flex-1"><AiPitchDemo /></div>
            <p className="mt-3 text-xs font-mono uppercase tracking-wider text-slate-500 group-hover:text-[#ff4d73] transition-colors">
              ▶ Live: AI writing your pitch
            </p>
          </div>
          <div className="group flex flex-col">
            <div className="flex-1"><SendDemo /></div>
            <p className="mt-3 text-xs font-mono uppercase tracking-wider text-slate-500 group-hover:text-[#ff4d73] transition-colors">
              ▶ Live: campaign sending safely
            </p>
          </div>
        </div>

        {/* Row 3: chart.js benefits */}
        <div className="mb-14">
          <div className="text-center mb-8">
            <div className="inline-flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-widest mb-2">
              <BarChart3 className="w-4 h-4" /> The Benefits — Measured
            </div>
            <h3 className="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit',sans-serif]">
              More replies. Less hunting. Same you, minus the busywork.
            </h3>
          </div>
          <BenefitCharts />
        </div>

        {/* CTA */}
        <div className="text-center">
          <button
            onClick={onOpenTrial}
            className="px-8 py-4 rounded-lg bg-gradient-to-r from-[#ff003b] via-[#e60028] to-[#b3001f] hover:from-[#ff1a4b] hover:to-[#e60028] text-white font-extrabold text-lg uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_30px_rgba(255,0,59,0.4)] hover:shadow-[0_0_40px_rgba(255,0,59,0.6)] hover:-translate-y-0.5 border border-[#ff4d73]/60 transition-all duration-200 inline-flex items-center gap-3 cursor-pointer group"
          >
            <span>RUN YOUR FIRST EXTRACTION FREE</span>
            <ArrowRight className="w-5 h-5 group-hover:translate-x-1.5 transition-transform" />
          </button>
        </div>
      </div>
    </section>
  );
};


