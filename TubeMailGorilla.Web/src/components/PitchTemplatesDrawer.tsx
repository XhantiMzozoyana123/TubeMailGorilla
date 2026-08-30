import React, { useState } from 'react';
import { X, Copy, Check, Sparkles, Send, Mail, CheckCircle2, Zap } from 'lucide-react';
import { PITCH_TEMPLATES } from '../data/pitchTemplates';

interface PitchTemplatesDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  onOpenTrial: () => void;
}

export const PitchTemplatesDrawer: React.FC<PitchTemplatesDrawerProps> = ({ isOpen, onClose, onOpenTrial }) => {
  const [copiedId, setCopiedId] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleCopy = (body: string, subject: string, id: string) => {
    const fullText = `Subject: ${subject}\n\n${body}`;
    navigator.clipboard.writeText(fullText);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="w-full max-w-md sm:max-w-xl bg-[#0b0d14] border-l border-[#ff003b]/40 h-full flex flex-col shadow-[0_0_50px_rgba(255,0,59,0.2)]">
        {/* Header */}
        <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-[#11131c]">
          <div>
            <div className="flex items-center gap-2">
              <h3 className="font-extrabold text-white text-lg font-['Rajdhani',sans-serif] uppercase tracking-wider">
                Tactical Pitch Library
              </h3>
              <span className="px-2 py-0.5 rounded bg-[#1c0e14] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold">
                PROVEN SCRIPTS
              </span>
            </div>
            <p className="text-xs text-slate-400 font-mono">Battle-tested cold outreach templates tailored for pitching YouTubers</p>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer border border-slate-700"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-5 flex-1 overflow-y-auto space-y-6">
          {PITCH_TEMPLATES.map((tmpl) => (
            <div
              key={tmpl.id}
              className="p-5 rounded-xl bg-[#0f1118] border border-slate-800 hover:border-[#ff003b]/40 transition-all space-y-3 shadow-md"
            >
              <div className="flex items-start justify-between gap-2">
                <div>
                  <h4 className="font-bold text-white text-base font-['Rajdhani',sans-serif] text-lg uppercase tracking-wide">{tmpl.title}</h4>
                  <p className="text-xs text-slate-400 mt-0.5 font-mono">Best for: {tmpl.bestFor}</p>
                </div>
                <span className="px-2.5 py-1 rounded bg-[#15241b] border border-emerald-500/40 text-emerald-400 text-xs font-mono font-bold shrink-0">
                  {tmpl.conversionRate}
                </span>
              </div>

              <div className="p-2.5 rounded-lg bg-[#050608] border border-slate-800 text-xs text-[#ff4d73] font-mono">
                <span className="text-slate-400">Subject: </span>{tmpl.subject}
              </div>

              <div className="p-3.5 rounded-xl bg-[#050608] border border-slate-800 text-xs text-slate-300 whitespace-pre-line leading-relaxed font-sans max-h-44 overflow-y-auto">
                {tmpl.body}
              </div>

              <button
                onClick={() => handleCopy(tmpl.body, tmpl.subject, tmpl.id)}
                className="w-full py-2.5 rounded-lg bg-[#151824] hover:bg-[#202538] border border-slate-700 text-slate-200 text-xs font-mono font-bold flex items-center justify-center gap-2 transition-colors cursor-pointer"
              >
                {copiedId === tmpl.id ? (
                  <>
                    <Check className="w-3.5 h-3.5 text-emerald-400" />
                    <span className="text-emerald-400">SCRIPT COPIED!</span>
                  </>
                ) : (
                  <>
                    <Copy className="w-3.5 h-3.5 text-[#ff003b]" />
                    <span>COPY FULL SCRIPT</span>
                  </>
                )}
              </button>
            </div>
          ))}
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-slate-800 bg-[#11131c] flex items-center justify-between gap-3">
          <p className="text-xs text-slate-400 font-mono">Get creator emails with TubeMail Gorilla</p>
          <button
            onClick={() => {
              onClose();
              onOpenTrial();
            }}
            className="px-4 py-2 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_15px_rgba(255,0,59,0.3)] cursor-pointer border border-[#ff4d73]"
          >
            Start Free Trial →
          </button>
        </div>
      </div>
    </div>
  );
};
