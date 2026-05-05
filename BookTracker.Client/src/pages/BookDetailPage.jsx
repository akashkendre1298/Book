import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api, { getAssetUrl } from '../api/axios';
import { useAuth } from '../context/AuthContext';

const BookDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [book, setBook] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({});
  const { user } = useAuth();
  
  const isOwner = user && book && (user.id === book.userId || user.role === 'Admin');

  useEffect(() => {
    fetchBook();
  }, [id]);

  const fetchBook = async () => {
    try {
      const response = await api.get(`/books/${id}`);
      setBook(response.data);
      setFormData(response.data);
    } catch (error) {
      console.error('Error fetching book:', error);
      navigate('/');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (status) => {
    try {
      const response = await api.patch(`/books/${id}/status`, { status });
      setBook(response.data);
    } catch (error) {
      console.error('Error updating status:', error);
      alert('Archive update failed. Check connection.');
    }
  };

  const handleUpdateProgress = async (e) => {
    const page = parseInt(e.target.value);
    if (isNaN(page)) return;
    try {
      const response = await api.patch(`/books/${id}/progress`, { currentPage: page });
      if (response.data) {
        setBook(response.data);
      }
    } catch (error) {
      console.error('Error updating progress:', error);
    }
  };

  const handleUpdateRating = async (rating) => {
    try {
      const response = await api.patch(`/books/${id}/rating`, { rating });
      setBook(response.data);
    } catch (error) {
      console.error('Error updating rating:', error);
    }
  };

  const handleRequestPublic = async () => {
    try {
      await api.post(`/books/${id}/recommend`);
      alert('Request submitted to the High Curator.');
      fetchBook();
    } catch (error) {
      console.error('Request failed:', error);
      alert(error.response?.data?.message || 'Failed to submit request.');
    }
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you wish to remove this volume from your collection?')) {
      try {
        await api.delete(`/books/${id}`);
        navigate('/');
      } catch (error) {
        console.error('Error deleting book:', error);
      }
    }
  };

  if (loading || !book) return <div className="flex justify-center items-center h-screen font-serif italic text-ink-muted">Retrieving the folio...</div>;

  const progressPercent = book.totalPages ? Math.round((book.currentPage / book.totalPages) * 100) : 0;

  const statusMap = {
    'WantToRead': { label: 'To Read', value: 'WantToRead' },
    'Reading': { label: 'Reading', value: 'Reading' },
    'Read': { label: 'Finished', value: 'Read' }
  };

  return (
    <div className="max-w-6xl mx-auto px-6 py-12">
      <button 
        onClick={() => navigate('/')}
        className="text-[10px] font-sans uppercase tracking-widest text-ink/40 hover:text-ink mb-12 flex items-center gap-2 transition-colors"
      >
        ← Back to Library
      </button>

      <div className="grid grid-cols-1 md:grid-cols-12 gap-16">
        {/* Book Cover Column */}
        <div className="md:col-span-4 lg:col-span-3">
          <div className="aspect-[2/3] bg-paper-darker shadow-xl relative group overflow-hidden border border-ink/5">
            {book.coverImageUrl ? (
              <img src={getAssetUrl(book.coverImageUrl)} alt={book.title} className="w-full h-full object-cover" />
            ) : (
              <div className="w-full h-full flex items-center justify-center p-8 text-center border-2 border-dashed border-ink/10">
                <span className="font-serif italic text-ink/20">Cover image not archived</span>
              </div>
            )}
            {isOwner && (
              <label className="absolute inset-0 bg-ink/60 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer">
                <span className="text-paper text-xs font-sans uppercase tracking-widest font-semibold">Upload New Cover</span>
                <input type="file" className="hidden" onChange={async (e) => {
                  const file = e.target.files[0];
                  if (!file) return;
                  const formData = new FormData();
                  formData.append('file', file);
                  try {
                    await api.post(`/books/${id}/cover`, formData, {
                      headers: { 'Content-Type': 'multipart/form-data' }
                    });
                    fetchBook();
                  } catch (error) {
                    alert(error.response?.data?.message || 'Error uploading cover. Ensure file is an image.');
                  }
                }} />
              </label>
            )}
          </div>

          <div className="mt-8 space-y-6">
            <div>
              <span className="text-[10px] font-sans uppercase tracking-widest text-ink/40 block mb-2">Reading Status</span>
              <div className="flex flex-wrap gap-2">
                {Object.values(statusMap).map((s) => (
                  <button
                    key={s.value}
                    onClick={() => isOwner && handleUpdateStatus(s.value)}
                    disabled={!isOwner}
                    className={`px-3 py-1 text-[10px] font-sans uppercase tracking-wider border transition-all ${
                      book.status === s.value
                        ? 'bg-ink text-paper border-ink' 
                        : 'border-ink/20 text-ink/60 hover:border-ink'
                    } ${!isOwner ? 'cursor-default opacity-80' : 'cursor-pointer'}`}
                  >
                    {s.label}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <span className="text-[10px] font-sans uppercase tracking-widest text-ink/40 block mb-2">Personal Rating</span>
              <div className="flex gap-1">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    onClick={() => isOwner && handleUpdateRating(star)}
                    disabled={!isOwner}
                    className={`text-xl transition-colors ${
                      star <= (book.rating || 0) ? 'text-clay' : 'text-ink/10 hover:text-clay/40'
                    } ${!isOwner ? 'cursor-default' : 'cursor-pointer'}`}
                  >
                    ★
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Book Info Column */}
        <div className="md:col-span-8 lg:col-span-9">
          <header className="mb-12 border-b border-ink/10 pb-8 flex flex-col md:flex-row justify-between items-start gap-6">
            <div>
              <h1 className="text-4xl md:text-6xl mb-4 leading-tight">{book.title}</h1>
              <h2 className="text-2xl md:text-3xl font-serif italic text-ink-muted">by {book.author}</h2>
            </div>
            {isOwner && (
              <button 
                onClick={() => navigate(`/books/${id}/edit`)}
                className="px-6 py-3 border border-ink/10 text-[10px] font-sans uppercase tracking-[0.2em] hover:bg-ink hover:text-paper transition-all w-full md:w-auto"
              >
                Modify Record
              </button>
            )}
          </header>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-16">
            <div className="space-y-12">
              <section>
                <h3 className="text-xs font-sans font-bold uppercase tracking-widest text-ink/30 mb-6 pb-2 border-b border-ink/5">Bibliographic Data</h3>
                <dl className="grid grid-cols-2 gap-y-6 text-sm font-sans">
                  <div>
                    <dt className="text-ink/40 uppercase text-[10px] tracking-wider mb-1">Genre</dt>
                    <dd className="font-serif italic text-lg">{book.genre || 'Unclassified'}</dd>
                  </div>
                  <div>
                    <dt className="text-ink/40 uppercase text-[10px] tracking-wider mb-1">Publication</dt>
                    <dd className="font-serif text-lg">{book.publicationYear || 'N/A'}</dd>
                  </div>
                  <div className="col-span-2">
                    <dt className="text-ink/40 uppercase text-[10px] tracking-wider mb-1">ISBN</dt>
                    <dd className="font-serif tracking-widest">{book.isbn || '---'}</dd>
                  </div>
                </dl>
              </section>

              <section>
                <h3 className="text-xs font-sans font-bold uppercase tracking-widest text-ink/30 mb-6 pb-2 border-b border-ink/5">Reading Progress</h3>
                <div className="space-y-6">
                  <div className="flex justify-between items-baseline font-serif">
                    <div className="flex items-baseline gap-2">
                      <input 
                        type="number" 
                        defaultValue={book.currentPage}
                        onBlur={handleUpdateProgress}
                        readOnly={!isOwner || book.status !== 'Reading'}
                        className={`w-16 bg-transparent border-b border-ink/20 text-2xl outline-none focus:border-clay transition-colors ${(!isOwner || book.status !== 'Reading') ? 'cursor-default' : ''}`}
                        min="0"
                        max={book.totalPages || 9999}
                      />
                      <span className="text-ink/40">/ {book.totalPages || '???'} pages</span>
                    </div>
                    <span className="text-clay text-lg italic">{progressPercent}%</span>
                  </div>
                  <div className="h-1 bg-ink/5 w-full relative">
                    <div 
                      className="absolute top-0 left-0 h-full bg-sage transition-all duration-500"
                      style={{ width: `${progressPercent}%` }}
                    ></div>
                  </div>
                </div>
              </section>
            </div>

            <div className="space-y-12">
              <section>
                <h3 className="text-xs font-sans font-bold uppercase tracking-widest text-ink/30 mb-6 pb-2 border-b border-ink/5">Critical Review</h3>
                <textarea 
                  className="w-full bg-paper-darker border border-ink/5 p-6 font-serif italic text-ink-muted leading-relaxed min-h-[200px] outline-none focus:border-clay/20 transition-all"
                  placeholder="Record your impressions of this volume..."
                  value={book.review || ''}
                  onChange={(e) => isOwner && setBook({ ...book, review: e.target.value })}
                  onBlur={async () => {
                    if (!isOwner) return;
                    try {
                      await api.patch(`/books/${id}`, { review: book.review });
                    } catch (error) {
                      console.error('Error saving review:', error);
                    }
                  }}
                  readOnly={!isOwner}
                ></textarea>
              </section>

              <div className="flex flex-col sm:flex-row gap-4 sm:gap-8 pt-12 items-start sm:items-center">
                {!book.isApproved && (book.visibility === 'Private' || book.visibility === 0) && (
                  <button 
                    onClick={handleRequestPublic}
                    disabled={book.moderationStatus === 'Pending' || book.moderationStatus === 1}
                    className={`px-8 py-3 bg-clay text-paper text-[10px] font-sans uppercase tracking-[0.2em] hover:bg-clay-dark transition-all shadow-xl w-full sm:w-auto ${
                      (book.moderationStatus === 'Pending' || book.moderationStatus === 1) ? 'opacity-50 cursor-not-allowed' : ''
                    }`}
                  >
                    {(book.moderationStatus === 'Pending' || book.moderationStatus === 1) ? 'Pending Approval' : 'Request for Public Library'}
                  </button>
                )}
                <button 
                  onClick={handleDelete}
                  className="text-[10px] font-sans uppercase tracking-widest text-red-800/40 hover:text-red-800 transition-colors w-full sm:w-auto text-left sm:text-center"
                >
                  Remove from Library
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookDetailPage;
