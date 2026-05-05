# Athenaeum Archive — End-to-End User Flow

## Complete Flow: Login → All Paths

```mermaid
flowchart TD
    START([🌐 User Opens App]) --> AUTH_CHECK{Authenticated?}

    %% ─────────────── AUTH ───────────────
    AUTH_CHECK -- No --> LOGIN[/📋 Login Page/]
    AUTH_CHECK -- Yes --> ROLE_CHECK

    LOGIN --> LOGIN_SUBMIT[Enter Email + Password]
    LOGIN_SUBMIT --> LOGIN_VALID{Valid?}
    LOGIN_VALID -- No --> LOGIN_ERR[❌ Error Toast]
    LOGIN_ERR --> LOGIN
    LOGIN_VALID -- Yes --> SET_COOKIE[🍪 Set JWT HttpOnly Cookie]
    SET_COOKIE --> ROLE_CHECK{Role?}

    LOGIN --> GOTO_REGISTER[New here? Register]
    GOTO_REGISTER --> REGISTER[/📝 Register Page/]
    REGISTER --> REG_SUBMIT[Enter Email + Password]
    REG_SUBMIT --> REG_VALID{Valid?}
    REG_VALID -- No --> REG_ERR[❌ Field Errors]
    REG_ERR --> REGISTER
    REG_VALID -- Yes --> AUTO_LOGIN[Auto Login → User Role]
    AUTO_LOGIN --> SET_COOKIE

    %% ─────────────── ROLE SPLIT ───────────────
    ROLE_CHECK -- Admin --> ADMIN_DASH
    ROLE_CHECK -- User --> USER_DASH

    %% ══════════════════════════════════════
    %%            USER FLOW
    %% ══════════════════════════════════════
    USER_DASH([🏠 User Dashboard])
    USER_DASH --> UD_STATS[📊 View Reading Stats\nCurrently Reading · Want to Read · Finished]
    USER_DASH --> UD_NAV{Navigate To}

    UD_NAV --> MY_COLL[/📚 My Collection/]
    UD_NAV --> PUB_LIB[/🌍 Public Library/]
    UD_NAV --> ADD_BOOK_BTN[➕ Add New Book]

    %% ── My Collection ──
    MY_COLL --> MC_BROWSE[Browse Private Books]
    MC_BROWSE --> MC_FILTER[Search / Filter by Genre]
    MC_FILTER --> MC_BOOK[/📖 Book Detail Page/]

    MC_BOOK --> MC_OWNER_CHECK{Am I the Owner?}
    MC_OWNER_CHECK -- Yes --> MC_ACTIONS{Actions}
    MC_OWNER_CHECK -- No --> MC_VIEW[👁️ View Only]

    MC_ACTIONS --> MC_EDIT[✏️ Edit Book Details]
    MC_EDIT --> MC_SAVE[Save Changes]
    MC_SAVE --> MC_BOOK

    MC_ACTIONS --> MC_STATUS[🔄 Change Reading Status]
    MC_STATUS --> MC_STATUS_OPT{Status}
    MC_STATUS_OPT -- Reading --> MC_PAGE[📄 Update Current Page]
    MC_STATUS_OPT -- Finished --> MC_RATE[⭐ Add Rating & Review]
    MC_STATUS_OPT -- Dropped --> MC_DROP[🚫 Mark Dropped]
    MC_PAGE --> MC_BOOK
    MC_RATE --> MC_BOOK
    MC_DROP --> MC_BOOK

    MC_ACTIONS --> MC_REQ_PUB{Request Public?}
    MC_REQ_PUB -- Already Pending --> MC_LOCKED[🔒 Button Disabled]
    MC_REQ_PUB -- Not Requested --> MC_SUBMIT_REQ[📩 Submit to Admin]
    MC_SUBMIT_REQ --> MC_PENDING[Status = Pending\nAwaiting Admin Review]
    MC_PENDING --> ADMIN_REVIEW_WAIT([⏳ Waiting for Admin...])

    MC_ACTIONS --> MC_DELETE[🗑️ Delete Book]
    MC_DELETE --> MC_CONFIRM{Confirm?}
    MC_CONFIRM -- Yes --> MC_DELETED[Book Removed]
    MC_CONFIRM -- No --> MC_BOOK

    %% ── Public Library ──
    PUB_LIB --> PL_BROWSE[Browse All Approved Public Books]
    PL_BROWSE --> PL_FILTER[Search / Filter by Genre]
    PL_FILTER --> PL_BOOK[/📖 Book Detail Page/]
    PL_BOOK --> PL_OWNER{Am I Owner?}
    PL_OWNER -- Yes --> MC_ACTIONS
    PL_OWNER -- No --> PL_VIEW[👁️ Read-Only View]

    %% ── Add Book ──
    ADD_BOOK_BTN --> ADD_FORM[/📝 Add Book Form/]
    ADD_FORM --> ADD_FIELDS[Title · Author · Genre · Year · Pages]
    ADD_FIELDS --> ADD_COVER[📷 Upload Cover Image - Optional]
    ADD_COVER --> ADD_SUBMIT{Submit}
    ADD_SUBMIT -- Fail --> ADD_ERR[❌ Validation Errors]
    ADD_ERR --> ADD_FORM
    ADD_SUBMIT -- Success --> ADD_SAVED[Book Saved as Private\nStatus: Want to Read]
    ADD_SAVED --> MY_COLL

    %% ── Logout ──
    USER_DASH --> USER_LOGOUT[🚪 Logout]
    USER_LOGOUT --> CLEAR_COOKIE[Clear JWT Cookie]
    CLEAR_COOKIE --> LOGIN

    %% ══════════════════════════════════════
    %%            ADMIN FLOW
    %% ══════════════════════════════════════
    ADMIN_DASH([🛡️ Admin Dashboard])
    ADMIN_DASH --> ADMIN_NAV{Select Tab}

    ADMIN_NAV --> TAB_METRICS[📊 Metrics Tab]
    ADMIN_NAV --> TAB_USERS[👥 Users Tab]
    ADMIN_NAV --> TAB_LIBRARY[📚 Library Tab]
    ADMIN_NAV --> TAB_MOD[🛡️ Moderation Tab]

    %% ── Metrics ──
    TAB_METRICS --> MET_SHOW[Total Users · Total Books · Public Books\nActive Users · Pending Requests]

    %% ── Users ──
    TAB_USERS --> USR_SEARCH[Search by Email]
    USR_SEARCH --> USR_SELECT[Select User]
    USR_SELECT --> USR_ACT{Action}
    USR_ACT --> USR_TOGGLE[🔘 Toggle Active / Inactive]
    USR_ACT --> USR_DELETE[🗑️ Delete User + All Books]

    %% ── Library ──
    TAB_LIBRARY --> LIB_BROWSE[Browse All Books in System]
    LIB_BROWSE --> LIB_SEARCH[Search by Title / Author]
    LIB_SEARCH --> LIB_SELECT[Select Book]
    LIB_SELECT --> LIB_ACT{Action}
    LIB_ACT --> LIB_TOGGLE[🔄 Toggle Public / Private]
    LIB_ACT --> LIB_DELETE[🗑️ Delete Book]

    %% ── Moderation ──
    TAB_MOD --> MOD_QUEUE[View Pending Queue]
    MOD_QUEUE --> ADMIN_REVIEW_WAIT
    ADMIN_REVIEW_WAIT --> MOD_REVIEW[Review Book Request]
    MOD_REVIEW --> MOD_DEC{Decision}
    MOD_DEC -- ✅ Approve --> MOD_APPROVE[Book → Public\nIsApproved = true\nVisible in Library]
    MOD_DEC -- ❌ Reject --> MOD_REJECT[Book stays Private\nModerationStatus = Rejected]

    %% ── Admin Add Book ──
    ADMIN_DASH --> ADMIN_ADD[➕ Add Book]
    ADMIN_ADD --> ADD_FORM
    ADD_FORM --> ADMIN_ROLE_SAVE{Role = Admin}
    ADMIN_ROLE_SAVE --> ADMIN_BOOK_PUB[Book Saved as Public\nAuto-Approved\nVisible Immediately]
    ADMIN_BOOK_PUB --> TAB_LIBRARY

    %% ── Admin Logout ──
    ADMIN_DASH --> ADMIN_LOGOUT[🚪 Logout]
    ADMIN_LOGOUT --> CLEAR_COOKIE

    %% ─────────────── STYLING ───────────────
    style START fill:#2d2d2d,color:#fff,stroke:none
    style USER_DASH fill:#4a3728,color:#f5f0e8,stroke:#8b7355
    style ADMIN_DASH fill:#1a2744,color:#e8f0ff,stroke:#4a6fa5
    style LOGIN fill:#3d3d3d,color:#fff,stroke:#666
    style REGISTER fill:#3d3d3d,color:#fff,stroke:#666
    style MOD_APPROVE fill:#1a4a1a,color:#90ee90,stroke:#2d7a2d
    style MOD_REJECT fill:#4a1a1a,color:#ffb0b0,stroke:#7a2d2d
    style MC_DELETED fill:#4a1a1a,color:#ffb0b0,stroke:#7a2d2d
    style ADD_SAVED fill:#1a4a1a,color:#90ee90,stroke:#2d7a2d
    style ADMIN_BOOK_PUB fill:#1a4a1a,color:#90ee90,stroke:#2d7a2d
    style MC_PENDING fill:#4a3a1a,color:#ffd080,stroke:#8b6914
    style ADMIN_REVIEW_WAIT fill:#4a3a1a,color:#ffd080,stroke:#8b6914
```

---

## Quick Reference: Key Decision Points

| Decision | Condition | Outcome |
|----------|-----------|---------|
| `AUTH_CHECK` | JWT Cookie present & valid | Redirect to role dashboard |
| `ROLE_CHECK` | `role === "Admin"` | Admin Dashboard |
| `ROLE_CHECK` | `role === "User"` | User Dashboard |
| `MC_OWNER_CHECK` | `book.userId === currentUser.id` | Edit/Delete/Status controls visible |
| `ADD_SUBMIT` (User) | Role = User | Book saved **Private** |
| `ADD_SUBMIT` (Admin) | Role = Admin | Book saved **Public + Auto-Approved** |
| `MC_REQ_PUB` | `moderationStatus === "Pending"` | Button locked |
| `MOD_DEC → Approve` | Admin clicks Approve | `isApproved=true`, `visibility=Public` |
| `MOD_DEC → Reject` | Admin clicks Reject | `visibility=Private`, `status=Rejected` |

---

*Athenaeum Archive · E2E Flow · v1.0*
