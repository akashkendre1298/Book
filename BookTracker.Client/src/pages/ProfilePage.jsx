import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { motion } from 'framer-motion';
import { Shield, Mail, Calendar, LogOut, BookOpen, Award, Hash } from 'lucide-react';

const ProfilePage = () => {
  const [profile, setProfile] = useState(null);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [profileRes, statsRes] = await Promise.all([
          api.get('/auth/me'),
          api.get('/dashboard/stats')
        ]);
        setProfile(profileRes.data);
        setStats(statsRes.data);
      } catch (error) {
        console.error('Error fetching profile data:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('token');
    window.location.href = '/login';
  };

  if (loading) {
    return (
      <div className="flex flex-col justify-center items-center h-[60vh] gap-6">
        <div className="w-12 h-12 border-t-2 border-clay rounded-full animate-spin"></div>
        <span className="font-serif italic text-ink/30 text-xl">Consulting the Grand Index...</span>
      </div>
    );
  }

  const initial = profile?.email?.charAt(0).toUpperCase() || 'A';
  const joinedDate = new Date(profile?.createdAt).toLocaleDateString('en-US', { 
    month: 'long', 
    year: 'numeric' 
  });

  return (
    <div className="max-w-6xl mx-auto px-6 py-12">
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-12">
        {/* Left Column: Identity Card */}
        <div className="lg:col-span-4 space-y-8">
          <div className="bg-paper-darker border border-ink/10 p-10 relative overflow-hidden group shadow-sm">
            <div className="absolute top-0 right-0 w-32 h-32 bg-clay/5 -mr-16 -mt-16 rounded-full blur-3xl transition-all group-hover:bg-clay/10" />
            
            <div className="relative z-10 flex flex-col items-center text-center">
              <div className="w-24 h-24 rounded-full border border-clay/30 flex items-center justify-center mb-8 bg-paper shadow-inner group-hover:border-clay transition-colors duration-500">
                <span className="text-5xl font-serif text-clay select-none">{initial}</span>
              </div>
              
              <h2 className="text-2xl font-serif text-ink mb-1 tracking-tight">
                {profile?.email.split('@')[0]}
              </h2>
              <p className="text-[10px] font-sans uppercase tracking-[0.3em] text-ink/40 mb-8">
                Official Archivist
              </p>

              <div className="w-full space-y-4 border-t border-ink/5 pt-8">
                <div className="flex items-center gap-4 text-ink/60">
                  <Mail className="w-4 h-4 text-clay/60" />
                  <span className="text-xs font-sans truncate">{profile?.email}</span>
                </div>
                <div className="flex items-center gap-4 text-ink/60">
                  <Calendar className="w-4 h-4 text-clay/60" />
                  <span className="text-xs font-sans">Member since {joinedDate}</span>
                </div>
                <div className="flex items-center gap-4 text-ink/60">
                  <Hash className="w-4 h-4 text-clay/60" />
                  <span className="text-xs font-sans tracking-widest uppercase">ID: {profile?.id.substring(0, 8)}</span>
                </div>
              </div>
            </div>
          </div>

          <button 
            onClick={handleLogout}
            className="w-full group flex items-center justify-between p-6 bg-clay/5 hover:bg-clay/10 border border-clay/10 transition-all duration-300"
          >
            <div className="flex items-center gap-4">
              <LogOut className="w-5 h-5 text-clay group-hover:translate-x-1 transition-transform" />
              <span className="text-[11px] font-sans uppercase tracking-[0.2em] font-bold text-clay">Terminate Session</span>
            </div>
            <Shield className="w-4 h-4 text-clay/30" />
          </button>
        </div>

        {/* Right Column: Dossier Details */}
        <div className="lg:col-span-8 space-y-12">
          {/* Stats Section */}
          <section className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[
              { label: 'Total Volumes', value: stats?.totalBooks || 0, icon: BookOpen },
              { label: 'Finished Books', value: stats?.readBooks || 0, icon: Award },
              { label: 'Pages Consumed', value: stats?.totalPagesRead || 0, icon: Hash }
            ].map((stat, idx) => (
              <motion.div 
                key={stat.label}
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.2 + (idx * 0.1) }}
                className="bg-paper p-8 border border-ink/5 flex flex-col items-center justify-center gap-4 hover:border-clay/20 transition-colors group"
              >
                <stat.icon className="w-5 h-5 text-ink/10 group-hover:text-clay/40 transition-colors" />
                <span className="text-4xl font-serif text-ink tracking-tighter">{stat.value}</span>
                <span className="text-[9px] font-sans uppercase tracking-[0.2em] text-ink/40 font-bold">{stat.label}</span>
              </motion.div>
            ))}
          </section>

          {/* Dossier Content */}
          <div className="space-y-12 pt-4">
            <section>
              <h3 className="text-xs font-sans font-bold uppercase tracking-[0.4em] text-ink/30 mb-10 pb-4 border-b border-ink/10 flex items-center justify-between">
                Curatorial Standing
                <span className="text-[10px] font-serif italic normal-case text-clay/60">Verified Member</span>
              </h3>
              
              <div className="prose prose-ink max-w-none">
                <p className="font-serif text-xl italic text-ink/70 leading-relaxed mb-8">
                  "As a verified custodian of the Athenaeum, your role involves the preservation and cataloging of literary artifacts. Your contributions ensure the continuity of the private archive's intellectual heritage."
                </p>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-x-16 gap-y-10 font-sans">
                  <div>
                    <h4 className="text-[10px] uppercase tracking-widest text-ink/40 mb-3 font-bold">Access Level</h4>
                    <p className="text-sm border-l-2 border-clay pl-4 py-1 italic">Level IV: Master Archivist</p>
                  </div>
                  <div>
                    <h4 className="text-[10px] uppercase tracking-widest text-ink/40 mb-3 font-bold">Archive Integrity</h4>
                    <p className="text-sm border-l-2 border-sage pl-4 py-1 italic">Optimal (100% Sync)</p>
                  </div>
                  <div>
                    <h4 className="text-[10px] uppercase tracking-widest text-ink/40 mb-3 font-bold">Collection Scope</h4>
                    <p className="text-sm border-l-2 border-clay pl-4 py-1 italic">Private & Encrypted</p>
                  </div>
                  <div>
                    <h4 className="text-[10px] uppercase tracking-widest text-ink/40 mb-3 font-bold">Digital Signature</h4>
                    <p className="text-sm border-l-2 border-ink/20 pl-4 py-1 italic">RSA-4096 Enabled</p>
                  </div>
                </div>
              </div>
            </section>

            <section className="bg-ink/5 p-12 text-center relative group overflow-hidden">
              <div className="absolute inset-0 bg-paper/50 opacity-0 group-hover:opacity-100 transition-opacity duration-700 blur-xl" />
              <div className="relative z-10">
                <h3 className="font-serif text-2xl italic text-ink/40 mb-6">"Knowledge is a treasure, but practice is the key to it."</h3>
                <div className="w-12 h-px bg-clay/30 mx-auto mb-6" />
                <p className="text-[10px] font-sans uppercase tracking-[0.3em] text-ink/30">Thomas Fuller</p>
              </div>
            </section>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
