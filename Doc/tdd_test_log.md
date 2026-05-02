# 🧪 TDD Execution Log: Book Collection Tracker (.NET)

> [!IMPORTANT]
> This log tracks the evolution of the project from initial failures (RED) to verified implementation (GREEN). It preserves the state of each test case through both phases to demonstrate the TDD lifecycle.

---

## 🔴 Phase 1: RED (Initial State)
*This section documents the initial state of the system where all 48 test cases were failing or unimplemented.*

### 🔐 1. Authentication (11 Tests)
| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Password Hashing | Secure one-way hash using BCrypt |
| 🔴 | **Unit** | Password Verification | Correctly matches hash to plain text |
| 🔴 | **Unit** | Email Validation | Rejects non-RFC compliant emails |
| 🔴 | **Unit** | Password Strength | Minimum 8 chars, mixed case, symbols |
| 🔴 | **API** | User Registration | 201 Created on valid input |
| 🔴 | **API** | Invalid Email Reg | 400 Bad Request for malformed email |
| 🔴 | **API** | Short Password Reg | 400 Bad Request for < 8 chars |
| 🔴 | **API** | Duplicate Email Check | 409 Conflict if email exists |
| 🔴 | **API** | Invalid Login | 401 Unauthorized for wrong credentials |
| 🔴 | **API** | Protected Access | 401 Unauthorized without Bearer token |
| 🔴 | **API** | Session Logout | 204 No Content; session end |
| 🔴 | **Edge** | Case Normalization | Email normalization to lowercase |

### 📚 2. Book Management (12 Tests)
| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Required Fields | Fails if Title/Author is null |
| 🔴 | **Unit** | ISBN-10 | Supports 10-digit ISBN validation |
| 🔴 | **Unit** | ISBN-13 (Hyphens) | Supports 13-digit hyphenated ISBN |
| 🔴 | **Unit** | Pub Year (1450) | Validates lower boundary of 1450 |
| 🔴 | **Unit** | Pub Year (Future) | Rejects years beyond current + 1 |
| 🔴 | **API** | Create Book | 201 Created with returned ID |
| 🔴 | **API** | GetAll Books | 200 OK with user-specific collection |
| 🔴 | **API** | Update Metadata | 200 OK with reflected changes |
| 🔴 | **API** | Delete Book | 204 No Content; removed from DB |
| 🔴 | **API** | Get 404 | 404 NotFound for invalid IDs |
| 🔴 | **File** | Cover Upload | Handles multipart/form-data image upload |
| 🔴 | **Edge** | Duplicate ISBN | 409 Conflict for same ISBN per user |

### 📖 3. Reading Status & Progress (7 Tests)
| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **Unit** | Progress Constraint | `current_page` cannot exceed `total_pages` |
| 🔴 | **API** | Status Transition | Updates from 'Want to Read' to 'Reading' |
| 🔴 | **API** | Progress Update | PATCH updates current page count |
| 🔴 | **API** | Progress 400 | Rejects progress exceeding total pages |
| 🔴 | **API** | Auto-Completion | Sets status to 'Read' at 100% progress |
| 🔴 | **API** | Add Rating | Saves 1-5 star rating |
| 🔴 | **Edge** | Multiple Ratings | Overwrites previous rating correctly |

