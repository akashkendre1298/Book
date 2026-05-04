import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { BookOpen, Upload, FileText, Image as ImageIcon, CheckCircle, ArrowLeft, Loader2 } from 'lucide-react';
import { motion } from 'framer-motion';

const AddBookPage = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    author: '',
    genre: '',
    isbn: '',
    totalPages: '',
    publicationYear: '',
    isPublic: false
  });
  const [pdfFile, setPdfFile] = useState(null);
  const [coverFile, setCoverFile] = useState(null);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleFileChange = (e, setter) => {
    const file = e.target.files[0];
    if (file) setter(file);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    console.log('Ingestion triggered. PDF:', pdfFile, 'Cover:', coverFile);

    setLoading(true);
    const data = new FormData();
    if (pdfFile) data.append('pdf', pdfFile);
    if (coverFile) data.append('cover', coverFile);
    data.append('title', formData.title);
    data.append('author', formData.author);
    data.append('genre', formData.genre);
    data.append('isbn', formData.isbn);
    data.append('totalPages', formData.totalPages);
    data.append('publicationYear', formData.publicationYear);
    data.append('isPublic', formData.isPublic);

    try {
      await api.post('/books/upload', data, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      navigate('/books');
    } catch (error) {
      console.error('Upload failed:', error);
      alert('Failed to register volume. Please check the files and try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto py-12 space-y-12">
      <header className="flex flex-col gap-4">
        <button
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 text-[10px] font-sans uppercase tracking-[0.3em] text-ink/40 hover:text-clay transition-all w-fit"
        >
          <ArrowLeft className="w-4 h-4" /> Return to Archive
        </button>
        <h1 className="text-6xl font-serif text-ink tracking-tighter">Digital Ingestion</h1>
        <p className="font-serif italic text-ink/60 text-xl leading-relaxed">
          Registering a new manuscript into the archive. Volumes marked as public will undergo curation before global publication.
        </p>
      </header>

      <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-2 gap-16">
        {/* Metadata Section */}
        <div className="space-y-8">
          <h3 className="text-[10px] font-sans uppercase tracking-[0.4em] text-ink/30 font-bold border-b border-ink/10 pb-4">Metadata</h3>

          <div className="space-y-6">
            <div className="space-y-2">
              <label className="text-[10px] font-sans uppercase tracking-widest text-ink/60 font-bold">Manuscript Title</label>
              <input
                required
                name="title"
                value={formData.title}
                onChange={handleInputChange}
                className="w-full bg-paper-darker border border-ink/10 p-4 font-serif text-lg outline-none focus:border-clay/30 transition-all"
                placeholder="The name of the volume..."
              />
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-sans uppercase tracking-widest text-ink/60 font-bold">Primary Author</label>
              <input
                required
                name="author"
                value={formData.author}
                onChange={handleInputChange}
                className="w-full bg-paper-darker border border-ink/10 p-4 font-serif text-lg outline-none focus:border-clay/30 transition-all"
                placeholder="The creator of the work..."
              />
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-sans uppercase tracking-widest text-ink/60 font-bold">Literary Genre</label>
              <input
                name="genre"
                value={formData.genre}
                onChange={handleInputChange}
                className="w-full bg-paper-darker border border-ink/10 p-4 font-serif text-lg outline-none focus:border-clay/30 transition-all"
                placeholder="e.g. Philosophy, Science, Fiction..."
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <label className="text-[10px] font-sans uppercase tracking-widest text-ink/60 font-bold">ISBN</label>
                <input
                  name="isbn"
                  value={formData.isbn}
                  onChange={handleInputChange}
                  className="w-full bg-paper-darker border border-ink/10 p-4 font-serif text-lg outline-none focus:border-clay/30 transition-all"
                  placeholder="ISBN-10 or ISBN-13..."
                />
              </div>
              <div className="space-y-2">
                <label className="text-[10px] font-sans uppercase tracking-widest text-ink/60 font-bold">Total Pages</label>
                <input
                  type="number"
                  name="totalPages"
                  value={formData.totalPages}
                  onChange={handleInputChange}
                  className="w-full bg-paper-darker border border-ink/10 p-4 font-serif text-lg outline-none focus:border-clay/30 transition-all"
                  placeholder="Page count..."
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-sans uppercase tracking-widest text-ink/60 font-bold">Publication Year</label>
              <input
                type="number"
                name="publicationYear"
                value={formData.publicationYear}
                onChange={handleInputChange}
                className="w-full bg-paper-darker border border-ink/10 p-4 font-serif text-lg outline-none focus:border-clay/30 transition-all"
                placeholder="Year of first publication..."
              />
            </div>

            <div className="pt-6 flex items-center gap-4 group cursor-pointer">
              <input
                type="checkbox"
                id="isPublic"
                name="isPublic"
                checked={formData.isPublic}
                onChange={handleInputChange}
                className="w-5 h-5 accent-clay border-ink/20 rounded-none cursor-pointer"
              />
              <label htmlFor="isPublic" className="flex flex-col cursor-pointer">
                <span className="text-xs font-sans font-bold uppercase tracking-widest">Public Library Recommendation</span>
                <span className="text-[10px] text-ink/40 font-serif italic">Submit this volume for global curation?</span>
              </label>
            </div>
          </div>
        </div>

        {/* File Ingestion Section */}
        <div className="space-y-12">

          {/* Cover Upload */}
          <div className="space-y-4">
            <h3 className="text-[10px] font-sans uppercase tracking-[0.4em] text-ink/30 font-bold border-b border-ink/10 pb-4">Artistic Cover (Optional)</h3>
            <label
              htmlFor="cover-upload"
              className={`
              border-2 border-dashed flex flex-col items-center justify-center p-8 transition-all cursor-pointer group
              ${coverFile ? 'border-clay/20 bg-clay/5' : 'border-ink/10 hover:border-clay/30 bg-paper-darker'}
            `}>
              <input
                id="cover-upload"
                type="file"
                accept="image/*"
                className="sr-only"
                onChange={(e) => handleFileChange(e, setCoverFile)}
              />
              {coverFile ? (
                <>
                  <ImageIcon className="w-8 h-8 text-clay mb-2" />
                  <span className="text-[10px] font-sans font-bold uppercase tracking-widest text-clay">{coverFile.name}</span>
                </>
              ) : (
                <>
                  <Upload className="w-8 h-8 text-ink/10 group-hover:text-clay/40 transition-colors mb-2" />
                  <span className="text-[9px] font-sans uppercase tracking-widest text-ink/40">Select Cover Image</span>
                </>
              )}
            </label>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-ink text-paper py-6 flex items-center justify-center gap-4 group hover:bg-clay hover:text-ink transition-all disabled:opacity-50"
          >
            {loading ? (
              <Loader2 className="w-5 h-5 animate-spin" />
            ) : (
              <>
                <BookOpen className="w-5 h-5 group-hover:scale-110 transition-transform" />
                <span className="text-[10px] font-sans uppercase tracking-[0.4em] font-bold">Initiate Ingestion</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default AddBookPage;
