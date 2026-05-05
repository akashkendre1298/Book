# Athenaeum Security & Code Quality Audit Report

**Audit Date:** 2026-05-05  
**Reviewer:** Senior Code Reviewer (Antigravity)  
**Project:** Athenaeum Book Collection Tracker  
**Overall Health Score:** 4/10  

---

## 🔴 Critical Issues

### 1. Mass Assignment (Over-Posting) Vulnerability
*   **Issue:** The `Book` entity is used directly as an input parameter in `BooksController.Create` and `Update` methods.
*   **Impact:** A malicious user can craft a request including `"IsApproved": true, "Visibility": 1` to bypass the moderation system and inject books into the Public Library without curator approval.
*   **Fix Recommendation:** Implement separate Data Transfer Objects (DTOs) for creation and updates (e.g., `BookCreateDto`) that exclude administrative and system-managed fields.

### 2. Hardcoded Cryptographic Secrets
*   **Issue:** The JWT Signing Key is hardcoded in `Program.cs` and `AuthService.cs` as a fallback: `"super_secret_key_1234567890123456"`.
*   **Impact:** If an attacker knows this key (which is now in source control), they can forge valid JWT tokens for any user, including Admin accounts.
*   **Fix Recommendation:** Remove fallbacks. Use `builder.Configuration["Jwt:Key"]` and ensure it is provided via Environment Variables or a Secret Manager (e.g., Azure Key Vault, AWS Secrets Manager) in production.

### 3. Insecure JWT Storage (XSS Risk)
*   **Issue:** The React frontend stores the JWT token in `localStorage`.
*   **Impact:** Any Cross-Site Scripting (XSS) vulnerability on the site allows an attacker to steal the user's authentication token and hijack their session.
*   **Fix Recommendation:** Use `HttpOnly` and `Secure` cookies for token storage, or at minimum, store the token in-memory and use a refresh token pattern.

### 4. Lack of Authentication Rate Limiting
*   **Issue:** The `/api/v1/auth/login` endpoint has no rate limiting or brute-force protection.
*   **Impact:** Attackers can run automated dictionary attacks against user accounts without being blocked or throttled.
*   **Fix Recommendation:** Implement `AspNetCoreRateLimit` or use the built-in rate limiting middleware introduced in .NET 7+ to throttle login attempts per IP/Email.

### 5. Insecure File Upload Handling
*   **Issue:** Files are saved directly to the web root (`/uploads`) using user-provided extensions without content validation (magic bytes).
*   **Impact:** Potential for Path Traversal or execution of malicious scripts if the server is misconfigured to execute files in the uploads directory.
*   **Fix Recommendation:** Validate file signatures (not just extensions), restrict allowed MIME types strictly, and serve files through a dedicated controller that checks permissions, or use a cloud storage provider (S3/Azure Blob).

---

## 🟠 Improvements Needed

### 1. Missing Pagination on List Endpoints
*   **Issue:** `GetAllUsers`, `GetAllBooks`, and `GetPublicLibrary` return all records in a single response.
*   **Why it matters:** As the library grows, these requests will become increasingly slow and eventually crash the application or the client browser due to memory exhaustion.
*   **Suggested improvement:** Implement `Skip` and `Take` parameters in the API and use `PagedList` patterns.

### 2. Performance Bottlenecks (N+1 & Large Payload)
*   **Issue:** `BookService.GetStatsAsync` loads all user books into memory to calculate counts and sums.
*   **Why it matters:** Inefficient use of database resources and high latency for users with large collections.
*   **Suggested improvement:** Use EF Core's `CountAsync()`, `SumAsync()`, and `GroupBy()` to perform calculations on the database server.

### 3. CSV Injection Risk in Export
*   **Issue:** The `ExportCsvAsync` method manually concatenates strings without escaping or checking for formula characters (`=`, `+`, `-`, `@`).
*   **Why it matters:** Malicious book titles can execute commands in Excel/Google Sheets when the user opens the export.
*   **Suggested improvement:** Use a library like `CsvHelper` to handle escaping and add a prefix to potential formula characters.

### 4. Database Indexing
*   **Issue:** The `Books` table lacks an index on `UserId`.
*   **Why it matters:** Since almost every query filters by `UserId`, the database must perform a full table scan for every request, which scales poorly.
*   **Suggested improvement:** Add a non-clustered index on `UserId` in `AppDbContext.OnModelCreating`.

### 5. Violation of DRY & Separation of Concerns
*   **Issue:** `BooksController` contains complex file system logic. `AdminController` has duplicated logging logic.
*   **Why it matters:** Makes the code harder to test, maintain, and refactor.
*   **Suggested improvement:** Create a `FileStorageService` for upload logic and an `ActionFilter` or `AuditService` for administrative logging.

---

## 🟢 Good Practices Found

*   **IDOR Protection:** The `BookService` correctly checks ownership (`UserId == userId`) before allowing access to private records.
*   **Password Security:** Proper use of `BCrypt.Net` for password hashing ensures that even if the database is leaked, passwords are not immediately exposed.
*   **Visual Design:** The frontend follows a cohesive, premium "Archive" aesthetic that enhances user engagement.
*   **State Neutrality:** The Public Library implementation correctly strips user-specific reading state from public records.

---

## 📌 Final Summary

### Top 5 Risks
1.  **Administrative Bypass:** Over-posting on book creation.
2.  **Account Takeover:** Hardcoded JWT secrets.
3.  **Credential Stuffing:** Missing rate limiting on Auth.
4.  **Session Hijacking:** JWT exposure in `localStorage`.
5.  **Service Denial:** Lack of pagination on core entities.

### Priority Action Plan
1.  **IMMEDIATE:** Remove hardcoded JWT keys and implement DTOs for `Book` creation/updates.
2.  **URGENT:** Add rate limiting to the Auth Controller and add a database index to `Books.UserId`.
3.  **SHORT TERM:** Implement pagination for all list endpoints and move file logic to a service.
4.  **STABILIZATION:** Move JWT to HttpOnly cookies and implement proper CSV escaping.
