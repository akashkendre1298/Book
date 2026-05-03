import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Lock, Mail, UserPlus } from 'lucide-react';

const RegisterPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }
    setError('');
    setLoading(true);
    try {
      await register(email, password);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Please check your information.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center">
      <div className="grid md:grid-cols-2 max-w-5xl w-full bg-paper border border-ink/20 shadow-2xl overflow-hidden min-h-[600px]">
        {/* Left Side - Quotes/Branding */}
        <div className="hidden md:flex flex-col justify-between p-12 border-r border-ink/20 bg-paper-darker relative overflow-hidden">
          <div className="absolute top-0 right-0 w-32 h-full bg-gradient-to-l from-ink/5 to-transparent pointer-events-none" />
          
          <div className="relative z-10">
            <span className="font-sans text-[10px] uppercase tracking-[0.3em] text-ink/40">The Archive</span>
            <div className="mt-20">
              <p className="font-serif text-3xl italic text-ink leading-relaxed">
                “Every book is a world unto itself.”
              </p>
              <p className="font-sans text-xs uppercase tracking-widest text-ink/50 mt-4">— Unknown Collector</p>
            </div>
          </div>

          <div className="relative z-10 flex justify-between items-end">
            <span className="font-serif text-5xl text-ink/10 select-none">II / IV</span>
            <span className="font-sans text-[10px] uppercase tracking-widest text-ink/30">Private Library Editions</span>
          </div>
        </div>

        {/* Right Side - Form */}
        <div className="flex flex-col justify-center p-12 lg:p-20">
          <div className="mb-12">
            <h1 className="text-4xl font-serif text-ink tracking-tight text-center md:text-left">Join the Registry</h1>
            <p className="font-sans text-sm text-ink/60 mt-2 text-center md:text-left">Create your private collection credentials.</p>
          </div>

          {error && (
            <div className="bg-clay/10 border-l-2 border-clay p-4 mb-8 text-clay text-sm font-sans italic">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="flex flex-col space-y-2">
              <label className="font-sans text-[10px] uppercase tracking-widest text-ink/50 flex items-center gap-2">
                <Mail className="w-3 h-3" /> Electronic Mail
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="input-field text-lg"
                placeholder="reader@library.com"
                required
              />
            </div>

            <div className="flex flex-col space-y-2">
              <label className="font-sans text-[10px] uppercase tracking-widest text-ink/50 flex items-center gap-2">
                <Lock className="w-3 h-3" /> Create Access Key
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="input-field text-lg"
                placeholder="••••••••"
                required
              />
            </div>

            <div className="flex flex-col space-y-2">
              <label className="font-sans text-[10px] uppercase tracking-widest text-ink/50 flex items-center gap-2">
                <Lock className="w-3 h-3" /> Confirm Access Key
              </label>
              <input
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="input-field text-lg"
                placeholder="••••••••"
                required
              />
            </div>

            <div className="pt-6 flex flex-col gap-6">
              <button
                type="submit"
                disabled={loading}
                className="btn-primary w-full py-4 text-sm flex items-center justify-center gap-2"
              >
                <UserPlus className="w-4 h-4" />
                {loading ? 'Creating Identity...' : 'Initialize Account'}
              </button>
              
              <div className="text-center">
                <Link to="/login" className="font-sans text-[10px] uppercase tracking-widest text-ink/40 hover:text-ink transition-colors">
                  Already registered? Sign in here
                </Link>
              </div>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
