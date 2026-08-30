import React, { useState } from 'react';
import { X, CheckCircle2, ArrowRight, ShieldCheck, Sparkles, Youtube, Zap, Lock } from 'lucide-react';
import confetti from 'canvas-confetti';

interface FreeTrialModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const FreeTrialModal: React.FC<FreeTrialModalProps> = ({ isOpen, onClose }) => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [software, setSoftware] = useState('Adobe Premiere Pro');
  const [niche, setNiche] = useState('Gaming & Tech');
  const [isSuccess, setIsSuccess] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsSuccess(true);
    confetti({
      particleCount: 75,
      spread: 80,
      origin: { y: 0.6 },
      colors: ['#ff003b', '#ff4d73', '#ffffff', '#111827']
    });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="bg-[#0b0d14] border border-[#ff003b]/40 rounded-2xl w-full max-w-lg shadow-[0_0_50px_rgba(255,0,59,0.25)] overflow-hidden relative">
        {/* Close Button */}
        <button
          onClick={onClose}
          className="absolute top-4 right-4 p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer z-10 border border-slate-700"
        >
          <X className="w-5 h-5" />
        </button>

        {!isSuccess ? (
          <div className="p-6 sm:p-8">
            {/* Header */}
            <div className="text-center mb-6">
                            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-[#ff003b] to-[#a30026] flex items-center justify-center text-2xl mx-auto mb-3 shadow-[0_0_20px_rgba(255,0,59,0.4)] border border-[#ff4d73]/40">
                                <img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-8 h-8 object-contain" />
              </div>
              <h3 className="text-2xl font-extrabold text-white font-['Rajdhani',sans-serif] uppercase tracking-wider">
                Start Your Free Trial
              </h3>
              <p className="text-xs text-slate-300 mt-1 font-mono">
                No complicated setup. Find your first leads in minutes.
              </p>
            </div>

            {/* Form */}
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="text-xs font-bold text-slate-300 block mb-1.5 font-mono uppercase text-[11px]">Your Name</label>
                <input
                  type="text"
                  required
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g. Jordan Miller"
                  className="w-full px-3.5 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:border-[#ff003b] focus:ring-1 focus:ring-[#ff003b] transition-colors font-mono"
                />
              </div>

              <div>
                <label className="text-xs font-bold text-slate-300 block mb-1.5 font-mono uppercase text-[11px]">Email Address</label>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="jordan@edits.com"
                  className="w-full px-3.5 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:border-[#ff003b] focus:ring-1 focus:ring-[#ff003b] transition-colors font-mono"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-xs font-bold text-slate-300 block mb-1.5 font-mono uppercase text-[11px]">Primary Software</label>
                  <select
                    value={software}
                    onChange={(e) => setSoftware(e.target.value)}
                    className="w-full px-3 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-xs text-white focus:outline-none focus:border-[#ff003b] font-mono"
                  >
                    <option value="Adobe Premiere Pro">Premiere Pro</option>
                    <option value="DaVinci Resolve">DaVinci Resolve</option>
                    <option value="After Effects">After Effects</option>
                    <option value="Final Cut Pro">Final Cut Pro</option>
                    <option value="CapCut Desktop">CapCut Desktop</option>
                  </select>
                </div>

                <div>
                  <label className="text-xs font-bold text-slate-300 block mb-1.5 font-mono uppercase text-[11px]">Ideal Client Niche</label>
                  <select
                    value={niche}
                    onChange={(e) => setNiche(e.target.value)}
                    className="w-full px-3 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-xs text-white focus:outline-none focus:border-[#ff003b] font-mono"
                  >
                    <option value="Gaming & Tech">Gaming & Tech</option>
                    <option value="Finance & Business">Finance & Business</option>
                    <option value="Fitness & Health">Fitness & Health</option>
                    <option value="Vlogs & Lifestyle">Vlogs & Lifestyle</option>
                    <option value="Documentaries">Documentaries</option>
                    <option value="Podcasts & Shorts">Podcasts & Shorts</option>
                  </select>
                </div>
              </div>

              {/* Free Trial Value Points */}
              <div className="p-3.5 rounded-xl bg-[#050608] border border-slate-800 space-y-1.5 text-xs text-slate-300 font-mono">
                <div className="flex items-center gap-2">
                  <CheckCircle2 className="w-3.5 h-3.5 text-[#ff003b] shrink-0" />
                  <span className="font-sans">Unlimited YouTube creator niche searches</span>
                </div>
                <div className="flex items-center gap-2">
                  <CheckCircle2 className="w-3.5 h-3.5 text-[#ff003b] shrink-0" />
                  <span className="font-sans">100 free verified creator email extractions</span>
                </div>
                <div className="flex items-center gap-2">
                  <CheckCircle2 className="w-3.5 h-3.5 text-[#ff003b] shrink-0" />
                  <span className="font-sans">Instant access to cold email pitch templates</span>
                </div>
              </div>

              <button
                type="submit"
                className="w-full py-3.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.4)] transition-all flex items-center justify-center gap-2 cursor-pointer mt-2 border border-[#ff4d73]"
              >
                <span>CREATE MY ACCOUNT & START TRIAL →</span>
              </button>
            </form>

            <p className="text-[11px] text-slate-400 text-center mt-4 flex items-center justify-center gap-1.5 font-mono">
              <Lock className="w-3 h-3 text-slate-500" />
              <span>No credit card required. Cancel anytime.</span>
            </p>
          </div>
        ) : (
          <div className="p-8 text-center space-y-5">
            <div className="w-16 h-16 rounded-full bg-[#1b0e14] text-[#ff003b] flex items-center justify-center text-3xl mx-auto border border-[#ff003b]/40 shadow-[0_0_20px_rgba(255,0,59,0.3)] animate-bounce">
              ⚡
            </div>
            <h3 className="text-2xl font-extrabold text-white font-['Rajdhani',sans-serif] uppercase tracking-wider">
              Welcome to TubeMail Gorilla, {name || 'Editor'}!
            </h3>
            <p className="text-sm text-slate-300 leading-relaxed font-mono text-xs">
              Your free trial has been activated. You now have full access to the YouTube creator lead search engine for <strong className="text-[#ff4d73]">{niche}</strong>.
            </p>

            <div className="p-4 rounded-xl bg-[#050608] border border-slate-800 text-left text-xs space-y-2 text-slate-300 font-mono">
              <p className="font-bold text-[#ff4d73] flex items-center gap-2">
                <span>⚡ Quick Start Checklist:</span>
              </p>
              <p>1. Use the Search Console to find 10 creators in your niche.</p>
              <p>2. Extract their emails and review their editing pain points.</p>
              <p>3. Send a tailored 30-second sample cut using our cold pitch scripts.</p>
            </div>

            <button
              onClick={onClose}
              className="w-full py-3.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.4)] transition-colors cursor-pointer border border-[#ff4d73]"
            >
              Start Searching Channels Now →
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
