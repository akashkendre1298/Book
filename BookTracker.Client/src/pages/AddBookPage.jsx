import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import BookForm from '../components/BookForm';

const AddBookPage = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    author: '',
    isbn: '',
    genre: '',
    totalPages: '',
    publicationYear: '',
    coverImageUrl: '',
    status: 0 // To Read
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const bookToCreate = {
        ...formData,
        totalPages: formData.totalPages ? parseInt(formData.totalPages) : null,
        publicationYear: formData.publicationYear ? parseInt(formData.publicationYear) : null
      };

      await api.post('/books', bookToCreate);
      navigate('/collection');
    } catch (err) {
      console.error('Error adding book:', err);
      setError(err.response?.data?.message || 'An error occurred while archiving the volume.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto px-6 py-12">
      <header className="mb-20 text-center">
        <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-clay mb-4 block">Archival Request</span>
        <h1 className="text-6xl mb-6">Register New Volume</h1>
        <div className="w-24 h-px bg-ink/10 mx-auto"></div>
        <p className="mt-8 font-serif italic text-ink-muted text-xl max-w-2xl mx-auto leading-relaxed">
          Begin the formal indexing process for a new addition to the private collection.
        </p>
      </header>

      <BookForm 
        formData={formData}
        setFormData={setFormData}
        handleSubmit={handleSubmit}
        loading={loading}
        error={error}
      />
    </div>
  );
};

export default AddBookPage;
