import React from 'react';
import { X, Download, Trash2, Mail, ExternalLink, Sparkles, CheckCircle2, DollarSign, Zap } from 'lucide-react';
import { ProspectItem, CreatorLead } from '../types';

interface ProspectListDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  savedLeads: ProspectItem[];
  onUpdateStatus: (id: string, status: ProspectItem['status']) => void;
  onRemoveLead: (id: string) => void;
  onOpenPitch: (creator: CreatorLead) => void;
  onOpenTrial: () => void;
}

export const ProspectListDrawer: React.FC<ProspectListDrawerProps> = ({
  isOpen,
  onClose,
  savedLeads,
  onUpdateStatus,
  onRemoveLead,
  onOpenPitch,
  onOpenTrial,
}) => {
  if (!isOpen) return null;

  const handleExportCSV = () => {
    if (savedLeads.length === 0) return;
    const headers = ['Channel Name', 'Handle', 'Niche', 'Subscribers', 'Videos/Month', 'Email', 'Status', 'Pitch Angle'];
    const rows = savedLeads.map((l) => [
      `"${l.channelName}"`,
      `"${l.handle}"`,
      `"${l.niche}"`,
      `"${l.subscribers}"`,
      `"${l.videosPerMonth}"`,
      `"${l.email}"`,
      `"${l.status}"`,
      `"${l.recommendedPitchAngle}"`,
    ]);

    const csvContent = 'data:text/csv;charset=utf-8,' + [headers.join(','), ...rows.map((e) => e.join(','))].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `tubemail-gorilla-prospects-${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const statusOptions: ProspectItem['status'][] = ['Saved', 'Emailed', 'In Discussion', 'Closed Client'];

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="w-full max-w-md sm:max-w-xl bg-[#0b0d14] border-l border-[#ff003b]/40 h-full flex flex-col shadow-[0_0_50px_rgba(255,0,59,0.2)]">
        {/* Header */}
        <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-[#11131c]">
          <div>
            <div className="flex items-center gap-2">
              <h3 className="font-extrabold text-white text-lg font-['Rajdhani',sans-serif] uppercase tracking-wider">
                Prospect Outreach Pipeline
              </h3>
              <span className="px-2 py-0.5 rounded bg-[#1c0e14] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold">
                {savedLeads.length} LEADS
              </span>
            </div>
            <p className="text-xs text-slate-400 font-mono">Track and execute client acquisitions</p>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer border border-slate-700"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Action toolbar */}
        <div className="p-3 bg-[#050608] border-b border-slate-800 flex items-center justify-between gap-2 px-5 font-mono text-xs">
          <button
            onClick={handleExportCSV}
            disabled={savedLeads.length === 0}
            className="px-3 py-1.5 rounded-lg bg-[#151824] hover:bg-[#202538] disabled:opacity-40 border border-slate-700 text-xs font-semibold text-slate-200 flex items-center gap-1.5 transition-colors cursor-pointer"
          >
            <Download className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>EXPORT CSV</span>
          </button>

          <span className="text-[11px] text-[#ff4d73]">
            Target: 2 clients ($500/mo)
          </span>
        </div>

        {/* Lead items list */}
        <div className="p-5 flex-1 overflow-y-auto space-y-3">
          {savedLeads.length > 0 ? (
            savedLeads.map((lead) => (
              <div
                key={lead.id}
                className="p-4 rounded-xl bg-[#0f1118] border border-slate-800 hover:border-[#ff003b]/50 transition-all space-y-3 shadow-md"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-center gap-3">
                    <img
                      src={lead.avatar}
                      alt={lead.channelName}
                      className="w-10 h-10 rounded-lg object-cover ring-1 ring-[#ff003b]/40 shadow-sm"
                      referrerPolicy="no-referrer"
                    />
                    <div>
                      <h4 className="font-bold text-white text-sm font-['Rajdhani',sans-serif] text-base">{lead.channelName}</h4>
                      <p className="text-xs text-slate-400 font-mono">{lead.niche} • {lead.subscribers}</p>
                    </div>
                  </div>

                  <button
                    onClick={() => onRemoveLead(lead.id)}
                    className="text-slate-500 hover:text-rose-400 transition-colors p-1"
                    title="Remove from pipeline"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>

                <div className="p-2 rounded-lg bg-[#050608] border border-slate-800 text-xs font-mono text-slate-300 flex items-center justify-between">
                  <span>{lead.email}</span>
                  <button
                    onClick={() => navigator.clipboard.writeText(lead.email)}
                    className="text-[10px] text-[#ff003b] hover:underline uppercase font-bold"
                  >
                    Copy
                  </button>
                </div>

                {/* Status Selector */}
                <div className="flex items-center justify-between pt-1 text-xs font-mono">
                  <span className="text-slate-400 text-[11px] font-semibold">STAGE:</span>
                  <select
                    value={lead.status}
                    onChange={(e) => onUpdateStatus(lead.id, e.target.value as ProspectItem['status'])}
                    className="bg-[#050608] border border-slate-700 rounded-lg px-2.5 py-1 text-xs text-slate-200 focus:outline-none focus:border-[#ff003b]"
                  >
                    {statusOptions.map((opt) => (
                      <option key={opt} value={opt}>
                        {opt}
                      </option>
                    ))}
                  </select>

                  <button
                    onClick={() => onOpenPitch(lead)}
                    className="px-3 py-1 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-bold text-xs uppercase font-['Rajdhani',sans-serif] flex items-center gap-1 transition-colors cursor-pointer shadow-[0_0_10px_rgba(255,0,59,0.3)] border border-[#ff4d73]/40"
                  >
                    <Zap className="w-3 h-3" />
                    <span>Pitch</span>
                  </button>
                </div>
              </div>
            ))
          ) : (
            <div className="py-16 text-center text-slate-400 space-y-3 font-mono">
              <span className="text-4xl block animate-pulse">📋</span>
              <p className="text-base font-semibold text-slate-200 font-sans">Your Prospect List is Empty</p>
              <p className="text-xs max-w-xs mx-auto text-slate-400">
                Head to the Live Search Console to extract YouTubers in your favorite niche and build your outreach pipeline!
              </p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-4 border-t border-slate-800 bg-[#11131c] flex items-center justify-between gap-3">
          <p className="text-xs text-slate-400 font-mono">Unlock 500k+ YouTubers with Free Trial</p>
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
