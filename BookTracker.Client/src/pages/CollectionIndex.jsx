import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { useNavigate } from 'react-router-dom';
import { 
  Search, Filter, Plus, Book as BookIcon, ChevronDown, 
  MoreVertical, Edit3, Trash2, Star, Eye, EyeOff, 
  FileText, Library, User as UserIcon
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import PdfReader from '../components/PdfReader';
import { getAssetUrl } from '../api/axios';

const CollectionIndex = () => {
  const navigate = useNavigate();
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState('');
  const [activeCategory, setActiveCategory] = useState('all');
  const [readerFile, setReaderFile] = useState(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchBooks = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (query) params.append('query', query);
      params.append('page', page);
      params.append('pageSize', 12);
      
      let endpoint = '/books/completed';
      if (activeCategory === 'public') endpoint = '/books/public';
      if (activeCategory === 'personal') endpoint = '/books/my';

      const response = await api.get(`${endpoint}?${params.toString()}`);
      // Handle PagedResult { items, totalCount, pageNumber, pageSize, totalPages }
      setBooks(response.data.items || []);
      setTotalPages(response.data.totalPages || 1);
    } catch (err) {
      console.error('Failed to fetch books', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    setPage(1); // Reset to first page when category or query changes
  }, [query, activeCategory]);

  useEffect(() => {
    const timer = setTimeout(fetchBooks, 300);
    return () => clearTimeout(timer);
  }, [query, activeCategory, page]);

  const handleDelete = async (e, id, title) => {
    e.stopPropagation();
    if (window.confirm(`Are you sure you wish to remove "${title}" from the archive?`)) {
      try {
        await api.delete(`/books/${id}`);
        fetchBooks();
      } catch (err) {
        alert('Failed to remove volume.');
      }
    }
  };

  const handleRequestPublic = async (e, id) => {
    e.stopPropagation();
    try {
      await api.post(`/books/${id}/recommend`);
      alert('Request submitted to the High Curator.');
      fetchBooks();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to submit request.');
    }
  };

  const readingStatusMap = {
    'WantToRead': { label: 'Want to Read', color: 'text-ink/40 bg-ink/5' },
    'Reading': { label: 'Reading', color: 'text-sage bg-sage/10' },
    'Read': { label: 'Finished', color: 'text-ink bg-ink/10' },
  };

  const user = JSON.parse(localStorage.getItem('user') || '{}');
  const currentUserId = user.id;
  
  const displayedBooks = books;

  return (
    <div className="space-y-12 animate-in fade-in duration-700">
      {/* Header & Categories */}
      <section className="flex flex-col md:flex-row md:items-end justify-between gap-8 border-b border-ink/10 pb-12">
        <div className="space-y-6 w-full md:w-auto">
          <div className="space-y-2">
            <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-clay">Master Catalogue</span>
            <h1 className="text-4xl md:text-6xl font-serif text-ink tracking-tighter leading-none">The Archive</h1>
          </div>
          
          <nav className="flex flex-wrap gap-4 md:gap-8 border-b border-transparent">
            {[
              { id: 'all', label: 'Complete Index', icon: BookIcon },
              { id: 'public', label: 'Public Library', icon: Library },
              { id: 'personal', label: 'My Collection', icon: UserIcon }
            ].map(cat => (
              <button
                key={cat.id}
                onClick={() => setActiveCategory(cat.id)}
                className={`flex items-center gap-3 pb-4 transition-all relative ${
                  activeCategory === cat.id ? 'text-ink font-bold' : 'text-ink/30 hover:text-ink/60'
                }`}
              >
                <cat.icon className="w-4 h-4" />
                <span className="text-[10px] font-sans uppercase tracking-[0.2em]">{cat.label}</span>
                {activeCategory === cat.id && (
                  <motion.div layoutId="catTab" className="absolute bottom-0 left-0 right-0 h-0.5 bg-clay" />
                )}
              </button>
            ))}
          </nav>
        </div>

        <div className="flex flex-col sm:flex-row flex-wrap items-start sm:items-center gap-4 w-full md:w-auto">
          <div className="relative group w-full sm:w-auto">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink/30" />
            <input
              type="text"
              placeholder="Filter by title, author..."
              className="bg-paper-darker border border-ink/10 px-10 py-3 font-sans text-sm outline-none focus:border-clay/30 transition-all w-full sm:w-[300px]"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
          </div>
          <button 
            onClick={() => navigate('/books/add')}
            className="bg-ink text-paper px-6 py-3 flex items-center justify-center gap-3 text-[10px] font-sans uppercase tracking-[0.2em] hover:bg-clay hover:text-ink transition-all shadow-lg w-full sm:w-auto"
          >
            <Plus className="w-4 h-4" /> Register Volume
          </button>
        </div>
      </section>

      {/* Grid */}
      {loading ? (
        <div className="py-32 text-center">
          <div className="w-12 h-12 border-t-2 border-clay rounded-full animate-spin mx-auto mb-6"></div>
          <span className="font-serif italic text-ink/30 text-xl">Consulting the Grand Index...</span>
        </div>
      ) : (
        <div className="space-y-16">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-x-8 gap-y-16">
            <AnimatePresence>
              {displayedBooks.map((book) => (
                <motion.div
                  key={book.id}
                  layout
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.9 }}
                  className="group flex flex-col gap-6"
                >
                  {/* Book Cover */}
                  <div 
                    className="aspect-[2/3] bg-paper-darker border border-ink/10 relative overflow-hidden shadow-sm group-hover:shadow-2xl transition-all duration-500"
                  >
                    {book.coverImageUrl ? (
                      <img src={getAssetUrl(book.coverImageUrl)} alt="" className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" />
                    ) : (
                      <div className="w-full h-full flex flex-col items-center justify-center p-8 text-center gap-4 bg-ink/[0.02]">
                        <BookIcon className="w-12 h-12 text-ink/5" />
                        <span className="font-serif text-ink/20 italic text-sm line-clamp-2">{book.title}</span>
                      </div>
                    )}
                    
                    {/* Overlay Actions */}
                    <div className="absolute inset-0 bg-ink/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center justify-center gap-4">
                      {book.pdfUrl && (
                        <button 
                          onClick={() => setReaderFile({ url: getAssetUrl(book.pdfUrl), title: book.title })}
                          className="bg-paper text-ink px-6 py-2 flex items-center gap-3 text-[9px] font-sans uppercase tracking-[0.2em] font-bold hover:bg-clay transition-all"
                        >
                          <FileText className="w-4 h-4" /> Open Manuscript
                        </button>
                      )}
                      <button 
                        onClick={() => navigate(`/books/${book.id}`)}
                        className="text-paper/60 hover:text-paper text-[9px] font-sans uppercase tracking-[0.2em] border-b border-transparent hover:border-paper transition-all"
                      >
                        View Record
                      </button>
                    </div>

                    {/* Badges */}
                    <div className="absolute top-4 left-4 flex flex-col gap-2">
                      {book.visibility === 'Public' || book.visibility === 1 ? (
                        <div className="bg-clay text-paper p-1.5 shadow-lg" title="Public Volume">
                          <Library className="w-3 h-3" />
                        </div>
                      ) : (
                        <div className="bg-ink text-paper p-1.5 shadow-lg" title="Private Record">
                          <EyeOff className="w-3 h-3" />
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Book Info */}
                  <div className="space-y-3">
                    <div className="flex justify-between items-start">
                      <span className={`text-[9px] uppercase tracking-[0.2em] px-2 py-0.5 font-bold ${readingStatusMap[book.status]?.color || ''}`}>
                        {readingStatusMap[book.status]?.label}
                      </span>
                      {book.userId === currentUserId && (
                        <div className="flex gap-4 items-center">
                          {!book.isApproved && (
                            <button 
                              onClick={(e) => handleRequestPublic(e, book.id)}
                              disabled={book.moderationStatus === 'Pending' || book.moderationStatus === 1}
                              className={`text-[8px] font-sans uppercase tracking-widest px-2 py-1 border transition-all ${
                                (book.moderationStatus === 'Pending' || book.moderationStatus === 1)
                                  ? 'border-clay/20 text-clay/40 cursor-not-allowed'
                                  : 'border-clay/40 text-clay hover:bg-clay hover:text-paper'
                              }`}
                            >
                              {(book.moderationStatus === 'Pending' || book.moderationStatus === 1) ? 'Pending' : 'Request Public'}
                            </button>
                          )}
                          <button onClick={() => navigate(`/books/${book.id}/edit`)} className="text-ink/10 hover:text-clay transition-colors"><Edit3 className="w-4 h-4" /></button>
                          <button onClick={(e) => handleDelete(e, book.id, book.title)} className="text-ink/10 hover:text-red-800 transition-colors"><Trash2 className="w-4 h-4" /></button>
                        </div>
                      )}
                    </div>
                    
                    <h3 className="font-serif text-xl leading-tight group-hover:text-clay transition-colors line-clamp-2">{book.title}</h3>
                    <p className="font-sans text-[10px] uppercase tracking-[0.2em] text-ink/40 font-bold">{book.author}</p>
                  </div>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex justify-center items-center gap-8 border-t border-ink/5 pt-12">
              <button 
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                className="text-[10px] font-sans uppercase tracking-widest disabled:opacity-20 hover:text-clay transition-colors"
              >
                Previous
              </button>
              <div className="flex gap-4">
                {[...Array(totalPages)].map((_, i) => (
                  <button 
                    key={i}
                    onClick={() => setPage(i + 1)}
                    className={`text-[10px] font-sans w-6 h-6 flex items-center justify-center transition-all ${
                      page === i + 1 ? 'bg-ink text-paper font-bold' : 'text-ink/30 hover:text-ink'
                    }`}
                  >
                    {i + 1}
                  </button>
                ))}
              </div>
              <button 
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="text-[10px] font-sans uppercase tracking-widest disabled:opacity-20 hover:text-clay transition-colors"
              >
                Next Sector
              </button>
            </div>
          )}
        </div>
      )}

      {/* Empty State */}
      {displayedBooks.length === 0 && !loading && (
        <div className="py-32 text-center space-y-6 max-w-md mx-auto">
          <Library className="w-16 h-16 text-ink/5 mx-auto" />
          <h2 className="font-serif text-2xl text-ink/40 italic">No volumes found in this sector.</h2>
          <p className="font-sans text-[10px] text-ink/30 leading-relaxed uppercase tracking-widest">
            Adjust your filters or register a new manuscript to populate the shelves.
          </p>
        </div>
      )}

      {/* PDF Reader Modal */}
      <AnimatePresence>
        {readerFile && (
          <PdfReader 
            fileUrl={readerFile.url} 
            title={readerFile.title} 
            onClose={() => setReaderFile(null)} 
          />
        )}
      </AnimatePresence>
    </div>
  );
};


export default CollectionIndex;
