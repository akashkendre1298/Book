# 🏛️ Athenaeum: The Private Archive

[![Backend Tests](https://img.shields.io/badge/xUnit-48%20Passed-success?style=for-the-badge&logo=dotnet)](./Doc/tdd_test_log.md)
[![Frontend Tests](https://img.shields.io/badge/Vitest-69%20Passed-success?style=for-the-badge&logo=vite)](./Doc/tdd_test_log.md)
[![KPI Fulfillment](https://img.shields.io/badge/KPIs-100%25-clay?style=for-the-badge)](./Doc/kpi_audit_report.md)
[![Stack](https://img.shields.io/badge/Stack-.NET%2010%20%2B%20React%2019-sage?style=for-the-badge)](./README.md)

**Athenaeum** is a premium, full-stack book archiving and social reading platform. It combines **.NET 10** and **React 19** to deliver a secure, multi-tenant library experience — complete with role-based access control, public/private collections, admin moderation, and cover art uploads.

---

## 🌟 Feature Highlights

| Area | Feature |
|------|---------|
| 📚 **Collection Management** | Add, edit, delete books with cover image & PDF uploads |
| 🔄 **Reading Tracker** | Track status (Want to Read / Reading / Finished / Dropped), current page, rating & review |
| 🌍 **Public Library** | Browse all admin-approved public books with search & genre filters |
| 📩 **Moderation Flow** | Users can request to publish private books; Admins approve or reject via a dedicated queue |
| 🛡️ **Admin Dashboard** | System metrics, user management (activate/deactivate/delete), full library control |
| 🔍 **Search & Filter** | Full-text search by title/author and genre-based category filtering |
| 📤 **CSV Export** | One-click export of personal collection in RFC-4180 format |
| 🔒 **Secure Auth** | HttpOnly JWT cookies, BCrypt password hashing, rate-limited login |
| 📱 **Fully Responsive** | Mobile-first layout with hamburger menu, fluid typography, and adaptive grids |

---

## 🛠️ Technical Stack

| Component | Technology | Notes |
| :--- | :--- | :--- |
| **Backend** | `.NET 10 Web API` | Clean architecture, Service pattern, EF Core |
| **Frontend** | `React 19 (Vite + Tailwind CSS)` | Framer Motion animations, responsive design |
| **Database** | `PostgreSQL` | EF Core Migrations, indexed queries |
| **Auth** | `JWT + HttpOnly Cookies` | BCrypt hashing, role-based authorization |
| **File Storage** | `Local /uploads/` | Covers in `/covers/`, PDFs in `/books/` |
| **API Docs** | `Scalar + OpenAPI` | Available at `/scalar` in development |
| **Deployment** | `Docker Compose` | Multi-container: API + Client + PostgreSQL |
| **Rate Limiting** | `ASP.NET Core Rate Limiter` | 5 auth attempts/min, 10 API req/sec |

---

## 🚀 Getting Started

### 🐋 Docker (Recommended)
```bash
docker-compose up --build
```
| Service | URL |
|---------|-----|
| Frontend | `http://localhost:3000` |
| API | `http://localhost:5000` |
| API Docs (Scalar) | `http://localhost:5000/scalar` |

### 💻 Local Development
```bash
# Terminal 1 — Backend API (port 5128)
cd BookTracker.Api
dotnet run

# Terminal 2 — Frontend (port 5173)
cd BookTracker.Client
npm install && npm run dev
```

---

## 🔑 Seeded Accounts

> The database is pre-seeded with the following accounts and 15 real book entries.

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `akash@gmail.com` | `akash123` |
| **User** | `suraj@gmail.com` | `suraj123` |

### 📖 Seeded Books (15 total)

**Admin Books — Public (10):**
The Shining · 1984 · The Hobbit · To Kill a Mockingbird · Dracula · Dune · Pride and Prejudice · The Catcher in the Rye · Frankenstein · The Great Gatsby

**User Books — Private (5):**
It · Neuromancer · The Name of the Wind · Fahrenheit 451 · The Haunting of Hill House

---

## 🧪 Quality Assurance

Built on a TDD foundation with **117 verified tests** across the full stack.

| Suite | Count | Status |
|-------|-------|--------|
| Backend (xUnit) | 48 | ✅ All Passed |
| Frontend (Vitest) | 69 | ✅ All Passed |
| **Total** | **117** | ✅ |

> Full TDD logs and KPI audit reports are in the [Doc/](./Doc/) folder.

---

## 📂 Repository Structure

```
Athenaeum/
├── BookTracker.Api/         # .NET 10 Web API
│   ├── Controllers/         # Auth, Books, Admin, Dashboard
│   ├── Services/            # Business logic layer
│   ├── Data/                # EF Core DbContext + Seeder
│   ├── Models/              # Domain models + DTOs
│   └── uploads/             # Cover images & PDF manuscripts
├── BookTracker.Client/      # React 19 SPA
│   └── src/
│       ├── pages/           # Dashboard, Collection, Detail, Admin
│       ├── components/      # Layout, BookCard, etc.
│       └── api/             # Axios instance + helpers
├── BookTracker.Tests/       # xUnit integration tests
├── Doc/                     # PRD, KPI, API docs, flow diagrams
│   ├── api_documentation.md
│   ├── e2e-flow-diagram.md
│   ├── user-flow-diagram.md
│   └── kpi_audit_report.md
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## 📐 User Flow

See the complete end-to-end flow in [`Doc/e2e-flow-diagram.md`](./Doc/e2e-flow-diagram.md), covering both the User and Admin journeys from login to every terminal state via Mermaid diagrams.

---

🏛️ **Athenaeum** — *Preserving the heritage of the private collection.*
