# Athenaeum Archive — User Flow Diagram

> **Product Perspective:** This diagram captures every user journey through the system — from initial landing to full book lifecycle management — ensuring no critical path is missed and all role boundaries are respected.  
> **Frontend Perspective:** These flows map directly to route guards, conditional renders, and state transitions in the React client.

---

## 1. Authentication Flow

```mermaid
flowchart TD
    A([🌐 Visit App]) --> B{Authenticated?}
    B -- No --> C[/Login Page/]
    B -- Yes --> D{Role?}

    C --> E[Enter Email + Password]
    E --> F{Valid Credentials?}
    F -- No --> G[❌ Show Error Toast]
    G --> C
    F -- Yes --> H[Set HttpOnly JWT Cookie]
    H --> D

    C --> I[Don't have account?]
    I --> J[/Register Page/]
    J --> K[Enter Email + Password]
    K --> L{Validation Pass?}
    L -- No --> M[❌ Show Field Errors]
    M --> J
    L -- Yes --> N[Create Account → Auto Login]
    N --> D

    D -- Admin --> O[(Admin Dashboard)]
    D -- User --> P[(User Dashboard)]
```

---

## 2. User Dashboard & Reading Tracker Flow

```mermaid
flowchart TD
    P([User Dashboard]) --> Q[View Reading Stats]
    Q --> R[Currently Reading]
    Q --> S[Want to Read]
    Q --> T[Finished Books]

    P --> U[/Public Library/]
    U --> V[Browse All Public Books]
    V --> W[Search & Filter by Genre]
    W --> X[/Book Detail Page/]
    X --> Y{Is Owner?}

    Y -- Yes --> Z[Edit / Delete Book]
    Y -- No --> AA[View Only - Read Mode]

    P --> AB[/My Collection/]
    AB --> AC[View Private Books]
    AC --> AD[/Book Detail Page/]
    AD --> AE[Update Reading Status]
    AE --> AF{Status}
    AF -- Reading --> AG[Update Current Page]
    AF -- Finished --> AH[Add Rating & Review]
    AF -- Dropped --> AI[Mark as Dropped]

    AD --> AJ[Request to Make Public]
    AJ --> AK{Already Pending?}
    AK -- Yes --> AL[🔒 Button Disabled]
    AK -- No --> AM[📩 Submit Moderation Request]
    AM --> AN[Status = Pending]
```

---

## 3. Add Book Flow (User & Admin)

```mermaid
flowchart TD
    BA([+ Register New Volume]) --> BB[/Add Book Form/]
    BB --> BC[Fill: Title, Author, Genre, Year, Pages]
    BC --> BD[Upload Cover Image - Optional]
    BD --> BE[Upload PDF Manuscript - Optional]
    BE --> BF{Submit}
    BF -- Validation Fail --> BG[❌ Show Inline Errors]
    BG --> BB
    BF -- Success --> BH{User Role?}
    BH -- User --> BI[Book Saved as Private]
    BH -- Admin --> BJ[Book Saved as Public + Auto-Approved]
    BI --> BK[/My Collection/]
    BJ --> BL[/Public Library/]
```

---

## 4. Admin Dashboard Flow

```mermaid
flowchart TD
    O([Admin Dashboard]) --> OA[📊 Metrics Tab]
    O --> OB[👥 Users Tab]
    O --> OC[📚 Library Tab]
    O --> OD[🛡️ Moderation Tab]

    OA --> OA1[View Total Users, Books, Public Books]

    OB --> OB1[Search Users by Email]
    OB1 --> OB2{Select User}
    OB2 --> OB3[Toggle Active/Inactive]
    OB2 --> OB4[Delete User]

    OC --> OC1[Browse All Books in System]
    OC1 --> OC2[Search by Title / Author]
    OC2 --> OC3{Book Action}
    OC3 --> OC4[Toggle Visibility Public/Private]
    OC3 --> OC5[Delete Book]

    OD --> OD1[View Pending Moderation Queue]
    OD1 --> OD2{Review Request}
    OD2 -- ✅ Approve --> OD3[Book → Public + IsApproved = true]
    OD2 -- ❌ Reject --> OD4[Book stays Private + Status = Rejected]
```

---

## 5. Book Visibility & Moderation State Machine

```mermaid
stateDiagram-v2
    [*] --> Private : Book Created by User
    [*] --> Public : Book Created by Admin

    Private --> Pending : User Requests Public Access
    Pending --> Public : Admin Approves
    Pending --> Private : Admin Rejects

    Public --> Private : Admin Revokes Visibility
    Private --> [*] : Book Deleted
    Public --> [*] : Book Deleted
```

---

## 6. End-to-End Happy Path (Normal User)

```mermaid
sequenceDiagram
    actor U as User
    participant FE as React Client
    participant API as .NET API
    participant DB as PostgreSQL

    U->>FE: Open App
    FE->>API: GET /auth/me (check cookie)
    API-->>FE: 401 Unauthorized
    FE-->>U: Show Login Page

    U->>FE: Submit Login
    FE->>API: POST /auth/login
    API->>DB: Verify credentials (BCrypt)
    DB-->>API: User record
    API-->>FE: Set HttpOnly JWT Cookie
    FE-->>U: Redirect to Dashboard

    U->>FE: Add a New Book
    FE->>API: POST /books (+ cover image)
    API->>DB: Save Book (Private, WantToRead)
    DB-->>API: Book saved
    API-->>FE: 201 Created
    FE-->>U: Show in My Collection

    U->>FE: Change Status to Reading
    FE->>API: PATCH /books/{id}/status
    API->>DB: Update Status + CurrentPage
    DB-->>API: Updated
    API-->>FE: 200 OK
    FE-->>U: Dashboard updates live

    U->>FE: Request Public Access
    FE->>API: POST /books/{id}/recommend
    API->>DB: Set ModerationStatus = Pending
    DB-->>API: Updated
    API-->>FE: 200 OK
    FE-->>U: Button shows "Pending Approval"
```

---

## 7. End-to-End Happy Path (Admin Moderation)

```mermaid
sequenceDiagram
    actor A as Admin
    participant FE as React Client
    participant API as .NET API
    participant DB as PostgreSQL

    A->>FE: Login as Admin
    FE->>API: POST /auth/login
    API-->>FE: JWT Cookie (Role=Admin)
    FE-->>A: Redirect to Admin Dashboard

    A->>FE: Navigate to Moderation Tab
    FE->>API: GET /admin/recommendations
    API->>DB: Fetch Pending Books
    DB-->>API: List of books
    API-->>FE: Moderation Queue
    FE-->>A: Show Pending Cards

    A->>FE: Click "Approve"
    FE->>API: POST /admin/recommendations/{id}/approve
    API->>DB: Set IsApproved=true, Visibility=Public
    DB-->>API: Updated
    API-->>FE: 200 OK
    FE-->>A: Card removed from queue
    Note over FE: Book now visible in Public Library
```

---

*Generated by Lead Frontend + PM personas — Athenaeum Archive v1.0*
