import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { useNavigate } from 'react-router-dom';
import { Search, Filter, Plus, Book as BookIcon, ChevronDown, MoreVertical, Edit3, Trash2, Star } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const CollectionIndex = () => {
  const navigate = useNavigate();
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');
  const [status, setStatus] = useState('');
  const [genre, setGenre] = useState('');
  const [rating, setRating] = useState('');
  const [sortBy, setSortBy] = useState('dateadded');

  const fetchBooks = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (query) params.append('query', query);
      if (status) params.append('status', status);
      if (genre) params.append('genre', genre);
      if (rating) params.append('rating', rating);
      if (sortBy) params.append('sortBy', sortBy);

      const response = await api.get(`/books?${params.toString()}`);
      setBooks(response.data);
    } catch (err) {
      console.error('Failed to fetch books', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (e, id, title) => {
    e.stopPropagation();
    if (window.confirm(`Are you sure you wish to remove "${title}" from the archive? This action is permanent.`)) {
      try {
        await api.delete(`/books/${id}`);
        fetchBooks();
      } catch (err) {
        alert('Failed to remove volume from the archive.');
      }
    }
  };

  useEffect(() => {
    const timer = setTimeout(fetchBooks, 300);
    return () => clearTimeout(timer);
  }, [query, status, genre, rating, sortBy]);

  const readingStatusMap = {
    0: { label: 'Want to Read', color: 'text-ink/40 bg-ink/5' },
    1: { label: 'Reading', color: 'text-sage bg-sage/10' },
    2: { label: 'Finished', color: 'text-ink bg-ink/10' },
  };

  return (
    <div className="space-y-12">
      {/* Header & Search */}
      <section className="flex flex-col md:flex-row md:items-end justify-between gap-8 border-b border-ink/10 pb-12">
        <div className="space-y-4">
          <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-ink/30">Catalogue Raisonné</span>
          <h1 className="text-6xl font-serif text-ink tracking-tighter">The Collection</h1>
          <p className="font-serif italic text-ink/60 text-lg">Managing {books.length} volumes in the private archive.</p>
        </div>

        <div className="flex flex-wrap items-center gap-4">
          <div className="relative group">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink/30 group-focus-within:text-ink transition-colors" />
            <input
              type="text"
              placeholder="Search Title, Author, or ISBN..."
              className="bg-paper-darker border border-ink/10 px-10 py-3 font-sans text-sm outline-none focus:border-ink transition-all w-[300px]"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
          </div>
          <button 
            onClick={() => navigate('/books/add')}
            className="btn-primary flex items-center gap-2 py-3"
          >
            <Plus className="w-4 h-4" /> Add Volume
          </button>
        </div>
      </section>

      {/* Filters & Sorting */}
      <section className="flex flex-wrap items-center gap-8 py-4 border-b border-ink/5">
        <div className="flex items-center gap-4">
          <Filter className="w-4 h-4 text-ink/40" />
          <select 
            className="bg-transparent font-sans text-xs uppercase tracking-widest outline-none cursor-pointer hover:text-ink transition-colors"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="0">Want to Read</option>
            <option value="1">Reading</option>
            <option value="2">Finished</option>
          </select>
          <Filter className="w-4 h-4 text-ink/40 ml-4" />
          <select 
            className="bg-transparent font-sans text-xs uppercase tracking-widest outline-none cursor-pointer hover:text-ink transition-colors"
            value={genre}
            onChange={(e) => setGenre(e.target.value)}
          >
            <option value="">All Genres</option>
            {[...new Set(books.map(b => b.genre).filter(Boolean))].map(g => (
              <option key={g} value={g}>{g}</option>
            ))}
          </select>

          <Star className="w-4 h-4 text-ink/40 ml-4" />
          <select 
            className="bg-transparent font-sans text-xs uppercase tracking-widest outline-none cursor-pointer hover:text-ink transition-colors"
            value={rating}
            onChange={(e) => setRating(e.target.value)}
          >
            <option value="">All Ratings</option>
            {[5, 4, 3, 2, 1].map(r => (
              <option key={r} value={r}>{r} Stars</option>
            ))}
          </select>
        </div>

        <div className="flex items-center gap-4 ml-auto">
          <span className="text-[10px] font-sans uppercase tracking-widest text-ink/30">Sort by:</span>
          <select 
            className="bg-transparent font-sans text-xs uppercase tracking-widest outline-none cursor-pointer hover:text-ink transition-colors"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="dateadded">Recently Added</option>
            <option value="title">Alphabetical (Title)</option>
            <option value="author">Alphabetical (Author)</option>
            <option value="rating">Highest Rated</option>
          </select>
        </div>
      </section>

      {/* Grid */}
      {loading ? (
        <div className="py-20 text-center font-serif italic text-ink/30 text-2xl animate-pulse">
          Consulting the index...
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-x-8 gap-y-16">
          <AnimatePresence>
            {books.map((book) => (
              <motion.div
                key={book.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.95 }}
                className="group flex flex-col gap-6"
              >
                {/* Book Cover Placeholder */}
                <div 
                  onClick={() => navigate(`/books/${book.id}`)}
                  className="aspect-[2/3] bg-paper-darker border border-ink/10 relative overflow-hidden shadow-sm group-hover:shadow-xl transition-all duration-500 cursor-pointer"
                >
                  {book.coverImageUrl ? (
                    <img src={book.coverImageUrl} alt={book.title} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex flex-col items-center justify-center p-8 text-center gap-4">
                      <BookIcon className="w-12 h-12 text-ink/10" />
                      <span className="font-serif text-ink/20 italic text-sm">{book.title}</span>
                    </div>
                  )}
                  <div className="absolute inset-0 bg-ink/0 group-hover:bg-ink/5 transition-colors" />
                  
                  {/* Rating Badge */}
                  {book.rating > 0 && (
                    <div className="absolute top-4 right-4 bg-paper px-2 py-1 text-[10px] font-sans font-bold border border-ink/20">
                      {book.rating}.0
                    </div>
                  )}
                </div>

                {/* Book Info */}
                <div className="space-y-2">
                  <div className="flex justify-between items-start">
                    <span className={`text-[9px] uppercase tracking-[0.2em] px-2 py-1 font-bold ${readingStatusMap[book.status]?.color || ''}`}>
                      {readingStatusMap[book.status]?.label}
                    </span>
                    <div className="flex gap-4">
                      <button 
                        onClick={(e) => { e.stopPropagation(); navigate(`/books/${book.id}/edit`); }}
                        className="text-ink/20 hover:text-clay transition-colors"
                        title="Edit Record"
                      >
                        <Edit3 className="w-4 h-4" />
                      </button>
                      <button 
                        onClick={(e) => handleDelete(e, book.id, book.title)}
                        className="text-ink/20 hover:text-red-800 transition-colors"
                        title="Delete Record"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                  
                  <h3 
                    onClick={() => navigate(`/books/${book.id}`)}
                    className="font-serif text-xl leading-tight group-hover:text-clay transition-colors cursor-pointer line-clamp-2"
                  >
                    {book.title}
                  </h3>
                  <p className="font-sans text-xs uppercase tracking-widest text-ink/50 italic">
                    {book.author}
                  </p>
                  
                  {book.totalPages && book.status === 1 && (
                    <div className="pt-4 space-y-2">
                      <div className="h-0.5 bg-ink/5 w-full relative">
                        <div 
                          className="absolute h-full bg-sage transition-all duration-1000" 
                          style={{ width: `${(book.currentPage / book.totalPages) * 100}%` }}
                        />
                      </div>
                      <div className="flex justify-between font-sans text-[9px] uppercase tracking-widest text-ink/40">
                        <span>{Math.round((book.currentPage / book.totalPages) * 100)}% Complete</span>
                        <span>p. {book.currentPage} / {book.totalPages}</span>
                      </div>
                    </div>
                  )}
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      )}

      {books.length === 0 && !loading && (
        <div className="py-32 text-center space-y-6 max-w-md mx-auto">
          <BookIcon className="w-16 h-16 text-ink/5 mx-auto" />
          <h2 className="font-serif text-2xl text-ink/40 italic">The shelves are currently empty.</h2>
          <p className="font-sans text-sm text-ink/30 leading-relaxed uppercase tracking-widest">
            Begin your archival process by adding the first volume to your private library collection.
          </p>
          <button 
            onClick={() => navigate('/books/add')}
            className="btn-primary mt-8"
          >
            Register New Volume
          </button>
        </div>
      )}
    </div>
  );
};

export default CollectionIndex;
