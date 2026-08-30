import React from 'react';
import { Lock, HardDrive, Shield, Server, CheckCircle2 } from 'lucide-react';

export const SecureAndPrivate: React.FC = () => {
  return (
    <section className="py-24 bg-[#08090d] relative" id="security-and-privacy">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#0e4490]/40 text-[#4da3ff] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(77,163,255,0.15)]">
            <Shield className="w-3.5 h-3.5 text-[#4da3ff]" />
            <span>BUILT AROUND YOUR PRIVACY</span>
          </div>
          <h2 className="text-3xl sm:text-5xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4 leading-tight">
            Your data, your hardware — <br />
            <span className="text-[#4da3ff]">we never see a thing.</span>
          </h2>
          <p className="text-slate-300 text-lg sm:text-xl">
            TubeMail Gorilla keeps your leads and AI workloads on your device. The security model came first.
          </p>
        </div>

        {/* Two core pillars */ }
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-16">
          <div className="bg-[#0f1118] border border-slate-800 rounded-2xl p-7 flex flex-col gap-4 shadow-xl">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-xl bg-[#151824] border border-slate-700 flex items-center justify-center">
                <HardDrive className="w-5 h-5 text-[#4da3ff]" />
              </div>
              <h3 className="text-xl font-bold text-white font-['Outfit',sans-serif]">Leads live on your laptop</h3>
            </div>
                                    <p className="text-slate-300 text-sm leading-relaxed">
              Stored locally on your machine. Never uploaded — delete the app and it goes with it.
            </p>
            <ul className="mt-auto flex flex-wrap gap-3 text-xs">
              <li className="flex items-center gap-1.5 bg-[#141724] border border-slate-800 rounded-full px-3 py-1">
                <CheckCircle2 className="w-3.5 h-3.5 text-[#2E9E5B]" /> Local storage
              </li>
              <li className="flex items-center gap-1.5 bg-[#141724] border border-slate-800 rounded-full px-3 py-1">
                <CheckCircle2 className="w-3.5 h-3.5 text-[#2E9E5B]" /> No sync to cloud
              </li>
            </ul>
          </div>

          <div className="bg-[#0f1118] border border-slate-800 rounded-2xl p-7 flex flex-col gap-4 shadow-xl">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-xl bg-[#151824] border border-slate-700 flex items-center justify-center">
                <Server className="w-5 h-5 text-[#4da3ff]" />
              </div>
              <h3 className="text-xl font-bold text-white font-['Outfit',sans-serif]">AI runs on your GPU</h3>
            </div>
                        <p className="text-slate-300 text-sm leading-relaxed">
              AI icebreakers run locally on your GPU — never in the cloud. Your prompts, contacts and output stay on your machine. Only your plan entitlement is synced.
            </p>
            <ul className="mt-auto flex flex-wrap gap-3 text-xs">
              <li className="flex items-center gap-1.5 bg-[#141724] border border-slate-800 rounded-full px-3 py-1">
                <CheckCircle2 className="w-3.5 h-3.5 text-[#2E9E5B]" /> Local LLM inference
              </li>
              <li className="flex items-center gap-1.5 bg-[#141724] border border-slate-800 rounded-full px-3 py-1">
                <CheckCircle2 className="w-3.5 h-3.5 text-[#2E9E5B]" /> Zero prompt logging
              </li>
            </ul>
          </div>
        </div>

        {/* Simple tech requirement note — full details in FAQ */}
        <div className="bg-[#0f1118] border border-slate-800 rounded-2xl p-6 text-center">
          <p className="text-sm text-slate-300 font-mono">
            <Lock className="inline-block w-4 h-4 mr-1.5 mb-0.5 text-[#4da3ff]" />
            Runs on Windows. AI icebreakers use your NVIDIA GPU when available — <span className="text-[#4da3ff]">CPU-only mode works too.</span> See the FAQ below for full requirements.
          </p>
        </div>
      </div>
    </section>
  );
};

