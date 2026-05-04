

# Completion KPIs: Admin & Library System

These KPIs extend the core Book Collection Tracker and validate the **Admin Panel, Shared Library, and Recommendation System**.

All KPIs are **binary (Pass/Fail)** and must pass for the system to be considered complete.

---

## 🏛️ 1. Executive Dashboard (Metrics)

| #  | KPI                                           | Verification Method           |
| :- | :-------------------------------------------- | :---------------------------- |
| 75 | Admin dashboard shows total registered users  | Count matches database        |
| 76 | Admin dashboard shows total archived books    | Includes all public/private   |
| 77 | Admin dashboard shows pending approvals       | Count matches moderation queue|
| 78 | Stats update dynamically on dashboard load    | Perform action; stats update  |

---

## 👥 2. User Management (Archivist Registry)

| #  | KPI                                         | Verification Method                      |
| :- | :------------------------------------------ | :--------------------------------------- |
| 45 | Admin can view all registered archivists    | Open users page; list is populated       |
| 46 | Admin can search archivists by email        | Use search bar; results filter correctly |
| 47 | Admin can block/suspend user access         | Suspend user; login attempt fails        |
| 48 | Admin can unblock/restore user access       | Restore user; login succeeds             |
| 49 | Admin can permanently delete user records   | Delete user; removed from DB             |

---

## 📚 3. Library Management (Global Oversight)

| #  | KPI                                       | Verification Method                   |
| :- | :---------------------------------------- | :------------------------------------ |
| 50 | Admin can view ALL volumes in the archive | Access library tab; all books visible |
| 51 | Admin can edit metadata for ANY volume    | Update title/author; changes persist  |
| 52 | Admin can delete ANY volume globally      | Delete book; removed for all users    |
| 53 | Admin can see volume ownership details    | Verify uploader email is visible      |
| 54 | PDF Manuscripts are optional for curation | Register metadata-only book; succeeds |

---

## 📨 4. Recommendation Moderation

| #  | KPI                                        | Verification Method                 |
| :- | :----------------------------------------- | :---------------------------------- |
| 62 | User can request to publish a book         | Click “Recommend”; request created  |
| 63 | Recommendation stored with PENDING status  | Inspect DB; status = PENDING        |
| 64 | Admin can view all pending requests        | Open moderation queue; list visible |
| 65 | Admin can approve a request                | Click approve; book becomes PUBLIC  |
| 66 | Admin can reject a request                 | Click reject; book stays PRIVATE    |
| 67 | Rejected books remain accessible to owner  | Login as owner; book still present  |

---

## 🛡️ Security & Access Control

| #  | KPI                                            | Verification Method                    |
| :- | :--------------------------------------------- | :------------------------------------- |
| 79 | All admin APIs require ADMIN role              | Test API with user token; denied       |
| 80 | Unauthorized users cannot access admin panel   | Hit `/admin` as user; 403 or redirect  |
| 81 | Admin actions are logged in Audit system       | Perform action; check AuditLogs table  |
| 82 | JWT validation required for all curator tools  | Remove token; requests fail            |

---

# ✅ Completion Criteria

The Admin & Library System is considered **complete only when all KPIs (45–82) pass successfully**, ensuring absolute jurisdiction for the High Curator.

---
