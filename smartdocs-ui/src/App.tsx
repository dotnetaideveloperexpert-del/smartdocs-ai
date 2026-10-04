import { BrowserRouter, Routes, Route, Link, useNavigate } from 'react-router-dom';
import Login from './pages/Login';
import Documents from './pages/Documents';
import Chat from './pages/Chat';

function Nav() {
  const navigate = useNavigate();
  const token = localStorage.getItem('token');

  const logout = () => {
    localStorage.removeItem('token');
    navigate('/');
  };

  return (
    <nav style={{
      padding: '12px 24px',
      borderBottom: '1px solid #dfe3ee',
      display: 'flex',
      alignItems: 'center',
      gap: 24,
      background: '#fff',
      boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
    }}>
      <strong style={{ color: '#3b5bdb', fontSize: 16 }}>SmartDocs AI</strong>
      <Link to="/" style={linkStyle}>Login</Link>
      {token && <Link to="/documents" style={linkStyle}>Documents</Link>}
      {token && <Link to="/chat" style={linkStyle}>Chat</Link>}
      <div style={{ flex: 1 }} />
      {token && (
        <button
          onClick={logout}
          style={{
            background: 'none',
            border: '1px solid #dfe3ee',
            padding: '6px 12px',
            borderRadius: 6,
            cursor: 'pointer',
            color: '#5a6480',
            fontSize: 13,
          }}
        >
          Logout
        </button>
      )}
    </nav>
  );
}

const linkStyle: React.CSSProperties = {
  color: '#1b2236',
  textDecoration: 'none',
  fontSize: 14,
  fontWeight: 500,
};

export default function App() {
  return (
    <BrowserRouter>
      <Nav />
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/documents" element={<Documents />} />
        <Route path="/chat" element={<Chat />} />
      </Routes>
    </BrowserRouter>
  );
}