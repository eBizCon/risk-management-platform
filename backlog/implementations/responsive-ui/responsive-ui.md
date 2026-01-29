# Responsive UI - Implementierungsplan

**User Story ID:** responsive-ui  
**Status:** Geplant  
**Erstellt:** 2026-01-29

---

## User Story

**Als** Anwender der Risikomanagement-Plattform (Applicant oder Processor)  
**möchte ich** die Anwendung auf mobilen Endgeräten vollständig und komfortabel nutzen können  
**damit** ich auch unterwegs oder auf Smartphone/Tablet effizient arbeiten kann, ohne dass Inhalte abgeschnitten werden oder unkontrolliertes horizontales Scrollen erforderlich ist.

---

## Akzeptanzkriterien

**Szenario 1: Navigation auf Smartphones (< 768px)**  
- **Given** ich öffne die Anwendung auf einem Gerät mit Viewport-Breite zwischen 360px und 767px  
- **When** ich navigiere durch die Anwendung  
- **Then** ist ein Mobile-Menü verfügbar (z.B. Hamburger-Icon zum Ein-/Ausklappen)  
- **And** alle Navigationspunkte sind ohne horizontales Scrollen erreichbar  
- **And** Benutzerinformationen (Name, Rolle) und Logout-Button sind bedienbar  

**Szenario 2: Navigation auf Tablets (768px - 1024px)**  
- **Given** ich öffne die Anwendung auf einem Tablet mit Viewport-Breite zwischen 768px und 1024px  
- **When** ich navigiere durch die Anwendung  
- **Then** ist die Navigation platzsparend aber vollständig sichtbar dargestellt  
- **And** alle Navigationspunkte sind gut klickbar ohne Überlappungen  

**Szenario 3: Tabellen auf Smartphones - Card-Layout**  
- **Given** ich öffne eine Seite mit tabellarischer Darstellung (z.B. `/applications`, `/processor`)  
- **When** mein Viewport kleiner als 768px ist  
- **Then** werden die Daten automatisch als Card-Layout angezeigt (ein Antrag = eine Karte)  
- **And** alle relevanten Informationen (Name, Status, Score, Ampel, Rate, Datum) sind in der Karte sichtbar  
- **And** Aktions-Buttons (Ansehen, Bearbeiten, Löschen) sind in der Karte bedienbar mit ausreichender Touch-Fläche  

**Szenario 4: Tabellen auf Tablets - Optimierte Ansicht**  
- **Given** ich öffne eine Seite mit tabellarischer Darstellung  
- **When** mein Viewport zwischen 768px und 1024px liegt  
- **Then** wird die Tabelle in einer tablet-optimierten Ansicht dargestellt  
- **And** alle Spalten sind lesbar ohne horizontales Scrollen  
- **And** die Darstellung nutzt den verfügbaren Platz optimal aus  

**Szenario 5: Formulare auf Smartphones**  
- **Given** ich fülle ein Formular aus (z.B. neuer Antrag unter `/applications/new`)  
- **When** ich auf einem Gerät mit Viewport-Breite zwischen 360px und 767px bin  
- **Then** sind alle Eingabefelder vollständig sichtbar ohne horizontales Scrollen  
- **And** Labels und Felder sind vertikal angeordnet (full width)  
- **And** Buttons haben mindestens 44×44px Touch-Fläche  
- **And** Submit-/Cancel-Buttons sind am unteren Rand gut erreichbar  

**Szenario 6: Formulare auf Tablets**  
- **Given** ich fülle ein Formular aus  
- **When** ich auf einem Tablet mit Viewport-Breite zwischen 768px und 1024px bin  
- **Then** sind Felder in einem optimierten Layout angeordnet (z.B. 2-spaltig wo sinnvoll)  
- **And** die Formulare nutzen den verfügbaren Platz effizient  

**Szenario 7: Keine abgeschnittenen Inhalte**  
- **Given** ich nutze die Anwendung auf einem Gerät zwischen 360px und 1024px Breite  
- **When** ich durch alle verfügbaren Seiten navigiere  
- **Then** sind keine Texte, Buttons oder UI-Elemente abgeschnitten  
- **And** kein unkontrolliertes horizontales Scrollen erforderlich  

---

## Business Rules

- **Breakpoints**: 
  - Mobile (Smartphone): 360px - 767px → Card-Layout, Mobile-Menü
  - Tablet: 768px - 1024px → Eigenes optimiertes Layout, platzsparende Navigation
  - Desktop: ≥ 1025px → Vollständiges Desktop-Layout
- **Alle Seiten betroffen**: Home, `/applications`, `/applications/new`, `/applications/[id]`, `/applications/[id]/edit`, `/processor`
- **Automatische Anpassung**: Layout wechselt automatisch basierend auf Viewport-Breite (responsive, kein manueller Toggle)
- **Touch-Target-Größe**: Minimum 44×44px für alle interaktiven Elemente auf Touchscreens

