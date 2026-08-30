import React, { useEffect, useState } from 'react';
import { Check, X, ArrowRight, ShieldCheck, Sparkles } from 'lucide-react';
import { SubscriptionTier } from '../types';
import { fetchPlans, SubscriptionPlanInfo } from '../services/api';

interface SubscriptionPageProps {
  isLoggedIn: boolean;
  currentTier: string;
  onOpenUpgrade: (tier: SubscriptionTier) => void;
  onOpenRegister: () => void;
}

interface Plan {
  id: SubscriptionTier;
  name: string;
  price: number;
  tagline: string;
  badge?: string;
  cta: string;
  features: { label: string; included: boolean }[];
}

const PLAN: Plan = {
  id: 'pro',
  name: 'TubeMail Gorilla Pro',
  price: 9.99,
  tagline: 'One flat monthly price — cancel anytime.',
  badge: '14-DAY FREE TRIAL',
  cta: 'Get Pro — Start Free Trial',
  features: [
    { label: '14-day full-access trial (no card required)', included: true },
    { label: 'Unlimited YouTube lead extraction', included: true },
    { label: 'Verified-email & editing-need scoring filter', included: true },
    { label: 'AI pitch generator + templates', included: true },
    { label: 'Prospect pipeline tracker', included: true },
    { label: 'Desktop app pairing key', included: true },
    { label: 'Recurring billing via PayPal', included: true },
    { label: 'Cancel anytime in one click', included: true },
    { label: 'Priority support', included: true },
  ],
};

export const SubscriptionPage: React.FC<SubscriptionPageProps> = ({
  isLoggedIn,
  currentTier,
  onOpenUpgrade,
  onOpenRegister,
}) => {
  const [livePlan, setLivePlan] = useState<SubscriptionPlanInfo | null>(null);

  // The plan catalog lives in the backend's appsettings.json. Show it live
  // when reachable; otherwise keep the bundled defaults above.
  useEffect(() => {
    let cancelled = false;
    fetchPlans().then((plans) => {
      if (!cancelled && plans.length > 0) setLivePlan(plans[0]);
    });
    return () => { cancelled = true; };
  }, []);

  const planName = livePlan?.name ? `TubeMail Gorilla ${livePlan.name}` : PLAN.name;
  const planPrice = livePlan?.monthlyPrice ?? PLAN.price;
  const planTagline = livePlan?.tagline ?? PLAN.tagline;
  const featureList: { label: string; included: boolean }[] = livePlan?.features.length
    ? livePlan.features.map((f) => ({ label: f, included: true }))
    : PLAN.features;

  const isCurrent = isLoggedIn && currentTier === PLAN.id;

  return (
    <div className="min-h-screen bg-[#07080b] pt-32 pb-24">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="text-center max-w-3xl mx-auto mb-16" id="subscription-header">
          <span className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-[#ff003b]/10 border border-[#ff003b]/40 text-[#ff4d73] text-[11px] font-mono font-bold tracking-widest uppercase mb-5">
            <Sparkles className="w-3.5 h-3.5" />
            Pricing
          </span>
                                        <h1 className="font-['Rajdhani',sans-serif] text-4xl sm:text-5xl font-extrabold uppercase tracking-wide text-white">
            {planName}
          </h1>
          <p className="mt-3 text-slate-400 text-lg">
            14-day free trial · ${planPrice}/mo billed via PayPal · Cancel anytime
          </p>
        </div>


        {/* Single plan card */}
        <div className="max-w-xl mx-auto" id="subscription-plan">
          <div className="relative rounded-2xl p-1 bg-gradient-to-r from-[#ff003b] via-[#e60028] to-[#cc0024] shadow-[0_0_45px_rgba(255,0,59,0.35)]">
            <div className="h-full rounded-2xl bg-[#0c0e15] border border-slate-900 p-8 flex flex-col">
                            <div className="flex items-center gap-4">
                                <h2 className="font-['Rajdhani',sans-serif] text-3xl font-extrabold uppercase tracking-wider text-white">
                  {planName}
                </h2>
              </div>

              <p className="text-sm text-slate-400 mt-2.5">{planTagline}</p>

              <div className="mt-6 flex items-end gap-1">
                <span className="font-['Rajdhani',sans-serif] text-6xl font-extrabold text-white">
                  ${planPrice}
                </span>
                <span className="text-slate-400 text-sm mb-1.5">/month</span>
              </div>
              <p className="mt-1 text-[11px] font-mono text-slate-500 tracking-wider uppercase">
                Billed monthly via PayPal · Cancel anytime
              </p>

                            <ul className="mt-8 space-y-3.5 flex-1">
                {featureList.map((f) => (
                  <li key={f.label} className="flex items-start gap-2.5 text-sm">
                    {f.included ? (
                      <Check className="w-4 h-4 mt-0.5 text-[#ff003b] shrink-0" />
                    ) : (
                      <X className="w-4 h-4 mt-0.5 text-slate-600 shrink-0" />
                    )}
                    <span className={f.included ? 'text-slate-200' : 'text-slate-500 line-through'}>
                      {f.label}
                    </span>
                  </li>
                ))}
              </ul>

              <div className="mt-8">
                {isCurrent ? (
                  <button
                    disabled
                    className="w-full py-3.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-400 text-sm font-mono font-bold uppercase tracking-wider flex items-center justify-center gap-2 cursor-default"
                  >
                    <ShieldCheck className="w-4 h-4" />
                    Your Current Plan
                  </button>
                ) : (
                  <button
                    id="subscription-cta-pro"
                    onClick={() => (isLoggedIn ? onOpenUpgrade(PLAN.id) : onOpenRegister())}
                    className="w-full py-3.5 rounded-xl bg-gradient-to-r from-[#ff003b] via-[#e60028] to-[#cc0024] hover:from-[#ff1a4b] hover:to-[#e60028] text-white font-extrabold uppercase tracking-wider text-sm flex items-center justify-center gap-2 shadow-lg shadow-[#ff003b]/30 hover:shadow-[#ff003b]/60 transition-all duration-200"
                  >
                    {PLAN.cta}
                    {!isLoggedIn && <ArrowRight className="w-4 h-4" />}
                  </button>
                )}
                            </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
