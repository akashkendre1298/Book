# 🏛️ Athenaeum: The Private Archive

[![Full-Stack Verification](https://img.shields.io/badge/Tests-117%20Passed-success?style=for-the-badge&logo=dotnet)](./Doc/tdd_test_log.md)
[![KPI Fulfillment](https://img.shields.io/badge/KPIs-100%25-clay?style=for-the-badge)](./Doc/kpi_audit_report.md)
[![Architecture](https://img.shields.io/badge/Stack-.NET%2010%20%2B%20React%2019-sage?style=for-the-badge)](./README.md)

**Athenaeum** is a premium, full-stack book archiving system designed for the modern bibliophile. It combines the rigorous architectural standards of **.NET 10** with a high-end, **executive aesthetic** to provide a secure and sophisticated space for your private collection.

---

## 🌟 The Archivist's Experience

- **🏛️ Executive Dashboard**: A high-contrast "Archivist's Desk" view with glassmorphism stats and genre distribution.
- **📚 Folio Management**: Complete lifecycle tracking—from ISBN registration to persistent cover art uploads.
- **🎯 Annual Ambitions**: Set and track personalized reading goals with dynamic fulfillment gauges.
- **🔍 The Grand Index**: Advanced multi-criteria search and filtering for thousands of volumes.
- **📂 Data Portability**: Instant CSV export in RFC-4180 format for complete control over your library data.
- **🛡️ Secure Vault**: Industry-standard JWT authentication with encrypted password storage via BCrypt.

---

## 🛠️ Technical Pedigree

| Component | Technology | Role |
| :--- | :--- | :--- |
| **Backend** | `.NET 10 Core` | Enterprise-grade RESTful API |
| **Frontend** | `React 19 (Vite)` | High-performance SPA with Framer Motion |
| **Styling** | `Vanilla CSS` | Custom "Athenaeum" executive design system |
| **Database** | `PostgreSQL` | Relational storage with EF Core Migrations |
| **Documentation** | `Scalar & Swagger` | Dual-portal API documentation |
| **Deployment** | `Docker Compose` | Multi-container orchestration |

---

## 🚀 Deployment Folio

### 🐋 The Docker Way (Recommended)
Launch the entire Athenaeum ecosystem in seconds:
```bash
docker-compose up --build
```
- **Library Access**: `http://localhost:3000`
- **Archive API**: `http://localhost:5000`
- **Grand Index (Scalar)**: `http://localhost:5000/scalar`

### 💻 Local Development
1. **Initialize API**: `cd BookTracker.Api && dotnet run` (Listens on `:5128`)
2. **Initialize Frontend**: `cd BookTracker.Client && npm run dev` (Listens on `:5173`)

---

## 🧪 Quality Assurance & TDD

The Athenaeum is built on a foundation of **100% Test Coverage**. We follow a strict TDD (Red-Green-Refactor) lifecycle to ensure that every feature is verified before implementation.

- **Backend (xUnit)**: 48 Tests Passed
- **Frontend (Vitest)**: 69 Tests Passed
- **Total Verification**: **117 PASSED**

> [!TIP]
> Detailed TDD logs and KPI audit reports are available in the [Doc/](./Doc/) folder, documenting the verified status of all 39 business requirements.

---

## 📂 Repository Structure

- `BookTracker.Api/`: The engine. Clean architecture with Service-Repository pattern.
- `BookTracker.Client/`: The face. Responsive, accessible, and high-performance React UI.
- `Doc/`: The blueprints. Contains PRDs, KPI lists, and TDD audit logs.
- `uploads/`: The persistent volume for book cover art.

---

🏛️ **Athenaeum** — *Preserving the heritage of the private collection.*
