import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { Shield, Users, BookOpen, Activity, UserPlus, Search } from 'lucide-react';

const AdminDashboard = () => {
  const [stats, setStats] = useState(null);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const [statsRes, usersRes] = await Promise.all([
        api.get('/admin/stats'),
        api.get('/admin/users')
      ]);
      setStats(statsRes.data);
      setUsers(usersRes.data);
    } catch (error) {
      console.error('Error fetching admin data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePromote = async (userId) => {
    if (window.confirm('Are you sure you want to promote this archivist to High Curator status?')) {
      try {
        await api.post(`/admin/users/${userId}/promote`);
        fetchData();
      } catch (error) {
        console.error('Promotion failed:', error);
      }
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col justify-center items-center h-[60vh] gap-6">
        <div className="w-12 h-12 border-t-2 border-ink rounded-full animate-spin"></div>
        <span className="font-serif italic text-ink/30 text-xl">Accessing Master Records...</span>
      </div>
    );
  }

  const filteredUsers = users.filter(u => 
    u.email.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="space-y-16 animate-in fade-in duration-700">
      {/* Curator Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-end gap-8 pb-12 border-b border-ink/10">
        <div>
          <span className="font-sans text-[10px] uppercase tracking-[0.4em] text-clay mb-4 block">System Oversight Portal</span>
          <h1 className="text-6xl mb-6">Curator Overview</h1>
          <p className="font-serif italic text-ink-muted text-xl max-w-2xl leading-relaxed">
            Maintaining the integrity of the grand archive. Currently overseeing {stats?.totalUsers} archivists and {stats?.totalBooks} volumes.
          </p>
        </div>
        <div className="bg-ink text-paper p-6 flex items-center gap-6 shadow-2xl">
          <Shield className="w-8 h-8 text-clay" />
          <div>
            <span className="block text-[8px] font-sans uppercase tracking-[0.3em] opacity-50">System Status</span>
            <span className="text-sm font-sans font-bold tracking-widest">{stats?.systemHealth} • Online</span>
          </div>
        </div>
      </header>

      {/* Global Metrics */}
      <section className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {[
          { label: 'Registered Archivists', value: stats?.totalUsers, icon: Users },
          { label: 'Global Volumes', value: stats?.totalBooks, icon: BookOpen },
          { label: 'Genre Diversity', value: Object.keys(stats?.genreDistribution || {}).length, icon: Activity },
          { label: 'Storage Status', value: 'Optimal', icon: Shield }
        ].map((item, idx) => (
          <div key={idx} className="bg-paper-darker p-8 border border-ink/5 hover:border-clay/20 transition-colors group">
            <item.icon className="w-5 h-5 text-ink/10 group-hover:text-clay/40 transition-colors mb-4" />
            <span className="block text-4xl font-serif text-ink mb-1">{item.value}</span>
            <span className="block text-[9px] font-sans uppercase tracking-widest text-ink/40 font-bold">{item.label}</span>
          </div>
        ))}
      </section>

      {/* Archivist Registry */}
      <section className="space-y-8">
        <div className="flex flex-col md:flex-row justify-between items-baseline gap-8">
          <h3 className="text-xs font-sans font-bold uppercase tracking-[0.4em] text-ink/30">Archivist Registry</h3>
          <div className="relative w-full md:w-96">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-ink/20" />
            <input 
              type="text" 
              placeholder="Search by Email..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full bg-paper-darker border border-ink/10 pl-12 pr-4 py-3 text-xs font-sans focus:border-clay/30 outline-none transition-all"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead>
              <tr className="border-b border-ink/5 text-left text-[10px] font-sans uppercase tracking-widest text-ink/40">
                <th className="pb-4 font-bold">Archivist</th>
                <th className="pb-4 font-bold">Role</th>
                <th className="pb-4 font-bold">Joined the Circle</th>
                <th className="pb-4 font-bold text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="font-serif">
              {filteredUsers.map((u) => (
                <tr key={u.id} className="border-b border-ink/5 group hover:bg-paper-darker transition-colors">
                  <td className="py-6 text-lg">{u.email}</td>
                  <td className="py-6">
                    <span className={`text-[10px] font-sans uppercase tracking-widest px-2 py-1 ${u.role === 'Admin' ? 'bg-clay text-paper' : 'bg-ink/5 text-ink/40'}`}>
                      {u.role}
                    </span>
                  </td>
                  <td className="py-6 text-ink/60 italic">
                    {new Date(u.createdAt).toLocaleDateString('en-GB', { day: 'numeric', month: 'long', year: 'numeric' })}
                  </td>
                  <td className="py-6 text-right">
                    {u.role !== 'Admin' && (
                      <button 
                        onClick={() => handlePromote(u.id)}
                        className="flex items-center gap-2 text-clay hover:text-ink transition-colors ml-auto group"
                      >
                        <UserPlus className="w-4 h-4 group-hover:scale-110 transition-transform" />
                        <span className="text-[10px] font-sans uppercase tracking-widest font-bold">Promote</span>
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {filteredUsers.length === 0 && (
            <div className="py-20 text-center text-ink/20 font-serif italic">No archivists found matching your query.</div>
          )}
        </div>
      </section>
    </div>
  );
};

export default AdminDashboard;