### 🔍 4. Search & Filtering (10 Tests)
| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **API** | Search Title | Partial match search on Title |
| 🔴 | **API** | Search Author | Partial match search on Author |
| 🔴 | **API** | Filter Status | Filter results by ReadingStatus enum |
| 🔴 | **API** | Filter Genre | Filter results by string genre |
| 🔴 | **API** | Filter Combined | Filter by both Status and Genre |
| 🔴 | **API** | Sort Rating | Sort by rating (descending) |
| 🔴 | **API** | Sort Date | Sort by date added (newest first) |
| 🔴 | **API** | Sort Title | Sort alphabetically by title |
| 🔴 | **Edge** | Case Insensitive | Search works regardless of casing |
| 🔴 | **Edge** | Special Characters | Handles encoded chars (e.g., C#) |

### 📊 5. Statistics & Dashboard (8 Tests)
| Status | Test Category | Test Case | Expected Outcome |
| :---: | :--- | :--- | :--- |
| 🔴 | **API** | Stats Summary | Total books, pages read, reading count |
| 🔴 | **API** | Genre Dist | Distribution map of books by genre |
| 🔴 | **API** | Goal Setting | Create yearly reading goal |
| 🔴 | **API** | Goal Update | Update existing yearly goal |
| 🔴 | **API** | CSV Export | Download collection in RFC-4180 format |
| 🔴 | **API** | Export Empty | Returns headers only for empty collections |
| 🔴 | **Edge** | Zero Dataset | Aggregates return 0 instead of errors |
| 🔴 | **Edge** | Large Numbers | Handles millions of pages correctly |

---

## ✅ Phase 2: GREEN (Final Verification)
*This section documents the verified state where all 48 test cases are PASSED.*

### 🔐 1. Authentication (11 Tests)
| Status | Test Category | Test Case | Current Result |
| :---: | :--- | :--- | :--- |
| ✅ | **Unit** | Password Hashing | PASSED (BCrypt.Net) |
| ✅ | **Unit** | Password Verification | PASSED |
| ✅ | **Unit** | Email Validation | PASSED |
| ✅ | **Unit** | Password Strength | PASSED |
| ✅ | **API** | User Registration | PASSED |
| ✅ | **API** | Invalid Email Reg | PASSED |
| ✅ | **API** | Short Password Reg | PASSED |
| ✅ | **API** | Duplicate Email Check | PASSED |
| ✅ | **API** | Invalid Login | PASSED |
| ✅ | **API** | Protected Access | PASSED |
| ✅ | **API** | Session Logout | PASSED |
| ✅ | **Edge** | Case Normalization | PASSED |

### 📚 2. Book Management (12 Tests)
| Status | Test Category | Test Case | Current Result |
| :---: | :--- | :--- | :--- |
| ✅ | **Unit** | Required Fields | PASSED |
| ✅ | **Unit** | ISBN-10 | PASSED |
| ✅ | **Unit** | ISBN-13 (Hyphens) | PASSED |
| ✅ | **Unit** | Pub Year (1450) | PASSED |
| ✅ | **Unit** | Pub Year (Future) | PASSED |
| ✅ | **API** | Create Book | PASSED |
| ✅ | **API** | GetAll Books | PASSED |
| ✅ | **API** | Update Metadata | PASSED |
| ✅ | **API** | Delete Book | PASSED |
| ✅ | **API** | Get 404 | PASSED |
| ✅ | **File** | Cover Upload | PASSED |
| ✅ | **Edge** | Duplicate ISBN | PASSED |

### 📖 3. Reading Status & Progress (7 Tests)
| Status | Test Category | Test Case | Current Result |
| :---: | :--- | :--- | :--- |
| ✅ | **Unit** | Progress Constraint | PASSED |
| ✅ | **API** | Status Transition | PASSED |
| ✅ | **API** | Progress Update | PASSED |
| ✅ | **API** | Progress 400 | PASSED |
| ✅ | **API** | Auto-Completion | PASSED |
| ✅ | **API** | Add Rating | PASSED |
| ✅ | **Edge** | Multiple Ratings | PASSED |

### 🔍 4. Search & Filtering (10 Tests)
| Status | Test Category | Test Case | Current Result |
| :---: | :--- | :--- | :--- |
| ✅ | **API** | Search Title | PASSED |
| ✅ | **API** | Search Author | PASSED |
| ✅ | **API** | Filter Status | PASSED |
| ✅ | **API** | Filter Genre | PASSED |
| ✅ | **API** | Filter Combined | PASSED |
| ✅ | **API** | Sort Rating | PASSED |
| ✅ | **API** | Sort Date | PASSED |
| ✅ | **API** | Sort Title | PASSED |
| ✅ | **Edge** | Case Insensitive | PASSED |
| ✅ | **Edge** | Special Characters | PASSED |

### 📊 5. Statistics & Dashboard (8 Tests)
| Status | Test Category | Test Case | Current Result |
| :---: | :--- | :--- | :--- |
| ✅ | **API** | Stats Summary | PASSED |
| ✅ | **API** | Genre Dist | PASSED |
| ✅ | **API** | Goal Setting | PASSED |
| ✅ | **API** | Goal Update | PASSED |
| ✅ | **API** | CSV Export | PASSED |
| ✅ | **API** | Export Empty | PASSED |
| ✅ | **Edge** | Zero Dataset | PASSED |
| ✅ | **Edge** | Large Numbers | PASSED |

---

## 🚀 Final Summary
- **Total Tests Implemented**: 48
- **RED State Count**: 48/48
- **GREEN State Count**: 48/48
- **Success Rate**: 100%

> [!TIP]
> The backend logic is now fully verified. The project maintains a complete TDD audit trail ensuring that every implemented feature was first modeled as a failing requirement.
