import { useEffect, useState } from 'react';
import { api } from '../api/client';

interface Doc {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  status: string;
  createdAt: string;
}

export default function Documents() {
  const [docs, setDocs] = useState<Doc[]>([]);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      const { data } = await api.get('/documents');
      setDocs(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const upload = async (file: File) => {
    setError('');
    setUploading(true);
    const form = new FormData();
    form.append('file', file);

    try {
      await api.post('/documents', form);
      await load();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Upload failed');
    } finally {
      setUploading(false);
    }
  };

  const remove = async (id: string) => {
    if (!confirm('Delete this document?')) return;
    try {
      await api.delete(`/documents/${id}`);
      await load();
    } catch (err) {
      setError('Delete failed');
    }
  };

  const formatSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  return (
    <div style={{ padding: 40, maxWidth: 960, margin: 'auto' }}>
      <h1 style={{ marginBottom: 8 }}>My Documents</h1>
      <p style={{ color: '#5a6480', marginBottom: 24, fontSize: 14 }}>
        Upload PDF, DOCX, or TXT files (max 20 MB)
      </p>

      {/* Upload Area */}
      <div style={{
        border: '2px dashed #dfe3ee',
        borderRadius: 12,
        padding: 40,
        textAlign: 'center',
        background: '#f9fafb',
        marginBottom: 24,
      }}>
        <input
          type="file"
          accept=".pdf,.docx,.txt"
          onChange={e => e.target.files?.[0] && upload(e.target.files[0])}
          disabled={uploading}
          id="file-input"
          style={{ display: 'none' }}
        />
        <label
          htmlFor="file-input"
          style={{
            cursor: uploading ? 'wait' : 'pointer',
            color: '#3b5bdb',
            fontWeight: 600,
            fontSize: 15,
          }}
        >
          {uploading ? 'Uploading...' : '📁 Click to upload a file'}
        </label>
        <p style={{ color: '#9aa4c0', fontSize: 12, marginTop: 8 }}>
          or drag and drop
        </p>
      </div>

      {error && (
        <div style={{
          padding: 12,
          background: '#ffe3e3',
          color: '#c92a2a',
          borderRadius: 6,
          marginBottom: 16,
        }}>
          {error}
        </div>
      )}

      {/* Documents Table */}
      {loading ? (
        <p>Loading...</p>
      ) : docs.length === 0 ? (
        <div style={{ textAlign: 'center', padding: 40, color: '#9aa4c0' }}>
          No documents yet. Upload your first file!
        </div>
      ) : (
        <table style={{
          width: '100%',
          borderCollapse: 'collapse',
          background: '#fff',
          borderRadius: 8,
          overflow: 'hidden',
          boxShadow: '0 2px 8px rgba(0,0,0,0.05)',
        }}>
          <thead>
            <tr style={{ background: '#e6ebff' }}>
              <th style={th}>File Name</th>
              <th style={th}>Size</th>
              <th style={th}>Status</th>
              <th style={th}>Uploaded</th>
              <th style={th}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {docs.map(d => (
              <tr key={d.id} style={{ borderTop: '1px solid #dfe3ee' }}>
                <td style={td}>📄 {d.fileName}</td>
                <td style={td}>{formatSize(d.sizeBytes)}</td>
                <td style={td}>
                  <span style={{
                    padding: '2px 8px',
                    borderRadius: 12,
                    fontSize: 12,
                    background: d.status === 'Ready' ? '#d3f9d8' : '#fff3bf',
                    color: d.status === 'Ready' ? '#2f9e44' : '#e67700',
                  }}>
                    {d.status}
                  </span>
                </td>
                <td style={{ ...td, fontSize: 13, color: '#5a6480' }}>
                  {new Date(d.createdAt).toLocaleString()}
                </td>
                <td style={td}>
                  <button
                    onClick={() => remove(d.id)}
                    style={{
                      background: '#ffe3e3',
                      color: '#c92a2a',
                      border: 'none',
                      padding: '6px 12px',
                      borderRadius: 4,
                      cursor: 'pointer',
                      fontSize: 12,
                    }}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

const th: React.CSSProperties = {
  padding: 12,
  textAlign: 'left',
  fontWeight: 600,
  fontSize: 14,
  color: '#1b2236',
};

const td: React.CSSProperties = {
  padding: 12,
  fontSize: 14,
};