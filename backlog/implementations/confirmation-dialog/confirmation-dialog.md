# User Story: Bestätigungsdialog für Antragseinreichung

## User Story

**Als** Antragsteller  
**möchte ich** vor jeder Antragseinreichung einen Bestätigungsdialog sehen  
**damit** ich versehentliche Einreichungen vermeiden und meine Entscheidung bewusst bestätigen kann

## Einreichungsstellen im System

Basierend auf der Code-Analyse gibt es **3 Stellen**, an denen ein Antrag eingereicht werden kann:

1. **Neuer Antrag erstellen** (`/applications/new`)
   - Button "Antrag einreichen" im `ApplicationForm`
   - Form Action mit `action=submit`

2. **Antrag bearbeiten** (`/applications/[id]/edit`)
   - Button "Antrag einreichen" im `ApplicationForm`
   - Form Action mit `action=submit`

3. **Antragsdetailseite** (`/applications/[id]`)
   - Button "Einreichen" (nur bei Status `draft`)
   - API-Call an `/api/applications/[id]/submit`

## Akzeptanzkriterien

### Szenario 1: Bestätigungsdialog bei Neuanlage eines Antrags
**Given** ich bin als Antragsteller angemeldet  
**And** ich befinde mich auf der Seite "Neuer Antrag" (`/applications/new`)  
**And** ich habe alle Pflichtfelder ausgefüllt  
**When** ich auf "Antrag einreichen" klicke  
**Then** wird ein Bestätigungsdialog angezeigt  
**And** der Dialog entspricht dem Look & Feel der Anwendung  
**And** der Dialog bietet die Optionen "Bestätigen" und "Abbrechen"

### Szenario 2: Bestätigungsdialog beim Bearbeiten eines Antrags
**Given** ich bin als Antragsteller angemeldet  
**And** ich befinde mich auf der Bearbeitungsseite eines Entwurfs (`/applications/[id]/edit`)  
**And** ich habe Änderungen vorgenommen  
**When** ich auf "Antrag einreichen" klicke  
**Then** wird ein Bestätigungsdialog angezeigt  
**And** der Dialog entspricht dem Look & Feel der Anwendung  
**And** der Dialog bietet die Optionen "Bestätigen" und "Abbrechen"

### Szenario 3: Bestätigungsdialog auf der Antragsdetailseite
**Given** ich bin als Antragsteller angemeldet  
**And** ich befinde mich auf der Detailseite eines Entwurfs (`/applications/[id]`)  
**And** der Antrag hat den Status "draft"  
**When** ich auf den Button "Einreichen" klicke  
**Then** wird ein Bestätigungsdialog angezeigt  
**And** der Dialog entspricht dem Look & Feel der Anwendung  
**And** der Dialog bietet die Optionen "Bestätigen" und "Abbrechen"

### Szenario 4: Antrag wird nach Bestätigung eingereicht
**Given** der Bestätigungsdialog ist an einer beliebigen Einreichungsstelle geöffnet  
**When** ich auf "Bestätigen" klicke  
**Then** wird der Antrag eingereicht  
**And** das System verhält sich wie bisher nach einer Einreichung  
**And** der Dialog wird geschlossen

### Szenario 5: Antrag wird bei Abbruch nicht eingereicht
**Given** der Bestätigungsdialog ist an einer beliebigen Einreichungsstelle geöffnet  
**When** ich auf "Abbrechen" klicke  
**Then** wird der Antrag nicht eingereicht  
**And** der Dialog wird geschlossen  
**And** ich bleibe auf der aktuellen Seite  
**And** alle eingegebenen Daten bleiben erhalten

### Szenario 6: Konsistentes Verhalten über alle Einreichungsstellen
**Given** ich bin als Antragsteller angemeldet  
**When** ich an einer der 3 Einreichungsstellen einen Antrag einreiche  
**Then** erscheint immer der gleiche Bestätigungsdialog  
**And** das Verhalten ist konsistent (gleicher Text, gleiche Buttons, gleiches Styling)

## Business Rules / Fachliche Hinweise

