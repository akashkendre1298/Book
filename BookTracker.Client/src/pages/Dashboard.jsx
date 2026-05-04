import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Book as BookIcon, ChevronRight, Star, Edit3, Download } from 'lucide-react';
import api, { getAssetUrl } from '../api/axios';

const Dashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState(null);
  const [currentlyReading, setCurrentlyReading] = useState([]);
  const [toRead, setToRead] = useState([]);
  const [recentlyFinished, setRecentlyFinished] = useState([]);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  const handleExport = async () => {
    try {
      setExporting(true);
      const response = await api.get('/books/export', { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `athenaeum_export_${new Date().toISOString().split('T')[0]}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (error) {
      console.error('Export failed:', error);
    } finally {
      setExporting(false);
    }
  };

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const [statsRes, readingRes, toReadRes, finishedRes] = await Promise.all([
          api.get('/dashboard/stats'),
          api.get('/books?status=1'),
          api.get('/books?status=0'),
          api.get('/books?status=2&sortBy=dateadded')
        ]);

        setStats(statsRes.data);
        setCurrentlyReading(readingRes.data.slice(0, 1)); // Featured current book
        setToRead(toReadRes.data.slice(0, 2)); // Next 2 volumes
        setRecentlyFinished(finishedRes.data.slice(0, 3)); // Last 3 cataloged
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (loading) return <div className="flex justify-center items-center h-screen font-serif italic text-ink-muted text-2xl">Consulting the archives...</div>;

  const featuredBook = currentlyReading[0];

  return (
    <div className="max-w-6xl mx-auto px-6 py-12">
      <header className="mb-20 flex flex-col md:flex-row justify-between items-start md:items-end gap-8">
        <div>
          <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-clay mb-4 block">Curated Collection • Spring 2024</span>
          <h1 className="text-6xl mb-6">Greetings, Collector.</h1>
          <p className="font-serif italic text-ink-muted text-xl max-w-2xl leading-relaxed">
            The morning light hits the shelves. You have {stats?.booksReading || 0} volumes awaiting your attention this week.
          </p>
        </div>
        <button
          onClick={handleExport}
          disabled={exporting}
          className="group flex items-center gap-3 px-6 py-3 bg-paper-darker border border-ink/10 hover:border-clay/30 transition-all disabled:opacity-50"
        >
          <Download className={`w-4 h-4 ${exporting ? 'animate-pulse' : 'group-hover:text-clay'} transition-colors`} />
          <span className="text-[10px] font-sans uppercase tracking-[0.2em] font-bold text-ink/60 group-hover:text-ink">
            {exporting ? 'Preparing Archive...' : 'Download Archive'}
          </span>
        </button>
      </header>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-16">
        {/* Featured Book Section */}
        <section className="lg:col-span-7">
          <h3 className="text-[10px] font-sans font-bold uppercase tracking-widest text-ink/30 mb-8 pb-2 border-b border-ink/5">Currently Studying</h3>
          {featuredBook ? (
            <div className="group">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-center">
                <div
                  onClick={() => navigate(`/books/${featuredBook.id}`)}
                  className="aspect-[2/3] bg-paper-darker border border-ink/10 shadow-2xl transition-transform duration-700 group-hover:scale-[1.02] cursor-pointer"
                >
                  {featuredBook.coverImageUrl ? (
                    <img src={getAssetUrl(featuredBook.coverImageUrl)} alt={featuredBook.title} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex flex-col items-center justify-center p-8 text-center gap-4">
                      <BookIcon className="w-12 h-12 text-ink/5" />
                      <span className="font-serif text-ink/20 italic text-sm">{featuredBook.title}</span>
                    </div>
                  )}
                </div>
                <div className="space-y-6">
                  <h2
                    onClick={() => navigate(`/books/${featuredBook.id}`)}
                    className="text-4xl leading-tight group-hover:text-clay transition-colors cursor-pointer"
                  >
                    {featuredBook.title}
                  </h2>
                  <p className="font-sans text-xs uppercase tracking-widest text-ink/50 italic">by {featuredBook.author}</p>
                  <p className="font-serif italic text-ink-muted leading-relaxed line-clamp-4">
                    {featuredBook.review || "A profound exploration awaiting your further impressions in the private journal."}
                  </p>
                  <div className="pt-4 flex items-center gap-6">
                    <Link 
                      to={`/books/${featuredBook.id}`} 
                      className="flex items-center gap-2 text-clay hover:text-ink transition-colors"
                    >
                      <span className="text-sm font-sans uppercase tracking-widest font-bold">Continue Reading</span>
                      <ChevronRight className="w-4 h-4" />
                    </Link>
                    <Link 
                      to={`/books/${featuredBook.id}/edit`} 
                      className="text-ink/20 hover:text-clay transition-colors"
                    >
                      <Edit3 className="w-4 h-4" />
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <div className="py-20 text-center bg-paper-darker border border-dashed border-ink/10">
              <p className="font-serif italic text-ink/30">No active studies currently cataloged.</p>
              <Link to="/collection" className="text-[10px] font-sans uppercase tracking-[0.2em] text-clay mt-4 inline-block hover:underline">Browse the Index</Link>
            </div>
          )}

          <div className="mt-20 p-12 bg-paper-darker border border-ink/5 relative overflow-hidden">
            <div className="relative z-10 text-center">
              <span className="text-[40px] font-serif italic text-ink/10 block mb-4">“</span>
              <p className="text-2xl font-serif italic text-ink/80 mb-6">
                A room without books is like a body without a soul.
              </p>
              <span className="text-[10px] font-sans uppercase tracking-widest text-ink/40">— Marcus Tullius Cicero</span>
            </div>
          </div>
        </section>

        {/* Sidebar Sections */}
        <aside className="lg:col-span-5 space-y-20">
          <section>
            <div className="flex justify-between items-baseline mb-8 pb-2 border-b border-ink/5">
              <h3 className="text-[10px] font-sans font-bold uppercase tracking-widest text-ink/30">To Read Next</h3>
              <Link to="/collection" className="text-[10px] font-sans uppercase tracking-widest text-clay hover:underline">View Index</Link>
            </div>
            <ul className="space-y-8">
              {toRead.map((book) => (
                <li key={book.id} className="group">
                  <Link to={`/books/${book.id}`} className="flex gap-6 items-center">
                    <div className="w-16 aspect-[2/3] bg-paper-darker border border-ink/10 flex-shrink-0">
                      {book.coverImageUrl && <img src={getAssetUrl(book.coverImageUrl)} className="w-full h-full object-cover opacity-60 group-hover:opacity-100 transition-opacity" alt="" />}
                    </div>
                    <div>
                      <h4 className="text-lg leading-tight group-hover:text-clay transition-colors">{book.title}</h4>
                      <p className="text-[10px] font-sans uppercase tracking-widest text-ink/40 mt-1">{book.author}</p>
                    </div>
                  </Link>
                </li>
              ))}
              {toRead.length === 0 && <li className="font-serif italic text-ink/30 text-sm">The upcoming shelf is bare.</li>}
            </ul>
          </section>

          <section>
            <h3 className="text-[10px] font-sans font-bold uppercase tracking-widest text-ink/30 mb-8 pb-2 border-b border-ink/5">Recently Cataloged</h3>
            <ul className="space-y-6">
              {recentlyFinished.map((book) => (
                <li key={book.id} className="group">
                  <Link to={`/books/${book.id}`}>
                    <div className="flex justify-between items-start mb-1">
                      <h4 className="text-sm font-sans font-bold uppercase tracking-widest text-ink group-hover:text-clay transition-colors">{book.title}</h4>
                      <div className="flex text-clay scale-75">
                        {[...Array(book.rating || 0)].map((_, i) => <Star key={i} className="w-4 h-4 fill-current" />)}
                      </div>
                    </div>
                    <p className="font-serif italic text-xs text-ink-muted">
                      Finished {new Date(book.updatedAt).toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })}
                    </p>
                  </Link>
                </li>
              ))}
              {recentlyFinished.length === 0 && <li className="font-serif italic text-ink/30 text-sm">No recently finished volumes.</li>}
            </ul>
          </section>
        </aside>
      </div>
    </div>
  );
};

export default Dashboard;
