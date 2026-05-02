# 🧪 TDD Execution Log: Book Collection Tracker (.NET)

> [!IMPORTANT]
> **Current Phase**: 🔴 **RED** (Initial Failing Tests)  
> **Last Updated**: 2026-05-02  
> **Technology Stack**: ASP.NET Core 10, xUnit, FluentAssertions, Moq, PostgreSQL.

---

## 🔐 1. Authentication
*Focus: Security, Identity, and Access Control.*

| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Password Hashing | Secure one-way hash using BCrypt |
| 🔴 | **Unit** | Password Verification | Correctly matches hash to plain text |
| 🔴 | **Unit** | Email Validation | Rejects non-RFC compliant emails |
| 🔴 | **Unit** | Password Strength | Minimum 8 chars, mixed case, symbols |
| 🔴 | **API** | User Registration | 201 Created on valid input |
| 🔴 | **API** | Duplicate Email Check | 409 Conflict if email exists |
| 🔴 | **API** | Invalid Login | 401 Unauthorized for wrong credentials |
| 🔴 | **API** | Protected Access | 401 Unauthorized without Bearer token |
| 🔴 | **API** | Session Logout | Token/Session invalidation |
| 🔴 | **Edge** | Case Normalization | Email `User@Ex.com` -> `user@ex.com` |

---

## 📚 2. Book Management
*Focus: Core CRUD operations and data integrity.*

| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Required Fields | Fails if Title/Author is null |
| 🔴 | **Unit** | ISBN Formatting | Supports ISBN-10 and ISBN-13 |
| 🔴 | **Unit** | Pub Year Range | Rejects future years or pre-1450 |
| 🔴 | **API** | Create Book | 201 Created with returned ID |
| 🔴 | **API** | Update Metadata | 200 OK with reflected changes |
| 🔴 | **API** | Delete Book | 204 No Content; removed from DB |
| 🔴 | **File** | Cover Upload | Validates image MIME types (JPEG/PNG) |
| 🔴 | **File** | Payload Size | Rejects uploads > 5MB |
| 🔴 | **Edge** | Duplicate ISBN | Prevents multiple entries for same user |

---

## 📖 3. Reading Status & Progress
*Focus: Real-time progress tracking and business rules.*

| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Progress Constraint | `current_page` cannot exceed `total_pages` |
| 🔴 | **Unit** | Rating Range | Validates stars are strictly 1–5 |
| 🔴 | **API** | Status Transition | Updates from 'Want to Read' to 'Reading' |
| 🔴 | **API** | Completion Auto-Date | Sets `finish_date` when status is 'Read' |
| 🔴 | **Edge** | Boundary Progress | Handles `page 0` and `page total` |

---

## 🔍 4. Search & Filtering
*Focus: Performance and complex query logic.*

| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | AND Logic | Combining Genre + Rating filters |
| 🔴 | **Unit** | OR Logic | Combining multiple Status filters |
| 🔴 | **Unit** | Sorting | Correct order for Rating/DateAdded |
| 🔴 | **API** | Search Query | Case-insensitive matching on Title/Author |
| 🔴 | **API** | Empty Search | Returns all books (unfiltered) |
| 🔴 | **Edge** | Special Characters | Handles search for `C#`, `O'Reilly`, etc. |

---

## 📊 5. Statistics & Dashboard
*Focus: Data aggregation and reporting.*

| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Stats Aggregator | Correctly sums pages/counts from dataset |
| 🔴 | **Unit** | Goal Tracking | Calculates % progress against yearly goal |
| 🔴 | **API** | Dashboard Data | 200 OK with full statistics JSON |
| 🔴 | **API** | CSV Export | Valid RFC-4180 CSV binary stream |
| 🔴 | **Edge** | Zero Dataset | Returns valid structure with 0 counts |

---

## 🐳 6. System & Infrastructure
*Focus: Deployment and environmental stability.*

| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **System** | Docker Startup | `docker-compose up` results in healthy state |
| 🔴 | **System** | Data Persistence | Volume mounts retain data after restart |
| 🔴 | **System** | Env Validation | Fails gracefully if DB connection string missing |

---

## 🚀 Execution Summary
- **Total Tests**: 47
- **Passed**: 0
- **Failed**: 47
- **Pending**: 0

> [!TIP]
> To move to **GREEN**, start with the `AuthService` implementation and verify against the `AuthTests` suite.
