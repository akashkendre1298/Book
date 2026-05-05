# Athenaeum Archive — API Documentation (v1.0)

**Base URL (Local):** `http://localhost:5128/api/v1`  
**Base URL (Docker):** `http://localhost:5000/api/v1`  
**Interactive Docs:** `http://localhost:5128/scalar` *(development only)*

## Authentication

All endpoints except `/auth/login` and `/auth/register` require a valid session.  
Authentication is handled via **HttpOnly JWT Cookies** (set automatically on login).

> The cookie name is `athenaeum_auth` and is read server-side — no `Authorization` header needed from the browser client.

**Rate Limits:**
- `/auth/*` — 5 requests/minute
- All other endpoints — 10 requests/second

---

## 1. Auth (`/api/v1/auth`)

### `POST /auth/register`
Register a new user account.

**Request Body (JSON):**
```json
{
  "email": "user@example.com",
  "password": "StrongPassword1"
}
```
> Password must be 8+ characters, contain uppercase, lowercase, and a digit.

**Responses:**
- `201 Created` — User registered; auto-login JWT cookie set.
- `400 Bad Request` — Validation failed or email already exists.
- `429 Too Many Requests` — Rate limit exceeded.

---

### `POST /auth/login`
Authenticate and receive a JWT session cookie.

**Request Body (JSON):**
```json
{
  "email": "akash@gmail.com",
  "password": "akash123"
}
```

**Response `200 OK`:**
```json
{
  "token": "eyJhbG...",
  "user": {
    "id": "uuid",
    "email": "akash@gmail.com",
    "role": "Admin",
    "isActive": true,
    "createdAt": "2026-05-05T00:00:00Z"
  }
}
```
- `401 Unauthorized` — Invalid credentials.
- `403 Forbidden` — Account is deactivated by admin.

---

### `POST /auth/logout`
Clear the JWT session cookie.

**Response:** `204 No Content`

---

### `GET /auth/me`
Return the currently authenticated user's profile.

