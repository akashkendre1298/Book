

# Completion KPIs: Admin & Library System

These KPIs extend the core Book Collection Tracker and validate the **Admin Panel, Shared Library, and Recommendation System**.

All KPIs are **binary (Pass/Fail)** and must pass for the system to be considered complete.

---

## 👨‍💼 Admin Management

| #  | KPI                                        | Verification Method                              |
| :- | :----------------------------------------- | :----------------------------------------------- |
| 40 | Admin role exists in system                | Inspect database; role field contains 'ADMIN'    |
| 41 | Admin can log in and access admin panel    | Login as admin; `/admin` route accessible        |
| 42 | Non-admin users cannot access admin routes | Try accessing `/admin`; receive 403/redirect     |
| 43 | Admin middleware protects all admin APIs   | Inspect backend; all admin routes use role check |
| 44 | Admin session persists like normal users   | Restart browser; admin remains logged in         |

---

## 👥 User Management (Admin)

| #  | KPI                                         | Verification Method                      |
| :- | :------------------------------------------ | :--------------------------------------- |
| 45 | Admin can view all registered users         | Open admin users page; list is populated |
| 46 | Admin can search/filter users               | Apply search/filter; results update      |
| 47 | Admin can deactivate/block a user           | Disable user; login attempt fails        |
| 48 | Admin can delete a user                     | Delete user; user removed from DB        |
| 49 | Admin actions reflect immediately in system | Perform action; verify real-time effect  |

---

## 📚 Admin Library Management (PDF Upload)

| #  | KPI                                       | Verification Method                   |
| :- | :---------------------------------------- | :------------------------------------ |
| 50 | Admin can upload a book with PDF file     | Upload PDF; book appears in library   |
| 51 | Uploaded book is visible to all users     | Login as different user; book visible |
| 52 | PDF file is stored and accessible via URL | Open file URL; PDF loads correctly    |
| 53 | Admin can edit library book details       | Update metadata; changes reflected    |
| 54 | Admin can delete library books            | Delete book; removed for all users    |

---

## 📖 PDF Reading Experience

| #  | KPI                                         | Verification Method                           |
| :- | :------------------------------------------ | :-------------------------------------------- |
| 55 | Users can open and read PDF inside app      | Click book; PDF viewer loads                  |
| 56 | PDF loads without breaking layout           | Check UI; no overflow or crash                |
| 57 | Large PDFs load with acceptable performance | Open large file; loads within reasonable time |

---

## 🔐 Book Visibility Rules

| #  | KPI                                         | Verification Method                   |
| :- | :------------------------------------------ | :------------------------------------ |
| 58 | Admin-uploaded books are PUBLIC             | Verify visibility = PUBLIC in DB      |
| 59 | User-uploaded books are PRIVATE by default  | Upload as user; not visible to others |
| 60 | Private books visible only to owner + admin | Login as another user; cannot see     |
| 61 | Public books visible to all users           | Verify across multiple accounts       |

---

## 🚀 Recommendation System (User → Admin → Public)

| #  | KPI                                        | Verification Method                 |
| :- | :----------------------------------------- | :---------------------------------- |
| 62 | User can request to publish a book         | Click “Recommend”; request created  |
| 63 | Recommendation stored with PENDING status  | Inspect DB; status = PENDING        |
| 64 | Admin can view all recommendation requests | Open admin panel; list visible      |
| 65 | Admin can approve a request                | Click approve; status updated       |
| 66 | Approved book becomes PUBLIC               | Verify visibility change            |
| 67 | Approved book visible to all users         | Login as another user; book appears |
| 68 | Admin can reject a request                 | Click reject; status updated        |
| 69 | Rejected book remains PRIVATE              | Verify not visible to others        |
| 70 | User can still access rejected book        | Login as owner; book accessible     |

---

## 🔄 Library Integration with User Collection

| #  | KPI                                                   | Verification Method                 |
| :- | :---------------------------------------------------- | :---------------------------------- |
| 71 | User can add library book to collection               | Click “Add”; appears in My Books    |
| 72 | User can track reading progress for library books     | Update progress; saved correctly    |
| 73 | User data (progress, rating) is stored separately     | Inspect DB; stored in mapping table |
| 74 | Removing from collection does not delete library book | Remove book; still exists globally  |

---

## 📊 Admin Analytics Dashboard

| #  | KPI                                           | Verification Method           |
| :- | :-------------------------------------------- | :---------------------------- |
| 75 | Admin dashboard shows total users             | Count matches DB              |
| 76 | Admin dashboard shows total books             | Includes all types            |
| 77 | Admin dashboard shows public vs private books | Data correctly split          |
| 78 | Admin dashboard updates in real-time          | Add/remove data; stats update |

---

## 🛡️ Security & Access Control

| #  | KPI                                            | Verification Method                    |
| :- | :--------------------------------------------- | :------------------------------------- |
| 79 | All admin APIs require ADMIN role              | Test API with user token; denied       |
| 80 | File access respects visibility rules          | Try accessing private file; blocked    |
| 81 | Unauthorized users cannot access PDFs directly | Hit file URL; access denied if private |
| 82 | JWT validation required for protected routes   | Remove token; request fails            |

---

## 🧪 Testing & Reliability

| #  | KPI                                            | Verification Method            |
| :- | :--------------------------------------------- | :----------------------------- |
| 83 | Unit tests exist for role-based access control | Run tests; pass                |
| 84 | Unit tests exist for recommendation flow       | Approve/reject tested          |
| 85 | Unit tests exist for visibility logic          | Public/private logic validated |
| 86 | All new tests pass successfully                | Run test suite; all green      |

---

## 🐳 Deployment & Storage

| #  | KPI                                         | Verification Method            |
| :- | :------------------------------------------ | :----------------------------- |
| 87 | PDF files persist across container restarts | Restart Docker; files remain   |
| 88 | File storage works in Docker volume         | Inspect volume; files present  |
| 89 | Admin features work in deployed environment | Test in container; no failures |

---

# ✅ Completion Criteria

The Admin & Library System is considered **complete only when all KPIs (40–89) pass successfully**, in addition to the base system KPIs.

---
