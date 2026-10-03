import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import Login from './pages/Login';
import Documents from './pages/Documents';
import Chat from './pages/Chat';

export default function App() {
  return (
    <BrowserRouter>
      <nav style={{
        padding: 16,
        borderBottom: '1px solid #ddd',
        display: 'flex',
        gap: 16,
        background: '#fff',
      }}>
        <Link to="/">Login</Link>
        <Link to="/documents">Documents</Link>
        <Link to="/chat">Chat</Link>
      </nav>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/documents" element={<Documents />} />
        <Route path="/chat" element={<Chat />} />
      </Routes>
    </BrowserRouter>
  );
}