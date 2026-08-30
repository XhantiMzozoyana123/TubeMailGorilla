import React, { useState } from 'react';
import { X, Copy, Check, Sparkles, Send, Mail, RefreshCw, Layers, Zap } from 'lucide-react';
import { CreatorLead, PitchTemplate } from '../types';
import { PITCH_TEMPLATES } from '../data/pitchTemplates';

interface PitchGeneratorModalProps {
  creator: CreatorLead | null;
  isOpen: boolean;
  onClose: () => void;
}

export const PitchGeneratorModal: React.FC<PitchGeneratorModalProps> = ({ creator, isOpen, onClose }) => {
  const [selectedTemplateId, setSelectedTemplateId] = useState<string>(PITCH_TEMPLATES[0].id);
  const [copied, setCopied] = useState(false);
  const [editorName, setEditorName] = useState('Alex');
  const [portfolioLink, setPortfolioLink] = useState('portfolio.com/alex-edits');

  if (!isOpen || !creator) return null;

  const currentTemplate = PITCH_TEMPLATES.find((t) => t.id === selectedTemplateId) || PITCH_TEMPLATES[0];

  const formattedSubject = currentTemplate.subject
    .replace(/\{\{channelName\}\}/g, creator.channelName)
    .replace(/\{\{creatorName\}\}/g, creator.channelName.split(' ')[0]);

  const formattedBody = currentTemplate.body
    .replace(/\{\{creatorName\}\}/g, creator.channelName.split(' ')[0])
    .replace(/\{\{channelName\}\}/g, creator.channelName)
    .replace(/\{\{sampleVideoTitle\}\}/g, creator.sampleVideoTitle)
    .replace(/\{\{videosPerMonth\}\}/g, creator.videosPerMonth.toString())
    .replace(/\{\{niche\}\}/g, creator.niche)
    .replace(/\[Your Name\]/g, editorName)
    .replace(/\[Your Portfolio Link\]/g, portfolioLink);

  const handleCopy = () => {
    const fullText = `Subject: ${formattedSubject}\n\n${formattedBody}`;
    navigator.clipboard.writeText(fullText);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleMailto = () => {
    const mailtoUrl = `mailto:${creator.email}?subject=${encodeURIComponent(formattedSubject)}&body=${encodeURIComponent(formattedBody)}`;
    window.open(mailtoUrl, '_blank');
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="bg-[#0b0d14] border border-[#ff003b]/40 rounded-2xl w-full max-w-2xl max-h-[90vh] flex flex-col shadow-[0_0_50px_rgba(255,0,59,0.25)] overflow-hidden">
        {/* Modal Header */}
        <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-[#11131c]">
          <div className="flex items-center gap-3">
            <img
              src={creator.avatar}
              alt={creator.channelName}
              className="w-10 h-10 rounded-lg object-cover ring-2 ring-[#ff003b]/50 shadow-[0_0_10px_rgba(255,0,59,0.3)]"
              referrerPolicy="no-referrer"
            />
            <div>
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-white text-base font-['Rajdhani',sans-serif] tracking-wide text-lg">
                  TARGET: {creator.channelName.toUpperCase()}
                </h3>
                <span className="px-2 py-0.5 rounded bg-[#1c0e14] border border-[#ff003b]/40 text-[#ff4d73] text-[10px] font-bold font-mono">
                  {creator.category}
                </span>
              </div>
              <p className="text-xs text-slate-400 font-mono">{creator.email}</p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer border border-slate-700"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Modal Body */}
        <div className="p-5 space-y-4 overflow-y-auto flex-1">
          {/* Template Selector Pills */}
          <div>
            <label className="text-xs font-bold text-[#ff4d73] block mb-2 font-mono uppercase text-[11px]">
              Select Tactical Pitch Angle:
            </label>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
              {PITCH_TEMPLATES.map((tmpl) => (
                <button
                  key={tmpl.id}
                  onClick={() => setSelectedTemplateId(tmpl.id)}
                  className={`p-2.5 rounded-lg text-left border text-xs transition-all cursor-pointer ${
                    selectedTemplateId === tmpl.id
                      ? 'bg-[#1e0e14] border-[#ff003b] text-white shadow-[0_0_15px_rgba(255,0,59,0.25)]'
                      : 'bg-[#0f1118] border-slate-800 text-slate-400 hover:border-slate-700 hover:text-slate-200'
                  }`}
                >
                  <p className="font-bold truncate text-slate-100 font-['Rajdhani',sans-serif] text-sm uppercase">{tmpl.title}</p>
                  <p className="text-[10px] text-emerald-400 font-mono font-semibold mt-0.5">{tmpl.conversionRate}</p>
                </button>
              ))}
            </div>
          </div>

          {/* Quick Customizer */}
          <div className="grid grid-cols-2 gap-3 p-3 rounded-xl bg-[#050608] border border-slate-800">
            <div>
              <label className="text-[10px] font-semibold text-slate-400 block mb-1 font-mono uppercase">Your Name</label>
              <input
                type="text"
                value={editorName}
                onChange={(e) => setEditorName(e.target.value)}
                className="w-full px-2.5 py-1.5 rounded-lg bg-[#0f1118] border border-slate-700 text-xs text-white focus:outline-none focus:border-[#ff003b] font-mono"
              />
            </div>
            <div>
              <label className="text-[10px] font-semibold text-slate-400 block mb-1 font-mono uppercase">Portfolio / Loom Link</label>
              <input
                type="text"
                value={editorName ? portfolioLink : ''}
                onChange={(e) => setPortfolioLink(e.target.value)}
                className="w-full px-2.5 py-1.5 rounded-lg bg-[#0f1118] border border-slate-700 text-xs text-white focus:outline-none focus:border-[#ff003b] font-mono"
              />
            </div>
          </div>

          {/* Generated Pitch View */}
          <div className="space-y-2">
            <div>
              <span className="text-[10px] font-bold text-slate-400 uppercase tracking-wider block mb-1 font-mono">
                Generated Subject Line
              </span>
              <div className="p-2.5 rounded-lg bg-[#050608] border border-[#ff003b]/30 text-xs text-slate-200 font-medium font-mono text-[#ff4d73]">
                {formattedSubject}
              </div>
            </div>

            <div>
              <span className="text-[10px] font-bold text-slate-400 uppercase tracking-wider block mb-1 font-mono">
                Email Body Payload
              </span>
              <div className="p-3.5 rounded-xl bg-[#050608] border border-slate-800 text-xs text-slate-200 whitespace-pre-line leading-relaxed font-sans max-h-56 overflow-y-auto">
                {formattedBody}
              </div>
            </div>
          </div>
        </div>

        {/* Modal Footer Actions */}
        <div className="p-4 border-t border-slate-800 bg-[#11131c] flex items-center justify-between gap-3">
          <button
            onClick={handleMailto}
            className="px-4 py-2.5 rounded-lg bg-[#151824] hover:bg-[#202538] border border-slate-700 text-xs font-bold text-slate-200 flex items-center gap-2 transition-colors cursor-pointer font-mono"
          >
            <Send className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>Launch Email App</span>
          </button>

          <div className="flex items-center gap-2">
            <button
              onClick={handleCopy}
              className="px-5 py-2.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] flex items-center gap-2 transition-colors cursor-pointer shadow-[0_0_15px_rgba(255,0,59,0.3)] border border-[#ff4d73]"
            >
              {copied ? (
                <>
                  <Check className="w-4 h-4" />
                  <span>Payload Copied!</span>
                </>
              ) : (
                <>
                  <Copy className="w-4 h-4" />
                  <span>Copy Subject & Pitch</span>
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
