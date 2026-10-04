import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const endpoint = mode === 'login' ? '/auth/login' : '/auth/register';
      const body = mode === 'login'
        ? { email, password }
        : { name: email.split('@')[0], email, password };

      const { data } = await api.post(endpoint, body);
      localStorage.setItem('token', data.token);
      navigate('/documents');
    } catch (err: any) {
      const msg = err.response?.data?.message
        || (mode === 'login' ? 'Invalid email or password' : 'Registration failed');
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      minHeight: '100vh',
      background: '#f4f6fb',
    }}>
      <div style={{
        background: '#fff',
        padding: 40,
        borderRadius: 12,
        boxShadow: '0 4px 20px rgba(0,0,0,0.1)',
        width: 400,
      }}>
        <h1 style={{ margin: '0 0 8px', fontSize: 24, color: '#1b2236' }}>
          SmartDocs AI
        </h1>
        <p style={{ margin: '0 0 24px', color: '#5a6480', fontSize: 14 }}>
          {mode === 'login' ? 'Sign in to your account' : 'Create a new account'}
        </p>

        <form onSubmit={submit}>
          <label style={{ display: 'block', marginBottom: 6, fontSize: 13, fontWeight: 600 }}>
            Email
          </label>
          <input
            type="email"
            value={email}
            onChange={e => setEmail(e.target.value)}
            required
            style={{
              width: '100%',
              padding: 10,
              border: '1px solid #dfe3ee',
              borderRadius: 6,
              marginBottom: 16,
              fontSize: 14,
              boxSizing: 'border-box',
            }}
          />

          <label style={{ display: 'block', marginBottom: 6, fontSize: 13, fontWeight: 600 }}>
            Password
          </label>
          <input
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            required
            minLength={6}
            style={{
              width: '100%',
              padding: 10,
              border: '1px solid #dfe3ee',
              borderRadius: 6,
              marginBottom: 16,
              fontSize: 14,
              boxSizing: 'border-box',
            }}
          />

          {error && (
            <div style={{
              padding: 10,
              background: '#ffe3e3',
              color: '#c92a2a',
              borderRadius: 6,
              marginBottom: 16,
              fontSize: 13,
            }}>
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            style={{
              width: '100%',
              padding: 12,
              background: loading ? '#9aa4c0' : '#3b5bdb',
              color: '#fff',
              border: 'none',
              borderRadius: 6,
              fontSize: 15,
              fontWeight: 600,
              cursor: loading ? 'not-allowed' : 'pointer',
            }}
          >
            {loading ? 'Please wait...' : (mode === 'login' ? 'Sign In' : 'Create Account')}
          </button>
        </form>

        <button
          onClick={() => { setMode(mode === 'login' ? 'register' : 'login'); setError(''); }}
          style={{
            width: '100%',
            marginTop: 16,
            background: 'none',
            border: 'none',
            color: '#3b5bdb',
            cursor: 'pointer',
            fontSize: 13,
          }}
        >
          {mode === 'login' ? "Don't have an account? Register" : 'Already have an account? Sign In'}
        </button>
      </div>
    </div>
  );
}