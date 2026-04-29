import React, { useEffect, useState } from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import { getArticle, updateArticle, deleteArticle } from '../api/api';
import Modal from '../components/Modal/Modal';
import './ArticlePage.css';

const ArticlePage = () => {
  const { id } = useParams();

  const [article, setArticle] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({ title: '', content: '' });
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
        setFormData({ title: data.title, content: data.content });
      } catch (err) {
        setError(err.message);
      }
    };

    fetchArticle();
  }, [id]);


  const handleUpdate = async () => {
    try {
      setSaving(true);
      const updated = await updateArticle(id, formData);
      setArticle(updated);
      setEditMode(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setModal({
      isOpen: true,
      message: 'Delete this article?',
      type: 'confirm',
      onConfirm: async () => {
        try {
          await deleteArticle(id);
          navigate('/');
        } catch (err) {
          setError(err.message);
        }
      }
    });
  };

  if (error) return <div className="error">Error: {error}</div>;
  if (!article) return <div className="loading">Loading...</div>;

  return (
    <div className="article-container">

      {!editMode ? (
        <>
          <h1>{article.title}</h1>
          <p className="article-content">{article.content}</p>

          <div className="article-actions">
            <button onClick={() => setEditMode(true)} className="btn">Edit</button>
            <button onClick={handleDelete} className="btn btn-delete">Delete</button>
          </div>
        </>
      ) : (
        <>
          <input
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            className="input"
          />
          <textarea
            value={formData.content}
            onChange={(e) => setFormData({ ...formData, content: e.target.value })}
            className="textarea"
          />

          <div className="article-actions">
            <button onClick={handleUpdate} disabled={saving} className="btn">
              {saving ? 'Saving...' : 'Save'}
            </button>
            <button onClick={() => setEditMode(false)} className="btn">Cancel</button>
          </div>
        </>
      )}

      {modal.isOpen && (
        <Modal
          message={modal.message}
          type={modal.type}
          onConfirm={modal.onConfirm}
          onClose={() => setModal({ ...modal, isOpen: false })}
        />
      )}
    </div>
  );
};


export default ArticlePage;