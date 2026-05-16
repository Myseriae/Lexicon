# CSS Color Variables Reference

## Add to your CSS or use in components

```css
:root {
  /* Surfaces & Backgrounds */
  --surface: #faf9f8;                    /* Main background - Paper White */
  --surface-dim: #dadad9;                /* Darker surface for emphasis */
  --surface-container-low: #f4f3f2;      /* Light container background */
  --surface-container: #eeeeed;          /* Medium container background */
  --surface-container-high: #e9e8e7;     /* Darker container background */
  --surface-container-lowest: #ffffff;   /* Lightest surface */
  --surface-container-highest: #e3e2e1;  /* Darkest surface */
  
  /* Text Colors */
  --on-surface: #1a1c1c;                 /* Primary text - High contrast */
  --on-surface-variant: #43474c;         /* Secondary text - Lower contrast */
  
  /* Primary (Navy) - Scholarly Authority */
  --primary: #051625;                    /* Deep navy - Most intense */
  --primary-container: #1b2b3a;          /* Medium navy - Primary use */
  --on-primary: #ffffff;                 /* Text on navy backgrounds */
  --on-primary-container: #8292a5;       /* Secondary text on navy */
  
  /* Secondary (Sage Green) - Accent */
  --secondary: #566253;                  /* Muted sage */
  --on-secondary: #ffffff;
  --secondary-container: #d7e3d1;
  --on-secondary-container: #5a6657;
  
  /* Error States */
  --error: #ba1a1a;                      /* Error red */
  --on-error: #ffffff;
  --error-container: #ffdad6;
  --on-error-container: #93000a;
  
  /* Outlines & Borders */
  --outline: #74777c;                    /* Standard outline */
  --outline-variant: #c4c6cc;            /* Light outline - 1px borders */
  
  /* Typography */
  --font-serif: 'EB Garamond', serif;
  --font-sans: 'Inter', sans-serif;
}
```

## Color Usage Guidelines

### Primary Elements
```css
/* Headings, Important Text */
color: var(--primary-container);
font-family: var(--font-serif);

/* Primary Buttons */
background-color: var(--primary-container);
color: var(--on-primary);
```

### Body Content
```css
/* Body Text */
color: var(--on-surface);
font-family: var(--font-serif);

/* Secondary Text, Hints */
color: var(--on-surface-variant);
font-family: var(--font-serif);
```

### Borders & Dividers
```css
/* Ultra-thin dividers */
border: 1px solid var(--outline-variant);
border-bottom: 1px solid var(--outline-variant);

/* Interactive borders */
border: 1px solid var(--primary-container);
```

### Backgrounds
```css
/* Default background */
background-color: var(--surface);

/* Input/Form backgrounds */
background-color: var(--surface-container-low);

/* Container backgrounds */
background-color: var(--surface-container);
```

### Error/Warning States
```css
/* Error messages */
color: var(--error);
background-color: rgba(186, 26, 26, 0.08);
border: 1px solid rgba(186, 26, 26, 0.3);
```

## Typography Styles

### Headline - Large (Page Titles)
```css
font-family: var(--font-serif);
font-size: 3rem;
font-weight: 600;
line-height: 1.1;
letter-spacing: -0.02em;
color: var(--primary-container);
```

### Headline - Medium (Card Titles)
```css
font-family: var(--font-serif);
font-size: 1.75rem;
font-weight: 500;
line-height: 1.3;
color: var(--primary-container);
```

### Body - Reading (Article Text)
```css
font-family: var(--font-serif);
font-size: 1rem;
font-weight: 400;
line-height: 1.6;
color: var(--on-surface);
```

### UI Label - Large (Buttons, Navs)
```css
font-family: var(--font-sans);
font-size: 0.875rem;
font-weight: 600;
letter-spacing: 0.01em;
color: var(--primary-container);
```

### UI Label - Small (Tags, Metadata)
```css
font-family: var(--font-sans);
font-size: 0.75rem;
font-weight: 600;
letter-spacing: 0.08em;
text-transform: uppercase;
color: var(--primary-container);
```

## Interactive States

### Buttons
```css
.btn {
  background-color: var(--primary-container);
  color: var(--on-primary);
  border-radius: 0.25rem;
  padding: 0.5rem 1rem;
  transition: background-color 0.2s ease;
}

.btn:hover {
  background-color: var(--primary);
}

.btn:active {
  transform: scale(0.98);
}

.btn:focus {
  outline: 2px solid var(--primary-container);
  outline-offset: 2px;
}
```

### Links
```css
a {
  color: var(--primary-container);
  text-decoration: underline;
}

a:hover {
  opacity: 0.8;
}
```

### Input Fields
```css
input, textarea {
  background-color: var(--surface-container-low);
  border: 1px solid var(--outline-variant);
  color: var(--on-surface);
}

input:focus, textarea:focus {
  border-color: var(--primary-container);
  background-color: var(--surface-container-lowest);
}
```

## Spacing System

```css
:root {
  --base: 8px;                    /* Base unit for 8px grid */
  --gutter-desktop: 32px;         /* Column gutters */
  --margin-desktop: 64px;         /* Outer margins */
  --margin-mobile: 20px;          /* Mobile margins */
  --reading-column-max: 720px;    /* Reading column width */
}
```

## Border Radius

```css
:root {
  --radius-sm: 0.125rem;          /* Minimal rounding */
  --radius-default: 0.25rem;      /* Standard buttons/inputs */
  --radius-md: 0.375rem;          /* Medium roundness */
  --radius-lg: 0.5rem;            /* Large roundness */
  --radius-full: 9999px;          /* Fully rounded (pills) */
}
```

## Common Component Patterns

### Article Card
```css
.article-card {
  padding: 1.5rem 0;
  border-bottom: 1px solid var(--outline-variant);
  color: var(--on-surface);
}
```

### Modal
```css
.modal-overlay {
  background-color: rgba(5, 22, 37, 0.4);
  backdrop-filter: blur(2px);
}

.modal-content {
  background-color: var(--surface-container-lowest);
  border: 1px solid var(--outline-variant);
}
```

### Navbar
```css
.navbar {
  background-color: var(--surface);
  border-bottom: 1px solid var(--outline-variant);
}
```

### Footer
```css
.footer {
  background-color: var(--surface);
  border-top: 1px solid var(--outline-variant);
}
```

## Recommended Usage for Pages

### Home Page
- Background: var(--surface)
- Text: var(--on-surface)
- Headings: var(--primary-container) with serif font
- Borders: var(--outline-variant)

### Login/Register Pages
Similar to home, with:
- Form backgrounds: var(--surface-container-low)
- Form borders: var(--outline-variant) on focus → var(--primary-container)
- Buttons: var(--primary-container) background

### Article Page
- Background: var(--surface)
- Body text: var(--on-surface) with serif
- Links: var(--primary-container) with underline
- Code blocks: var(--surface-container-high)


