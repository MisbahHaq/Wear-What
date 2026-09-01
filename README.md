<div align="center">
<img width="1200" height="475" alt="GHBanner" src="https://ai.google.dev/static/site-assets/images/share-ais-513315318.png" />
</div>

# Wear What

**Wear What** is an editorial wardrobe-planning web app that helps you plan your outfits for the week at a glance. It blends high-impact color blocking with a weather-aware outfit engine so you never have to second-guess what to wear.

## What it does

- **Weekly Outfit Planner** — Build a 7-day outfit plan from the clothes in your digital closet. Each day picks a top, bottom, and dress that respect your style and haven't been worn together recently.
- **Weather Adaptation** — Fetches a 7-day forecast for your city and filters your wardrobe by season (summer/winter) so suggested outfits are actually wearable.
- **Digital Closet** — Add clothing items with photos, categorize them (tops, bottoms, dresses, shoes, accessories) and filter the view by category or style class.
- **Flat-Lay Scrapbook** — Compose flat-lay collage mood boards with your outfit picks, doodle stickers, washi-tape frames, and confetti accents — then view them in an editorial grid layout.
- **Style Vibe Check** — An AI-powered lookbook generator (via the Gemini API) that produces styled editorial mood scenes for your curated outfits.
- **Quick Swap** — Instantly swap one piece of an outfit for another option without regenerating the whole plan.
- **Auth & Sync** — Sign in with Google and sync your wardrobe and weekly plan across devices with Firestore. Local storage fallback keeps everything working offline too.

## Stack

| Layer            | Tech                                                  |
|------------------|-------------------------------------------------------|
| Framework        | React 19 + TypeScript                                 |
| Build / Dev      | Vite                                                  |
| Styling          | Tailwind CSS v4                                       |
| Auth & Database  | Firebase Auth (Google) + Firestore                    |
| AI               | Google GenAI SDK (`@google/genai`) — server-side calls|
| Animations       | Framer Motion, canvas-confetti                        |
| Icons            | Lucide React                                          |

Deployed on **Google Cloud Run** via AI Studio, with the Gemini API key injected at runtime from user secrets.

## Run Locally

**Prerequisites:** Node.js

1. Install dependencies:
   `npm install`
2. Set the `GEMINI_API_KEY` in [.env.local](.env.local) to your Gemini API key
3. Run the app:
   `npm run dev`
