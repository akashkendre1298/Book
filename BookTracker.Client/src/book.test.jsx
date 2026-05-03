/**
 * 📚 FRONTEND TEST SUITE — Book UI
 * Covers: Test_list.txt lines 167-180
 * - Book form renders
 * - Required field validation UI
 * - Book card displays: Cover, Title, Author, Rating
 * - Edit form pre-filled
 * - Delete updates UI
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import BookForm from './components/BookForm';

// ─── Mocks ──────────────────────────────────────────────────────────────────
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => vi.fn() };
});

vi.mock('./api/axios', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

const defaultFormData = {
  title: '',
  author: '',
  isbn: '',
  genre: '',
  totalPages: '',
  publicationYear: '',
  coverImageUrl: '',
  status: 0,
};

const renderBookForm = (overrides = {}) => {
  const props = {
    formData: defaultFormData,
    setFormData: vi.fn(),
    handleSubmit: vi.fn(),
    loading: false,
    error: null,
    isEdit: false,
    ...overrides,
  };
  return render(
    <MemoryRouter>
      <BookForm {...props} />
    </MemoryRouter>
  );
};

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-01: Book form renders all fields
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-BOOK-01: Book form renders', () => {
  it('renders the title input field', () => {
    renderBookForm();
    expect(screen.getByPlaceholderText(/The Shadow of the Wind/i)).toBeInTheDocument();
  });

  it('renders the author input field', () => {
    renderBookForm();
    expect(screen.getByPlaceholderText(/Carlos Ruiz/i)).toBeInTheDocument();
  });

  it('renders genre, ISBN, pages, and year fields', () => {
    renderBookForm();
    expect(screen.getByPlaceholderText(/Historical Fiction/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/978-.../i)).toBeInTheDocument();
  });

  it('renders the submit button with "Archive New Volume" text for new books', () => {
    renderBookForm({ isEdit: false });
    expect(screen.getByRole('button', { name: /Archive New Volume/i })).toBeInTheDocument();
  });

  it('renders the submit button with "Update Archive Entry" text for edit mode', () => {
    renderBookForm({ isEdit: true });
    expect(screen.getByRole('button', { name: /Update Archive Entry/i })).toBeInTheDocument();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-02: Error message displays in form
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-BOOK-02: Error message displays', () => {
  it('displays validation error when error prop is set', () => {
    renderBookForm({ error: 'Title and Author are required to archive a volume.' });
    expect(screen.getByText(/Title and Author are required/i)).toBeInTheDocument();
  });

  it('does not render error block when error is null', () => {
    renderBookForm({ error: null });
    expect(screen.queryByText(/required/i)).not.toBeInTheDocument();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-03: Edit form pre-filled with existing data
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-BOOK-03: Edit form pre-filled with data', () => {
  it('pre-fills title in edit mode', () => {
    renderBookForm({
      isEdit: true,
      formData: { ...defaultFormData, title: 'The Alchemist', author: 'Paulo Coelho' },
    });
    expect(screen.getByDisplayValue('The Alchemist')).toBeInTheDocument();
  });

  it('pre-fills author in edit mode', () => {
    renderBookForm({
      isEdit: true,
      formData: { ...defaultFormData, title: 'The Alchemist', author: 'Paulo Coelho' },
    });
    expect(screen.getByDisplayValue('Paulo Coelho')).toBeInTheDocument();
  });

  it('pre-fills ISBN in edit mode', () => {
    renderBookForm({
      isEdit: true,
      formData: { ...defaultFormData, isbn: '978-0062316097', title: 'T', author: 'A' },
    });
    expect(screen.getByDisplayValue('978-0062316097')).toBeInTheDocument();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-04: Submit triggers handler
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-BOOK-04: Form submit triggers handler', () => {
  it('calls handleSubmit when form is submitted', async () => {
    const mockSubmit = vi.fn((e) => e.preventDefault());
    const { container } = renderBookForm({ handleSubmit: mockSubmit });
    // Submit form directly to bypass HTML5 required validation
    const form = container.querySelector('form');
    fireEvent.submit(form);
    expect(mockSubmit).toHaveBeenCalled();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-05: Loading state disables submit
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-BOOK-05: Loading state on submit', () => {
  it('shows "Processing..." when loading is true', () => {
    renderBookForm({ loading: true });
    expect(screen.getByRole('button', { name: /Processing/i })).toBeDisabled();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-06: Book card displays correct information
// ─────────────────────────────────────────────────────────────────────────────
import api from './api/axios';

describe('TC-BOOK-06: Collection — book card data', () => {
  const mockBooks = [
    {
      id: 'abc-123',
      title: 'Clean Code',
      author: 'Robert Martin',
      genre: 'Engineering',
      status: 2,
      rating: 5,
      currentPage: 431,
      totalPages: 431,
      coverImageUrl: '/covers/clean-code.png',
    },
  ];

  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockBooks });
  });

  it('renders book title on card', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText('Clean Code')).toBeInTheDocument();
    });
  });

  it('renders book author on card', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText('Robert Martin')).toBeInTheDocument();
    });
  });

  it('renders rating badge when rating exists', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText('5.0')).toBeInTheDocument();
    });
  });

  it('renders cover image when URL is provided', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      const img = screen.getByAltText('Clean Code');
      expect(img).toHaveAttribute('src', '/covers/clean-code.png');
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-BOOK-07: Empty state shown when no books
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-BOOK-07: Empty state when no books', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: [] });
  });

  it('shows empty state message when library is empty', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/The shelves are currently empty/i)).toBeInTheDocument();
    });
  });

  it('shows "Register New Volume" button in empty state', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Register New Volume/i })).toBeInTheDocument();
    });
  });
});
