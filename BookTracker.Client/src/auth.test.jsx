/**
 * 🔐 FRONTEND TEST SUITE — Authentication UI
 * Covers: Test_list.txt lines 156-164
 * - Register form renders
 * - Login form renders
 * - Validation messages shown
 * - Invalid login shows error
 * - Successful login redirects
 * - Loading state during submit
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import { AuthContext } from './context/AuthContext';

// ─── Mocks ──────────────────────────────────────────────────────────────────
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});

beforeEach(() => {
  vi.clearAllMocks();
});

const renderWithAuth = (component, authValue = {}) => {
  const defaults = {
    user: null,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    loading: false,
  };
  return render(
    <AuthContext.Provider value={{ ...defaults, ...authValue }}>
      <MemoryRouter>{component}</MemoryRouter>
    </AuthContext.Provider>
  );
};

// ─────────────────────────────────────────────────────────────────────────────
// TC-AUTH-01: Login form renders
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-AUTH-01: Login form renders', () => {
  it('renders the login page heading', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByText(/Access the Archive/i)).toBeInTheDocument();
  });

  it('renders email input field', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByPlaceholderText(/reader@library\.com/i)).toBeInTheDocument();
  });

  it('renders password input field', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByPlaceholderText('••••••••')).toBeInTheDocument();
  });

  it('renders submit button', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByRole('button', { name: /Authorize Entry/i })).toBeInTheDocument();
  });

  it('renders a link to the registration page', () => {
    renderWithAuth(<LoginPage />);
    expect(screen.getByText(/Request Access/i)).toBeInTheDocument();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-AUTH-02: Register form renders
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-AUTH-02: Register form renders', () => {
  it('renders the registration page heading', () => {
    renderWithAuth(<RegisterPage />);
    expect(screen.getByText(/Join the Registry/i)).toBeInTheDocument();
  });

  it('renders email input', () => {
    renderWithAuth(<RegisterPage />);
    expect(screen.getByPlaceholderText(/reader@library\.com/i)).toBeInTheDocument();
  });

  it('renders two password fields (password + confirm)', () => {
    renderWithAuth(<RegisterPage />);
    const passwordFields = screen.getAllByPlaceholderText('••••••••');
    expect(passwordFields).toHaveLength(2);
  });

  it('renders submit button', () => {
    renderWithAuth(<RegisterPage />);
    expect(screen.getByRole('button', { name: /Initialize Account/i })).toBeInTheDocument();
  });

  it('renders link back to login', () => {
    renderWithAuth(<RegisterPage />);
    expect(screen.getByText(/Already registered\? Sign in here/i)).toBeInTheDocument();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-AUTH-03: Validation messages shown
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-AUTH-03: Validation messages shown', () => {
  it('shows error when passwords do not match', async () => {
    const user = userEvent.setup();
    renderWithAuth(<RegisterPage />);

    // Must fill email first (HTML5 required prevents submit without it)
    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'test@example.com');
    const [passwordField, confirmField] = screen.getAllByPlaceholderText('••••••••');
    await user.type(passwordField, 'Password123!');
    await user.type(confirmField, 'WrongPassword!');
    await user.click(screen.getByRole('button', { name: /Initialize Account/i }));

    await waitFor(() => {
      expect(screen.getByText(/Passwords do not match/i)).toBeInTheDocument();
    });
  });

  it('does not show mismatch error when passwords match', async () => {
    const user = userEvent.setup();
    const mockRegister = vi.fn().mockResolvedValue({ id: '1', email: 'test@test.com' });
    renderWithAuth(<RegisterPage />, { register: mockRegister });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'test@test.com');
    const [p1, p2] = screen.getAllByPlaceholderText('••••••••');
    await user.type(p1, 'Password123!');
    await user.type(p2, 'Password123!');
    await user.click(screen.getByRole('button', { name: /Initialize Account/i }));

    await waitFor(() => {
      expect(screen.queryByText(/Passwords do not match/i)).not.toBeInTheDocument();
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-AUTH-04: Invalid login shows error
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-AUTH-04: Invalid login shows error', () => {
  it('displays API error message on failed login', async () => {
    const user = userEvent.setup();
    const mockLogin = vi.fn().mockRejectedValue({
      response: { data: { message: 'Invalid credentials provided.' } },
    });
    renderWithAuth(<LoginPage />, { login: mockLogin });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'bad@user.com');
    await user.type(screen.getByPlaceholderText('••••••••'), 'wrongpass');
    await user.click(screen.getByRole('button', { name: /Authorize Entry/i }));

    await waitFor(() => {
      expect(screen.getByText(/Invalid credentials provided/i)).toBeInTheDocument();
    });
  });

  it('displays fallback error when no API message', async () => {
    const user = userEvent.setup();
    const mockLogin = vi.fn().mockRejectedValue({ response: null });
    renderWithAuth(<LoginPage />, { login: mockLogin });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'bad@user.com');
    await user.type(screen.getByPlaceholderText('••••••••'), 'wrongpass');
    await user.click(screen.getByRole('button', { name: /Authorize Entry/i }));

    await waitFor(() => {
      expect(screen.getByText(/Authentication failed/i)).toBeInTheDocument();
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-AUTH-05: Successful login redirects
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-AUTH-05: Successful login redirects to /', () => {
  it('calls navigate("/") on successful login', async () => {
    const user = userEvent.setup();
    const mockLogin = vi.fn().mockResolvedValue({ id: '1', email: 'test@test.com' });
    renderWithAuth(<LoginPage />, { login: mockLogin });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'test@test.com');
    await user.type(screen.getByPlaceholderText('••••••••'), 'Password123!');
    await user.click(screen.getByRole('button', { name: /Authorize Entry/i }));

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/');
    });
  });

  it('calls navigate("/") on successful registration', async () => {
    const user = userEvent.setup();
    const mockRegister = vi.fn().mockResolvedValue({ id: '2', email: 'new@test.com' });
    renderWithAuth(<RegisterPage />, { register: mockRegister });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'new@test.com');
    const [p1, p2] = screen.getAllByPlaceholderText('••••••••');
    await user.type(p1, 'Password123!');
    await user.type(p2, 'Password123!');
    await user.click(screen.getByRole('button', { name: /Initialize Account/i }));

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/');
    });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TC-AUTH-06: Loading state disables submit button
// ─────────────────────────────────────────────────────────────────────────────
describe('TC-AUTH-06: Loading state on submit', () => {
  it('shows "Verifying..." and disables button during login', async () => {
    const user = userEvent.setup();
    const mockLogin = vi.fn().mockImplementation(() => new Promise(() => {})); // never resolves
    renderWithAuth(<LoginPage />, { login: mockLogin });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'test@test.com');
    await user.type(screen.getByPlaceholderText('••••••••'), 'Password123!');
    await user.click(screen.getByRole('button', { name: /Authorize Entry/i }));

    await waitFor(() => {
      const btn = screen.getByRole('button', { name: /Verifying/i });
      expect(btn).toBeDisabled();
    });
  });

  it('shows "Creating Identity..." and disables button during registration', async () => {
    const user = userEvent.setup();
    const mockRegister = vi.fn().mockImplementation(() => new Promise(() => {}));
    renderWithAuth(<RegisterPage />, { register: mockRegister });

    await user.type(screen.getByPlaceholderText(/reader@library\.com/i), 'new@user.com');
    const [p1, p2] = screen.getAllByPlaceholderText('••••••••');
    await user.type(p1, 'Password123!');
    await user.type(p2, 'Password123!');
    await user.click(screen.getByRole('button', { name: /Initialize Account/i }));

    await waitFor(() => {
      const btn = screen.getByRole('button', { name: /Creating Identity/i });
      expect(btn).toBeDisabled();
    });
  });
});