---

## Implementierungsziel

Die Risikomanagement-Plattform wird für drei Viewport-Größen optimiert:
- **Mobile (360px-767px)**: Hamburger-Menü, Card-Layout für Tabellen, vertikal gestapelte Formulare
- **Tablet (768px-1024px)**: Kompakte Navigation, optimierte Tabellen-/Formularansicht
- **Desktop (≥1025px)**: Bisheriges Layout beibehalten

Alle Seiten (Home, Applications, Processor, Application Detail/Edit/New) werden responsive ohne manuelle Toggles.

---

## Annahmen

- TailwindCSS Standard-Breakpoints werden angepasst: `sm: 768px`, `md: 1024px`, `lg: 1280px`
- Analytics-Seite hat niedrigere Priorität und wird in separater Story behandelt
- Bestehende E2E-Tests müssen nach Responsive-Änderungen angepasst werden

---

## Impact Map

### Betroffene Layer/Module
- UI-Komponenten: ApplicationTable, ApplicationForm, Navigation (Layout)
- Alle Page-Komponenten: Home, Applications (List/Detail/Edit/New), Processor
- Styling: TailwindCSS Config, app.css
- Tests: E2E-Tests müssen responsive Varianten abdecken

### Neue Komponenten
- `src/lib/components/MobileMenu.svelte` - Mobile Navigation mit Hamburger-Icon
- `src/lib/components/ApplicationCard.svelte` - Card-Layout für mobile Tabellenansicht

### Geänderte Komponenten
- `src/routes/+layout.svelte` - Responsive Navigation
- `src/lib/components/ApplicationTable.svelte` - Responsive Table/Card Switch
- `src/lib/components/ApplicationForm.svelte` - Responsive Formular-Layout
- `src/routes/+page.svelte` - Responsive Grid-Anpassungen
- `src/routes/applications/+page.svelte` - Mobile Filter-Anpassungen
- `src/routes/processor/+page.svelte` - Responsive Stats-Cards und Filter
- `src/routes/applications/[id]/+page.svelte` - Responsive Detail-Layout
- `tailwind.config.js` - Custom Breakpoints
- `src/app.css` - Neue responsive Utilities

### Nicht betroffen
- Backend/Server-Komponenten (keine Änderungen)
- Datenbank-Schema
- API-Endpoints
- Analytics-Seite (explizit ausgeschlossen)

---

## Änderungsplan (Code-Ebene)

### 1. TailwindCSS Konfiguration anpassen

**Datei:** `tailwind.config.js`

**Änderung:**
```javascript
theme: {
  extend: {
    screens: {
      'sm': '768px',   // Tablet start
      'md': '1024px',  // Desktop start
      'lg': '1280px'   // Large desktop
    }
  }
}
```

**Begründung:** Mobile-First Breakpoints definieren

---

### 2. Responsive Utility Classes hinzufügen

**Datei:** `src/app.css`

**Neue CSS-Klassen:**
```css
/* Touch-optimized buttons for mobile */
@media (max-width: 767px) {
  .btn-primary, .btn-secondary {
    min-height: 44px;
    min-width: 44px;
  }
}

/* Mobile card layout */
.mobile-card {
  @apply bg-white border border-border rounded-lg p-4 space-y-3;
}

.mobile-card-header {
  @apply flex justify-between items-start;
}

.mobile-card-body {
  @apply space-y-2 text-sm;
}

.mobile-card-actions {
  @apply flex gap-2 pt-2 border-t border-border;
}
```

---

### 3. Mobile Navigation Komponente erstellen

**Datei:** `src/lib/components/MobileMenu.svelte` (NEU)

**Props:**
```typescript
interface Props {
  isOpen: boolean;
  onClose: () => void;
  user: { name: string; role: 'applicant' | 'processor' } | null;
  isApplicant: boolean;
  isProcessor: boolean;
}
```

**Features:**
- Overlay + Slide-in Menu von links
- Z-Index > Navigation (z-50)
- Menüpunkte: Home, Meine Anträge (Applicant), Anträge bearbeiten (Processor)
- User Info + Logout unten im Menu
- Click außerhalb → onClose()
- ESC-Taste → onClose()
- Body-Scroll sperren wenn offen

**Data-Testids:**
- `mobile-menu`
- `mobile-menu-overlay`
- `mobile-menu-close`

---

### 4. Hauptnavigation responsive machen

**Datei:** `src/routes/+layout.svelte`

