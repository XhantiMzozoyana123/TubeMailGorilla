/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useEffect } from 'react';
import { Navbar } from './components/Navbar';
import { Hero } from './components/Hero';
import { BeforeAfter } from './components/BeforeAfter';
import { SocialProof } from './components/SocialProof';
import { BuiltForEditors } from './components/BuiltForEditors';
import { FeaturesShowcase } from './components/FeaturesShowcase';
import { HowItWorks } from './components/HowItWorks';
import { SecureAndPrivate } from './components/SecureAndPrivate';
import { RevenueCalculator } from './components/RevenueCalculator';
import { FaqSection } from './components/FaqSection';
import { FinalCta } from './components/FinalCta';
import { Footer } from './components/Footer';
import { FreeTrialModal } from './components/FreeTrialModal';
import { PitchGeneratorModal } from './components/PitchGeneratorModal';
import { ProspectListDrawer } from './components/ProspectListDrawer';
import { PitchTemplatesDrawer } from './components/PitchTemplatesDrawer';
import { CreatorLead, ProspectItem, UserProfile, SubscriptionDetails, SubscriptionTier } from './types';
import { MOCK_CREATORS } from './data/mockCreators';
import { getStoredUser, saveStoredUser } from './services/authService';
import { captureSubscription, saveJwt, loadJwt, logoutRequest } from './services/api';
import { AuthModal } from './components/AuthModal';
import { AccountPortal } from './components/AccountPortal';
import { PayPalCheckoutModal } from './components/PayPalCheckoutModal';
import { SubscriptionPage } from './components/SubscriptionPage';

const getInitialView = (): 'home' | 'subscription' =>
  typeof window !== 'undefined' && window.location.hash === '#/subscription' ? 'subscription' : 'home';

