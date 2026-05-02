# PRD: Book Collection Tracker (v1.0)

## Problem Statement
Book enthusiasts often struggle to organize their physical or digital libraries, track their reading progress across multiple books, and maintain a history of their thoughts and ratings. Existing solutions are either too complex, lack privacy, or do not offer a seamless way to visualize reading statistics and goals.

The **Book Collection Tracker** aims to provide a personal, streamlined, and visually appealing dashboard for users to manage their literary journey.

## Goals & Success Metrics
### Business & User Goals
- **Personal Organization**: Provide a single source of truth for all books owned or read.
- **Engagement**: Encourage reading through progress tracking and yearly goals.
- **Data Ownership**: Ensure users can export their data (CSV) and run the app locally (Docker).

### Success Metrics
- **Completion Rate**: 100% of the defined 39 KPIs passed.
- **User Satisfaction**: Seamless UX for book entry (under 30 seconds per book).
- **Reliability**: 0% data loss across Docker container restarts.

## Target Audience
- **Avid Readers**: People who read multiple books and want to track progress.
- **Book Collectors**: Individuals who want to catalog their library with metadata and covers.
- **Data Enthusiasts**: Users who enjoy visualizing their reading habits through charts and stats.

## Scope
### In Scope
- User Authentication (Registration, Login, Logout, Password Hashing).
- CRUD operations for Books (Title, Author, ISBN, Genre, Year, Cover Image).
- Reading Status Management (Want to Read, Currently Reading, Read).
- Progress Tracking (Page/Total Pages) for active books.
- Reviews and 5-star Rating system.
- Advanced Search and Multi-criteria Filtering.
- Dashboard with statistics and Genre/Rating charts.
- Yearly Reading Goals.
- CSV Export.
- Dockerized environment for easy deployment.

### Out of Scope
- Social features (Friends, shared lists, public profiles).
- Integration with external APIs (Goodreads, Google Books) for auto-fetching metadata (v1).
- Mobile Native App (Responsive web only).
- Multi-language support (English only for MVP).

## User Stories
- **As a New User**, I want to **create an account**, so that **my collection is saved securely**.
- **As a Reader**, I want to **add a book with its cover image**, so that **I can visually browse my library**.
- **As a Student**, I want to **update my current page number**, so that **I can see how much of the book I have left**.
- **As a Critic**, I want to **write a review and rate a book**, so that **I can remember my thoughts on it later**.
- **As a Librarian**, I want to **search by ISBN or author**, so that **I can quickly find a specific volume**.
- **As a Goal-Oriented Reader**, I want to **set a target for books per year**, so that **I stay motivated to read more**.

## Functional Requirements
- **FR-01 (Auth)**: System must hash passwords using bcrypt before storage.
- **FR-02 (Book CRUD)**: System must require 'Title' and 'Author' for all book entries.
- **FR-03 (Images)**: System must support image uploads for book covers.
- **FR-04 (Status)**: System must allow switching between 'Want to Read', 'Currently Reading', and 'Read'.
- **FR-05 (Progress)**: System must calculate progress percentage based on 'Current Page' and 'Total Pages'.
- **FR-06 (Search)**: Search functionality must be case-insensitive and support partial matches.
- **FR-07 (Dashboard)**: Dashboard must update in real-time as books are added or statuses change.
- **FR-08 (Export)**: CSV export must include all book metadata and review notes.

## Non-Functional Requirements
- **Performance**: Page load times under 2 seconds; search results returned in <500ms.
- **Security**: Protected routes (cannot access collection without login); secure session management.
- **Reliability**: SQLite/PostgreSQL data persistence via Docker volumes.
- **Scalability**: Handle up to 5,000 books per user without performance degradation.

## UX / Flow Notes
1. **Landing Page**: Login/Register options.
2. **Dashboard**: High-level stats (Total, Read, Pages) and Yearly Goal progress bar.
3. **Collection View**: Grid of book cards with covers. Hovering shows quick actions (Edit, Delete).
4. **Add/Edit Modal**: Form for book details and file upload for cover.
5. **Book Detail Page**: Full metadata, reading progress slider, and review/rating section.

## Edge Cases
- **Duplicate ISBNs**: System should alert user if a book with the same ISBN already exists.
- **Large Image Uploads**: System should compress or reject images larger than 5MB.
- **Empty States**: "Your collection is empty. Add your first book!" message with a CTA.
- **Invalid Progress**: Prevent 'Current Page' from exceeding 'Total Pages'.

## MVP Definition
The MVP is reached when a user can register, log in, add books with images, track reading progress on "Currently Reading" books, filter their list, and view their dashboard stats in a Docker-deployed environment.

## Open Questions
- Should we include a "Borrowed/Lent" status in v1?
- Do we need to support PDF/Epub metadata extraction? (Proposed: No, for MVP).
- Preference for Database? (Proposed: SQLite for simplicity or PostgreSQL for robust Docker usage).

## Proposed Tech Stack
- **Frontend**: React (Vite) + Vanilla CSS (Premium UI with Glassmorphism).
- **Backend**: Node.js (Express) + JWT for Auth.
- **Database**: PostgreSQL (relational structure for books/users).
- **Storage**: Local filesystem (Docker volume) or Cloudinary for book covers.
- **DevOps**: Docker & Docker Compose.

## Performance & Security KPIs
- **P-KPI 01**: Average API response time < 200ms.
- **S-KPI 01**: All sensitive endpoints require a valid JWT.
- **S-KPI 02**: All sensitive endpoints require a valid JWT. (Duplicate in draft, fixed).
- **S-KPI 03**: SQL Injection protection via ORM/Parameterization.
- **S-KPI 04**: XSS protection for reviews and notes.
