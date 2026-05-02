# TDD Test Execution Log - Backend (.NET)

## Phase: RED (Initial Failing Tests)
Date: 2026-05-02
Technology: ASP.NET Core, xUnit, FluentAssertions

---

### 🔐 1. Authentication
#### Unit Tests
- [FAIL] Hash password before saving
- [FAIL] Compare hashed password correctly
- [FAIL] Validate email format
- [FAIL] Validate password strength
#### API Tests
- [FAIL] Register valid data (201)
- [FAIL] Register duplicate email (409)
- [FAIL] Register invalid email (400)
- [FAIL] Register weak password (400)
- [FAIL] Login valid credentials (200)
- [FAIL] Login wrong password (401)
- [FAIL] Login non-existing user (401/404)
- [FAIL] Access protected route with token (200)
- [FAIL] Access protected route without token (401)
- [FAIL] Logout clears session/token
#### Edge Cases
- [FAIL] Email case normalization
- [FAIL] Empty fields
- [FAIL] Large payload input

---

### 📚 2. Book Management
#### Unit Tests
- [FAIL] Validate required fields (title, author)
- [FAIL] Validate ISBN format
- [FAIL] Validate publication year range
- [FAIL] Handle optional fields
#### API Tests
- [FAIL] Create book (valid) (201)
- [FAIL] Create book (missing title) (400)
- [FAIL] Create book (missing author) (400)
- [FAIL] Get all books (200)
- [FAIL] Get single book (200)
- [FAIL] Update book (200)
- [FAIL] Update non-existing book (404)
- [FAIL] Delete book (200)
- [FAIL] Delete non-existing book (404)
#### File Upload
- [FAIL] Upload valid image
- [FAIL] Upload invalid file (400)
- [FAIL] Upload large file (rejected)
#### Edge Cases
- [FAIL] Duplicate ISBN
- [FAIL] Very long text
- [FAIL] Special characters

---

### 📖 3. Reading Status & Progress
#### Unit Tests
- [FAIL] Valid status transitions
- [FAIL] Progress <= total pages
- [FAIL] Progress >= 0
- [FAIL] Rating between 1–5
#### API Tests
- [FAIL] Update reading status (200)
- [FAIL] Update progress (200)
- [FAIL] Invalid progress (400)
- [FAIL] Mark as completed (stores date)
- [FAIL] Add rating (200)
- [FAIL] Invalid rating (400)
- [FAIL] Add review (200)
#### Edge Cases
- [FAIL] Progress = 0
- [FAIL] Progress = total pages
- [FAIL] Repeated updates

---

### 🔍 4. Search & Filtering
#### Unit Tests
- [FAIL] Filter by title/author/ISBN/genre/status/rating
- [FAIL] Sorting logic (title, author, rating, date)
- [FAIL] AND/OR filtering logic
#### API Tests
- [FAIL] Search returns correct results
- [FAIL] Empty search returns all
- [FAIL] Invalid filter handled
#### Edge Cases
- [FAIL] Case-insensitive search
- [FAIL] Partial match search
- [FAIL] Special characters
- [FAIL] Large dataset

---

### 📊 5. Statistics & Dashboard
#### Unit Tests
- [FAIL] Total books calculation
- [FAIL] Read books count
- [FAIL] Pages read calculation
- [FAIL] Genre/Rating distribution
- [FAIL] Goal progress calculation
#### API Tests
- [FAIL] Fetch dashboard stats (200)
- [FAIL] Set reading goal (200)
- [FAIL] Invalid goal (400)
- [FAIL] Export CSV (correct format)
#### Edge Cases
- [FAIL] Zero books
- [FAIL] Large dataset

---

### 🐳 6. Docker & System
- [FAIL] App starts with docker-compose
- [FAIL] DB connection success
- [FAIL] DB persists after restart
- [FAIL] API works inside container
- [FAIL] Missing env variables handled

---
Next Step: Implement C# xUnit tests for ALL remaining backend cases.
