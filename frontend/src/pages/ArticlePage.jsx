import React, { useEffect, useState } from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import { getArticle, updateArticle, deleteArticle } from '../api/api';
import Modal from '../components/Modal/Modal';
import './ArticlePage.css';

const ArticlePage = () => {
  const { id } = useParams();

  const [article, setArticle] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({ title: '', content: '', summary: '' });
  const [modal, setModal] = useState({ isOpen: false, message: '', type: '', onConfirm: null });
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchArticle = async () => {
      try {
        setError(null);
        const data = await getArticle(id);
        setArticle(data);
        setFormData({ title: data.title, content: data.content, summary: data.summary || '' });
      } catch (err) {
        setError(err.message);
      }
    };

    fetchArticle();
  }, [id]);


  const handleUpdate = async () => {
    try {
      setError(null);
      setSaving(true);
      const updated = await updateArticle(id, formData);
      // Merge the response from backend with local formData to ensure fields aren't lost
      // if the backend response is incomplete but successful.
      setArticle(prev => ({ ...prev, ...formData, ...(updated || {}) }));
      setEditMode(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = () => {
    setModal({
      isOpen: true,
      message: 'Delete this article?',
      type: 'confirm',
      onConfirm: async () => {
        try {
          setSaving(true); // Reuse saving state to disable actions
          await deleteArticle(id);
          navigate('/');
        } catch (err) {
          setError(err.message);
          setSaving(false);
        }
      }
    });
  };

  if (!article && !error) return <div className="loading">Loading...</div>;

  return (
    <div className="article-container">
      {error && <div className="error">Error: {error}</div>}
      
      {article && !editMode ? (
        <>
          <h1>{article.title}</h1>
          <p className={`article-summary ${!article.summary ? 'no-summary' : ''}`}><strong>Summary:</strong> {article.summary || "Create a summary for this article."}</p>
          <p className="article-content">{article.content}</p>

          <div className="article-actions">
            <button onClick={() => setEditMode(true)} className="btn" disabled={saving}>Edit</button>
            <button onClick={handleDelete} className="btn btn-delete" disabled={saving}>Delete</button>
          </div>
        </>
      ) : article && (
        <>
          <input
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            className="input"
            placeholder="Title"
            disabled={saving}
          />
          <textarea
            value={formData.summary}
            onChange={(e) => setFormData({ ...formData, summary: e.target.value })}
            className="textarea summary-textarea"
            placeholder="Summary"
            disabled={saving}
          />
          <textarea
            value={formData.content}
            onChange={(e) => setFormData({ ...formData, content: e.target.value })}
            className="textarea"
            placeholder="Content"
            disabled={saving}
          />

          <div className="article-actions">
            <button onClick={handleUpdate} disabled={saving} className="btn">
              {saving ? 'Saving...' : 'Save'}
            </button>
            <button onClick={() => setEditMode(false)} className="btn" disabled={saving}>Cancel</button>
          </div>
        </>
      )}

      {modal.isOpen && (
        <Modal
            isOpen={modal.isOpen}
          message={modal.message}
          type={modal.type}
          onConfirm={modal.onConfirm}
          onClose={() => setModal(prev => ({ ...prev, isOpen: false }))}
        />
      )}
    </div>
  );
};


export default ArticlePage;