- Der Bestätigungsdialog ist ein Sicherheitsmechanismus gegen versehentliche Einreichungen
- Der Dialog muss an **allen 3 identifizierten Stellen** erscheinen
- Das Look & Feel muss konsistent mit der restlichen Anwendung sein (TailwindCSS, Lucide Icons)
- Der Dialog blockiert die weitere Interaktion bis eine Entscheidung getroffen wurde (modal)
- Nach Abbruch bleibt der Nutzer im aktuellen Kontext mit allen Daten
- Der aktuelle `confirm()`-Dialog in `/applications/[id]/+page.svelte:36` soll durch den neuen Dialog ersetzt werden

## Technische Hinweise

- **Bestehende Implementierung**: In `/applications/[id]/+page.svelte:36` existiert bereits ein `confirm()`-Dialog, der ersetzt werden soll
- **Einreichungslogik**: 
  - Form Actions: `/applications/new/+page.server.ts` und `/applications/[id]/edit/+page.server.ts`
  - API Endpoint: `/api/applications/[id]/submit/+server.ts`
- **Repository**: `submitApplication()` in `/server/services/repositories/application.repository.ts:62`

## Abhängigkeiten & Offene Fragen

- **Abhängigkeit**: Bestehende Antragseinreichungsfunktionalität (vorhanden)
- **Annahme**: TailwindCSS und Lucide Icons sind verfügbar für konsistentes Styling
- **Empfehlung**: Wiederverwendbare Dialog-Komponente erstellen für konsistente Nutzung an allen 3 Stellen

---

# Implementierungs-Blueprint

## 1) Implementierungsziel

Implementierung eines wiederverwendbaren, modalen Bestätigungsdialogs, der an allen drei Einreichungsstellen (Neuanlage, Bearbeitung, Detailseite) vor der Antragseinreichung angezeigt wird. Der Dialog ersetzt den bestehenden `confirm()`-Dialog und folgt dem Look & Feel der Anwendung (TailwindCSS, Lucide Icons). Bei Bestätigung wird die Einreichung durchgeführt, bei Abbruch bleibt der Nutzer auf der aktuellen Seite.

## 2) Annahmen & offene Fragen

### Annahmen:
- Der Dialog-Text kann generisch sein: "Möchten Sie diesen Antrag wirklich einreichen?"
- Nach erfolgreicher Einreichung bleibt das bisherige Verhalten (Redirect/Reload) unverändert
- Der Dialog soll modal sein (blockiert Interaktion mit dem Rest der Seite)
- Keyboard-Navigation (ESC = Abbrechen, Enter = Bestätigen) ist erwünscht

## 3) Impact Map

### Layer/Module betroffen:
- **UI-Komponenten**: Neue `ConfirmDialog.svelte` Komponente
- **Seiten**: 
  - `/applications/new/+page.svelte`
  - `/applications/[id]/edit/+page.svelte`
  - `/applications/[id]/+page.svelte`
- **E2E-Tests**: `e2e/applicant.test.ts`

### Neue Komponenten:
- `src/lib/components/ConfirmDialog.svelte` - Wiederverwendbarer Bestätigungsdialog

### Geänderte Komponenten:
- `src/lib/components/ApplicationForm.svelte` - Integration des Dialogs für Form-Einreichung
- `src/routes/applications/[id]/+page.svelte` - Ersetzung des `confirm()` durch neuen Dialog
- `e2e/applicant.test.ts` - Anpassung der Tests für Dialog-Interaktion

### Nicht betroffen:
- Backend-Logik (Server Actions, API Endpoints, Repositories)
- Datenbank-Schema
- Validierungslogik
- Scoring-Logik

## 4) Änderungsplan auf Code-Ebene

### 4.1 Neue Komponente: ConfirmDialog.svelte

**Datei**: `src/lib/components/ConfirmDialog.svelte`  
**Art**: Neu erstellen  
**Verantwortlichkeit**: Wiederverwendbarer modaler Bestätigungsdialog

**Props Interface**:
```typescript
interface Props {
  open: boolean;
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  onConfirm: () => void;
  onCancel: () => void;
}
```

