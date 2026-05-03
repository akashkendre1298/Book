import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api/axios';
import BookForm from '../components/BookForm';

const EditBookPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    author: '',
    isbn: '',
    genre: '',
    totalPages: '',
    publicationYear: '',
    coverImageUrl: '',
    status: 0
  });

  useEffect(() => {
    const fetchBook = async () => {
      try {
        const response = await api.get(`/books/${id}`);
        const book = response.data;
        setFormData({
          ...book,
          totalPages: book.totalPages || '',
          publicationYear: book.publicationYear || '',
          coverImageUrl: book.coverImageUrl || '',
          isbn: book.isbn || '',
          genre: book.genre || ''
        });
      } catch (err) {
        setError('Failed to retrieve volume details from the archive.');
      } finally {
        setLoading(false);
      }
    };
    fetchBook();
  }, [id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    try {
      const bookToUpdate = {
        ...formData,
        totalPages: formData.totalPages ? parseInt(formData.totalPages) : null,
        publicationYear: formData.publicationYear ? parseInt(formData.publicationYear) : null
      };

      await api.put(`/books/${id}`, bookToUpdate);
      navigate(`/books/${id}`);
    } catch (err) {
      setError(err.response?.data?.message || 'An error occurred while updating the archival record.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="flex justify-center items-center h-screen font-serif italic text-ink-muted">Consulting the catalog...</div>;

  return (
    <div className="max-w-4xl mx-auto px-6 py-12">
      <header className="mb-20 text-center">
        <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-clay mb-4 block">Record Amendment</span>
        <h1 className="text-6xl mb-6">Modify Index Entry</h1>
        <div className="w-24 h-px bg-ink/10 mx-auto"></div>
        <p className="mt-8 font-serif italic text-ink-muted text-xl max-w-2xl mx-auto leading-relaxed">
          Updating the bibliographic details for <span className="text-ink underline decoration-clay/30">{formData.title}</span>.
        </p>
      </header>

      <BookForm 
        formData={formData}
        setFormData={setFormData}
        handleSubmit={handleSubmit}
        loading={saving}
        error={error}
        isEdit={true}
      />
    </div>
  );
};

export default EditBookPage;
