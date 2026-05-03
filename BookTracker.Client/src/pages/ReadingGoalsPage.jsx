import React, { useState, useEffect } from 'react';
import api from '../api/axios';

const ReadingGoalsPage = () => {
  const [stats, setStats] = useState(null);
  const [goal, setGoal] = useState(12);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      const response = await api.get('/dashboard/stats');
      setStats(response.data);
      if (response.data.yearlyGoal) {
        setGoal(response.data.yearlyGoal);
      }
    } catch (error) {
      console.error('Error fetching stats:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateGoal = async () => {
    setSaving(true);
    try {
      await api.post('/dashboard/goal', { 
        targetYear: new Date().getFullYear(),
        goalCount: goal 
      });
      await fetchStats();
    } catch (error) {
      console.error('Error updating goal:', error);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="flex justify-center items-center h-screen font-serif italic text-ink-muted">Measuring the literary horizon...</div>;

  const currentYear = new Date().getFullYear();
  const progressPercent = stats ? Math.min(Math.round((stats.booksReadThisYear / goal) * 100), 100) : 0;

  return (
    <div className="max-w-4xl mx-auto px-6 py-12">
      <header className="mb-20 text-center">
        <span className="font-sans text-[10px] uppercase tracking-[0.3em] text-clay mb-4 block">Annual Objective</span>
        <h1 className="text-6xl mb-6">The {currentYear} Reading Goal</h1>
        <div className="w-24 h-px bg-ink/10 mx-auto"></div>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-12 items-center mb-20">
        <div className="text-center md:text-right">
          <span className="text-[10px] font-sans uppercase tracking-widest text-ink/40 block mb-2">Volumes Completed</span>
          <span className="text-5xl font-serif">{stats?.booksReadThisYear || 0}</span>
        </div>
        
        <div className="relative h-48 w-48 mx-auto flex items-center justify-center">
          <svg className="w-full h-full -rotate-90">
            <circle
              cx="96" cy="96" r="88"
              fill="none"
              stroke="currentColor"
              strokeWidth="1"
              className="text-ink/5"
            />
            <circle
              cx="96" cy="96" r="88"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeDasharray={552}
              strokeDashoffset={552 - (552 * progressPercent) / 100}
              className="text-sage transition-all duration-1000 ease-out"
            />
          </svg>
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className="text-3xl font-serif">{progressPercent}%</span>
            <span className="text-[8px] font-sans uppercase tracking-widest text-ink/40">Fulfilled</span>
          </div>
        </div>

        <div className="text-center md:text-left">
          <span className="text-[10px] font-sans uppercase tracking-widest text-ink/40 block mb-2">Annual Target</span>
          <span className="text-5xl font-serif">{goal}</span>
        </div>
      </div>

      <section className="bg-paper-darker border border-ink/5 p-12 max-w-2xl mx-auto">
        <h3 className="text-sm font-sans font-bold uppercase tracking-widest text-ink/60 mb-8 text-center">Update Your Ambition</h3>
        
        <div className="flex flex-col gap-8">
          <div className="flex items-center justify-between gap-12">
            <input 
              type="range" 
              min="1" 
              max="100" 
              value={goal} 
              onChange={(e) => setGoal(parseInt(e.target.value))}
              className="flex-grow accent-sage"
            />
            <span className="text-3xl font-serif w-12 text-right">{goal}</span>
          </div>
          
          <button 
            onClick={handleUpdateGoal}
            disabled={saving}
            className="btn-primary w-full py-4"
          >
            {saving ? 'Recording...' : 'Commit to Goal'}
          </button>
        </div>
        
        <p className="mt-8 text-center text-xs font-serif italic text-ink-muted">
          "A room without books is like a body without a soul." — Cicero
        </p>
      </section>
    </div>
  );
};

export default ReadingGoalsPage;