**Logik (Pseudocode)**:
```
1. Wenn open === false, rendere nichts
2. Rendere Overlay (backdrop) mit click handler → onCancel()
3. Rendere Dialog-Container (zentriert, modal)
4. Rendere Titel (optional)
5. Rendere Message
6. Rendere Button-Gruppe:
   - Abbrechen-Button (btn-secondary) → onCancel()
   - Bestätigen-Button (btn-primary) → onConfirm()
7. Keyboard-Handler:
   - ESC → onCancel()
   - Enter → onConfirm()
8. Focus-Trap: Focus bleibt im Dialog
9. data-testid für E2E-Tests:
   - Dialog: "confirm-dialog"
   - Confirm-Button: "confirm-dialog-confirm"
   - Cancel-Button: "confirm-dialog-cancel"
```

**Styling**:
- Backdrop: `fixed inset-0 bg-black/50 z-40`
- Dialog: `fixed inset-0 z-50 flex items-center justify-center p-4`
- Card: `card p-6 max-w-md w-full shadow-xl`
- Buttons: `btn-primary`, `btn-secondary` aus `app.css`

**Error/Edge Cases**:
- Wenn `open` wechselt zu `true`, setze Focus auf Confirm-Button
- Verhindere Body-Scroll wenn Dialog offen ist
- Click auf Dialog-Inhalt soll nicht schließen (nur Backdrop oder Buttons)

---

### 4.2 Änderung: ApplicationForm.svelte

**Datei**: `src/lib/components/ApplicationForm.svelte`  
**Art**: Ändern  
**Betroffene Bereiche**: Submit-Button Handler, Dialog-Integration

**Neue State-Variablen**:
```typescript
let showConfirmDialog = $state(false);
let pendingAction = $state<'submit' | null>(null);
```

**Neue Methoden**:

**`handleSubmitClick(event: Event)`**:
```
1. event.preventDefault()
2. Prüfe ob action === 'submit'
3. Wenn ja:
   - setze showConfirmDialog = true
   - setze pendingAction = 'submit'
4. Wenn nein (save):
   - Lasse Form normal submitten
```

**`handleConfirmSubmit()`**:
```
1. setze showConfirmDialog = false
2. Triggere Form-Submit programmatisch mit action='submit'
3. (Form Action Handler übernimmt Rest)
```

**`handleCancelSubmit()`**:
```
1. setze showConfirmDialog = false
2. setze pendingAction = null
```

**Template-Änderungen**:
```
1. Importiere ConfirmDialog
2. Ändere Submit-Button:
   - type="button" (statt submit)
   - onclick={handleSubmitClick}
3. Füge ConfirmDialog am Ende hinzu:
   <ConfirmDialog
     open={showConfirmDialog}
     message="Möchten Sie diesen Antrag wirklich einreichen? Nach der Einreichung ist keine Bearbeitung mehr möglich."
     confirmText="Antrag einreichen"
     cancelText="Abbrechen"
     onConfirm={handleConfirmSubmit}
     onCancel={handleCancelSubmit}
   />
```

---

### 4.3 Änderung: /applications/[id]/+page.svelte

**Datei**: `src/routes/applications/[id]/+page.svelte`  
**Art**: Ändern  
**Betroffene Bereiche**: `handleSubmit` Funktion, Dialog-Integration

**Entfernen**:
```typescript
// Zeile 36: confirm() entfernen
if (confirm('Möchten Sie diesen Antrag wirklich einreichen?...')) {
```

**Neue State-Variablen**:
```typescript
let showConfirmDialog = $state(false);
```

**Geänderte Methode `handleSubmit()`**:
```
1. Entferne confirm()
2. Setze showConfirmDialog = true
```

**Neue Methode `handleConfirmSubmit()`**:
```
1. setze showConfirmDialog = false
2. Führe API-Call aus:
   const response = await fetch(`/api/applications/${app.id}/submit`, {
     method: 'POST'
   });
3. if (response.ok):
   window.location.reload();
4. else:
   // Fehlerbehandlung (optional: Toast/Error-Message)
```

**Neue Methode `handleCancelSubmit()`**:
```
1. setze showConfirmDialog = false
```

