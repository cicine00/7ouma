# 7OUMA - Plateforme de Services de Proximité 🇲🇦

> **"Ton quartier, tes services, en toute transparence"**  
> الحومة • Hyper-local • Transparent • Rapide

---

## 🚀 Démarrage rapide (1 commande)

```bash
docker-compose up -d
```

Accès :
| Service | URL |
|---------|-----|
| **Frontend PWA** | http://localhost:3000 |
| **API Gateway** | http://localhost:5000 |
| **Identity API** | http://localhost:5001/swagger |
| **Catalog API** | http://localhost:5002/swagger |
| **Booking API** | http://localhost:5003/swagger |
| **Payment API** | http://localhost:5004/swagger |
| **RabbitMQ UI** | http://localhost:15672 (admin/admin123) |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│         API GATEWAY :5000               │
│         (YARP / Ocelot .NET)            │
└────────────────┬────────────────────────┘
                 │
    ┌────────────┼────────────┬────────────┐
    ▼            ▼            ▼            ▼
 Identity     Catalog      Booking     Payment
  :5001        :5002        :5003       :5004
    │            │            │            │
 Postgres     Postgres     Postgres    Postgres
 +Redis       +PostGIS     +Redis      +Redis
                 │            │
              RabbitMQ ◄──────┘
              SignalR (Tracking Live)
```

---

## 📁 Structure du projet

```
7ouma/
├── backend/
│   ├── ApiGateway/              # YARP Gateway
│   └── Services/
│       ├── Identity/            # Auth JWT + Profils + Fidélité
│       ├── Catalog/             # Recherche géo PostGIS + Pricing IA
│       ├── Booking/             # Matching + Devis + Tracking SignalR
│       └── Payment/             # Stripe + CMI + Commission 5%
├── frontend/                    # React 18 + TypeScript + PWA
│   └── src/
│       ├── pages/               # HomePage, Search, Login, Bookings
│       ├── components/          # BottomNav, Cards, Forms
│       ├── services/            # API Axios + interceptors
│       ├── stores/              # Zustand (Auth)
│       └── types/               # TypeScript types partagés
└── docker-compose.yml           # Orchestration complète
```

---

## 💡 Fonctionnalités clés

### 🤖 Pricing IA Hybride
1. L'IA estime une fourchette de prix basée sur l'historique (ex: 150-200 DH)
2. Les prestataires ajustent selon la complexité réelle
3. Le client reçoit **3 devis comparatifs**
4. Le client choisit : prix + note + distance

### 📍 Hyper-local (الحومة)
- Recherche **PostGIS** dans un rayon de 5km
- Affichage distance en temps réel
- Quartiers marocains (حومة)

### 🛵 Tracking Live
- **SignalR** WebSocket pour suivi GPS en temps réel
- "Ahmed est à 5 min de chez vous"
- Notifications push (Firebase FCM)

### 💳 Programme Fidélité
- 1 DH dépensé = 1 point
- 100 points = 10 DH de réduction
- Parrainage : 50 points par ami inscrit

---

## 🛠️ Stack Technique

| Couche | Tech |
|--------|------|
| Backend | .NET 8 Web API (microservices) |
| ORM | Entity Framework Core 8 |
| Base de données | PostgreSQL 16 + PostGIS |
| Cache | Redis 7 |
| Messaging | RabbitMQ |
| Temps réel | SignalR + Redis pub/sub |
| Auth | JWT + Refresh Tokens |
| Frontend | React 18 + TypeScript + Vite |
| PWA | vite-plugin-pwa + Service Workers |
| UI | Tailwind CSS |
| State | Zustand |
| API | TanStack Query + Axios |
| DevOps | Docker + Docker Compose |
| Paiement | Stripe + CMI Gateway |
| Notifications | Firebase Cloud Messaging |

---

## 📅 Roadmap MVP (12 Sprints / 3 mois)

- [x] **Sprint 1-2** : Architecture + Docker + Identity Service
- [x] **Sprint 3-4** : Catalog + Recherche géolocalisée PostGIS
- [x] **Sprint 5-6** : Booking + Matching 3 prestataires
- [ ] **Sprint 7-8** : Payment + Stripe + Commission 5%
- [x] **Sprint 9-10** : Frontend PWA React
- [ ] **Sprint 11-12** : Photos upload + SignalR tracking + Push

### Phase 2
- ⚡ Mode Urgence (< 2h)
- 🎥 Chat vidéo (Twilio/Agora)
- 🤖 Chatbot IA (WhatsApp Business)
- ⭐ Reviews & ratings
- 📱 Apps natives iOS/Android

---

## ⚙️ Développement local

### Prérequis
- Docker Desktop
- .NET 8 SDK
- Node.js 20+
- VS Code ou Visual Studio 2022

### Backend seul
```bash
cd backend/Services/Identity/src
dotnet run
```

### Frontend seul
```bash
cd frontend
npm install
npm run dev
# → http://localhost:3000
```

### Tout avec Docker
```bash
docker-compose up -d
docker-compose logs -f identity-service
```

---

## 🔑 Variables d'environnement

Copier `.env.example` vers `.env` et remplir :

```env
JWT_SECRET=your_secret_key_min_32_chars
STRIPE_SECRET_KEY=sk_test_...
GOOGLE_MAPS_API_KEY=AIza...
FIREBASE_KEY=...
```

---

## 👨‍💻 Auteur

Projet portfolio - Architecture microservices .NET + React PWA  
Inspiré du contexte marocain : الحومة, proximité, confiance

---

*7OUMA © 2024 - Tous droits réservés*
