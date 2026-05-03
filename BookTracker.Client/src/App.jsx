import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CollectionIndex from './pages/CollectionIndex';
import BookDetailPage from './pages/BookDetailPage';
import AddBookPage from './pages/AddBookPage';
import ReadingGoalsPage from './pages/ReadingGoalsPage';
import ProfilePage from './pages/ProfilePage';
import Dashboard from './pages/Dashboard';
import EditBookPage from './pages/EditBookPage';

// Protected Route Component
const ProtectedRoute = ({ children }) => {
  const { user, loading } = useAuth();
  
  if (loading) {
    return (
      <div className="min-h-screen bg-paper flex items-center justify-center">
        <div className="font-serif italic text-2xl animate-pulse">Opening the archive...</div>
      </div>
    );
  }
  
  if (!user) {
    return <Navigate to="/login" replace />;
  }
  
  return children;
};

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      
      <Route path="/" element={
        <ProtectedRoute>
          <Layout>
            <Dashboard />
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/collection" element={
        <ProtectedRoute>
          <Layout>
            <CollectionIndex />
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/books/add" element={
        <ProtectedRoute>
          <Layout>
            <AddBookPage />
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/books/:id/edit" element={
        <ProtectedRoute>
          <Layout>
            <EditBookPage />
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/books/:id" element={
        <ProtectedRoute>
          <Layout>
            <BookDetailPage />
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/goals" element={
        <ProtectedRoute>
          <Layout>
            <ReadingGoalsPage />
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/profile" element={
        <ProtectedRoute>
          <Layout>
            <ProfilePage />
          </Layout>
        </ProtectedRoute>
      } />
      
      {/* Fallback */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <Router>
        <AppRoutes />
      </Router>
    </AuthProvider>
  );
}

export default App;
