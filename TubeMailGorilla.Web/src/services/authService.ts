import { UserProfile, SubscriptionDetails, PaymentTransaction, ApiEnvironmentState } from '../types';
import { PAYPAL_PLAN_ID, API_BASE_URL, SITE_URL } from './api';

export const DEFAULT_PLAN_ID = PAYPAL_PLAN_ID;
export const DEFAULT_API_BASE_URL = API_BASE_URL;
export const DEFAULT_SITE_URL = SITE_URL;

const STORAGE_KEY = 'tmg_web_user_profile';
const API_STATE_KEY = 'tmg_api_env_state';

export { generateDesktopKey } from './api';

const INITIAL_USER: UserProfile = {
  id: 'usr_tmg_98421',
  name: 'Alex Vance',
  email: 'alex.editor@tmgorilla.io',
  avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
  role: 'editor',
  software: 'Adobe Premiere Pro & After Effects',
  niche: 'Gaming & Tech Creators',
  token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c3JfdG1nXzk4NDIxIiwicm9sZSI6InBybyIsImlzcyI6InR1YmVtYWlsLWdvcmlsbGEtYXBpIiwiZXhwIjoxNzU2MDAwMDAwfQ.tmg_signature_hash_shared_db',
  tokenExpiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
  desktopPairingKey: 'TMG-8942-ELITE-77X',
  desktopAppSynced: true,
  createdAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(),
  subscription: {
    tier: 'trial',
    status: 'trialing',
    planId: DEFAULT_PLAN_ID,
    amount: 0,
    currency: 'USD',
    interval: 'month',
    startedAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    renewsAt: new Date(Date.now() + 11 * 24 * 60 * 60 * 1000).toISOString(),
    leadsExtractedThisMonth: 18,
    leadsLimit: 100,
  }
};

export const DEFAULT_TRANSACTIONS: PaymentTransaction[] = [
  {
    id: 'tx_pp_9182301',
    date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    amount: 0.00,
    currency: 'USD',
        description: 'Free plan started',
    paypalCaptureId: 'PP-SETUP-TOKEN-9941',
    status: 'COMPLETED'
  }
];

export const getStoredUser = (): UserProfile | null => {
  try {
    const data = localStorage.getItem(STORAGE_KEY);
    if (!data) return null; // No stored session -> visitor is logged out
    return JSON.parse(data);
  } catch (err) {
    return null;
  }
};

export const saveStoredUser = (user: UserProfile | null): void => {
  if (!user) {
    localStorage.removeItem(STORAGE_KEY);
  } else {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
  }
};

export const getApiEnvironment = (): ApiEnvironmentState => {
  try {
    const data = localStorage.getItem(API_STATE_KEY);
    if (data) return JSON.parse(data);
  } catch (e) {}

  return {
    apiBaseUrl: DEFAULT_API_BASE_URL,
    siteUrl: DEFAULT_SITE_URL,
    paypalPlanId: DEFAULT_PLAN_ID,
    status: 'connected',
    lastPingMs: 24
  };
};

export const saveApiEnvironment = (env: ApiEnvironmentState): void => {
  localStorage.setItem(API_STATE_KEY, JSON.stringify(env));
};
