# Athenaeum: Book Collection Tracker

Athenaeum is a premium, full-stack application designed for book enthusiasts to catalog their libraries, track reading progress, and visualize their literary journey.

## 🚀 Quick Start (Docker)

The easiest way to run the entire stack is using Docker Compose.

1. **Clone the repository**
### 🐋 Option 1: Docker (Recommended)
1. **Start the application**:
   ```bash
   docker-compose up --build
   ```
2. **Access**:
   - **Frontend**: `http://localhost:3000`
   - **Backend API**: `http://localhost:5000`
   - **Documentation**: `http://localhost:5000/scalar`

### 💻 Option 2: Local Development
1. **Start Backend**: `cd BookTracker.Api && dotnet run`
2. **Start Frontend**: `cd BookTracker.Client && npm run dev`
3. **Access**:
   - **Frontend**: `http://localhost:5173`
   - **Backend API**: `http://localhost:5128`
   - **Documentation**: `http://localhost:5128/scalar`

## 🛠️ Tech Stack

- **Frontend**: React 19 (Vite) + Vanilla CSS (Premium Glassmorphism UI)
- **Backend**: ASP.NET Core 10 (.NET 10 Preview)
- **Database**: PostgreSQL (Dockerized)
- **Documentation**: Native OpenAPI + Scalar UI

## 📚 API Documentation

The API is fully documented using **Scalar**. With the server running, navigate to:
👉 `http://localhost:5000/scalar`

### Key Endpoints

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Create a new account | No |
| `POST` | `/api/v1/auth/login` | Obtain a JWT Token | No |
| `GET` | `/api/v1/books` | List all books (with search/filter) | Yes |
| `POST` | `/api/v1/books` | Add a new volume | Yes |
| `POST` | `/api/v1/books/{id}/cover` | Upload a cover image | Yes |
| `GET` | `/api/v1/dashboard/stats` | Get collection statistics | Yes |
| `GET` | `/api/v1/books/export` | Export library as CSV | Yes |

## 🧪 Testing

### Frontend (Vitest)
```bash
cd BookTracker.Client
npm test
```

### Backend (xUnit)
```bash
cd BookTracker.Tests
dotnet test
```

## 📂 Project Structure

- `BookTracker.Api/`: ASP.NET Core Backend.
- `BookTracker.Client/`: React Frontend.
- `Doc/`: PRD, KPI, and Design documentation.
- `uploads/`: Persistent storage for book covers (mapped as Docker volume).

---
*Created with ❤️ for book collectors everywhere.*