**Änderungen:**
- Hamburger-Icon hinzufügen (nur < 768px sichtbar)
- Desktop-Navigation kompaktere Abstände für Tablet
- MobileMenu Component einbinden
- State: `mobileMenuOpen`
- Funktionen: `toggleMobileMenu()`, `closeMobileMenu()`

**Data-Testid:** `nav-mobile-toggle`

---

### 5. ApplicationCard Komponente erstellen

**Datei:** `src/lib/components/ApplicationCard.svelte` (NEU)

**Props:**
```typescript
interface Props {
  application: Application;
  showActions?: boolean;
  isApplicantView?: boolean;
  onView?: (id: number) => void;
  onEdit?: (id: number) => void;
  onDelete?: (id: number) => void;
}
```

**Layout:**
- Card-Header: Name + StatusBadge
- Card-Body: 2-spaltiges Grid mit allen relevanten Infos
- Card-Actions: Touch-optimierte Buttons (≥44px)

**Data-Testids:**
- `application-card-{id}`
- `card-view-btn-{id}`
- `card-edit-btn-{id}`
- `card-delete-btn-{id}`

---

### 6. ApplicationTable responsive erweitern

**Datei:** `src/lib/components/ApplicationTable.svelte`

**Änderungen:**
- Mobile View (< 768px): ApplicationCard-Liste
- Table View (≥ 768px): Bestehende Tabelle
- Tablet-Optimierung: kompaktere Spalten (px-4 statt px-6)

**Markup-Struktur:**
```svelte
<div class="block sm:hidden">
  <!-- Card-Liste für Mobile -->
</div>
<div class="hidden sm:block overflow-x-auto">
  <!-- Tabelle für Tablet/Desktop -->
</div>
```

---

### 7. ApplicationForm responsive anpassen

**Datei:** `src/lib/components/ApplicationForm.svelte`

**Grid-Anpassungen:**
- Name: Full width auf allen Größen
- Finanz-Felder: `grid-cols-1 sm:grid-cols-2 md:grid-cols-3`
- Payment Default Radio: `flex-col sm:flex-row`
- Action Buttons: `flex-col sm:flex-row`, `w-full sm:w-auto`

---

### 8. Home Page responsive anpassen

**Datei:** `src/routes/+page.svelte`

**Änderungen:**
- Hero-Titel: `text-2xl sm:text-3xl md:text-4xl`
- Feature Cards: `grid-cols-1 sm:grid-cols-2 md:grid-cols-3`
- CTA Buttons: `flex-col sm:flex-row gap-4`

---

### 9. Applications List Page responsive anpassen

**Datei:** `src/routes/applications/+page.svelte`

**Änderungen:**
- Header: `flex-col sm:flex-row sm:justify-between gap-4`
- "Neuer Antrag" Button: Full width auf Mobile
- ApplicationTable nutzt automatisch neue responsive Features

---

### 10. Processor Page responsive anpassen

**Datei:** `src/routes/processor/+page.svelte`

**Änderungen:**
- Stats Cards: `grid-cols-1 sm:grid-cols-2 md:grid-cols-4`
- Filter-Bereich: wie Applications Page
- ApplicationTable: automatisch responsive

---

### 11. Application Detail Page responsive anpassen

**Datei:** `src/routes/applications/[id]/+page.svelte`

**Änderungen:**
- Header Actions: `flex-col sm:flex-row gap-3`
- Main Layout: `grid-cols-1 md:grid-cols-1 lg:grid-cols-3`
- Detail-Karten: `grid-cols-1 sm:grid-cols-2`

---

### 12. Application Edit/New Pages

**Dateien:**
- `src/routes/applications/[id]/edit/+page.svelte`
- `src/routes/applications/new/+page.svelte`

**Änderungen:**
- Nutzen ApplicationForm → automatisch responsive
- Back-Button Layout ggf. anpassen

---

## Daten- & Contract-Änderungen

**Keine Änderungen erforderlich.**

- Keine Schema-Änderungen
- Keine neuen API-Endpoints
- Keine DTO-Änderungen
- Rein frontend-seitige Anpassung

---

## Testplan

### E2E Tests (Playwright)

**Neue Test-Datei:** `e2e/responsive.test.ts`

#### Suite 1: Mobile Navigation (375x667)
```typescript
test('Mobile menu öffnen und schließen', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
  await page.goto('/');
  await expect(page.getByTestId('nav-mobile-toggle')).toBeVisible();
  await page.getByTestId('nav-mobile-toggle').click();
  await expect(page.getByTestId('mobile-menu')).toBeVisible();
  await page.getByTestId('mobile-menu-overlay').click();
  await expect(page.getByTestId('mobile-menu')).not.toBeVisible();
});
```

