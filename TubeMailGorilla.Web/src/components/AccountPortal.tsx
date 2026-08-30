import React, { useEffect, useState } from 'react';
import { X, Download, AlertTriangle, LogOut, Monitor } from 'lucide-react';
import { UserProfile } from '../types';
import { DEFAULT_TRANSACTIONS } from '../services/authService';
import { cancelSubscription, SITE_URL, fetchMySubscription, MySubscriptionStatus } from '../services/api';

interface AccountPortalProps {
  isOpen: boolean;
  onClose: () => void;
  user: UserProfile;
  onUpdateUser: (updated: UserProfile) => void;
  onLogout: () => void;
  onOpenUpgradeModal: () => void;
}

export const AccountPortal: React.FC<AccountPortalProps> = ({
  isOpen,
  onClose,
  user,
  onUpdateUser,
  onLogout,
  onOpenUpgradeModal,
}) => {
    const [tab, setTab] = useState<'subscription' | 'desktop'>('subscription');
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);
  const [isCanceling, setIsCanceling] = useState(false);
  const [liveStatus, setLiveStatus] = useState<MySubscriptionStatus | null>(null);

  // Always show the authoritative subscription state from the server.
  useEffect(() => {
    if (!isOpen) return;
    let cancelled = false;
    fetchMySubscription().then((s) => { if (!cancelled) setLiveStatus(s); });
    return () => { cancelled = true; };
  }, [isOpen]);

  if (!isOpen) return null;

  const isPro = liveStatus
    ? liveStatus.isSubscribed
    : user.subscription.tier === 'pro' || user.subscription.tier === 'agency';

  const handleCancelSubscription = async () => {
    setIsCanceling(true);
    const res = await cancelSubscription();
    setIsCanceling(false);
    setShowCancelConfirm(false);
    if (!res.success) {
      console.warn('Cancel failed:', res.message);
    }
    const updated: UserProfile = {
      ...user,
      subscription: {
        ...user.subscription,
        tier: 'trial',
        status: 'canceled',
        leadsLimit: 100,
        amount: 0,
      }
    };
        onUpdateUser(updated);
    // Re-pull the authoritative state so the UI reflects the cancellation.
    fetchMySubscription().then((s) => setLiveStatus(s));
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-6 bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="bg-[#0b0d14] border border-[#ff003b]/50 rounded-2xl w-full max-w-3xl max-h-[92vh] flex flex-col shadow-[0_0_60px_rgba(255,0,59,0.35)] overflow-hidden">
        {/* Header */}
        <div className="p-4 sm:p-6 border-b border-slate-800 flex items-center justify-between bg-[#0e1017]">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-[#ff003b] to-[#800014] p-0.5 shadow-lg shadow-[#ff003b]/30">
              <img
                src={user.avatar || 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80'}
                alt={user.name}
                className="w-full h-full object-cover rounded-lg"
              />
            </div>
            <div>
              <h3 className="text-xl font-bold text-white font-['Rajdhani',sans-serif] uppercase tracking-wide">
                {user.name}
              </h3>
              <p className="text-xs text-slate-400">{user.email}</p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={onLogout}
              className="px-3 py-1.5 rounded-lg bg-[#151824] hover:bg-[#22273d] text-slate-300 hover:text-rose-400 text-xs flex items-center gap-1.5 transition-colors border border-slate-800 cursor-pointer"
            >
              <LogOut className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Sign Out</span>
            </button>
            <button
              onClick={onClose}
              className="p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer border border-slate-700"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-slate-800 bg-[#07080b] px-4 sm:px-6 text-sm font-bold">
          {(['subscription', 'desktop'] as const).map((id) => (
            <button
              key={id}
              onClick={() => setTab(id)}
              className={`py-3 px-4 border-b-2 transition-all cursor-pointer ${
                tab === id
                  ? 'border-[#ff003b] text-[#ff4d73] bg-[#ff003b]/5'
                  : 'border-transparent text-slate-400 hover:text-white'
              }`}
            >
              {id === 'subscription' ? 'Subscription' : 'Desktop App'}
            </button>
          ))}
                </div>

        <div className="p-4 sm:p-6 overflow-y-auto flex-1 space-y-6">

          {/* ---------------- SUBSCRIPTION ---------------- */}
          {tab === 'subscription' && (
            <div className="space-y-6">
              <div className="p-6 rounded-xl bg-[#0f1118] border border-[#ff003b]/40 space-y-4">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                  <div>
                    <span className="text-xs text-slate-400 uppercase">Your plan</span>
                                        <h3 className="text-2xl font-extrabold text-white font-['Rajdhani',sans-serif] uppercase mt-0.5">
                      {isPro ? (liveStatus?.planName ?? 'Pro') : 'Free'}
                    </h3>
                  </div>

                  <span className={`px-3 py-1 rounded-full text-xs font-bold w-fit ${
                    isPro
                      ? 'bg-emerald-500/10 border border-emerald-500/30 text-emerald-400'
                      : 'bg-slate-700/30 border border-slate-600 text-slate-300'
                  }`}>
                    {isPro ? 'Active' : 'Free plan'}
                  </span>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 p-4 bg-[#050608] rounded-xl border border-slate-800 text-sm">
                                    <div>
                    <span className="text-slate-500 block mb-0.5">Billing</span>
                    <span className="text-white font-bold">
                      {isPro
                        ? `$${(liveStatus?.price ?? user.subscription.amount).toFixed(2)} / month`
                        : 'Free'}
                    </span>
                  </div>
                  <div>
                    <span className="text-slate-500 block mb-0.5">{isPro ? 'Next payment' : 'Upgrade anytime'}</span>
                    <span className="text-white font-bold">
                      {isPro
                        ? (liveStatus?.nextBillingDate
                            ? new Date(liveStatus.nextBillingDate).toLocaleDateString()
                            : new Date(user.subscription.renewsAt).toLocaleDateString())
                        : '—'}
                    </span>
                  </div>
                </div>

                {!showCancelConfirm && (
                  <div className="flex flex-wrap items-center gap-3 pt-1">
                    {isPro ? (
                      <button
                        onClick={() => setShowCancelConfirm(true)}
                        className="px-4 py-2 rounded-lg bg-[#151824] hover:bg-rose-950/40 text-slate-400 hover:text-rose-400 text-sm transition-colors border border-slate-800 cursor-pointer"
                      >
                        Cancel subscription
                      </button>
                    ) : (
                      <button
                        onClick={onOpenUpgradeModal}
                        className="px-5 py-2.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-sm uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_15px_rgba(255,0,59,0.3)] transition-colors cursor-pointer border border-[#ff4d73]"
                      >
                        Upgrade to Pro — $9.99/mo
                      </button>
                    )}
                  </div>
                )}

                {showCancelConfirm && (
                  <div className="p-4 rounded-xl bg-rose-950/20 border border-rose-500/50 space-y-3">
                    <div className="flex items-center gap-2 text-rose-400 font-bold text-sm">
                      <AlertTriangle className="w-4 h-4" />
                      <span>Cancel your subscription?</span>
                    </div>
                    <p className="text-xs text-slate-300 leading-relaxed">
                      Your plan stays active until the end of the current billing period, then you'll be moved back to the free plan.
                    </p>
                    <div className="flex items-center gap-3 pt-1">
                      <button
                        onClick={handleCancelSubscription}
                        disabled={isCanceling}
                        className="px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-500 text-white font-bold text-sm transition-colors cursor-pointer"
                      >
                        {isCanceling ? 'Cancelling…' : 'Yes, cancel'}
                      </button>
                      <button
                        onClick={() => setShowCancelConfirm(false)}
                        className="px-4 py-2 rounded-lg bg-[#151824] text-slate-300 text-sm cursor-pointer"
                      >
                        Keep my plan
                      </button>
                    </div>
                  </div>
                )}
              </div>

              {/* Billing history */}
              <div className="space-y-3">
                <h4 className="text-sm font-bold text-white uppercase tracking-wider text-slate-300">
                  Billing history
                </h4>

                <div className="overflow-x-auto rounded-xl border border-slate-800">
                  <table className="w-full text-left text-xs">
                    <thead className="bg-[#0e1017] text-slate-400 border-b border-slate-800">
                      <tr>
                        <th className="p-3">Date</th>
                        <th className="p-3">Description</th>
                        <th className="p-3">Amount</th>
                        <th className="p-3">Status</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-800/60 bg-[#050608]">
                      {DEFAULT_TRANSACTIONS.map((tx) => (
                        <tr key={tx.id} className="hover:bg-[#0f1118] transition-colors">
                          <td className="p-3 text-slate-300 whitespace-nowrap">{new Date(tx.date).toLocaleDateString()}</td>
                          <td className="p-3 text-white font-medium">{tx.description}</td>
                          <td className="p-3 text-white whitespace-nowrap">${tx.amount.toFixed(2)}</td>
                          <td className="p-3">
                            <span className="px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-400 text-[10px] font-bold">
                              Paid
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          )}

          {/* ---------------- DESKTOP APP ---------------- */}
          {tab === 'desktop' && (
            <div className="space-y-6">
              <div className="p-6 rounded-xl bg-[#0f1118] border border-slate-800 space-y-4">
                <div className="flex items-center gap-3">
                  <div className="w-12 h-12 rounded-lg bg-[#141724] border border-[#ff003b]/40 flex items-center justify-center text-2xl">
                    🖥️
                  </div>
                  <div>
                    <h4 className="text-lg font-bold text-white font-['Rajdhani',sans-serif] uppercase">
                      TubeMail Gorilla for Windows
                    </h4>
                    <p className="text-xs text-slate-400">
                      Search YouTube, extract leads and send outreach emails — right from your desktop.
                    </p>
                  </div>
                </div>

                <a
                  href={`${SITE_URL}/downloads/TubeMailGorilla-Setup.exe`}
                  download
                  className="w-full py-3.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] text-white font-extrabold text-sm uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_15px_rgba(255,0,59,0.3)] transition-colors cursor-pointer border border-[#ff4d73]/40 flex items-center justify-center gap-2 no-underline"
                >
                  <Download className="w-4 h-4" />
                  Download for Windows
                </a>

                <ul className="text-xs text-slate-400 space-y-1.5">
                  <li>• Windows 10 or later</li>
                  <li>• Uses the same account as this website — just sign in</li>
                  <li>• Included with your Pro subscription</li>
                </ul>
              </div>

              <div className="p-4 rounded-xl bg-[#050608] border border-slate-800 flex items-start gap-3 text-xs text-slate-400">
                <Monitor className="w-4 h-4 mt-0.5 shrink-0 text-[#ff4d73]" />
                <span>
                  Lead extractions run inside the desktop app. Your subscription works on both —
                  sign in with the same email on each.
                </span>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
