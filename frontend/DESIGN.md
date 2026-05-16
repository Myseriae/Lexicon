---
name: Lexicon Scholarly System
colors:
  surface: '#faf9f8'
  surface-dim: '#dadad9'
  surface-bright: '#faf9f8'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f4f3f2'
  surface-container: '#eeeeed'
  surface-container-high: '#e9e8e7'
  surface-container-highest: '#e3e2e1'
  on-surface: '#1a1c1c'
  on-surface-variant: '#43474c'
  inverse-surface: '#2f3130'
  inverse-on-surface: '#f1f0f0'
  outline: '#74777c'
  outline-variant: '#c4c6cc'
  surface-tint: '#506071'
  primary: '#051625'
  on-primary: '#ffffff'
  primary-container: '#1b2b3a'
  on-primary-container: '#8292a5'
  inverse-primary: '#b8c8dc'
  secondary: '#566253'
  on-secondary: '#ffffff'
  secondary-container: '#d7e3d1'
  on-secondary-container: '#5a6657'
  tertiary: '#221101'
  on-tertiary: '#ffffff'
  tertiary-container: '#392510'
  on-tertiary-container: '#a98b6f'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d3e4f8'
  primary-fixed-dim: '#b8c8dc'
  on-primary-fixed: '#0c1d2b'
  on-primary-fixed-variant: '#384858'
  secondary-fixed: '#d9e6d4'
  secondary-fixed-dim: '#bdcab8'
  on-secondary-fixed: '#141e13'
  on-secondary-fixed-variant: '#3e4a3c'
  tertiary-fixed: '#ffdcbe'
  tertiary-fixed-dim: '#e2c0a2'
  on-tertiary-fixed: '#291805'
  on-tertiary-fixed-variant: '#5a422b'
  background: '#faf9f8'
  on-background: '#1a1c1c'
  surface-variant: '#e3e2e1'
typography:
  display-lg:
    fontFamily: EB Garamond
    fontSize: 48px
    fontWeight: '600'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: EB Garamond
    fontSize: 36px
    fontWeight: '600'
    lineHeight: '1.2'
  headline-md:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '500'
    lineHeight: '1.3'
  body-reading:
    fontFamily: EB Garamond
    fontSize: 20px
    fontWeight: '400'
    lineHeight: '1.6'
  body-reading-mobile:
    fontFamily: EB Garamond
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  ui-label-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '600'
    lineHeight: '1.4'
    letterSpacing: 0.01em
  ui-label-sm:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '500'
    lineHeight: '1.2'
  caption:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.4'
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  base: 8px
  reading-column-max: 720px
  gutter-desktop: 32px
  margin-desktop: 64px
  margin-mobile: 20px
---

## Brand & Style

The design system is rooted in the "Digital Library" aesthetic—an environment that prioritizes deep focus, intellectual authority, and the tactile heritage of printed media. It is designed for an audience of researchers, students, and long-form readers who value substance over distraction.

The style is a sophisticated blend of **Minimalism** and **Modern Editorial**. It leverages heavy white space to mimic wide book margins and high-contrast typography to ensure absolute legibility. By utilizing a "paper-first" philosophy, the UI retreats into the background, allowing the content to serve as the primary interface. The emotional response should be one of calm, reliability, and academic rigor.

## Colors

The palette is inspired by traditional archival materials.
- **Primary (Navy):** A deep, scholarly navy (#1B2B3A) used for primary actions, links, and branding elements to signal authority.
- **Secondary (Library Green):** A muted, organic green (#606C5D) for success states or subtle callouts.
- **Neutral (Paper):** The foundation of the system is #FDFCFB, a warm "paper" white that reduces eye strain compared to pure digital white.
- **Ink (Text):** All body text and headings use #1A1A1A to provide a sharp, high-contrast reading experience reminiscent of fresh ink on a page.

## Typography

This design system employs a dual-font strategy to distinguish between content and navigation.

**Content & Reading:** `EB Garamond` is used for all narrative text. It brings a classical, literary feel to the platform. For long-form articles, the font size is intentionally generous (20px) with a tall line-height (1.6) to facilitate effortless reading.

**Interface & Wayfinding:** `Inter` provides a functional, neutral contrast. It is used for all "chrome" elements—navigation bars, buttons, input fields, and metadata labels. This separation ensures the user instinctively knows what is content to be read and what is a tool to be used.

## Layout & Spacing

The layout philosophy follows a **Fixed Grid** model for reading and a **Fluid Grid** for discovery.

- **The Reading Room:** Articles are centered in a 720px fixed-width column to maintain an ideal characters-per-line count (65-75 chars), mirroring a book page.
- **Vertical Rhythm:** A strict 8px baseline grid is used to ensure all elements—from headings to pull-quotes—align harmoniously.
- **Discovery Grid:** Article cards and search results use a 12-column grid with wide 32px gutters to prevent the interface from feeling cluttered.
- **Margins:** Desktop views utilize generous 64px outer margins to create a "letterbox" effect, centering the user's focus on the scholarship.

## Elevation & Depth

This design system avoids heavy drop shadows and modern blurs, favoring a flat, editorial depth model.

1.  **Low-Contrast Outlines:** Instead of shadows, surfaces are defined by thin (1px) borders in a slightly darker paper tone (#EAE7E4).
2.  **Tonal Stacking:** Elements like popovers or modals use a slightly brighter white than the background to appear "closer" to the reader, finished with a very soft, diffused 10% opacity navy shadow to provide just enough lift to signify interactivity.
3.  **Ink Overlays:** Interacting with an element (like hovering over a card) should result in a subtle background tint change rather than a physical "lift."

## Shapes

To maintain a scholarly and traditional feel, the shape language is disciplined and conservative.

We use **Soft (Level 1)** roundedness. A radius of 4px (0.25rem) is applied to buttons, input fields, and article cards. This is just enough to take the "edge" off the digital screen while maintaining the structural integrity of a printed document. UI elements that are strictly informational (like tags or chips) may use slightly more rounding to differentiate them from functional buttons.

## Components

- **Buttons:** Primary buttons use the scholarly navy background with white Inter typography. They are rectangular with 4px corners. Ghost buttons use a 1px navy border.
- **Article Cards:** Cards are styled with a 1px border (#EAE7E4) and no shadow. The title is always EB Garamond, while the metadata (date, read time) is Inter.
- **Input Fields:** Search bars and text inputs use a clean 1px border. When focused, the border color changes to the primary navy with no outer glow.
- **Navigation Bar:** A persistent, minimal top bar with a thin bottom border. The logo is set in EB Garamond (Bold), while navigation links use Inter (Semi-bold, 14px).
- **Progress Indicator:** A thin, navy horizontal bar at the top of the screen indicates reading progress through an article.
- **Footnotes:** Sized at 14px Inter, appearing at the bottom of sections or in a side-rail if horizontal space allows, mimicking academic journals.