#### Suite 2: Mobile Tables - Card Layout (375x667)
```typescript
test('Applications als Cards auf Mobile', async ({ authenticatedPage }) => {
  await authenticatedPage.setViewportSize({ width: 375, height: 667 });
  await authenticatedPage.goto('/applications');
  await expect(authenticatedPage.locator('.mobile-card').first()).toBeVisible();
  await expect(authenticatedPage.locator('table')).not.toBeVisible();
});
```

#### Suite 3: Tablet Layout (768x1024)
```typescript
test('Tablet nutzt Tabellen-Ansicht', async ({ authenticatedPage }) => {
  await authenticatedPage.setViewportSize({ width: 768, height: 1024 });
  await authenticatedPage.goto('/applications');
  await expect(authenticatedPage.locator('table')).toBeVisible();
  await expect(authenticatedPage.locator('.mobile-card')).not.toBeVisible();
});
```

#### Suite 4: Formulare auf Mobile (375x667)
```typescript
test('Formular vollständig bedienbar auf Mobile', async ({ authenticatedPage }) => {
  await authenticatedPage.setViewportSize({ width: 375, height: 667 });
  await authenticatedPage.goto('/applications/new');
  
  // Alle Felder sichtbar
  await expect(authenticatedPage.getByTestId('input-name')).toBeVisible();
  await expect(authenticatedPage.getByTestId('input-income')).toBeVisible();
  
  // Kein horizontales Scrollen
  const horizontalOverflow = await authenticatedPage.evaluate(() => {
    return document.documentElement.scrollWidth > document.documentElement.clientWidth;
  });
  expect(horizontalOverflow).toBe(false);
});
```

#### Suite 5: Keine abgeschnittenen Inhalte
```typescript
const viewports = [
  { width: 360, height: 640 },
  { width: 768, height: 1024 },
  { width: 1024, height: 768 }
];

for (const viewport of viewports) {
  test(`Kein horizontales Scrollen auf ${viewport.width}px`, async ({ page }) => {
    await page.setViewportSize(viewport);
    const pages = ['/', '/applications', '/applications/new', '/processor'];
    
    for (const path of pages) {
      await page.goto(path);
      const hasHorizontalScroll = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      expect(hasHorizontalScroll).toBe(false);
    }
  });
}
```

### Bestehende Tests anpassen

**Dateien:**
- `e2e/applicant.test.ts`
- `e2e/processor.test.ts`

**Änderung:** Viewport-Tests für Mobile hinzufügen

---

## Risiken & Abhängigkeiten

### Technische Risiken

**Risiko 1: Breakpoint-Konflikte (MEDIUM)**
- Bestehende `sm:`-Klassen erwarten 640px, neues `sm:` ist 768px
- Mitigation: Vollständiger Grep-Search nach `sm:` und `md:` Klassen

**Risiko 2: E2E-Test-Stabilität (LOW)**
- Tests könnten bei Viewport-Änderungen flaky werden
- Mitigation: Explicit waits und stabile data-testids

**Risiko 3: Performance auf alten Geräten (LOW)**
- Card-Listen mit vielen Elementen könnten langsam sein
- Mitigation: Pagination bereits vorhanden

### Abhängigkeiten

- Keine externen Team-Abhängigkeiten
- Keine neuen Dependencies erforderlich
- Kein Backend-Deployment nötig

### Rollout

- Kein Breaking Change - Progressive Enhancement
- Bestehende Desktop-User nicht betroffen
- Mobile-User profitieren sofort

---

## Execution Steps (Empfohlene Reihenfolge)

1. TailwindCSS Config anpassen
2. Responsive Utilities in app.css
3. MobileMenu Komponente erstellen
4. Navigation responsive machen
5. ApplicationCard Komponente erstellen
6. ApplicationTable responsive erweitern
7. ApplicationForm responsive anpassen
8. Alle Page-Komponenten anpassen
9. E2E-Tests schreiben/anpassen
10. Manual Testing durchführen
11. Fixes basierend auf Test-Ergebnissen

---

## Verifikation

```bash
# Linting & Type-Checking
npm run check

# E2E Tests
npm run test:e2e

# E2E Tests nur responsive
npm run test:e2e -- e2e/responsive.test.ts

# Development Server für manuelles Testing
npm run dev

# Build check
npm run build
```

---

## Manual Testing Checklist

- [ ] Touch-Targets ≥ 44x44px auf allen interaktiven Elementen
- [ ] Horizontales Scrollen nur wo beabsichtigt
- [ ] Alle Texte lesbar ohne Zoom
- [ ] Navigation auf allen Viewports bedienbar
- [ ] Formulare ausfüllbar ohne Frustration
- [ ] Tabellen/Cards zeigen alle Informationen
- [ ] Mobile Menu öffnet/schließt korrekt
- [ ] Viewport 360px, 768px, 1024px getestet

---

**Status:** Bereit zur Implementierung
