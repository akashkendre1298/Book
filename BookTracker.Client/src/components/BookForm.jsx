import React from 'react';
import { Book as BookIcon, Type, User, Bookmark, Hash, Layers, Calendar } from 'lucide-react';

const BookForm = ({ formData, setFormData, handleSubmit, loading, error, isEdit = false }) => {
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-12">
      {error && (
        <div className="bg-clay/10 border-l-2 border-clay p-4 text-clay text-sm font-sans italic">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-x-12 gap-y-10">
        {/* Title */}
        <div className="md:col-span-2 group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <Type className="w-3 h-3" /> Book Title *
          </label>
          <input 
            type="text" 
            name="title"
            value={formData.title}
            onChange={handleChange}
            placeholder="e.g. The Shadow of the Wind"
            className="w-full bg-transparent border-b border-ink/10 py-3 text-3xl font-serif outline-none focus:border-clay transition-all placeholder:text-ink/10"
            required
          />
        </div>

        {/* Author */}
        <div className="md:col-span-2 group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <User className="w-3 h-3" /> Author Name *
          </label>
          <input 
            type="text" 
            name="author"
            value={formData.author}
            onChange={handleChange}
            placeholder="e.g. Carlos Ruiz Zafón"
            className="w-full bg-transparent border-b border-ink/10 py-3 text-2xl font-serif outline-none focus:border-clay transition-all placeholder:text-ink/10"
            required
          />
        </div>

        {/* Genre */}
        <div className="group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <Bookmark className="w-3 h-3" /> Genre
          </label>
          <input 
            type="text" 
            name="genre"
            value={formData.genre}
            onChange={handleChange}
            placeholder="e.g. Historical Fiction"
            className="w-full bg-transparent border-b border-ink/10 py-2 font-serif italic outline-none focus:border-clay transition-all placeholder:text-ink/10"
          />
        </div>

        {/* ISBN */}
        <div className="group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <Hash className="w-3 h-3" /> ISBN
          </label>
          <input 
            type="text" 
            name="isbn"
            value={formData.isbn}
            onChange={handleChange}
            placeholder="978-..."
            className="w-full bg-transparent border-b border-ink/10 py-2 font-sans tracking-widest outline-none focus:border-clay transition-all placeholder:text-ink/10"
          />
        </div>

        {/* Total Pages */}
        <div className="group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <Layers className="w-3 h-3" /> Total Pages
          </label>
          <input 
            type="number" 
            name="totalPages"
            value={formData.totalPages}
            onChange={handleChange}
            className="w-full bg-transparent border-b border-ink/10 py-2 font-sans outline-none focus:border-clay transition-all"
          />
        </div>

        {/* Publication Year */}
        <div className="group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <Calendar className="w-3 h-3" /> Publication Year
          </label>
          <input 
            type="number" 
            name="publicationYear"
            value={formData.publicationYear}
            onChange={handleChange}
            className="w-full bg-transparent border-b border-ink/10 py-2 font-sans outline-none focus:border-clay transition-all"
          />
        </div>

        {/* Cover Image URL */}
        <div className="md:col-span-2 group">
          <label className="text-[10px] font-sans font-bold uppercase tracking-[0.3em] text-ink/40 mb-3 flex items-center gap-2">
            <BookIcon className="w-3 h-3" /> Cover Image URL
          </label>
          <input 
            type="url" 
            name="coverImageUrl"
            value={formData.coverImageUrl}
            onChange={handleChange}
            placeholder="https://images.com/cover.jpg"
            className="w-full bg-transparent border-b border-ink/10 py-2 font-sans text-xs outline-none focus:border-clay transition-all placeholder:text-ink/10"
          />
          <p className="mt-2 text-[9px] font-sans uppercase tracking-widest text-ink/30 italic">Provide a direct link to the volume's cover art for the archive.</p>
        </div>
      </div>

      <div className="pt-12 flex flex-col md:flex-row gap-6">
        <button 
          type="submit" 
          disabled={loading}
          className="btn-primary flex-grow py-5 text-sm uppercase tracking-[0.2em]"
        >
          {loading ? 'Processing...' : isEdit ? 'Update Archive Entry' : 'Archive New Volume'}
        </button>
        <button 
          type="button"
          onClick={() => window.history.back()}
          className="btn-secondary px-16 py-5 text-sm uppercase tracking-[0.2em]"
        >
          Cancel
        </button>
      </div>
    </form>
  );
};

export default BookForm;
