# TubeMailGorilla.Web

Next.js (App Router) website for TubeMailGorilla — handles account login, registration and (PayPal-powered) subscription upgrades/cancellation against the shared .NET API.

## Run it

```bash
# 1. Ensure the .NET API is running
dotnet run --project ../TubeMailGorilla.Api --urls http://localhost:5076

# 2. This site
npm install
npm run dev        # http://localhost:3000 (hot reload)
# or `npm run build && npm start` for production
```

## Environment (.env.local)

| Variable | Purpose |
|---|---|
| `API_BASE_URL` | .NET API origin (default `https://api.tubemailgorilla.xyz`) |
| `SITE_URL` | Public origin of this site, used to build PayPal return URLs |

## How it works

- **Auth:** `/register` and `/login` server-actions call the .NET API JWT endpoints. The token is stored in an **httpOnly cookie** (`tmg_token`) and never exposed to browser JS. `src/middleware.ts` protects `/account`.
- **Shared identity:** the site uses the same MySQL Identity DB as the desktop app — one account works everywhere.
- **Payments (recurring):** `/account` → *Upgrade* → server calls `POST /api/payments/create` (creates a PayPal subscription against `PayPalSettings:PlanId`) → redirects to PayPal approval → PayPal returns to `/subscription/capture?token=<subId>` → the site captures/activates and swaps the refreshed premium JWT into the cookie. *Cancel* calls `POST /api/payments/cancel`, which cancels the recurring billing **at PayPal** before removing local access.

## Setting up the PayPal billing plan (required once)

PayPal recurring payments need a billing **Plan** (a `P-...` id) that is created once and pinned in the API's `appsettings.json` under `PayPalSettings:PlanId`. Do NOT auto-provision plans at runtime.

1. **Enable Subscriptions** for your PayPal REST app:
   developer.paypal.com → **Apps & Credentials** → open/edit the app → tick **Subscriptions**.
   (Until enabled, PayPal's Subscriptions API returns an empty `404`.)
2. **Create the plan** — from the repo root:
   ```powershell
   powershell -ExecutionPolicy Bypass -File create-paypal-plan.ps1
   ```
   This creates the Product + monthly Plan at the price in `appsettings.json` (`Pricing:Amount`) and writes the plan id into `PayPalSettings:PlanId` automatically.

Once the id is set, upgrading in the app or website subscribes buyers to that exact plan and cancellation stops recurring charges at PayPal.