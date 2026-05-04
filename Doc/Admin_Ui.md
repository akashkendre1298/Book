# Admin UI Specification

This document defines all **Admin Screens, Layout, and Interactions** for the Book Collection Tracker.

The Admin Panel is responsible for:

* Managing users
* Managing public (library) books
* Moderating user recommendations
* Viewing system-wide analytics

---

# 🧭 Admin Layout

## Structure

```txt
Sidebar (Left)
Topbar (Header)
Main Content Area
```

---

## 📌 Sidebar Navigation

* Dashboard
* Users
* Library
* Recommendations

---

## 🔝 Topbar

* Global Search
* Profile Menu

  * Role: ADMIN
  * Logout

---

# 🏠 1. Admin Dashboard

## Purpose

Provide system-wide overview and insights

## Components

### 📊 Stats Cards

* Total Users
* Total Books
* Public Books
* Private Books
* Pending Recommendations

---

### 📈 Charts

* Books by Genre
* User Growth (optional)
* Reading Trends (optional)

---

## Behavior

* Data updates in real-time
* Clicking cards navigates to respective sections

---

# 👥 2. Users Management

## Purpose

Manage all registered users

## Layout

Table view

## Columns

* Name
* Email
* Status (Active / Blocked)
* Joined Date

---

## Actions

### 🔒 Block / Unblock

* Blocks user login
* Immediate effect

### 🗑 Delete User

* Removes user from system
* Confirmation required

---

## Behavior

* Search users by name/email
* Filter by status

---

# 📚 3. Library Management (Admin Library)

## Purpose

Manage all PUBLIC books

---

## Layout

* Grid View (default)
* Optional List View

---

## Book Card

### Shows

* Cover Image
* Title
* Author
* Tag: 🏢 Admin / 👤 User Published

---

## Actions

### ✏️ Edit Book

* Update metadata

### 🗑 Delete Book

* Removes book for all users

---

## Primary Action

* ➕ Upload New Book

---

# ⬆️ 4. Upload Book (Admin)

## Purpose

Add new public book (PDF)

---

## Form Fields

* Title (required)
* Author (required)
* Genre
* Upload PDF (required)
* Upload Cover Image

---

## Behavior

* On submit:

  * Book is saved as:

    * visibility = PUBLIC
    * roleOwner = ADMIN

* Success message shown

* Redirect to Library

---

# 📨 5. Recommendations (Moderation Panel)

## Purpose

Review user-submitted books for public publishing

---

## Layout

Table or card list

---

## Each Item

* Book Title
* Author
* Uploaded By (User)
* Preview (optional PDF or metadata)

---

## Actions

### ✅ Approve

* Book becomes PUBLIC
* Visible to all users

---

### ❌ Reject

* Book remains PRIVATE
* Only visible to owner + admin

---

## Behavior

* Status filter:

  * Pending
  * Approved
  * Rejected

* Default view: Pending

---

# 🔐 Access Control (UI Level)

* All admin screens require ADMIN role
* Non-admin users cannot access routes
* Actions hidden for unauthorized users

---

# 🏷️ Labels & Status Indicators

| Label             | Meaning                 |
| ----------------- | ----------------------- |
| 🏢 Admin          | Uploaded by admin       |
| 👤 User Published | Approved user book      |
| ⏳ Pending         | Awaiting admin approval |
| ❌ Rejected        | Not approved            |

---

# ⚠️ UX RULES

* Always show confirmation before delete
* Keep moderation actions clear and visible
* Avoid mixing private books in admin library view
* Highlight pending actions (notifications/badge)

---

# 🔁 Navigation Flow

## Approving a Book

```txt
Dashboard → Recommendations → Approve → Library
```

---

## Managing Users

```txt
Dashboard → Users → Block/Delete
```

---

## Uploading Book

```txt
Library → Upload Book → Save → Visible to all users
```

---

# ✅ FINAL GOAL

The Admin UI should:

* Provide **full control over system data**
* Keep workflows **fast and clear**
* Maintain **consistency with user UI**
* Enable **moderation without complexity**

---
