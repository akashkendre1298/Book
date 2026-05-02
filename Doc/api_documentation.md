# API Documentation: Book Collection Tracker (v1.0)

This document outlines the RESTful API contract for the Book Collection Tracker application.

## Base URL
`http://localhost:3000/api/v1`

## Authentication
All endpoints except Registration and Login require a **Bearer JWT Token** in the `Authorization` header.

```http
Authorization: Bearer <token>
```

---

## 1. Authentication (Auth)

### POST /auth/register
Register a new user account.
- **Request Body**:
  ```json
  {
    "email": "user@example.com*",
    "password": "strongpassword123*"
  }
  ```
- **Responses**:
  - `201 Created`: User registered successfully.
  - `400 Bad Request`: Email already exists or validation failed.

### POST /auth/login
Authenticate user and return a token.
- **Request Body**:
  ```json
  {
    "email": "user@example.com*",
    "password": "strongpassword123*"
  }
  ```
- **Responses**:
  - `200 OK`: Returns JWT token and user profile.
    ```json
    {
      "token": "eyJhbG...",
      "user": { "id": "uuid", "email": "user@example.com" }
    }
    ```
  - `401 Unauthorized`: Invalid credentials.

### POST /auth/logout
Terminate current session (Server-side token invalidation if using a whitelist, or simply client-side removal).
- **Responses**:
  - `204 No Content`: Logged out.

---

## 2. Book Management (Books)

### GET /books
Retrieve collection with search, filter, and sort.
- **Query Parameters**:
  - `q`: Search query (title, author, ISBN).
  - `genre`: Filter by genre (e.g., `FICTION`).
  - `status`: Filter by status (`WANT_TO_READ`, `CURRENTLY_READING`, `READ`).
  - `min_rating`: Filter by rating (1-5).
  - `sort_by`: `title`, `author`, `rating`, `created_at`.
  - `order`: `asc` or `desc`.
  - `page`: Page number (default 1).
  - `limit`: Items per page (default 20).
- **Responses**:
  - `200 OK`: List of books with pagination metadata.

### POST /books
Add a new book to the collection.
- **Request Body (Multipart/Form-Data)**:
  - `title*`: String
  - `author*`: String
  - `isbn`: String
  - `genre`: `genre_type` enum
  - `publication_year`: Integer
  - `total_pages`: Integer (required if tracking progress)
  - `cover`: File (Image upload)
- **Responses**:
  - `201 Created`: Book added.

### GET /books/:id
Get detailed information for a specific book.
- **Responses**:
  - `200 OK`: Book details.
  - `404 Not Found`: Book doesn't exist.

### PATCH /books/:id
Update book details, reading progress, or rating.
- **Request Body**:
  ```json
  {
    "status": "CURRENTLY_READING",
    "current_page": 150,
    "rating": 5,
    "review_notes": "Masterpiece."
  }
  ```
- **Constraint**: `current_page` <= `total_pages`.
- **Responses**:
  - `200 OK`: Updated book.

### DELETE /books/:id
Remove a book from the collection.
- **Responses**:
  - `204 No Content`: Deleted.

### GET /books/export
Export collection as CSV.
- **Responses**:
  - `200 OK`: Binary CSV file download.

---

## 3. Dashboard & Goals

### GET /dashboard/stats
Retrieve summary statistics for the dashboard charts.
- **Response Shape**:
  ```json
  {
    "total_books": 120,
    "read_count": 45,
    "total_pages_read": 15400,
    "genre_distribution": { "FICTION": 20, "SCI_FI": 15 },
    "rating_distribution": { "5": 10, "4": 25 },
    "yearly_goal_progress": { "target": 50, "current": 12 }
  }
  ```

### GET /goals
Get reading goals for the user.
- **Responses**:
  - `200 OK`: List of goals.

### POST /goals
Set a new reading goal for a year.
- **Request Body**:
  ```json
  {
    "target_year": 2026*,
    "target_count": 50*
  }
  ```
- **Responses**:
  - `201 Created`: Goal saved.

---

## Error Handling Example

All error responses follow this structure:
```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "Current page cannot exceed total pages.",
    "details": [
      { "field": "current_page", "issue": "value_too_high" }
    ]
  }
}
```

## Status Codes Summary
- `200 OK`: Request succeeded.
- `201 Created`: Resource created.
- `204 No Content`: Request succeeded (no body).
- `400 Bad Request`: Validation or business logic error.
- `401 Unauthorized`: Authentication required or failed.
- `403 Forbidden`: Authenticated but lacks permission.
- `404 Not Found`: Resource not found.
- `500 Internal Server Error`: Unexpected server error.
