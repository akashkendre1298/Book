import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';

// ─── Mocks ──────────────────────────────────────────────────────────────────
vi.mock('./api/axios', () => ({
  default: {
    get: vi.fn(),
  },
}));

import api from './api/axios';

describe('Collection Segregation — Category Switching', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    api.get.mockResolvedValue({ data: [] });
  });

  it('TC-SEG-01: Fetches from /books/completed by default (Complete Index)', async () => {
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);

    await waitFor(() => {
      expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/books/completed'));
    });
  });

  it('TC-SEG-02: Fetches from /books/public when Public Library tab is clicked', async () => {
    const user = userEvent.setup();
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);

    // Click Public Library tab
    const publicTab = screen.getByText(/Public Library/i);
    await user.click(publicTab);

    await waitFor(() => {
      expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/books/public'));
    });
  });

  it('TC-SEG-03: Fetches from /books/my when My Collection tab is clicked', async () => {
    const user = userEvent.setup();
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);

    // Click My Collection tab
    const myTab = screen.getByText(/My Collection/i);
    await user.click(myTab);

    await waitFor(() => {
      expect(api.get).toHaveBeenCalledWith(expect.stringContaining('/books/my'));
    });
  });

  it('TC-SEG-04: Data Isolation Check — Public Library Console Logging', async () => {
    const consoleSpy = vi.spyOn(console, 'log');
    const user = userEvent.setup();
    const { default: CollectionIndex } = await import('./pages/CollectionIndex');
    
    api.get.mockResolvedValue({ data: [{ id: '1', title: 'Public Book', visibility: 'Public', isApproved: true }] });
    
    render(<MemoryRouter><CollectionIndex /></MemoryRouter>);

    const publicTab = screen.getByText(/Public Library/i);
    await user.click(publicTab);

    await waitFor(() => {
      expect(consoleSpy).toHaveBeenCalledWith(expect.stringContaining('Public Library Sync'));
    });
    
    consoleSpy.mockRestore();
  });
});