**Response `200 OK`:**
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "role": "User",
  "isActive": true,
  "createdAt": "2026-05-05T00:00:00Z"
}
```
- `401 Unauthorized` — Not logged in.

---

## 2. Books (`/api/v1/books`)

> All book endpoints require authentication (`[Authorize]`).

### `GET /books` or `GET /books/my`
Get the authenticated user's private collection.

**Query Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `query` | string | Search by title or author |
| `status` | int | `0=WantToRead`, `1=Reading`, `2=Read`, `3=Dropped` |
| `sortBy` | string | `title`, `author`, `createdAt` (default) |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20) |

**Response `200 OK`:**
```json
{
  "items": [ { "id": "...", "title": "Dune", "author": "Frank Herbert", ... } ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

### `GET /books/public`
Browse the public library (admin-approved books only).

**Query Parameters:** `query`, `genre`, `sortBy`, `page`, `pageSize`

---

### `GET /books/completed`
Get only the user's finished (Read status) books.

**Query Parameters:** `query`, `sortBy`, `page`, `pageSize`

---

### `GET /books/{id}`
Get detailed info for a specific book.

- `200 OK` — Book details.
- `404 Not Found` — Book doesn't exist or access denied.

---

### `POST /books`
Add a new book to the user's collection (JSON body, no file).

**Request Body (JSON):**
```json
{
  "title": "The Shining",
  "author": "Stephen King",
  "isbn": "978-0385121675",
  "genre": "Horror",
  "publicationYear": 1977,
  "totalPages": 447
}
```
- `201 Created` — Book created (Private, WantToRead by default).
- `409 Conflict` — Duplicate ISBN in user's collection.

---

### `POST /books/upload`
Upload a new book with cover image and/or PDF file (`multipart/form-data`).

**Form Fields:**
| Field | Type | Required |
|-------|------|----------|
| `title` | string | ✅ |
| `author` | string | ✅ |
| `genre` | string | ❌ |
| `isbn` | string | ❌ |
| `totalPages` | int | ❌ |
| `publicationYear` | int | ❌ |
| `isPublic` | bool | ❌ |
| `cover` | file (.jpg/.png) | ❌ |
| `pdf` | file (.pdf) | ❌ |

- `201 Created` — Book saved; cover at `/uploads/covers/`, PDF at `/uploads/books/`.

---

### `PUT /books/{id}`
Replace all updatable fields of an existing book.

**Request Body (JSON):** Same shape as `POST /books`.
- `200 OK` — Updated book.
- `404 Not Found`.

---

### `PATCH /books/{id}`
Partial update (any field subset via JSON Patch).

```json
{ "review": "A masterpiece of atmospheric horror." }
```

---

### `PATCH /books/{id}/status`
Update reading status.

```json
{ "status": "Reading" }
```
> Accepts enum string (`"WantToRead"`, `"Reading"`, `"Read"`, `"Dropped"`) or integer (0–3).

---

### `PATCH /books/{id}/progress`
Update current page number.

```json
{ "currentPage": 150 }
```
- Validates: `0 ≤ currentPage ≤ totalPages`.

---

### `PATCH /books/{id}/rating`
Set star rating (1–5).

```json
{ "rating": 5 }
```

---

### `POST /books/{id}/cover`
Upload or replace cover image (`multipart/form-data`, field: `file`).

- Max size: **5 MB**
- Allowed types: `.jpg`, `.jpeg`, `.png`
- Returns: `{ "url": "/uploads/covers/filename.jpg" }`

---

### `POST /books/{id}/recommend`
Request that a private book be moved to the public library.

- `200 OK` — Moderation request created (status: `Pending`).
- `400 Bad Request` — Already pending, already approved, or not owner.

---

### `DELETE /books/{id}`
Permanently delete a book from the collection.

- `204 No Content` — Deleted.
- `403 Forbidden` — Not owner (unless Admin).

---

### `GET /books/export`
Export the user's full collection as a CSV file.

- `200 OK` — Returns `athenaeum_export_YYYYMMDD.csv`

---

## 3. Dashboard (`/api/v1/dashboard`)

### `GET /dashboard/stats`
Personal reading statistics for the authenticated user.

**Response `200 OK`:**
```json
{
  "totalBooks": 12,
  "readCount": 5,
  "readingCount": 2,
  "wantToReadCount": 5,
  "totalPagesRead": 1450,
  "genreDistribution": { "Horror": 4, "Fiction": 3 },
  "averageRating": 4.2
}
```

---

## 4. Admin (`/api/v1/admin`)

> All admin endpoints require `[Authorize(Roles = "Admin")]`.

### `GET /admin/stats`
System-wide metrics.

**Response `200 OK`:**
```json
{
  "totalUsers": 2,
  "totalBooks": 15,
  "pendingRecommendations": 1
}
```

---

### `GET /admin/users`
Paginated list of all registered users.

**Query Parameters:** `page`, `pageSize`

**Response includes:** `id`, `email`, `role`, `isActive`, `lastActiveAt`, `createdAt`, `totalUploads`

---

### `PUT /admin/users/{id}/status`
Toggle user active/inactive status.

**Request Body:** `true` (activate) or `false` (deactivate)

- `200 OK` — Status updated.
- `404 Not Found` — User not found.

---

### `DELETE /admin/users/{id}`
Permanently delete a user and all their books.

- `204 No Content` — Deleted.

---

### `GET /admin/library`
Paginated list of all books in the system (all users, all visibility).

**Query Parameters:** `page`, `pageSize`

**Response includes:** `id`, `title`, `author`, `isbn`, `genre`, `visibility`, `moderationStatus`, `isApproved`, `ownerEmail`, `createdAt`

---

### `PATCH /admin/library/{id}`
Admin partial update on any book (bypass ownership check).

**Request Body:** Any JSON field subset.

---

### `DELETE /admin/library/{id}`
Admin force-delete any book in the system.

- `204 No Content`.

---

### `GET /admin/recommendations`
List all pending public-library recommendations.

**Response (array):**
```json
[
  {
    "id": "rec-uuid",
    "bookId": "book-uuid",
    "title": "It",
    "author": "Stephen King",
    "genre": "Horror",
    "coverImageUrl": "/uploads/covers/...",
    "uploadedBy": "suraj@gmail.com",
    "requestedAt": "2026-05-05T10:00:00Z"
  }
]
```

---

### `POST /admin/recommendations/{id}/approve`
Approve a recommendation — book becomes Public and `isApproved = true`.

- `200 OK` — `{ "message": "Book approved and added to Public Library." }`
- `404 Not Found` — Recommendation not found.

---

### `POST /admin/recommendations/{id}/reject`
Reject a recommendation — book stays Private with `ModerationStatus = Rejected`.

- `200 OK` — `{ "message": "Book rejected." }`

---

## 5. File Serving

Uploaded files are served as static assets:

| Path | Content |
|------|---------|
| `/uploads/covers/{filename}` | Book cover images |
| `/uploads/books/{filename}` | PDF manuscripts |

---

## 6. Enums Reference

### ReadingStatus
| Value | Int |
|-------|-----|
| `WantToRead` | 0 |
| `Reading` | 1 |
| `Read` | 2 |
| `Dropped` | 3 |

### BookVisibility
| Value | Int |
|-------|-----|
| `Private` | 0 |
| `Public` | 1 |

### ModerationStatus
| Value | Int |
|-------|-----|
| `None` | 0 |
| `Pending` | 1 |
| `Approved` | 2 |
| `Rejected` | 3 |

---

## 7. Standard HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200 OK` | Request succeeded |
| `201 Created` | Resource created |
| `204 No Content` | Success, no response body |
| `400 Bad Request` | Validation or business logic error |
| `401 Unauthorized` | Not authenticated |
| `403 Forbidden` | Authenticated but unauthorized (wrong role or not owner) |
| `404 Not Found` | Resource not found |
| `409 Conflict` | Duplicate resource (e.g., same ISBN) |
| `429 Too Many Requests` | Rate limit exceeded |
| `500 Internal Server Error` | Unexpected server error |

---

*Athenaeum Archive API · v1.0 · Updated 2026-05-05*
