export interface CreatorLead {
  id: string;
  channelName: string;
  handle: string;
  avatar: string;
  subscribers: string;
  subscribersCount: number;
  niche: string;
  category: 'Gaming' | 'Tech' | 'Finance' | 'Fitness' | 'Vlogs' | 'Education' | 'Documentary';
  videosPerMonth: number;
  averageViews: string;
  email: string;
  emailVerified: boolean;
  editingNeedScore: number; // 1-100 score indicating high demand for an editor
  currentEditingStyle: string;
  recommendedPitchAngle: string;
  socials?: {
    twitter?: string;
    instagram?: string;
  };
  sampleVideoTitle: string;
  painPoint: string;
}

export interface PitchTemplate {
  id: string;
  title: string;
  category: string;
  subject: string;
  body: string;
  bestFor: string;
  conversionRate: string;
}

export interface ProspectItem extends CreatorLead {
  status: 'Saved' | 'Emailed' | 'In Discussion' | 'Closed Client';
  savedAt: string;
  customNotes?: string;
}

export interface FaqItem {
  question: string;
  answer: string;
}

export type SubscriptionTier = 'trial' | 'pro' | 'agency';
export type SubscriptionStatus = 'active' | 'pending' | 'canceled' | 'trialing' | 'past_due';

export interface SubscriptionDetails {
  tier: SubscriptionTier;
  status: SubscriptionStatus;
  planId: string;
  paypalSubscriptionId?: string;
  amount: number;
  currency: string;
  interval: 'month' | 'year';
  startedAt: string;
  renewsAt: string;
  leadsExtractedThisMonth: number;
  leadsLimit: number;
  lastPaymentAt?: string;
}

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  role: 'editor' | 'agency_owner' | 'admin';
  software: string;
  niche: string;
  token: string; // JWT token (simulated tmg_token)
  tokenExpiresAt: string;
  desktopPairingKey: string;
  desktopAppSynced: boolean;
  createdAt: string;
  subscription: SubscriptionDetails;
}

export interface PaymentTransaction {
  id: string;
  date: string;
  amount: number;
  currency: string;
  description: string;
  paypalCaptureId: string;
  status: 'COMPLETED' | 'PENDING' | 'REFUNDED';
}

export interface ApiEnvironmentState {
  apiBaseUrl: string;
  siteUrl: string;
  paypalPlanId: string;
  status: 'connected' | 'offline' | 'checking';
  lastPingMs: number;
}
