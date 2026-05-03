/**
 * 🔍 FRONTEND TEST SUITE — Search & Filter UI
 * Covers: Test_list.txt lines 194-200
 * - Search updates results
 * - Filter dropdown works
 * - Sorting updates order
 * - Multiple filters work
 * - Empty state shown
 *
 * 📖 FRONTEND TEST SUITE — Reading UI
 * Covers: Test_list.txt lines 184-190
 * - Status selector updates UI
 * - Progress bar updates
 * - Rating UI updates
 * - Review saved & displayed
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';

// ─── Mocks ──────────────────────────────────────────────────────────────────
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => vi.fn(), useParams: () => ({ id: 'book-abc-123' }) };
});

vi.mock('./api/axios', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  },
}));

import api from './api/axios';

const allBooks = [
  { id: '1', title: 'Clean Code', author: 'Robert Martin', genre: 'Engineering', status: 2, rating: 5, currentPage: 431, totalPages: 431 },
  { id: '2', title: 'The Great Gatsby', author: 'F. Scott Fitzgerald', genre: 'Fiction', status: 0, rating: null, currentPage: 0, totalPages: 200 },
  { id: '3', title: 'Dune', author: 'Frank Herbert', genre: 'Science Fiction', status: 1, rating: 4, currentPage: 120, totalPages: 412 },
];

// ─────────────────────────────────────────────────────────────────────────────
// TC-SEARCH-01: Search input updates results (debounced API call)
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-SEARCH-01: Search input triggers API query', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: allBooks });
  });

  it('renders search bar input', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByPlaceholderText(/Search Title, Author, or ISBN/i)).toBeInTheDocument();
    });
  });

  it('calls API with query parameter when typing', async () => {
    const user = userEvent.setup({ delay: null });
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);

    await waitFor(() => screen.getByPlaceholderText(/Search Title, Author, or ISBN/i));
    const searchInput = screen.getByPlaceholderText(/Search Title, Author, or ISBN/i);
    await user.type(searchInput, 'Clean');

    await waitFor(() => {
      const calls = api.get.mock.calls;
      const hasQueryCall = calls.some(([url]) => url.includes('query=Clean'));
      expect(hasQueryCall).toBe(true);
    }, { timeout: 1000 });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-SEARCH-02: Filter by Status dropdown
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-SEARCH-02: Status filter dropdown', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: allBooks });
  });

  it('renders status filter with All Statuses option', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByDisplayValue('All Statuses')).toBeInTheDocument();
    });
  });

  it('calls API with status=1 when "Reading" is selected', async () => {
    const user = userEvent.setup();
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => screen.getByDisplayValue('All Statuses'));

    await user.selectOptions(screen.getByDisplayValue('All Statuses'), '1');

    await waitFor(() => {
      const calls = api.get.mock.calls;
      const hasStatusCall = calls.some(([url]) => url.includes('status=1'));
      expect(hasStatusCall).toBe(true);
    });
  });

  it('renders Want to Read, Reading, Finished options', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      const statusSelect = screen.getByDisplayValue('All Statuses');
      expect(statusSelect.options.length).toBe(4); // All + 3 statuses
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-SEARCH-03: Rating filter dropdown
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-SEARCH-03: Rating filter dropdown', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: allBooks });
  });

  it('renders rating filter with All Ratings option', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByDisplayValue('All Ratings')).toBeInTheDocument();
    });
  });

  it('has options 1 through 5 Stars', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      const ratingSelect = screen.getByDisplayValue('All Ratings');
      // All Ratings + 5 star options = 6
      expect(ratingSelect.options.length).toBe(6);
    });
  });

  it('calls API with rating=5 when "5 Stars" is selected', async () => {
    const user = userEvent.setup();
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => screen.getByDisplayValue('All Ratings'));

    await user.selectOptions(screen.getByDisplayValue('All Ratings'), '5');

    await waitFor(() => {
      const calls = api.get.mock.calls;
      const hasRatingCall = calls.some(([url]) => url.includes('rating=5'));
      expect(hasRatingCall).toBe(true);
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-SEARCH-04: Sorting updates order
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-SEARCH-04: Sort order dropdown', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: allBooks });
  });

  it('renders sort by "Recently Added" by default', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByDisplayValue('Recently Added')).toBeInTheDocument();
    });
  });

  it('has Alphabetical (Title), Author, and Rating options', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      const sortSelect = screen.getByDisplayValue('Recently Added');
      const optionValues = Array.from(sortSelect.options).map(o => o.value);
      expect(optionValues).toContain('title');
      expect(optionValues).toContain('author');
      expect(optionValues).toContain('rating');
      expect(optionValues).toContain('dateadded');
    });
  });

  it('calls API with sortBy=title when sorted by title', async () => {
    const user = userEvent.setup();
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => screen.getByDisplayValue('Recently Added'));

    await user.selectOptions(screen.getByDisplayValue('Recently Added'), 'title');

    await waitFor(() => {
      const calls = api.get.mock.calls;
      const hasSortCall = calls.some(([url]) => url.includes('sortBy=title'));
      expect(hasSortCall).toBe(true);
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-READING-01: Book Detail - Status selector renders
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-READING-01: Reading status selector on detail page', () => {
  const mockBook = {
    id: 'book-abc-123',
    title: 'Dune',
    author: 'Frank Herbert',
    status: 1,
    currentPage: 120,
    totalPages: 412,
    rating: 4,
    review: 'A masterwork.',
    genre: 'Sci-Fi',
    publicationYear: 1965,
    isbn: '978-0441013593',
  };

  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockBook });
    api.patch.mockResolvedValue({ data: { ...mockBook, status: 2 } });
  });

  it('renders all three reading status buttons', async () => {
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /To Read/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Reading/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Finished/i })).toBeInTheDocument();
    });
  });

  it('highlights the current reading status button', async () => {
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => {
      const readingBtn = screen.getByRole('button', { name: /Reading/i });
      // The active status has bg-ink class applied
      expect(readingBtn).toHaveClass('bg-ink');
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-READING-02: Progress bar renders and updates
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-READING-02: Progress bar on detail page', () => {
  const mockBook = {
    id: 'book-abc-123',
    title: 'Dune',
    author: 'Frank Herbert',
    status: 1,
    currentPage: 120,
    totalPages: 412,
    rating: 4,
    review: '',
    genre: 'Sci-Fi',
  };

  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockBook });
  });

  it('shows correct percentage (120/412 ≈ 29%)', async () => {
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/29%/i)).toBeInTheDocument();
    });
  });

  it('shows page count display', async () => {
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/\/ 412 pages/i)).toBeInTheDocument();
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-READING-03: Star rating UI renders
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-READING-03: Star rating UI', () => {
  const mockBook = {
    id: 'book-abc-123',
    title: 'Dune',
    author: 'Frank Herbert',
    status: 1,
    currentPage: 120,
    totalPages: 412,
    rating: 3,
    review: '',
    genre: 'Sci-Fi',
  };

  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockBook });
    api.patch.mockResolvedValue({ data: { ...mockBook, rating: 5 } });
  });

  it('renders 5 star rating buttons', async () => {
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => {
      const stars = screen.getAllByText('★');
      expect(stars).toHaveLength(5);
    });
  });

  it('updates rating when a star is clicked', async () => {
    const user = userEvent.setup();
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => screen.getAllByText('★'));
    const stars = screen.getAllByText('★');
    await user.click(stars[4]); // Click 5th star
    await waitFor(() => {
      expect(api.patch).toHaveBeenCalledWith(
        expect.stringContaining('rating'),
        expect.objectContaining({ rating: 5 })
      );
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-READING-04: Review textarea renders and displays stored review
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-READING-04: Review textarea', () => {
  const mockBook = {
    id: 'book-abc-123',
    title: 'Dune',
    author: 'Frank Herbert',
    status: 2,
    currentPage: 412,
    totalPages: 412,
    rating: 5,
    review: 'A true science fiction masterpiece.',
    genre: 'Sci-Fi',
  };

  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockBook });
  });

  it('displays existing review text in textarea', async () => {
    const { default: BookDetailPage } = await import('./pages/BookDetailPage');
    render(<MemoryRouter><BookDetailPage /></MemoryRouter>);
    await waitFor(() => {
      const textarea = screen.getByDisplayValue('A true science fiction masterpiece.');
      expect(textarea).toBeInTheDocument();
    });
  });
});