**Template-Änderungen**:
```
1. Importiere ConfirmDialog
2. Ändere Button onclick:
   onclick={() => showConfirmDialog = true}
3. Füge ConfirmDialog hinzu:
   <ConfirmDialog
     open={showConfirmDialog}
     message="Möchten Sie diesen Antrag wirklich einreichen? Nach der Einreichung ist keine Bearbeitung mehr möglich."
     confirmText="Antrag einreichen"
     cancelText="Abbrechen"
     onConfirm={handleConfirmSubmit}
     onCancel={handleCancelSubmit}
   />
```

---

## 5) Daten- & Contract-Änderungen

**Keine Änderungen erforderlich**:
- Keine DB-Schema-Änderungen
- Keine API-Contract-Änderungen
- Keine DTO-Änderungen
- Backend-Logik bleibt unverändert

## 6) Testplan

### E2E Tests

**Datei**: `e2e/applicant.test.ts`

#### Test 1: Dialog erscheint bei Neuanlage mit Submit
```typescript
test('should show confirmation dialog when submitting new application', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/applications/new');
  // Felder ausfüllen
  await authenticatedPage.getByTestId('input-name').fill('Dialog Test');
  await authenticatedPage.getByTestId('input-income').fill('4000');
  await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
  await authenticatedPage.getByTestId('input-desired-rate').fill('400');
  await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
  await authenticatedPage.getByTestId('radio-payment-default-no').check();
  
  await authenticatedPage.getByTestId('btn-submit-application').click();
  
  // Dialog sollte sichtbar sein
  await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
  await expect(authenticatedPage.getByTestId('confirm-dialog-confirm')).toBeVisible();
  await expect(authenticatedPage.getByTestId('confirm-dialog-cancel')).toBeVisible();
});
```

#### Test 2: Antrag wird nach Bestätigung eingereicht
```typescript
test('should submit application after confirming dialog', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/applications/new');
  // Felder ausfüllen
  await authenticatedPage.getByTestId('input-name').fill('Confirm Test');
  await authenticatedPage.getByTestId('input-income').fill('4000');
  await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
  await authenticatedPage.getByTestId('input-desired-rate').fill('400');
  await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
  await authenticatedPage.getByTestId('radio-payment-default-no').check();
  
  await authenticatedPage.getByTestId('btn-submit-application').click();
  
  // Dialog bestätigen
  await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
  
  // Prüfe Redirect und Status
  await expect(authenticatedPage).toHaveURL(/\/applications\/\d+/);
  await expect(authenticatedPage.getByTestId('status-badge-submitted')).toBeVisible();
  
  // Dialog sollte geschlossen sein
  await expect(authenticatedPage.getByTestId('confirm-dialog')).not.toBeVisible();
});
```

#### Test 3: Antrag wird bei Abbruch nicht eingereicht
```typescript
test('should not submit application when canceling dialog', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/applications/new');
  // Felder ausfüllen
  await authenticatedPage.getByTestId('input-name').fill('Cancel Test');
  await authenticatedPage.getByTestId('input-income').fill('4000');
  await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
  await authenticatedPage.getByTestId('input-desired-rate').fill('400');
  await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
  await authenticatedPage.getByTestId('radio-payment-default-no').check();
  
  await authenticatedPage.getByTestId('btn-submit-application').click();
  await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
  
  // Dialog abbrechen
  await authenticatedPage.getByTestId('confirm-dialog-cancel').click();
  
  // Prüfe: Noch auf /applications/new
  await expect(authenticatedPage).toHaveURL('/applications/new');
  
  // Dialog geschlossen
  await expect(authenticatedPage.getByTestId('confirm-dialog')).not.toBeVisible();
  
  // Daten noch vorhanden
  await expect(authenticatedPage.getByTestId('input-name')).toHaveValue('Cancel Test');
});
```

#### Test 4: Dialog bei Bearbeitung
```typescript
test('should show confirmation dialog when submitting from edit page', async ({ authenticatedPage }) => {
  // Erst Draft erstellen
  await authenticatedPage.goto('/applications/new');
  await authenticatedPage.getByTestId('input-name').fill('Edit Test');
  await authenticatedPage.getByTestId('input-income').fill('4000');
  await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
  await authenticatedPage.getByTestId('input-desired-rate').fill('400');
  await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
  await authenticatedPage.getByTestId('radio-payment-default-no').check();
  await authenticatedPage.getByTestId('btn-save-draft').click();
  
  // Zur Edit-Seite
  await authenticatedPage.getByTestId('edit-application').click();
  
  // Submit klicken
  await authenticatedPage.getByTestId('btn-submit-application').click();
  
  // Dialog prüfen
  await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
});
```

