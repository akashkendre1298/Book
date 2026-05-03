import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import PageContainer from './components/PageContainer';
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
      <Route path="/login" element={<PageContainer><LoginPage /></PageContainer>} />
      <Route path="/register" element={<PageContainer><RegisterPage /></PageContainer>} />
      
      <Route path="/" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <Dashboard />
            </PageContainer>
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/collection" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <CollectionIndex />
            </PageContainer>
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/books/add" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <AddBookPage />
            </PageContainer>
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/books/:id/edit" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <EditBookPage />
            </PageContainer>
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/books/:id" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <BookDetailPage />
            </PageContainer>
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/goals" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <ReadingGoalsPage />
            </PageContainer>
          </Layout>
        </ProtectedRoute>
      } />

      <Route path="/profile" element={
        <ProtectedRoute>
          <Layout>
            <PageContainer>
              <ProfilePage />
            </PageContainer>
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