export default function App() {
  const [user, setUser] = useState<UserProfile | null>(() => getStoredUser());
  const [isAuthOpen, setIsAuthOpen] = useState(false);
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [isAccountOpen, setIsAccountOpen] = useState(false);
    const [isPayPalCheckoutOpen, setIsPayPalCheckoutOpen] = useState(false);
  const [checkoutTier, setCheckoutTier] = useState<SubscriptionTier>('pro');
  const [view, setView] = useState<'home' | 'subscription'>(getInitialView());


  const [isTrialOpen, setIsTrialOpen] = useState(false);
  const [isProspectDrawerOpen, setIsProspectDrawerOpen] = useState(false);
  const [isPitchDrawerOpen, setIsPitchDrawerOpen] = useState(false);
  const [activePitchCreator, setActivePitchCreator] = useState<CreatorLead | null>(null);
  
  // Seed with 2 initial sample prospects to showcase the pipeline immediately
  const [savedLeads, setSavedLeads] = useState<ProspectItem[]>(() => {
    return [
      {
        ...MOCK_CREATORS[0],
        status: 'Saved',
        savedAt: new Date().toISOString()
      },
      {
        ...MOCK_CREATORS[1],
        status: 'Emailed',
        savedAt: new Date().toISOString()
      }
    ];
  });

  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const [isProcessingPayment, setIsProcessingPayment] = useState(false);

  const showToast = (msg: string) => {
    setToastMessage(msg);
    setTimeout(() => setToastMessage(null), 3500);
  };

  const handleOpenAuth = (mode: 'login' | 'register') => {
    setAuthMode(mode);
    setIsAuthOpen(true);
  };

  const handleAuthSuccess = (authenticatedUser: UserProfile, message: string) => {
    setUser(authenticatedUser);
    saveStoredUser(authenticatedUser);
    showToast(message);
  };

  const handleUpdateUser = (updatedUser: UserProfile) => {
    setUser(updatedUser);
    saveStoredUser(updatedUser);
    showToast('Account details & credentials updated.');
  };

  const handleLogout = () => {
    setUser(null);
    saveStoredUser(null);
    logoutRequest(); // clears the persisted JWT
    setIsAccountOpen(false);
    showToast('Logged out. Session cookie invalidated.');
  };

  // ---- PayPal return flow -----------------------------------------------
  // After the buyer approves the subscription on PayPal, they are redirected
  // back here with ?subscription_id=... (or ?token=...). We capture it at the
  // .NET API (POST /api/payments/capture), which activates the subscription
  // and returns a refreshed premium JWT.
  React.useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const paypalFlag = params.get('paypal');
    const subscriptionId = params.get('subscription_id') ?? params.get('token') ?? '';

    if (!paypalFlag) return;

    // Strip query params immediately so refresh doesn't double-capture.
    window.history.replaceState({}, '', window.location.pathname);

    if (paypalFlag === 'cancel') {
      showToast('PayPal checkout cancelled — no charge was made.');
      return;
    }

    if (!loadJwt()) {
      showToast('Session expired during checkout. Please sign in again.');
      return;
    }

    setIsProcessingPayment(true);
    captureSubscription(subscriptionId).then((res) => {
      setIsProcessingPayment(false);
      if (res.success && res.token) {
        saveJwt(res.token); // refreshed JWT carries premium claims
        setUser((prev) => {
          if (!prev) return prev;
          const upgraded: UserProfile = {
            ...prev,
            token: res.token!,
            subscription: {
              ...prev.subscription,
                            tier: 'pro',
              status: 'active',
              amount: 9.99,
              interval: 'month',
              startedAt: new Date().toISOString(),
              renewsAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
              lastPaymentAt: new Date().toISOString(),
            },
          };
          saveStoredUser(upgraded);
          return upgraded;
        });
        showToast('⚡ Payment captured! Your Pro subscription is now active.');
      } else {
        showToast(res.message ?? 'Payment could not be completed.');
      }
    });
        // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Hash-based routing so a standalone /#/subscription page can render.
  React.useEffect(() => {
    const update = () => setView(getInitialView());
    update();
    window.addEventListener('hashchange', update);
    return () => window.removeEventListener('hashchange', update);
  }, []);

  const handleOpenUpgradeModal = (tier: SubscriptionTier = 'pro') => {
    if (!user) {
      setAuthMode('register');
      setIsAuthOpen(true);
      return;
    }
    setCheckoutTier(tier);
    setIsPayPalCheckoutOpen(true);
  };

  const handlePaymentSuccess = (newSub: SubscriptionDetails) => {
    if (user) {
      const updatedUser: UserProfile = {
        ...user,
        subscription: newSub,
      };
      setUser(updatedUser);
      saveStoredUser(updatedUser);
      showToast(`⚡ Subscribed to ${newSub.tier.toUpperCase()} via PayPal Plan ${newSub.planId}!`);
    }
  };

  const handleSaveLead = (creator: CreatorLead) => {
    if (savedLeads.some((l) => l.id === creator.id)) {
      setSavedLeads(savedLeads.filter((l) => l.id !== creator.id));
      showToast(`Removed ${creator.channelName} from your prospect list`);
    } else {
      const newProspect: ProspectItem = {
        ...creator,
        status: 'Saved',
        savedAt: new Date().toISOString(),
      };
      setSavedLeads([newProspect, ...savedLeads]);
            showToast(`Added ${creator.channelName} to your prospect pipeline!`);
    }
  };

  const handleUpdateStatus = (id: string, status: ProspectItem['status']) => {
    setSavedLeads(
      savedLeads.map((item) => (item.id === id ? { ...item, status } : item))
    );
    showToast(`Updated stage to "${status}"`);
  };

  const handleRemoveLead = (id: string) => {
    setSavedLeads(savedLeads.filter((l) => l.id !== id));
    showToast('Prospect removed');
  };

  const handleOpenPitchModal = (creator: CreatorLead) => {
    setActivePitchCreator(creator);
  };

  const scrollToDemo = () => {
    const el = document.getElementById('features-showcase');
    if (el) {
      el.scrollIntoView({ behavior: 'smooth' });
    }
  };

  return (
    <div className="min-h-screen bg-[#07080b] text-slate-100 font-['Plus_Jakarta_Sans',sans-serif] selection:bg-[#ff003b] selection:text-white">
      {/* Toast Notification */}
      {toastMessage && (
        <div className="fixed bottom-6 right-6 z-50 bg-[#12151f] border border-[#ff003b]/60 text-white px-4 py-3 rounded-xl shadow-[0_0_25px_rgba(255,0,59,0.3)] flex items-center gap-3 animate-in slide-in-from-bottom-5 duration-300">
                              <span className="text-lg"><img src="/src/images/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-6 h-6 object-contain" /></span>
          <span className="text-sm font-semibold">{toastMessage}</span>
        </div>
      )}

      {/* Main Navigation */}
      <Navbar
        onOpenTrial={() => {
          if (user) {
            setIsAccountOpen(true);
          } else {
            handleOpenAuth('register');
          }
        }}
        savedCount={savedLeads.length}
        onOpenProspectDrawer={() => setIsProspectDrawerOpen(true)}
        user={user}
        onOpenAuth={handleOpenAuth}
        onOpenAccount={() => setIsAccountOpen(true)}
      />

            {/* Main Website Sections */}
      {view === 'subscription' ? (
        <SubscriptionPage
          isLoggedIn={!!user}
          currentTier={user ? user.subscription.tier : 'trial'}
          onOpenUpgrade={handleOpenUpgradeModal}
          onOpenRegister={() => handleOpenAuth('register')}
        />
      ) : (
        <main id="main-content">
          {/* 1. BIG PROMISE */}
          <Hero
            onOpenTrial={() => {
              if (user) {
                setIsAccountOpen(true);
              } else {
                handleOpenAuth('register');
              }
            }}
            onScrollToDemo={scrollToDemo}
          />

          {/* 2. BEFORE / AFTER — instant value comprehension */}
          <BeforeAfter onOpenTrial={() => handleOpenAuth('register')} />

          {/* 3. SHOW THE PRODUCT — demo videos + live extraction demos */}
          <FeaturesShowcase onOpenTrial={() => handleOpenAuth('register')} />

          {/* 4. HOW IT WORKS — Search → Extract → Pitch → Close */}
          <HowItWorks
            onOpenTrial={() => handleOpenAuth('register')}
            onOpenPitchDrawer={() => setIsPitchDrawerOpen(true)}
          />

          {/* 5. PROOF — beta results */}
          <SocialProof onOpenTrial={() => handleOpenAuth('register')} />

          {/* 6. POSITIONING — marketplace vs direct outreach */}
          <BuiltForEditors onOpenTrial={() => handleOpenAuth('register')} />

          {/* 7. VALUE MATH — softened income framing + disclaimer */}
          <RevenueCalculator onOpenTrial={() => handleOpenUpgradeModal('pro')} />

          {/* 8. OBJECTION HANDLING — FAQ (incl. GPU requirements & skepticism) */}
          <FaqSection onOpenTrial={() => handleOpenAuth('register')} />

          {/* 9. TECHNOLOGY TRUST — privacy/local-first, moved below the outcome sell */}
          <SecureAndPrivate />

          {/* 10. FINAL CTA */}
          <FinalCta onOpenTrial={() => handleOpenAuth('register')} />
        </main>
      )}

      {/* Footer */}
      <Footer onOpenTrial={() => handleOpenAuth('register')} />

      {/* Modals and Side Drawers */}
      <AuthModal
        isOpen={isAuthOpen}
        initialMode={authMode}
        onClose={() => setIsAuthOpen(false)}
        onAuthSuccess={handleAuthSuccess}
      />

      {user && (
        <AccountPortal
          isOpen={isAccountOpen}
          onClose={() => setIsAccountOpen(false)}
          user={user}
          onUpdateUser={handleUpdateUser}
          onLogout={handleLogout}
          onOpenUpgradeModal={() => {
            setIsAccountOpen(false);
            handleOpenUpgradeModal('pro');
          }}
        />
      )}

      <PayPalCheckoutModal
        isOpen={isPayPalCheckoutOpen}
        onClose={() => setIsPayPalCheckoutOpen(false)}
        targetTier={checkoutTier}
        onPaymentSuccess={handlePaymentSuccess}
      />

      <FreeTrialModal
        isOpen={isTrialOpen}
        onClose={() => setIsTrialOpen(false)}
      />

      <PitchGeneratorModal
        creator={activePitchCreator}
        isOpen={!!activePitchCreator}
        onClose={() => setActivePitchCreator(null)}
      />

      <ProspectListDrawer
        isOpen={isProspectDrawerOpen}
        onClose={() => setIsProspectDrawerOpen(false)}
        savedLeads={savedLeads}
        onUpdateStatus={handleUpdateStatus}
        onRemoveLead={handleRemoveLead}
        onOpenPitch={handleOpenPitchModal}
        onOpenTrial={() => handleOpenAuth('register')}
      />

      <PitchTemplatesDrawer
        isOpen={isPitchDrawerOpen}
        onClose={() => setIsPitchDrawerOpen(false)}
        onOpenTrial={() => handleOpenAuth('register')}
      />
    </div>
  );
}