#### Test 5: Dialog auf Detailseite
```typescript
test('should show confirmation dialog when submitting from detail page', async ({ authenticatedPage }) => {
  // Draft erstellen
  await authenticatedPage.goto('/applications/new');
  await authenticatedPage.getByTestId('input-name').fill('Detail Test');
  await authenticatedPage.getByTestId('input-income').fill('4000');
  await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
  await authenticatedPage.getByTestId('input-desired-rate').fill('400');
  await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
  await authenticatedPage.getByTestId('radio-payment-default-no').check();
  await authenticatedPage.getByTestId('btn-save-draft').click();
  
  // Auf Detailseite
  await expect(authenticatedPage).toHaveURL(/\/applications\/\d+/);
  
  // Submit-Button klicken
  await authenticatedPage.getByTestId('submit-application').click();
  
  // Dialog prüfen
  await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
});
```

#### Test 6: ESC-Taste schließt Dialog
```typescript
test('should close dialog when pressing ESC key', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/applications/new');
  await authenticatedPage.getByTestId('input-name').fill('ESC Test');
  await authenticatedPage.getByTestId('input-income').fill('4000');
  await authenticatedPage.getByTestId('input-fixed-costs').fill('1500');
  await authenticatedPage.getByTestId('input-desired-rate').fill('400');
  await authenticatedPage.getByTestId('select-employment-status').selectOption('employed');
  await authenticatedPage.getByTestId('radio-payment-default-no').check();
  
  await authenticatedPage.getByTestId('btn-submit-application').click();
  
  // ESC drücken
  await authenticatedPage.keyboard.press('Escape');
  
  // Dialog geschlossen
  await expect(authenticatedPage.getByTestId('confirm-dialog')).not.toBeVisible();
  
  // Noch auf /applications/new
  await expect(authenticatedPage).toHaveURL('/applications/new');
});
```

#### Bestehende Tests anpassen:

**Test zu ändern**: `should submit a draft application` (Zeile 160-176)
```typescript
// Alte Zeile 171-175 ersetzen durch:
await authenticatedPage.getByTestId('submit-application').click();
await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
await expect(authenticatedPage.getByTestId('status-badge-submitted')).toBeVisible();
```

**Test zu ändern**: `should successfully create and submit an application directly` (Zeile 96-110)
```typescript
// Nach Zeile 106 hinzufügen:
await expect(authenticatedPage.getByTestId('confirm-dialog')).toBeVisible();
await authenticatedPage.getByTestId('confirm-dialog-confirm').click();
```

## 7) Risiken & Abhängigkeiten

### Technische Risiken:
- **Gering**: Focus-Trap und Keyboard-Navigation müssen korrekt implementiert werden
- **Gering**: Body-Scroll-Lock könnte mit anderen Komponenten kollidieren (aktuell keine anderen Modals vorhanden)
- **Gering**: E2E-Tests müssen angepasst werden (breaking change für bestehende Tests)

### Abhängigkeiten:
- Keine externen Team-Abhängigkeiten
- Keine Backend-Änderungen erforderlich
- Keine Deployment-Abhängigkeiten

### Rollout/Migrationsrisiken:
- **Kein Risiko**: Rein Frontend-Änderung
- **Kein Risiko**: Keine Breaking Changes für API
- **Hinweis**: E2E-Tests müssen nach Deployment angepasst werden

---

## Ausführungsreihenfolge:

1. **ConfirmDialog.svelte** erstellen und manuell testen
2. **ApplicationForm.svelte** anpassen und lokal testen
3. **/applications/[id]/+page.svelte** anpassen und lokal testen
4. **E2E-Tests** anpassen und ausführen
5. Alle Tests grün → Ready for Review

## Verifikations-Commands:

```bash
# E2E-Tests ausführen
npm run test:e2e

# Spezifische Test-Suite
npx playwright test e2e/applicant.test.ts

# Dev-Server für manuelle Tests
npm run dev
```
