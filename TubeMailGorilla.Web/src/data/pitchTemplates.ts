import { PitchTemplate } from '../types';

export const PITCH_TEMPLATES: PitchTemplate[] = [
  {
    id: 'retention-hook-pitch',
    title: 'The 30-Second Retention Boost Pitch',
    category: 'Long-Form YouTube',
    bestFor: 'Talking heads, Tech, Finance & Educational creators with high view potential',
    conversionRate: '24% reply rate',
    subject: 'Quick idea for your next video pacing on {{channelName}}',
    body: `Hey {{creatorName}},

Loved your recent video "{{sampleVideoTitle}}" — the insight at 04:15 was spot on.

I noticed your intro jumped straight in, but according to current YouTube retention curves, adding a 4-second dynamic hook with sound design in the first 10 seconds can increase average view duration by 15-20%.

I'm a video editor specializing in high-retention pacing (Premiere & After Effects). I put together a quick 30-second sample re-edit of your last intro showing what this looks like with motion graphics and tighter cuts.

Would you be open to me sending over the unlisted link? No strings attached at all.

Best,
[Your Name]
Video Editor & Pacing Specialist`
  },
  {
    id: 'shorts-repurposing-pitch',
    title: 'The "Turn 1 Video into 5 Shorts" Pitch',
    category: 'Short-Form Repurposing',
    bestFor: 'Podcasters, Streamers, and Long-form vloggers missing out on YouTube Shorts',
    conversionRate: '31% reply rate',
    subject: 'Made 2 free YouTube Shorts from your last video (+ quick question)',
    body: `Hey {{creatorName}},

Big fan of {{channelName}}. I noticed you put out great 15+ minute videos, but aren't posting daily YouTube Shorts / TikToks to capture new subscribers.

I took the best 45-second punchline from "{{sampleVideoTitle}}", formatted it for 9:16 vertical, added animated captions, sound effects, and color grading.

You can preview the finished Short here: [Link to Google Drive / Loom]

Feel free to post it directly to your channel! If you like the style, I can edit 12-15 of these each month for a flat monthly retainer so you get free subscriber growth without any extra filming.

Let me know what you think!

Cheers,
[Your Name]`
  },
  {
    id: 'burnout-relief-pitch',
    title: 'The "Save 20 Hours a Week" Creator Retainer Pitch',
    category: 'Full Channel Management',
    bestFor: 'Creators publishing 4-8 times a month looking to stop editing themselves',
    conversionRate: '19% reply rate (Highest Retainer Value)',
    subject: 'Helping you save 15+ hours a week on {{channelName}} editing',
    body: `Hey {{creatorName}},

I've been following {{channelName}} for a while now. Noticed you're consistently dropping {{videosPerMonth}} videos a month — that's incredible output, but I know how brutal the editing grind gets behind the scenes.

I help YouTubers in the {{niche}} space take video editing 100% off their plate:
- 48-hour turnarounds on raw footage
- Custom sound design, motion graphics, and B-roll sourcing
- Formatted thumbnails & retention-optimized pacing

I'd love to edit your next video completely free so you can test my speed and quality with zero risk.

Are you open to trying a test project this week?

Best,
[Your Name]
Portfolio: [Your Portfolio Link]`
  }
];

export const FAQ_DATA = [
  {
    question: "Is TubeMail Gorilla only for video editors?",
    answer: "TubeMail Gorilla is designed with video editors in mind, particularly editors looking to proactively find YouTube creators who may need editing services."
  },
  {
    question: "How does TubeMail Gorilla find leads?",
    answer: "TubeMail Gorilla helps you search YouTube and extract relevant creator information so you can build targeted prospect lists."
  },
  {
    question: "Can TubeMail Gorilla guarantee me clients?",
    answer: "No. TubeMail Gorilla provides the prospecting tools. Your results depend on factors such as your offer, pricing, outreach, follow-up and ability to close clients."
  },
  {
    question: "How much can I make?",
    answer: "There's no guaranteed income — and anyone who promises one is lying to you. What we can tell you: video editing retainers on YouTube commonly range from $250 to $1,000+ per month per client, and TubeMail Gorilla helps you find the creators to pitch. Your actual earnings depend on your skills, pricing, and how many clients you close."
  },
  {
    question: "Do I need to be an experienced editor?",
    answer: "Not necessarily. However, you should have a legitimate video-editing service or skillset that you can offer to potential clients."
  },
  {
    question: "How quickly can I find leads?",
    answer: "You can begin searching for potential YouTube leads as soon as you start using TubeMail Gorilla."
  },
  {
    question: "What are the system requirements?",
    answer: "TubeMail Gorilla runs on Windows 10/11. The AI pitch generator uses your NVIDIA GPU (GTX 10-series or newer, ideally 6-8 GB+ VRAM like an RTX 3060/4060) for fast generation — but a CPU-only mode is included so the core search and email extraction features work on any modern machine. High-end GPUs (RTX 4070 Ti+) can batch-generate hundreds of icebreakers at once."
  },
  {
    question: "Is this a get-rich-quick scheme?",
    answer: "No. TubeMail Gorilla is prospecting software — it finds YouTube creators and their business contact info faster than doing it manually. It doesn't promise income. What you earn depends entirely on your editing skills, your pricing, your outreach quality, and your ability to close clients. We just remove the hours of manual searching between you and potential clients."
  },
  {
    question: "How does the email extraction work? Is it safe?",
    answer: "TubeMail Gorilla searches publicly available YouTube channel data and extracts business contact emails that creators have chosen to publish for partnership inquiries. All extracted data stays locally on your machine — it's never uploaded to our servers. You remain responsible for following applicable anti-spam laws (like CAN-SPAM) in your outreach."
  }
];
