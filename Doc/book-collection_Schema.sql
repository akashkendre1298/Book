-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ==============================
-- ENUMS
-- ==============================

CREATE TYPE reading_status AS ENUM (
  'WANT_TO_READ',
  'CURRENTLY_READING',
  'READ'
);

CREATE TYPE genre_type AS ENUM (
  'FICTION',
  'NON_FICTION',
  'SCI_FI',
  'FANTASY',
  'BIOGRAPHY',
  'HISTORY',
  'MYSTERY',
  'THRILLER',
  'SELF_HELP',
  'OTHER'
);

-- ==============================
-- USERS TABLE
-- ==============================

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==============================
-- BOOKS TABLE
-- ==============================

CREATE TABLE books (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    user_id UUID NOT NULL
        REFERENCES users(id)
        ON DELETE CASCADE,

    title VARCHAR(255) NOT NULL,
    author VARCHAR(255) NOT NULL,
    isbn VARCHAR(20),

    genre genre_type,
    publication_year INTEGER,

    cover_image_url TEXT,

    total_pages INTEGER NOT NULL DEFAULT 0 CHECK (total_pages >= 0),
    current_page INTEGER NOT NULL DEFAULT 0 CHECK (current_page >= 0),

    status reading_status DEFAULT 'WANT_TO_READ',

    rating INTEGER CHECK (rating BETWEEN 1 AND 5),
    review_notes TEXT,

    finish_date DATE,

    -- Full-text search column
    search_vector tsvector,

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CONSTRAINT check_progress_limit CHECK (current_page <= total_pages),
    CONSTRAINT unique_user_isbn UNIQUE (user_id, isbn)
);

-- ==============================
-- READING GOALS (KPI SUPPORT)
-- ==============================

CREATE TABLE reading_goals (
    id SERIAL PRIMARY KEY,

    user_id UUID NOT NULL
        REFERENCES users(id)
        ON DELETE CASCADE,

    target_year INTEGER NOT NULL,
    target_count INTEGER NOT NULL CHECK (target_count > 0),

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(user_id, target_year)
);

-- ==============================
-- INDEXES (PERFORMANCE)
-- ==============================

-- Basic search indexes
CREATE INDEX idx_books_title ON books(title);
CREATE INDEX idx_books_author ON books(author);
CREATE INDEX idx_books_isbn ON books(isbn);

-- Filtering indexes
CREATE INDEX idx_books_user_status ON books(user_id, status);
CREATE INDEX idx_books_user_genre ON books(user_id, genre);
CREATE INDEX idx_books_user_rating ON books(user_id, rating);

-- Sorting
CREATE INDEX idx_books_created_at ON books(created_at DESC);

-- Full-text search index
CREATE INDEX idx_books_search_vector
ON books USING GIN(search_vector);

-- ==============================
-- TRIGGERS
-- ==============================

-- Auto-update updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_update_books_updated_at
BEFORE UPDATE ON books
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- ==============================
-- FULL-TEXT SEARCH TRIGGER
-- ==============================

CREATE OR REPLACE FUNCTION update_search_vector()
RETURNS TRIGGER AS $$
BEGIN
  NEW.search_vector :=
    to_tsvector('english',
      COALESCE(NEW.title, '') || ' ' ||
      COALESCE(NEW.author, '')
    );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_search_vector
BEFORE INSERT OR UPDATE ON books
FOR EACH ROW
EXECUTE FUNCTION update_search_vector();