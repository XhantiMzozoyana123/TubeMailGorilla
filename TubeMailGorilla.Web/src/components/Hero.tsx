import React from 'react';
import { ArrowRight, Search, Mail, Sparkles, Youtube, ShieldCheck, Zap } from 'lucide-react';
import demoVideo from '../videos/tubemailgorilla-app-demo-1.mp4';

interface HeroProps {
  onOpenTrial: () => void;
  onScrollToDemo: () => void;
}

export const Hero: React.FC<HeroProps> = ({ onOpenTrial, onScrollToDemo }) => {
  return (
    <section className="relative pt-32 pb-20 md:pt-40 md:pb-28 overflow-hidden bg-[#07080b]" id="hero-section">
      {/* Crimson Energy Background Glows */}
      <div className="absolute top-1/4 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[650px] h-[380px] bg-[#ff003b]/15 blur-[140px] rounded-full pointer-events-none" />
      <div className="absolute top-1/3 left-1/4 w-[450px] h-[300px] bg-[#990024]/20 blur-[150px] rounded-full pointer-events-none" />
      <div className="absolute top-1/2 right-1/4 w-[400px] h-[300px] bg-[#ff003b]/10 blur-[130px] rounded-full pointer-events-none" />

      {/* Carbon Matrix Grid background */}
      <div className="absolute inset-0 gorilla-grid-pattern opacity-60 pointer-events-none" />

      <div className="relative max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                {/* Top Tag / Pill */}
        <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs sm:text-sm font-mono font-bold mb-8 shadow-[0_0_15px_rgba(255,0,59,0.2)]">
          <span className="flex h-2 w-2 rounded-full bg-[#ff003b] animate-ping" />
          <span className="uppercase tracking-wider">FOR VIDEO EDITORS WHO'D RATHER EDIT THAN HUNT</span>
        </div>

        {/* Main Headline */}
        <h1 className="text-4xl sm:text-5xl lg:text-6xl font-extrabold tracking-tight text-white font-['Outfit',sans-serif] leading-[1.08] mb-6">
          Your Next Video Editing Client <br />
          Is Already on YouTube.
        </h1>

        {/* Subtitle */}
        <p className="max-w-3xl mx-auto text-lg sm:text-xl text-slate-300 leading-relaxed mb-10">
          TubeMail Gorilla finds YouTube creators, extracts their business contact info, and helps you send personalized pitches — so you can spend less time hunting for clients and more time editing.
        </p>

        {/* 4 Pillars Mini-Workflow Steps with Tech Frames */}
        <div className="max-w-3xl mx-auto grid grid-cols-2 sm:grid-cols-4 gap-3 mb-10 text-left">
          <div className="bg-[#0f1118]/90 border border-slate-800 hover:border-[#ff003b]/60 rounded-xl p-3.5 backdrop-blur-sm transition-all duration-200 group hover:shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-1">
              <Search className="w-3.5 h-3.5" />
              <span>Step 01</span>
            </div>
            <p className="text-sm font-bold text-slate-200 group-hover:text-white">Search YouTube</p>
          </div>

          <div className="bg-[#0f1118]/90 border border-slate-800 hover:border-[#ff003b]/60 rounded-xl p-3.5 backdrop-blur-sm transition-all duration-200 group hover:shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-1">
              <Zap className="w-3.5 h-3.5" />
              <span>Step 02</span>
            </div>
            <p className="text-sm font-bold text-slate-200 group-hover:text-white">Extract leads</p>
          </div>

          <div className="bg-[#0f1118]/90 border border-slate-800 hover:border-[#ff003b]/60 rounded-xl p-3.5 backdrop-blur-sm transition-all duration-200 group hover:shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-1">
              <Youtube className="w-3.5 h-3.5" />
              <span>Step 03</span>
            </div>
            <p className="text-sm font-bold text-slate-200 group-hover:text-white">Build client list</p>
          </div>

          <div className="bg-[#0f1118]/90 border border-slate-800 hover:border-[#ff003b]/60 rounded-xl p-3.5 backdrop-blur-sm transition-all duration-200 group hover:shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <div className="flex items-center gap-2 text-[#ff003b] text-xs font-mono font-bold uppercase tracking-wider mb-1">
              <Mail className="w-3.5 h-3.5" />
              <span>Step 04</span>
            </div>
            <p className="text-sm font-bold text-slate-200 group-hover:text-white">Start reaching out</p>
          </div>
        </div>

        {/* Primary CTA */}
        <div className="flex flex-col items-center justify-center gap-4 mb-5">
          <button
            onClick={onOpenTrial}
            className="w-full sm:w-auto px-10 py-5 rounded-xl bg-gradient-to-r from-[#ff003b] via-[#e60028] to-[#b3001f] hover:from-[#ff1a4b] hover:to-[#e60028] text-white font-extrabold text-xl uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_40px_rgba(255,0,59,0.4)] hover:shadow-[0_0_50px_rgba(255,0,59,0.6)] hover:-translate-y-1 border border-[#ff4d73]/60 transition-all duration-200 inline-flex items-center justify-center gap-3 group"
            id="hero-cta-trial"
          >
            <Sparkles className="w-6 h-6" />
            <span>Find My First Leads Free</span>
          </button>

          <button
            onClick={onScrollToDemo}
            className="underline-offset-4 hover:underline text-slate-300 hover:text-[#ff4d73] font-semibold text-sm font-['Rajdhani',sans-serif] uppercase tracking-wider transition-colors flex items-center gap-2 cursor-pointer py-1"
            id="hero-cta-demo"
          >
            <span>see it work</span>
            <ArrowRight className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
          </button>
        </div>

        {/* Reassurance text */}
        <p className="text-sm text-slate-400 flex items-center justify-center gap-2 font-medium">
          <ShieldCheck className="w-4 h-4 text-emerald-400" />
          <span>No credit card. Get your first creator leads in minutes.</span>
        </p>

        {/* Product demo video */}
        <div className="max-w-4xl mx-auto mt-12">
          <div className="relative rounded-2xl overflow-hidden border border-[#ff003b]/30 shadow-[0_0_40px_rgba(255,0,59,0.18)] bg-[#0f1118]">
            <video
              className="w-full h-auto block"
              src={demoVideo}
              autoPlay
              muted
              loop
              playsInline
              poster=""
            >
              <p className="p-4 text-slate-300">Your browser doesn't support the video. <a className="text-[#ff4d73] underline" href={demoVideo} download>Download the demo</a> instead.</p>
            </video>
          </div>
          <p className="mt-3 text-xs sm:text-sm text-slate-400 flex items-center justify-center gap-2 font-medium">
            <Sparkles className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>See how TubeMail Gorilla searches YouTube, extracts leads and generates your outreach in seconds.</span>
          </p>
        </div>

        {/* Trust Badges — capability-focused, no income claims */}
        <div className="mt-14 pt-8 border-t border-slate-800/90 grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
          <div className="p-3 rounded-xl bg-[#0e1017]/80 border border-slate-800/80">
            <p className="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit',sans-serif]">Real-Time</p>
            <p className="text-xs text-slate-400 mt-1 uppercase font-mono tracking-wider font-semibold">Creator Search</p>
          </div>
          <div className="p-3 rounded-xl bg-[#0e1017]/80 border border-[#ff003b]/30 shadow-[0_0_15px_rgba(255,0,59,0.1)]">
            <p className="text-2xl sm:text-3xl font-extrabold text-[#ff003b] font-['Outfit',sans-serif]">Verified</p>
            <p className="text-xs text-slate-400 mt-1 uppercase font-mono tracking-wider font-semibold">Business Emails</p>
          </div>
          <div className="p-3 rounded-xl bg-[#0e1017]/80 border border-slate-800/80">
            <p className="text-2xl sm:text-3xl font-extrabold text-emerald-400 font-['Outfit',sans-serif]">AI-Powered</p>
            <p className="text-xs text-slate-400 mt-1 uppercase font-mono tracking-wider font-semibold">Personalized Pitches</p>
          </div>
          <div className="p-3 rounded-xl bg-[#0e1017]/80 border border-slate-800/80">
            <p className="text-2xl sm:text-3xl font-extrabold text-white font-['Outfit',sans-serif]">100% Direct</p>
            <p className="text-xs text-slate-400 mt-1 uppercase font-mono tracking-wider font-semibold">Zero Platform Fees</p>
          </div>
        </div>
      </div>
    </section>
  );
};

