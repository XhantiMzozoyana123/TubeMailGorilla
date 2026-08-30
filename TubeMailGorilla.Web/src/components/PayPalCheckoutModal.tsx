import React, { useState } from 'react';
import { X, Lock, RefreshCw } from 'lucide-react';
import { SubscriptionTier } from '../types';
import { createSubscription, loadJwt } from '../services/api';

interface PayPalCheckoutModalProps {
  isOpen: boolean;
  onClose: () => void;
  targetTier: SubscriptionTier;
  onPaymentSuccess: (newSubscription: any) => void;
}

export const PayPalCheckoutModal: React.FC<PayPalCheckoutModalProps> = ({
  isOpen,
  onClose,
  targetTier,
  onPaymentSuccess,
}) => {
  const [isProcessing, setIsProcessing] = useState(false);
  const [step, setStep] = useState<'review' | 'redirecting'>('review');
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  if (!isOpen) return null;

  const planPrice = targetTier === 'agency' ? 79 : 9.99;

  const handleStartPayPalFlow = async () => {
    setErrorMsg(null);

    if (!loadJwt()) {
      setErrorMsg('Your session has expired. Please sign in again before upgrading.');
      return;
    }

    setIsProcessing(true);
    const result = await createSubscription();
    setIsProcessing(false);

    if (!result.success || !result.approvalUrl) {
            setErrorMsg(result.message ?? 'Could not start checkout. Please try again in a moment.');
      return;
    }

    setStep('redirecting');
    window.location.href = result.approvalUrl;
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="bg-[#0b0d14] border border-[#ff003b]/50 rounded-2xl w-full max-w-lg overflow-hidden">
        {/* Close Button */}
        <button
          onClick={onClose}
          className="absolute top-4 right-4 p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer border border-slate-700 z-10"
        >
          <X className="w-4 h-4" />
        </button>

        {step === 'review' && (
          <div className="p-6 sm:p-8">
            {/* Header */}
            <div className="text-center mb-6">
              <h3 className="text-2xl font-extrabold text-white font-['Rajdhani',sans-serif] uppercase tracking-wider">
                Upgrade to TubeMail Gorilla Pro
              </h3>
              <p className="text-xs text-slate-400 font-mono mt-1">
                ${planPrice}/mo &middot; 14-day free trial &middot; Cancel anytime
              </p>
            </div>

            {/* Price */}
            <div className="text-center mb-6 pb-4 border-b border-slate-800">
              <span className="text-5xl font-extrabold text-white font-mono">${planPrice}</span>
              <span className="text-slate-400 text-sm font-mono">/month</span>
            </div>

            {/* PayPal Button */}
            <div className="space-y-3">
              <button
                type="button"
                onClick={handleStartPayPalFlow}
                disabled={isProcessing || step === 'redirecting'}
                className="w-full py-4 rounded-xl bg-[#0070ba] hover:bg-[#003087] disabled:opacity-60 text-white font-extrabold uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-lg shadow-[#0070ba]/30 transition-all flex items-center justify-center gap-2.5 cursor-pointer"
              >
                {step === 'redirecting' ? (
                  <>
                    <RefreshCw className="w-4 h-4 animate-spin" />
                    <span className="font-mono text-xs font-bold uppercase">REDIRECTING TO PAYPAL...</span>
                  </>
                ) : (
                  <span className="font-mono text-xs font-bold uppercase tracking-wider">SUBSCRIBE WITH PAYPAL</span>
                )}
              </button>

              {errorMsg && (
                <div className="p-2.5 rounded-lg bg-rose-500/10 border border-rose-500/40 text-rose-300 text-xs font-mono">
                  {errorMsg}
                </div>
              )}

              <div className="flex items-center justify-center gap-2 text-[11px] text-slate-400 font-mono">
                <Lock className="w-3 h-3 text-slate-500" />
                <span>Secure PayPal billing &middot; Cancel anytime</span>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
