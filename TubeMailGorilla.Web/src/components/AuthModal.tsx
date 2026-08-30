import React, { useState } from 'react';
import { X, Lock, Mail, User, Sparkles, Shield, CheckCircle2, ArrowRight, KeyRound, Cpu } from 'lucide-react';
import { UserProfile, SubscriptionTier } from '../types';
import { loginRequest, registerRequest } from '../services/api';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialMode?: 'login' | 'register';
  onAuthSuccess: (user: UserProfile, message: string) => void;
}

export const AuthModal: React.FC<AuthModalProps> = ({
  isOpen,
  onClose,
  initialMode = 'login',
  onAuthSuccess,
}) => {
  const [mode, setMode] = useState<'login' | 'register'>(initialMode);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [software, setSoftware] = useState('Adobe Premiere Pro');
  const [niche, setNiche] = useState('Gaming & Tech');
  const [rememberDevice, setRememberDevice] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

    if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    if (!email.trim() || !password) {
      setErrorMsg('Email and password are required.');
      return;
    }

    setIsLoading(true);

    // Real .NET API JWT authentication:
    //   POST {API}/api/auth/login     { email, password }
    //   POST {API}/api/auth/register  { email, password, fullName }
    // On success the JWT is persisted (localStorage) and attached to the
    // local user profile so every subsequent payment call sends it as a
    // Bearer token.
    const result = mode === 'login'
      ? await loginRequest(email.trim(), password)
      : await registerRequest(name.trim(), email.trim(), password);

    setIsLoading(false);

    if (result.error || !result.user) {
      setErrorMsg(result.error ?? 'Authentication failed. Please try again.');
      return;
    }

        onAuthSuccess(
      result.user,
      mode === 'login'
        ? `Welcome back, ${result.user.name}!`
        : `Account created! Free trial activated with 100 creator leads.`
    );
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-200">
      <div className="bg-[#0b0d14] border border-[#ff003b]/50 rounded-2xl w-full max-w-md shadow-[0_0_50px_rgba(255,0,59,0.3)] overflow-hidden relative">
        {/* Top Decorative Cyber Line */}
        <div className="h-1 w-full bg-gradient-to-r from-[#ff003b] via-[#ff4d73] to-[#ff003b]" />

        {/* Close Button */}
        <button
          onClick={onClose}
          className="absolute top-4 right-4 p-2 rounded-lg bg-[#151824] hover:bg-[#202538] text-slate-400 hover:text-white transition-colors cursor-pointer border border-slate-700 z-10"
        >
          <X className="w-4 h-4" />
        </button>

        <div className="p-6 sm:p-7">
          {/* Header */}
          <div className="text-center mb-6">
                        <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-[#ff003b] via-[#cc002f] to-[#800014] flex items-center justify-center text-2xl mx-auto mb-3 shadow-[0_0_20px_rgba(255,0,59,0.4)] border border-[#ff4d73]/40">
                            <img src="/tubemailgorilla-icon.svg" alt="TubeMail Gorilla" className="w-8 h-8 object-contain" />
            </div>
            <h3 className="text-2xl font-extrabold text-white font-['Rajdhani',sans-serif] uppercase tracking-wider">
              {mode === 'login' ? 'Account Login' : 'Register New Account'}
            </h3>
            <div className="flex items-center justify-center gap-1.5 mt-1">
              <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
                            <p className="text-xs text-slate-400 font-mono">
                Secure access for video editors
              </p>
            </div>
          </div>

          {/* Mode Switcher Tabs */}
          <div className="grid grid-cols-2 p-1 bg-[#141724] rounded-lg border border-slate-800 mb-5 font-['Rajdhani',sans-serif] text-sm tracking-wider uppercase font-bold">
            <button
              type="button"
              onClick={() => { setMode('login'); setErrorMsg(null); }}
              className={`py-2 rounded-md transition-all cursor-pointer ${
                mode === 'login'
                  ? 'bg-[#ff003b] text-white shadow-[0_0_15px_rgba(255,0,59,0.4)]'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              Sign In
            </button>
            <button
              type="button"
              onClick={() => { setMode('register'); setErrorMsg(null); }}
              className={`py-2 rounded-md transition-all cursor-pointer ${
                mode === 'register'
                  ? 'bg-[#ff003b] text-white shadow-[0_0_15px_rgba(255,0,59,0.4)]'
                  : 'text-slate-400 hover:text-white'
              }`}
            >
              Register Free
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {mode === 'register' && (
              <div>
                <label className="text-xs font-bold text-slate-300 block mb-1.5 font-mono uppercase text-[11px]">
                  Full Name / Video Editor Brand
                </label>
                <div className="relative">
                  <User className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <input
                    type="text"
                    required
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="e.g. Alex Vance"
                    className="w-full pl-10 pr-3.5 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:border-[#ff003b] focus:ring-1 focus:ring-[#ff003b] transition-colors font-mono"
                  />
                </div>
              </div>
            )}

            <div>
              <label className="text-xs font-bold text-slate-300 block mb-1.5 font-mono uppercase text-[11px]">
                Email Address
              </label>
              <div className="relative">
                <Mail className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="editor@domain.com"
                  className="w-full pl-10 pr-3.5 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:border-[#ff003b] focus:ring-1 focus:ring-[#ff003b] transition-colors font-mono"
                />
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="text-xs font-bold text-slate-300 font-mono uppercase text-[11px]">
                  Password
                </label>
                                {mode === 'login' && (
                  <span className="text-[11px] text-slate-500 font-mono">
                    Forgot your password? Contact support.
                  </span>
                )}
              </div>
              <div className="relative">
                <Lock className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••••••"
                  className="w-full pl-10 pr-3.5 py-2.5 bg-[#050608] border border-slate-700 rounded-lg text-sm text-white placeholder-slate-500 focus:outline-none focus:border-[#ff003b] focus:ring-1 focus:ring-[#ff003b] transition-colors font-mono"
                />
              </div>
            </div>

            {mode === 'register' && (
              <div className="grid grid-cols-2 gap-3 pt-1">
                <div>
                  <label className="text-xs font-bold text-slate-300 block mb-1 font-mono uppercase text-[10px]">
                    Editing Suite
                  </label>
                  <select
                    value={software}
                    onChange={(e) => setSoftware(e.target.value)}
                    className="w-full px-2.5 py-2 bg-[#050608] border border-slate-700 rounded-lg text-xs text-white focus:outline-none focus:border-[#ff003b] font-mono"
                  >
                    <option value="Adobe Premiere Pro">Premiere Pro</option>
                    <option value="DaVinci Resolve">DaVinci Resolve</option>
                    <option value="Final Cut Pro">Final Cut Pro</option>
                    <option value="CapCut Desktop">CapCut Desktop</option>
                  </select>
                </div>
                <div>
                  <label className="text-xs font-bold text-slate-300 block mb-1 font-mono uppercase text-[10px]">
                    Target Niche
                  </label>
                  <select
                    value={niche}
                    onChange={(e) => setNiche(e.target.value)}
                    className="w-full px-2.5 py-2 bg-[#050608] border border-slate-700 rounded-lg text-xs text-white focus:outline-none focus:border-[#ff003b] font-mono"
                  >
                    <option value="Gaming & Tech">Gaming & Tech</option>
                    <option value="Finance & Business">Finance</option>
                    <option value="Documentary & Cinema">Documentary</option>
                    <option value="Fitness & Lifestyle">Fitness</option>
                  </select>
                </div>
              </div>
            )}

            {/* Remember / Security note */}
            <div className="flex items-center justify-between text-xs pt-1 font-mono text-slate-400">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={rememberDevice}
                  onChange={(e) => setRememberDevice(e.target.checked)}
                  className="rounded bg-[#050608] border-slate-700 text-[#ff003b] focus:ring-0"
                />
                                <span>Keep me signed in on this device</span>
              </label>
            </div>

            {errorMsg && (
              <div className="p-2.5 rounded-lg bg-rose-500/10 border border-rose-500/40 text-rose-300 text-xs font-mono">
                {errorMsg}
              </div>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3.5 rounded-lg bg-[#ff003b] hover:bg-[#ff1a4b] disabled:opacity-50 text-white font-extrabold text-xs uppercase tracking-wider font-['Rajdhani',sans-serif] shadow-[0_0_20px_rgba(255,0,59,0.4)] transition-all flex items-center justify-center gap-2 cursor-pointer border border-[#ff4d73]"
            >
              {isLoading ? (
                <div className="flex items-center gap-2">
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                    <span>SIGNING YOU IN...</span>
                </div>
              ) : (
                <>
                  <span>{mode === 'login' ? 'SIGN IN TO ACCOUNT →' : 'ACTIVATE FREE ACCOUNT →'}</span>
                </>
              )}
            </button>
                    </form>
        </div>
      </div>
    </div>
  );
};
