import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { 
  Shield, Users, BookOpen, Activity, UserPlus, Search, 
  UserMinus, Trash2, Eye, EyeOff, CheckCircle, XCircle,
  BarChart3, Settings2, Filter
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { getAssetUrl } from '../api/axios';

const AdminDashboard = () => {
  const [activeTab, setActiveTab] = useState('metrics');
  const [stats, setStats] = useState(null);
  const [users, setUsers] = useState([]);
  const [books, setBooks] = useState([]);
  const [recommendations, setRecommendations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterType, setFilterType] = useState('all');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [statsRes, usersRes, booksRes, recommendationsRes] = await Promise.all([
        api.get('/admin/stats'),
        api.get('/admin/users'),
        api.get('/admin/library'),
        api.get('/admin/recommendations')
      ]);
      setStats(statsRes.data);
      setUsers(usersRes.data);
      setBooks(booksRes.data);
      setRecommendations(recommendationsRes.data);
    } catch (error) {
      console.error('Error fetching admin data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async (userId, currentStatus) => {
    try {
      await api.put(`/admin/users/${userId}/status`, !currentStatus);
      fetchData();
    } catch (error) {
      console.error('Status update failed:', error);
    }
  };

  const handleDeleteUser = async (userId) => {
    if (window.confirm('IRREVERSIBLE ACTION: Purge this archivist from the records?')) {
      try {
        await api.delete(`/admin/users/${userId}`);
        fetchData();
      } catch (error) {
        console.error('Deletion failed:', error);
      }
    }
  };

  const handleApprove = async (id) => {
    try {
      await api.post(`/admin/recommendations/${id}/approve`);
      fetchData();
    } catch (error) {
      console.error('Approval failed:', error);
    }
  };

  const handleReject = async (id) => {
    try {
      await api.post(`/admin/recommendations/${id}/reject`);
      fetchData();
    } catch (error) {
      console.error('Rejection failed:', error);
    }
  };

  const handleDeleteBook = async (id) => {
    if (window.confirm('Permanently remove this volume from the archive?')) {
      try {
        await api.delete(`/admin/library/${id}`);
        fetchData();
      } catch (error) {
        console.error('Book deletion failed:', error);
      }
    }
  };

  const [editingBook, setEditingBook] = useState(null);
  const handleEditBook = (book) => {
    setEditingBook({ ...book });
  };

  const handleSaveBook = async () => {
    try {
      await api.patch(`/admin/library/${editingBook.id}`, editingBook);
      setEditingBook(null);
      fetchData();
    } catch (error) {
      console.error('Save failed:', error);
    }
  };

  if (loading && !stats) {
    return (
      <div className="flex flex-col justify-center items-center h-[60vh] gap-6">
        <div className="w-12 h-12 border-t-2 border-ink rounded-full animate-spin"></div>
        <span className="font-serif italic text-ink/30 text-xl text-center">Opening the Vault of Knowledge...</span>
      </div>
    );
  }

  const filteredUsers = users.filter(u => 
    u.email.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const filteredBooks = books.filter(b => 
    b.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    b.author.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="space-y-12 animate-in fade-in duration-700">
      {/* Curator Header */}
      <header className="flex flex-col lg:flex-row justify-between items-start lg:items-end gap-8 pb-10 border-b border-ink/10">
        <div>
          <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-clay mb-3 block">High Curator Jurisdiction</span>
          <h1 className="text-5xl md:text-6xl mb-4 leading-tight">Master Controls</h1>
          <nav className="flex gap-8 mt-6 overflow-x-auto pb-2 scrollbar-hide">
            {[
              { id: 'metrics', label: 'Dashboard', icon: BarChart3 },
              { id: 'users', label: 'Users', icon: Users },
              { id: 'library', label: 'Library', icon: BookOpen },
              { id: 'moderation', label: 'Moderation', icon: Activity, count: recommendations.length }
            ].map(tab => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-3 whitespace-nowrap pb-2 transition-all relative ${
                  activeTab === tab.id ? 'text-ink font-bold' : 'text-ink/40 hover:text-ink/60'
                }`}
              >
                <tab.icon className={`w-4 h-4 ${activeTab === tab.id ? 'text-clay' : ''}`} />
                <span className="text-[10px] font-sans uppercase tracking-[0.2em]">{tab.label}</span>
                {tab.count > 0 && (
                  <span className="bg-clay text-paper text-[8px] px-1.5 py-0.5 rounded-full">{tab.count}</span>
                )}
                {activeTab === tab.id && (
                  <motion.div 
                    layoutId="activeTab"
                    className="absolute bottom-0 left-0 right-0 h-0.5 bg-clay"
                  />
                )}
              </button>
            ))}
          </nav>
        </div>
        <div className="bg-ink text-paper px-8 py-5 flex items-center gap-6 shadow-xl rounded-sm">
          <Shield className="w-8 h-8 text-clay" />
          <div>
            <span className="block text-[8px] font-sans uppercase tracking-[0.3em] opacity-50">Archive Integrity</span>
            <span className="text-sm font-sans font-bold tracking-widest">Optimal Sync</span>
          </div>
        </div>
      </header>

      <main>
        <AnimatePresence mode="wait">
          {activeTab === 'metrics' && (
            <motion.div 
              key="metrics"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              className="space-y-12"
            >
              <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {[
                  { label: 'Total Users', value: stats?.totalUsers, icon: Users },
                  { label: 'Total Books', value: stats?.totalBooks, icon: BookOpen },
                  { label: 'Pending Approvals', value: stats?.pendingRecommendations, icon: Activity }
                ].map((item, idx) => (
                  <div key={idx} className="bg-paper-darker p-8 border border-ink/5 group">
                    <item.icon className="w-5 h-5 text-ink/10 group-hover:text-clay/40 transition-colors mb-4" />
                    <span className="block text-4xl font-serif text-ink mb-1">{item.value}</span>
                    <span className="block text-[9px] font-sans uppercase tracking-widest text-ink/40 font-bold">{item.label}</span>
                  </div>
                ))}
              </section>

              <div className="bg-paper-darker border border-ink/5 p-12 text-center">
                <p className="font-serif italic text-xl text-ink/60">
                  Select a category above to manage the library infrastructure.
                </p>
              </div>
            </motion.div>
          )}

          {activeTab === 'users' && (
            <motion.div 
              key="users"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="space-y-8"
            >
              <div className="flex flex-col md:flex-row justify-between items-baseline gap-6">
                <h3 className="text-[10px] font-sans uppercase tracking-[0.4em] text-ink/30 font-bold">User Management</h3>
                <div className="relative w-full md:w-96">
                  <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-ink/20" />
                  <input 
                    type="text" 
                    placeholder="Search users..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="w-full bg-paper-darker border border-ink/10 pl-12 pr-4 py-3 text-xs font-sans outline-none focus:border-clay/30 transition-all"
                  />
                </div>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b border-ink/5 text-left text-[9px] font-sans uppercase tracking-widest text-ink/40">
                      <th className="pb-4 font-bold">User</th>
                      <th className="pb-4 font-bold">Status</th>
                      <th className="pb-4 font-bold text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="font-serif">
                    {filteredUsers.map((u) => (
                      <tr key={u.id} className="border-b border-ink/5 group hover:bg-paper-darker transition-colors">
                        <td className="py-6">
                          <div className="flex flex-col">
                            <span className="text-lg">{u.email}</span>
                            <span className="text-[9px] font-sans uppercase tracking-widest opacity-30">Joined: {new Date(u.createdAt).toLocaleDateString()}</span>
                          </div>
                        </td>
                        <td className="py-6">
                          <span className={`text-[9px] font-sans uppercase tracking-widest px-2 py-1 ${u.isActive ? 'text-green-600 bg-green-50' : 'text-red-600 bg-red-50'}`}>
                            {u.isActive ? 'Active' : 'Blocked'}
                          </span>
                        </td>
                        <td className="py-6 text-right">
                          <div className="flex justify-end gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button 
                              onClick={() => handleToggleStatus(u.id, u.isActive)}
                              className={`p-2 transition-colors ${u.isActive ? 'text-red-400 hover:text-red-600' : 'text-green-400 hover:text-green-600'}`}
                              title={u.isActive ? "Block User" : "Unblock User"}
                            >
                              {u.isActive ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                            </button>
                            <button 
                              onClick={() => handleDeleteUser(u.id)}
                              className="p-2 text-ink/20 hover:text-red-600 transition-colors"
                              title="Delete User"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </motion.div>
          )}

          {activeTab === 'library' && (
            <motion.div 
              key="library"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="space-y-8"
            >
              <div className="flex flex-col md:flex-row justify-between items-baseline gap-6">
                <h3 className="text-[10px] font-sans uppercase tracking-[0.4em] text-ink/30 font-bold">Library Management</h3>
                <div className="relative w-full md:w-96">
                  <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-ink/20" />
                  <input 
                    type="text" 
                    placeholder="Filter books..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="w-full bg-paper-darker border border-ink/10 pl-12 pr-4 py-3 text-xs font-sans outline-none focus:border-clay/30 transition-all"
                  />
                </div>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b border-ink/5 text-left text-[9px] font-sans uppercase tracking-widest text-ink/40">
                      <th className="pb-4 font-bold">Book Details</th>
                      <th className="pb-4 font-bold">Owner</th>
                      <th className="pb-4 font-bold text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="font-serif">
                    {filteredBooks.map((b) => (
                      <tr key={b.id} className="border-b border-ink/5 group hover:bg-paper-darker transition-colors">
                        <td className="py-6">
                          <div className="flex flex-col">
                            <span className="text-lg">{b.title}</span>
                            <span className="text-[10px] font-sans uppercase tracking-[0.1em] text-ink/40">{b.author} • {b.genre}</span>
                          </div>
                        </td>
                        <td className="py-6 italic text-ink/40">{b.ownerEmail}</td>
                        <td className="py-6 text-right">
                          <div className="flex justify-end gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button 
                              onClick={() => handleEditBook(b)}
                              className="p-2 text-ink/20 hover:text-clay transition-colors"
                              title="Edit Details"
                            >
                              <Settings2 className="w-4 h-4" />
                            </button>
                            <button 
                              onClick={() => handleDeleteBook(b.id)}
                              className="p-2 text-ink/20 hover:text-red-600 transition-colors"
                              title="Delete Book"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Edit Modal */}
              <AnimatePresence>
                {editingBook && (
                  <div className="fixed inset-0 bg-ink/60 backdrop-blur-sm z-50 flex items-center justify-center p-6">
                    <motion.div 
                      initial={{ scale: 0.9, opacity: 0 }}
                      animate={{ scale: 1, opacity: 1 }}
                      exit={{ scale: 0.9, opacity: 0 }}
                      className="bg-paper p-10 max-w-lg w-full shadow-2xl border border-ink/10"
                    >
                      <h4 className="text-2xl mb-8">Edit Volume Record</h4>
                      <div className="space-y-6">
                        <div className="space-y-1">
                          <label className="text-[10px] font-sans uppercase tracking-widest text-ink/40">Title</label>
                          <input 
                            value={editingBook.title}
                            onChange={(e) => setEditingBook({...editingBook, title: e.target.value})}
                            className="w-full bg-paper-darker border border-ink/10 px-4 py-3 font-serif outline-none"
                          />
                        </div>
                        <div className="space-y-1">
                          <label className="text-[10px] font-sans uppercase tracking-widest text-ink/40">Author</label>
                          <input 
                            value={editingBook.author}
                            onChange={(e) => setEditingBook({...editingBook, author: e.target.value})}
                            className="w-full bg-paper-darker border border-ink/10 px-4 py-3 font-serif outline-none"
                          />
                        </div>
                        <div className="space-y-1">
                          <label className="text-[10px] font-sans uppercase tracking-widest text-ink/40">Genre</label>
                          <input 
                            value={editingBook.genre}
                            onChange={(e) => setEditingBook({...editingBook, genre: e.target.value})}
                            className="w-full bg-paper-darker border border-ink/10 px-4 py-3 font-serif outline-none"
                          />
                        </div>
                        <div className="space-y-1">
                          <label className="text-[10px] font-sans uppercase tracking-widest text-ink/40">Cover Image URL / Path</label>
                          <input 
                            value={editingBook.coverImageUrl || ''}
                            onChange={(e) => setEditingBook({...editingBook, coverImageUrl: e.target.value})}
                            className="w-full bg-paper-darker border border-ink/10 px-4 py-3 font-sans text-xs outline-none"
                            placeholder="/uploads/covers/..."
                          />
                        </div>
                        <div className="flex gap-4 pt-4">
                          <button 
                            onClick={handleSaveBook}
                            className="flex-1 bg-ink text-paper py-3 font-sans uppercase text-[10px] tracking-widest font-bold"
                          >
                            Save Changes
                          </button>
                          <button 
                            onClick={() => setEditingBook(null)}
                            className="flex-1 border border-ink/10 py-3 font-sans uppercase text-[10px] tracking-widest"
                          >
                            Cancel
                          </button>
                        </div>
                      </div>
                    </motion.div>
                  </div>
                )}
              </AnimatePresence>
            </motion.div>
          )}

          {activeTab === 'moderation' && (
            <motion.div 
              key="moderation"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="space-y-8"
            >
              <h3 className="text-[10px] font-sans uppercase tracking-[0.4em] text-ink/30 font-bold">Recommendation Moderation</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {recommendations.map((rec) => (
                  <div key={rec.id} className="bg-paper-darker border border-ink/10 p-6 flex gap-6 relative group overflow-hidden">
                    <div className="absolute top-0 right-0 p-2 bg-clay/10 text-[8px] font-sans uppercase tracking-widest text-clay font-bold">
                      Recommendation
                    </div>
                    <div className="w-20 h-28 bg-ink/5 flex-shrink-0 border border-ink/10">
                      {rec.coverImageUrl ? (
                        <img src={getAssetUrl(rec.coverImageUrl)} className="w-full h-full object-cover" alt="" />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center text-ink/10">
                          <BookOpen className="w-6 h-6" />
                        </div>
                      )}
                    </div>
                    <div className="flex flex-col justify-between flex-1">
                      <div>
                        <h4 className="text-xl font-serif mb-1">{rec.title}</h4>
                        <p className="text-[10px] font-sans uppercase tracking-widest text-ink/40 mb-2">{rec.author}</p>
                        <div className="text-[9px] font-serif italic text-ink/60">
                          From: <span className="text-ink font-bold not-italic">{rec.uploadedBy}</span>
                        </div>
                      </div>
                      <div className="flex gap-4 mt-4">
                        <button 
                          onClick={() => handleApprove(rec.id)}
                          className="flex-1 text-green-600 border border-green-600/20 py-2 hover:bg-green-600 hover:text-white transition-all text-[9px] font-sans uppercase tracking-widest font-bold"
                        >
                          Approve
                        </button>
                        <button 
                          onClick={() => handleReject(rec.id)}
                          className="flex-1 text-red-600 border border-red-600/20 py-2 hover:bg-red-600 hover:text-white transition-all text-[9px] font-sans uppercase tracking-widest font-bold"
                        >
                          Reject
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
                {recommendations.length === 0 && (
                  <div className="col-span-2 py-32 text-center border-2 border-dashed border-ink/5 flex flex-col items-center gap-4">
                    <Activity className="w-12 h-12 text-ink/5" />
                    <span className="font-serif italic text-ink/30 text-xl">Queue Clear</span>
                  </div>
                )}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </main>
    </div>
  );
};

export default AdminDashboard;
