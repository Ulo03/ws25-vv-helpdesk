# Mini-ServiceDesk

## Projektbeschreibung
Ein Mini-ServiceDesk zur Verwaltung von Support-Tickets und einer Knowledge-Base (FAQ/How-Tos). Dieses Projekt dient der praktischen Umsetzung der Kursinhalte: Containerisierung mit Docker, Versionsverwaltung mit Git und CI/CD-Pipelines.

## Team
- Lukas Ulovec
- Raphael Rumpler
- Patrick Speiser

## Funktionen
### Support-Tickets
- Erstellen, Anzeigen und Bearbeiten von Tickets.
- Statusverwaltung (Open / In Progress / Done).
- Kommentarfunktion für Tickets.

### Knowledge-Base
- Erstellen, Bearbeiten und Löschen von FAQ/How-To Artikeln.

### Web-App & Dashboard
- **Dashboard:** Anzeige von Datenbank-Status, Ticket-Anzahl (nach Status) und Artikel-Anzahl.
- **Tickets-Interface:** Management der Support-Anfragen.
- **Knowledge-Interface:** Management der Wissensdatenbank.

### Benutzerverwaltung
- Einfacher Login mit Rollen (User/Admin).
- Initialer Admin-Account.
- Registrierung und Anmeldung für Nutzer.

## Technischer Stack
- **.NET Aspire:** Orchestrierung der Services.
- **Entity Framework Core:** Datenzugriffsschicht.
- **Minimal APIs:** Backend-Logik.
- **Frontend:** Webapp Frontend.
- **Datenbank:** Relationale Datenbank (Docker-Container mit Volumes für Persistenz).
- **Sicherheit:** Datenbank ist nur intern erreichbar, Zugriff erfolgt ausschließlich über die Web-App.

## Git & CI/CD (GitLab)
- **Workflow:** Feature-Branches und Merge Requests.
- **Pipeline-Stages:**
  1. **Lint:** Code-Analyse.
  2. **Tests:** Automatisierte Tests.
  3. **Docker-Image:** Build und Push in die Registry.
  4. **Deployment:** Automatisches Deployment auf eine VM via Docker Compose (pull/up).

---
*Abgabe: Jänner 2026*
