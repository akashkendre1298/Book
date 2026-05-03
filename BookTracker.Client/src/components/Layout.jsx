import React from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Library, LayoutDashboard, Bookmark, LogOut } from 'lucide-react';

const Layout = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!user) return <>{children}</>;

  const navItems = [
    { name: 'Dashboard', path: '/', icon: LayoutDashboard },
    { name: 'Collection', path: '/collection', icon: Library },
    { name: 'Goals', path: '/goals', icon: LayoutDashboard },
    { name: 'Profile', path: '/profile', icon: Library },
  ];

  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b border-ink/10 bg-paper/80 backdrop-blur-md sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-3">
            <Bookmark className="w-8 h-8 text-ink" />
            <span className="font-serif text-2xl tracking-tight italic">Athenaeum</span>
          </Link>

          <nav className="hidden md:flex items-center gap-10">
            {navItems.map((item) => (
              <Link
                key={item.path}
                to={item.path}
                className={`font-sans text-sm font-semibold uppercase tracking-widest transition-colors ${
                  location.pathname === item.path ? 'text-ink' : 'text-ink/40 hover:text-ink'
                }`}
              >
                {item.name}
              </Link>
            ))}
            <Link 
              to="/books/add" 
              className="bg-ink text-paper px-4 py-2 text-[10px] font-sans uppercase tracking-widest font-semibold hover:bg-clay transition-colors"
            >
              Add Volume
            </Link>
            <button
              onClick={handleLogout}
              className="text-ink/40 hover:text-ink transition-colors"
              title="Logout"
            >
              <LogOut className="w-5 h-5" />
            </button>
          </nav>
        </div>
      </header>

      <main className="flex-1 max-w-7xl mx-auto px-6 py-12 w-full">
        {children}
      </main>

      <footer className="border-t border-ink/10 py-12">
        <div className="max-w-7xl mx-auto px-6 text-center">
          <p className="font-serif text-ink/40 italic">
            “A room without books is like a body without a soul.” — Cicero
          </p>
          <p className="font-sans text-[10px] uppercase tracking-[0.2em] text-ink/30 mt-4">
            © 2024 Private Library Editions • The Archive
          </p>
        </div>
      </footer>
    </div>
  );
};

export default Layout;
