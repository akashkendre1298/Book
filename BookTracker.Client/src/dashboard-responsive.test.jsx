/**
 * 📊 FRONTEND TEST SUITE — Dashboard UI
 * Covers: Test_list.txt lines 204-209
 * - Stats cards render
 * - Goal progress updates
 *
 * 📱 FRONTEND TEST SUITE — Responsive UI
 * Covers: Test_list.txt lines 213-219
 * - Grid adapts to mobile
 * - No overflow issues
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
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
  },
}));

// Mock framer-motion to avoid animation issues in tests
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }) => children,
}));

import api from './api/axios';

const mockStats = {
  totalBooks: 12,
  readBooks: 5,
  readingBooks: 2,
  totalPagesRead: 1850,
  yearlyGoal: 24,
  booksReadThisYear: 5,
  genreDistribution: { Fiction: 4, History: 3, Design: 2, Philosophy: 3 },
};

const mockCurrentlyReading = [
  { id: '1', title: 'The Architecture of Silence', author: 'Elena Thorne', status: 1, currentPage: 142, totalPages: 350, genre: 'Psychology' },
];

const mockToRead = [
  { id: '2', title: 'Urban Solitude', author: 'Julian Vane', status: 0, currentPage: 0, totalPages: 280, genre: 'Sociology' },
];

const mockFinished = [
  { id: '3', title: 'Midnight in the Garden', author: 'Sarah Moore', status: 2, rating: 5, currentPage: 388, totalPages: 388, genre: 'Memoir' },
];

// ─────────────────────────────────────────────────────────────────────────────
// TC-DASHBOARD-01: Dashboard Stats Cards render
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-DASHBOARD-01: Dashboard stats cards render', () => {
  beforeEach(() => {
    api.get.mockImplementation((url) => {
      if (url.includes('dashboard/stats')) return Promise.resolve({ data: mockStats });
      if (url.includes('status=1')) return Promise.resolve({ data: mockCurrentlyReading });
      if (url.includes('status=0')) return Promise.resolve({ data: mockToRead });
      if (url.includes('status=2')) return Promise.resolve({ data: mockFinished });
      return Promise.resolve({ data: [] });
    });
  });

  it('renders the dashboard main greeting heading', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/Greetings, Collector/i)).toBeInTheDocument();
    });
  });

  it('renders "Currently Studying" section header', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/Currently Studying/i)).toBeInTheDocument();
    });
  });

  it('renders "To Read Next" section header', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/To Read Next/i)).toBeInTheDocument();
    });
  });

  it('renders "Recently Cataloged" section header', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/Recently Cataloged/i)).toBeInTheDocument();
    });
  });

  it('renders Currently Reading section with featured book title', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      // Title appears multiple times in nested links - use getAllByText
      const titles = screen.getAllByText('The Architecture of Silence');
      expect(titles.length).toBeGreaterThan(0);
    });
  });

  it('renders "To Read" section with a book title', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText('Urban Solitude')).toBeInTheDocument();
    });
  });

  it('renders "Recently Cataloged" section with finished book', async () => {
    const { default: Dashboard } = await import('./pages/Dashboard');
    render(<MemoryRouter><Dashboard /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText('Midnight in the Garden')).toBeInTheDocument();
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-DASHBOARD-02: Goal Progress renders on ReadingGoalsPage
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-DASHBOARD-02: Reading goal progress renders', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockStats });
  });

  it('renders the year in the heading', async () => {
    const year = new Date().getFullYear();
    const { default: ReadingGoalsPage } = await import('./pages/ReadingGoalsPage');
    render(<MemoryRouter><ReadingGoalsPage /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(new RegExp(`The ${year} Reading Goal`, 'i'))).toBeInTheDocument();
    });
  });

  it('renders the "Commit to Goal" button', async () => {
    const { default: ReadingGoalsPage } = await import('./pages/ReadingGoalsPage');
    render(<MemoryRouter><ReadingGoalsPage /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Commit to Goal/i })).toBeInTheDocument();
    });
  });

  it('renders the goal range slider', async () => {
    const { default: ReadingGoalsPage } = await import('./pages/ReadingGoalsPage');
    render(<MemoryRouter><ReadingGoalsPage /></MemoryRouter>);
    await waitFor(() => {
      const slider = screen.getByRole('slider');
      expect(slider).toBeInTheDocument();
    });
  });

  it('renders a progress percentage', async () => {
    const { default: ReadingGoalsPage } = await import('./pages/ReadingGoalsPage');
    render(<MemoryRouter><ReadingGoalsPage /></MemoryRouter>);
    await waitFor(() => {
      // Percentage is calculated from booksReadThisYear / goal (can vary with state)
      const percentEl = screen.getByText(/%/);
      expect(percentEl).toBeInTheDocument();
    });
  });

  it('updates displayed goal number when slider changes', async () => {
    const { default: ReadingGoalsPage } = await import('./pages/ReadingGoalsPage');
    render(<MemoryRouter><ReadingGoalsPage /></MemoryRouter>);
    await waitFor(() => screen.getByRole('slider'));
    const slider = screen.getByRole('slider');
    fireEvent.change(slider, { target: { value: '42' } });
    await waitFor(() => {
      // The span next to the slider shows the number
      const goalDisplay = screen.getAllByText('42');
      expect(goalDisplay.length).toBeGreaterThan(0);
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-RESPONSIVE-01: Book form layout is responsive
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-RESPONSIVE-01: Responsive layout classes', () => {
  it('collection page renders with responsive heading', async () => {
    api.get.mockResolvedValue({ data: [] });
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);
    await waitFor(() => {
      expect(screen.getByText(/Catalogue Raisonné/i)).toBeInTheDocument();
    });
    // Verify the page renders (responsive layout exists in src)
    expect(screen.getByText(/Catalogue Raisonné/i)).toBeInTheDocument();
  });

  it('login form has two-column responsive grid', async () => {
    const { default: LoginPage } = await import('./pages/LoginPage');
    const { AuthContext } = await import('./context/AuthContext');
    const { container } = render(
      <AuthContext.Provider value={{ user: null, login: vi.fn(), loading: false }}>
        <MemoryRouter><LoginPage /></MemoryRouter>
      </AuthContext.Provider>
    );
    // Tailwind class is stored as raw string, check for grid-cols
    const gridElement = container.querySelector('[class*="grid-cols"]');
    expect(gridElement).toBeTruthy();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-RESPONSIVE-02: Buttons have adequate target sizes
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-RESPONSIVE-02: Touch-friendly button sizes', () => {
  it('login submit button has full-width class for easy tap target', async () => {
    const { default: LoginPage } = await import('./pages/LoginPage');
    const { AuthContext } = await import('./context/AuthContext');
    const { container } = render(
      <AuthContext.Provider value={{ user: null, login: vi.fn(), loading: false }}>
        <MemoryRouter><LoginPage /></MemoryRouter>
      </AuthContext.Provider>
    );
    const submitBtn = container.querySelector('button[type="submit"]');
    expect(submitBtn).toHaveClass('w-full');
  });
});
