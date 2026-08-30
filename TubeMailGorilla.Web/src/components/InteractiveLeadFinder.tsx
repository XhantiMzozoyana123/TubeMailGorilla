import React, { useState } from 'react';
import { Search, Filter, Mail, Copy, Check, Sparkles, Download, ExternalLink, BookmarkPlus, BookmarkCheck, TrendingUp, AlertCircle, Video, Flame, Zap, Cpu } from 'lucide-react';
import { MOCK_CREATORS } from '../data/mockCreators';
import { CreatorLead, ProspectItem } from '../types';

interface InteractiveLeadFinderProps {
  onSaveLead: (creator: CreatorLead) => void;
  onOpenPitchModal: (creator: CreatorLead) => void;
  savedLeads: ProspectItem[];
  onOpenTrial: () => void;
}

export const InteractiveLeadFinder: React.FC<InteractiveLeadFinderProps> = ({
  onSaveLead,
  onOpenPitchModal,
  savedLeads,
  onOpenTrial,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [minSubs, setMinSubs] = useState<number>(0);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const categories = ['All', 'Tech', 'Finance', 'Gaming', 'Fitness', 'Documentary', 'Vlogs', 'Education'];

  const filteredCreators = MOCK_CREATORS.filter((creator) => {
    const matchesSearch =
      creator.channelName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      creator.niche.toLowerCase().includes(searchTerm.toLowerCase()) ||
      creator.painPoint.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesCategory = selectedCategory === 'All' || creator.category === selectedCategory;
    const matchesSubs = creator.subscribersCount >= minSubs;

    return matchesSearch && matchesCategory && matchesSubs;
  });

  const handleCopyEmail = (email: string, id: string) => {
    navigator.clipboard.writeText(email);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  const isSaved = (id: string) => savedLeads.some((l) => l.id === id);

  return (
    <section className="py-20 bg-[#07080b] border-y border-slate-800/80 relative" id="lead-finder-demo">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative">
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto mb-12">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-lg bg-[#141724] border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold uppercase tracking-wider mb-4 shadow-[0_0_15px_rgba(255,0,59,0.15)]">
            <Cpu className="w-3.5 h-3.5 text-[#ff003b]" />
            <span>INTERACTIVE PROSPECT ENGINE MATRIX</span>
          </div>
          <h2 className="text-3xl sm:text-4xl font-extrabold text-white font-['Outfit',sans-serif] tracking-tight mb-4">
            Live TubeMail Gorilla Search Console
          </h2>
          <p className="text-slate-300 text-base sm:text-lg">
            Search active creator channels, inspect editing bottlenecks, extract verified contact emails, and generate tailored cold email pitches in seconds.
          </p>
        </div>

        {/* Sandbox App Interface Container */}
        <div className="bg-[#0b0d14] border border-[#ff003b]/30 rounded-2xl shadow-2xl overflow-hidden">
          {/* Top Control Bar */}
          <div className="p-4 sm:p-6 border-b border-slate-800/80 bg-[#0f1118] space-y-4">
            <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
              {/* Search Bar */}
              <div className="relative w-full md:w-96">
                <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder="Search by niche, game, software, keyword..."
                  className="w-full pl-10 pr-4 py-2.5 bg-[#07080b] border border-slate-700/80 rounded-lg text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-[#ff003b] focus:ring-1 focus:ring-[#ff003b] transition-colors font-mono"
                />
                {searchTerm && (
                  <button
                    onClick={() => setSearchTerm('')}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-slate-400 hover:text-white"
                  >
                    Clear
                  </button>
                )}
              </div>

              {/* Sub Tier quick filters */}
              <div className="flex items-center gap-2 text-xs text-slate-400 w-full md:w-auto overflow-x-auto pb-1 md:pb-0 font-mono">
                <span className="font-semibold text-slate-300 flex items-center gap-1 shrink-0">
                  <Filter className="w-3.5 h-3.5 text-[#ff003b]" /> Subs:
                </span>
                <button
                  onClick={() => setMinSubs(0)}
                  className={`px-3 py-1.5 rounded-md font-medium transition-colors shrink-0 ${
                    minSubs === 0 ? 'bg-[#ff003b] text-white font-bold' : 'bg-[#151824] text-slate-300 hover:bg-[#1e2338]'
                  }`}
                >
                  All Tiers
                </button>
                <button
                  onClick={() => setMinSubs(25000)}
                  className={`px-3 py-1.5 rounded-md font-medium transition-colors shrink-0 ${
                    minSubs === 25000 ? 'bg-[#ff003b] text-white font-bold' : 'bg-[#151824] text-slate-300 hover:bg-[#1e2338]'
                  }`}
                >
                  25k+ (Rising)
                </button>
                <button
                  onClick={() => setMinSubs(60000)}
                  className={`px-3 py-1.5 rounded-md font-medium transition-colors shrink-0 ${
                    minSubs === 60000 ? 'bg-[#ff003b] text-white font-bold' : 'bg-[#151824] text-slate-300 hover:bg-[#1e2338]'
                  }`}
                >
                  60k+ (Sweet Spot)
                </button>
                <button
                  onClick={() => setMinSubs(100000)}
                  className={`px-3 py-1.5 rounded-md font-medium transition-colors shrink-0 ${
                    minSubs === 100000 ? 'bg-[#ff003b] text-white font-bold' : 'bg-[#151824] text-slate-300 hover:bg-[#1e2338]'
                  }`}
                >
                  100k+ (High Budget)
                </button>
              </div>
            </div>

            {/* Category Pills */}
            <div className="flex items-center gap-2 overflow-x-auto pb-1 text-xs">
              <span className="text-slate-400 font-mono font-medium shrink-0">Niches:</span>
              {categories.map((cat) => (
                <button
                  key={cat}
                  onClick={() => setSelectedCategory(cat)}
                  className={`px-3 py-1 rounded-md whitespace-nowrap transition-all font-mono text-xs ${
                    selectedCategory === cat
                      ? 'bg-[#ff003b]/20 text-[#ff4d73] border border-[#ff003b]/60 font-bold'
                      : 'bg-[#141724] border border-slate-800 text-slate-400 hover:text-slate-200 hover:border-slate-700'
                  }`}
                >
                  {cat}
                </button>
              ))}
            </div>
          </div>

          {/* Results Grid */}
          <div className="p-4 sm:p-6 grid grid-cols-1 lg:grid-cols-2 gap-4">
            {filteredCreators.length > 0 ? (
              filteredCreators.map((creator) => {
                const saved = isSaved(creator.id);
                return (
                  <div
                    key={creator.id}
                    className="bg-[#0f1118] border border-slate-800/90 hover:border-[#ff003b]/50 rounded-xl p-5 transition-all duration-200 flex flex-col justify-between group shadow-sm hover:shadow-[0_0_20px_rgba(255,0,59,0.15)]"
                  >
                    <div>
                      {/* Creator Header */}
                      <div className="flex items-start justify-between gap-3 mb-3">
                        <div className="flex items-center gap-3">
                          <img
                            src={creator.avatar}
                            alt={creator.channelName}
                            className="w-12 h-12 rounded-full object-cover ring-2 ring-[#ff003b]/40"
                            referrerPolicy="no-referrer"
                          />
                          <div>
                            <div className="flex items-center gap-2">
                              <h3 className="font-bold text-white text-base font-['Outfit',sans-serif] group-hover:text-[#ff4d73] transition-colors">
                                {creator.channelName}
                              </h3>
                              <span className="px-2 py-0.5 rounded bg-[#161a29] text-slate-300 text-[10px] font-mono font-semibold border border-slate-700">
                                {creator.category}
                              </span>
                            </div>
                            <p className="text-xs text-slate-400 font-mono">{creator.handle} • {creator.niche}</p>
                          </div>
                        </div>

                        {/* Editing Need Score Badge */}
                        <div className="flex flex-col items-end">
                          <div className="flex items-center gap-1 px-2 py-1 rounded bg-[#ff003b]/15 border border-[#ff003b]/40 text-[#ff4d73] text-xs font-mono font-bold">
                            <Flame className="w-3.5 h-3.5 text-[#ff003b] fill-[#ff003b]" />
                            <span>{creator.editingNeedScore}% NEED</span>
                          </div>
                          <span className="text-[10px] font-mono text-slate-400 mt-0.5">High Editor Demand</span>
                        </div>
                      </div>

                      {/* Stats row */}
                      <div className="grid grid-cols-3 gap-2 py-2 px-3 rounded-lg bg-[#07080b] border border-slate-800 text-center mb-3 font-mono">
                        <div>
                          <p className="text-[10px] text-slate-400 uppercase">Subscribers</p>
                          <p className="text-sm font-bold text-slate-100">{creator.subscribers}</p>
                        </div>
                        <div>
                          <p className="text-[10px] text-slate-400 uppercase">Avg Views</p>
                          <p className="text-sm font-bold text-slate-100">{creator.averageViews}</p>
                        </div>
                        <div>
                          <p className="text-[10px] text-slate-400 uppercase">Upload Rate</p>
                          <p className="text-sm font-bold text-[#ff4d73]">{creator.videosPerMonth} vids/mo</p>
                        </div>
                      </div>

                      {/* Pain Point & Opportunity */}
                      <div className="space-y-2 text-xs mb-4 font-mono">
                        <div className="p-2.5 rounded-lg bg-[#07080b] border border-slate-800 text-slate-300">
                          <span className="font-semibold text-[#ff4d73] block mb-0.5">
                            🎯 Editing Pain Point:
                          </span>
                          <span className="font-sans text-xs text-slate-300">{creator.painPoint}</span>
                        </div>

                        <div className="p-2.5 rounded-lg bg-[#141724] border border-[#ff003b]/30 text-slate-300">
                          <span className="font-semibold text-[#ff708f] block mb-0.5">
                            💡 Strategic Pitch Angle:
                          </span>
                          <span className="font-sans text-xs text-slate-200">{creator.recommendedPitchAngle}</span>
                        </div>
                      </div>

                      {/* Detected Email Bar */}
                      <div className="flex items-center justify-between gap-2 p-2 rounded-lg bg-[#07080b] border border-slate-800 mb-4 font-mono">
                        <div className="flex items-center gap-2 overflow-hidden text-xs">
                          <Mail className="w-3.5 h-3.5 text-[#ff003b] shrink-0" />
                          <span className="text-slate-300 truncate">{creator.email}</span>
                          <span className="px-1.5 py-0.2 rounded bg-emerald-500/20 text-emerald-300 text-[10px] font-bold">
                            Verified
                          </span>
                        </div>
                        <button
                          onClick={() => handleCopyEmail(creator.email, creator.id)}
                          className="px-2 py-1 rounded bg-[#161a29] hover:bg-[#20273d] text-slate-200 text-xs font-semibold flex items-center gap-1 shrink-0 transition-colors cursor-pointer border border-slate-700"
                          title="Copy Email"
                        >
                          {copiedId === creator.id ? (
                            <>
                              <Check className="w-3 h-3 text-emerald-400" />
                              <span className="text-emerald-400">Copied</span>
                            </>
                          ) : (
                            <>
                              <Copy className="w-3 h-3 text-slate-400" />
                              <span>Copy</span>
                            </>
                          )}
                        </button>
                      </div>
                    </div>

                    {/* Card Actions */}
                    <div className="flex items-center gap-2 pt-3 border-t border-slate-800/80">
                      <button
                        onClick={() => onOpenPitchModal(creator)}
                        className="flex-1 py-2 px-3 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] flex items-center justify-center gap-1.5 shadow-[0_0_15px_rgba(255,0,59,0.3)] transition-all cursor-pointer border border-[#ff4d73]/40"
                      >
                        <Sparkles className="w-3.5 h-3.5" />
                        <span>Generate Cold Pitch</span>
                      </button>

                      <button
                        onClick={() => onSaveLead(creator)}
                        className={`py-2 px-3 rounded-lg text-xs font-semibold flex items-center gap-1.5 transition-colors cursor-pointer border font-mono ${
                          saved
                            ? 'bg-emerald-500/20 border-emerald-500/40 text-emerald-300'
                            : 'bg-[#151824] border-slate-700 text-slate-300 hover:bg-[#1e2338]'
                        }`}
                      >
                        {saved ? (
                          <>
                            <BookmarkCheck className="w-3.5 h-3.5 text-emerald-400" />
                            <span>Saved</span>
                          </>
                        ) : (
                          <>
                            <BookmarkPlus className="w-3.5 h-3.5 text-slate-400" />
                            <span>Save</span>
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                );
              })
            ) : (
              <div className="col-span-2 py-12 text-center text-slate-400">
                <AlertCircle className="w-8 h-8 text-[#ff003b] mx-auto mb-2" />
                <p className="text-base font-semibold text-slate-200">No creator channels found</p>
                <p className="text-sm">Try tweaking your keyword search or subscriber threshold.</p>
              </div>
            )}
          </div>

          {/* Sandbox Footer Banner */}
          <div className="p-4 bg-gradient-to-r from-[#0d0f17] via-[#1b0e14] to-[#0d0f17] border-t border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-4">
            <div className="flex items-center gap-3">
                            <div className="w-8 h-8 rounded-lg bg-[#ff003b]/20 border border-[#ff003b]/40 flex items-center justify-center text-[#ff003b]">
                                <img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-full h-full object-contain filter drop-shadow-[0_0_6px_rgba(255,0,59,0.5)]" />
              </div>
              <p className="text-xs sm:text-sm text-slate-300 text-center sm:text-left">
                <strong>Showing 8 of 500,000+ extracted creators.</strong> Start your free trial to unlock unlimited searches across any YouTube niche.
              </p>
            </div>
            <button
              onClick={onOpenTrial}
              className="px-4 py-2 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] whitespace-nowrap shadow-[0_0_15px_rgba(255,0,59,0.3)] transition-colors cursor-pointer border border-[#ff4d73]/50"
            >
              Unlock All 500k+ Leads →
            </button>
          </div>
        </div>
      </div>
    </section>
  );
};
