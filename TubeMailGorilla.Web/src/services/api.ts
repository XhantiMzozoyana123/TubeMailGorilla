export async function api<T>(
  path: string,
  options: { method?: string; body?: unknown; auth?: boolean } = {}
): Promise<ApiResult<T>> {
  const headers: Record<string, string> = {};
  if (options.body !== undefined) headers['Content-Type'] = 'application/json';
  if (options.auth) {
    const token = loadJwt();
    if (token) headers['Authorization'] = `Bearer ${token}`;
  }

  try {
    const res = await fetch(`${API_BASE_URL}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    });
    const text = await res.text();
    let data: T | null = null;
    try { data = text ? (JSON.parse(text) as T) : null; } catch { data = null; }
    return { ok: res.ok, status: res.status, data };
  } catch {
    return { ok: false, status: 0, data: null }; // API unreachable
  }
}

// ---------------- Auth ----------------

export async function loginRequest(email: string, password: string): Promise<{ user: UserProfile | null; error: string | null }> {
  const res = await api<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: { email, password },
  });

  if (!res.ok || !res.data?.success || !res.data.token) {
    return { user: null, error: res.data?.message ?? 'Login failed. Check your credentials or try again later.' };
  }

  saveJwt(res.data.token);
  return { user: buildUserFromAuth(email, res.data.token), error: null };
}

export async function registerRequest(fullName: string, email: string, password: string): Promise<{ user: UserProfile | null; error: string | null }> {
  if (password.length < 6) {
    return { user: null, error: 'Password must be at least 6 characters.' };
  }

  const res = await api<AuthResponse>('/api/auth/register', {
    method: 'POST',
    body: { email, password, fullName },
  });

  if (!res.ok || !res.data?.success || !res.data.token) {
    return { user: null, error: res.data?.message ?? 'Registration failed.' };
  }

  saveJwt(res.data.token);
  return { user: buildUserFromAuth(email, res.data.token, fullName), error: null };
}

export function logoutRequest(): void {
  clearJwt();
}

export const generateDesktopKey = (): string => {
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  const seg1 = Array.from({ length: 4 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
  const seg2 = Array.from({ length: 4 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
  const seg3 = Array.from({ length: 3 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
  return `TMG-${seg1}-${seg2}-${seg3}`;
};

/** Build the local UserProfile shell after a successful real auth call. */
function buildUserFromAuth(email: string, token: string, name?: string): UserProfile {
  const now = new Date();
  return {
    id: 'usr_tmg_' + Math.floor(10000 + Math.random() * 90000),
    name: name || email.split('@')[0].charAt(0).toUpperCase() + email.split('@')[0].slice(1),
    email,
    role: 'editor',
    software: 'Adobe Premiere Pro',
    niche: 'Gaming & Tech',
    token,
    tokenExpiresAt: new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000).toISOString(),
    desktopPairingKey: generateDesktopKey(),
    desktopAppSynced: true,
    createdAt: now.toISOString(),
    subscription: {
      tier: 'trial',
      status: 'trialing',
      planId: PAYPAL_PLAN_ID,
      amount: 0,
      currency: 'USD',
      interval: 'month',
      startedAt: now.toISOString(),
      renewsAt: new Date(now.getTime() + 14 * 24 * 60 * 60 * 1000).toISOString(),
      leadsExtractedThisMonth: 0,
      leadsLimit: 100,
    },
  };
}
import { UserProfile } from '../types';

// ---- Environment (Vite-style env vars, see .env.example) ----
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5076';
export const SITE_URL = import.meta.env.VITE_SITE_URL ?? window.location.origin;
export const PAYPAL_PLAN_ID = import.meta.env.VITE_PAYPAL_PLAN_ID ?? 'P-52B26258BH047653MNKKJTXY';

/**
 * Real HTTP client for the TubeMailGorilla .NET API (same contract the
 * Next.js server layer used):
 *   POST /api/auth/login     { email, password }            -> { success, message?, token? }
 *   POST /api/auth/register  { email, password, fullName }  -> { success, message?, token? }
 *   POST /api/payments/create { returnUrl, cancelUrl }      -> { success, orderId?, approvalUrl?, message? }
 *   POST /api/payments/capture { orderId } (Bearer)         -> { success, isSubscribed, message?, token? }
 *   POST /api/payments/cancel  (Bearer)                     -> { success, message? }
 */
export interface ApiResult<T> {
  ok: boolean;
  status: number;
  data: T | null;
}

export interface AuthResponse {
  success: boolean;
  message?: string;
  token?: string;
}

export interface CreatePaymentResponse {
  success: boolean;
  orderId?: string;
  approvalUrl?: string;
  message?: string;
}

export interface CapturePaymentResponse {
  success: boolean;
  isSubscribed: boolean;
  message?: string;
  token?: string;
}

const TOKEN_STORAGE_KEY = 'tmg_jwt';

export const saveJwt = (token: string): void => {
  try { localStorage.setItem(TOKEN_STORAGE_KEY, token); } catch {}
};

export const loadJwt = (): string | null => {
  try { return localStorage.getItem(TOKEN_STORAGE_KEY); } catch { return null; }
};

export const clearJwt = (): void => {
  try { localStorage.removeItem(TOKEN_STORAGE_KEY); } catch {}
};

// ---------------- Payments ----------------

/** A subscription plan as configured in the backend's appsettings.json. */
export interface SubscriptionPlanInfo {
  id: string;
  name: string;
  tagline: string;
  monthlyPrice: number;
  currency: string;
  leadsPerMonth: number;
  features: string[];
}

/**
 * Fetch the live plan catalog from the server. Falls back to null if the
 * API is unreachable — callers should keep showing their local defaults.
 */
export async function fetchPlans(): Promise<SubscriptionPlanInfo[]> {
  try {
    const res = await api<SubscriptionPlanInfo[]>('/api/payments/plans', { method: 'GET' });
    return res.data ?? [];
  } catch {
    return [];
  }
}

/** The signed-in user's current subscription, as stored on the server. */
export interface MySubscriptionStatus {
  isSubscribed: boolean;
  planId: string;
  planName: string;
  tagline?: string;
  price: number;
  currency: string;
  nextBillingDate?: string | null;
}

/** Fetch the authoritative subscription state for the signed-in user. */
export async function fetchMySubscription(): Promise<MySubscriptionStatus | null> {
  try {
    const res = await api<MySubscriptionStatus>('/api/payments/status', { method: 'GET', auth: true });
    return res.data ?? null;
  } catch {
    return null;
  }
}

/** Start a PayPal subscription: returns PayPal's approval URL to redirect to. */
export async function createSubscription(): Promise<CreatePaymentResponse> {
  const res = await api<CreatePaymentResponse>('/api/payments/create', {
    method: 'POST',
    auth: true,
    body: {
      // After approval PayPal drops the buyer back here; App.tsx detects the
      // subscription_id query param and captures server-side.
      returnUrl: `${SITE_URL}/?paypal=return`,
      cancelUrl: `${SITE_URL}/?paypal=cancel`,
    },
  });
  return res.data ?? { success: false, message: 'Could not reach the payment API.' };
}

/** Capture the approved subscription; refreshes the JWT with premium claims. */
export async function captureSubscription(subscriptionId: string): Promise<CapturePaymentResponse> {
  const res = await api<CapturePaymentResponse>('/api/payments/capture', {
    method: 'POST',
    auth: true,
    body: { orderId: subscriptionId },
  });
  return res.data ?? { success: false, isSubscribed: false, message: 'Could not reach the payment API.' };
}

/** Cancel the active subscription at the .NET API. */
export async function cancelSubscription(): Promise<{ success: boolean; message?: string }> {
  const res = await api<{ success: boolean; message?: string }>('/api/payments/cancel', {
    method: 'POST',
    auth: true,
  });
  return res.data ?? { success: false, message: 'Could not reach the payment API.' };
};