Build a Book Collection Tracker

Here are the **Key Performance Indicators (KPIs)** to evaluate whether the Book Collection Tracker is "Done" and meets the scope requirements.

---

## Completion KPIs: Book Collection Tracker

These KPIs are grouped by functional area and are designed to be **binary** (Pass/Fail). All must be **Pass** for the assignment to be considered complete.

---

### 🔐 User Management & Authentication

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 1 | User can register with email and password | Create a new account via UI; confirm success redirect/message. |
| 2 | Registered user can log in successfully | Log in with created credentials; session established. |
| 3 | Invalid login credentials are rejected with appropriate error | Attempt login with wrong password; see error message. |
| 4 | Session persists after browser restart | Close and reopen browser; user remains logged in. |
| 5 | User can log out and session is terminated | Click logout; attempt to access collection; redirected to login. |
| 6 | Password is stored hashed (not plaintext) | Inspect database; password column contains bcrypt hash or similar. |

---

### 📚 Book Management

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 7 | User can add a book with title, author, ISBN, genre, publication year | Submit book form; book appears in collection. |
| 8 | Book title and author are required fields | Submit missing title; validation error shown. |
| 9 | User can upload book cover image | Select image; thumbnail appears in listing. |
| 10 | User can edit book details | Modify book fields, save; changes reflected. |
| 11 | User can delete a book from collection | Delete book; removed from collection. |
| 12 | Books displayed in grid or list view | View collection; visual layout as expected. |
| 13 | Each book card shows cover, title, author, rating | View book card; all fields visible. |

---

### 📖 Reading Status & Progress

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 14 | User can set reading status (Want to Read, Currently Reading, Read) | Change status; book moves to appropriate shelf. |
| 15 | For "Currently Reading", user can track progress (page number/total pages) | Enter current page; progress bar updates. |
| 16 | User can mark book as finished with completion date | Mark as read; date recorded. |
| 17 | User can rate book (1‑5 stars) | Select rating; stars saved. |
| 18 | User can write review/notes for book | Enter review; saved with book. |

---

### 🔍 Search & Filtering

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 19 | User can search books by title, author, ISBN | Type search term; matching books appear. |
| 20 | User can filter by genre, reading status, rating | Apply filters; only matching books shown. |
| 21 | User can sort by title, author, rating, date added | Select sort option; order changes accordingly. |
| 22 | Advanced search with multiple criteria (AND/OR) | Combine filters; results correctly filtered. |

---

### 📊 Statistics & Insights

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 23 | Dashboard shows total books, read count, pages read | View dashboard; statistics calculated correctly. |
| 24 | Charts show distribution by genre, rating | View charts; data visualized. |
| 25 | Reading goal setting (e.g., books per year) | Set goal; progress tracked. |
| 26 | Export collection as CSV | Click export; CSV file downloads. |

---

### 📱 Responsive Design

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 27 | Book grid adapts to mobile (2‑3 columns) | Resize to mobile; layout adjusts. |
| 28 | Book details page readable on small screens | View book on mobile; content fits. |
| 29 | Touch‑friendly buttons for status changes | Tap status buttons on mobile; they work. |

---

### 🐳 Docker & Deployment

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 30 | `docker-compose up` builds and starts the application without errors | Run command; observe successful container startup. |
| 31 | Application is accessible at `http://localhost:3000` | Open browser to URL; login page loads. |
| 32 | Database file persists in a Docker volume/mount | Stop and restart container; previously added books remain. |
| 33 | All application features work inside the Docker container | Perform a smoke test (add book, change status, search). |

---

### 🧪 Testing & Documentation

| # | KPI | Verification Method |
| :-- | :-- | :-- |
| 34 | Unit test exists for book validation | Check test suite; run tests; verify pass. |
| 35 | Unit test exists for filtering logic | Check test suite; run tests; verify pass. |
| 36 | Unit test exists for statistics calculation | Check test suite; run tests; verify pass. |
| 37 | All unit tests pass | Run test command; observe all green. |
| 38 | README.md contains clear Docker setup instructions | Read README; follow steps; app starts without guesswork. |
| 39 | API endpoints are documented (in README or separate file) | Locate documentation; contains list of endpoints and example payloads. |

---