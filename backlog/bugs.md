# Bugs

---

## BUG-001: Logout Button overflow in Mobile-Ansicht

**Status:** Offen  
**Priorität:** Mittel  
**Bereich:** Navigation / Responsive UI

### Beschreibung
Der Logout Button ist in der Mobile-Ansicht in der Navigation sichtbar und führt zum horizontalen Scrollen der Seite.

### Schritte zur Reproduktion
1. Anwendung in Mobile-Ansicht öffnen (< 768px Breite)
2. Navigation betrachten

### Aktuelles Verhalten
Der Logout Button wird in der Desktop-Navigation angezeigt, obwohl diese in der Mobile-Ansicht nicht genug Platz bietet. Dies verursacht horizontales Scrollen.

### Erwartetes Verhalten
In der Mobile-Ansicht ist der Logout Button **nur** im MobileMenu sichtbar, nicht in der Desktop-Navigation.

---

## BUG-002: Pagination overflow in Mobile-Ansicht

**Status:** Offen  
**Priorität:** Mittel  
**Bereich:** Processor / Responsive UI

### Beschreibung
Die Pagination auf der "Anträge bearbeiten"-Seite führt in der Mobile-Ansicht zum horizontalen Scrollen.

### Schritte zur Reproduktion
1. Als Processor einloggen
2. "Anträge bearbeiten"-Seite öffnen
3. Mobile-Ansicht verwenden (< 768px Breite)

### Aktuelles Verhalten
Die Pagination-Komponente ist zu breit für die Mobile-Ansicht und verursacht horizontales Scrollen.

### Erwartetes Verhalten
Die Pagination passt sich der Mobile-Ansicht an und verursacht kein horizontales Scrollen.

---

## BUG-003: Neuer Antrag wird erst nach Seiten-Refresh in Liste angezeigt

**Status:** Offen  
**Priorität:** Hoch  
**Bereich:** Applicant / Antragsliste

### Beschreibung
Nach dem Einreichen eines neuen Antrags wird dieser nicht sofort in der "Meine Anträge"-Liste angezeigt.

### Schritte zur Reproduktion
1. Als Applicant einloggen
2. Neuen Antrag einreichen
3. Über "zurück" auf "Meine Anträge" navigieren

### Aktuelles Verhalten
Der neu eingereichte Antrag erscheint nicht in der Liste. Erst nach einem manuellen Seiten-Refresh wird er angezeigt.

### Erwartetes Verhalten
Der neue Antrag erscheint sofort in der Liste. Ein Refresh der Seite ist nicht notwendig.

### Offene Fragen
- Tritt das Problem auch bei "Als Entwurf speichern" auf?
- Tritt das Problem auch nach dem Editieren eines bestehenden Antrags auf?