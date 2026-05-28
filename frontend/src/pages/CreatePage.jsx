import React, { useState } from 'react';
import { createArticle, addTagToArticle } from '../api/api';
import { useNavigate } from 'react-router-dom';
import Modal from '../components/Modal/Modal';
import MDEditor from '@uiw/react-md-editor';
import '@uiw/react-md-editor/markdown-editor.css';
import './CreatePage.css';

const CreatePage = () => {
  const [formData, setFormData] = useState({
    title: '',
    summary: '',
    content: ''
  });
  const [tags, setTags] = useState([]);
  const [tagInput, setTagInput] = useState('');
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({ isOpen: false, message: '', type: '' });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setError(null);
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const created = await createArticle(formData);
      // attach tags (if any) via the tag endpoint
      if (created?.id && tags.length > 0) {
        for (const t of tags) {
          try {
            await addTagToArticle(created.id, t);
          } catch (err) {
            // non-fatal: continue attaching other tags
            console.error('Failed to add tag', t, err);
          }
        }
      }

      console.log('Form submitted successfully');
      setFormData({ title: '', summary: '', content: '' });
      setTags([]);
      setTagInput('');
      setModal({ isOpen: true, message: 'Article created successfully!', type: 'success' });
      // navigate to created article page if available
      if (created?.id) {
        navigate(`/article/${created.id}`);
      }
    } catch (err) {
      console.error('Error submitting form:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleAddTagLocal = () => {
    const name = (tagInput || '').trim();
    if (!name) return;
    // prevent duplicates (case-insensitive)
    if (tags.some(t => t.toLowerCase() === name.toLowerCase())) {
      setTagInput('');
      return;
    }
    setTags(prev => [...prev, name]);
    setTagInput('');
  };

  const handleTagKeyDown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddTagLocal();
    }
  };

  const handleRemoveTagLocal = (index) => {
    setTags(prev => prev.filter((_, i) => i !== index));
  };

  const closeModal = () => {
    setModal({ isOpen: false, message: '', type: '' });
  };

  return (
    <div className="create-container">
      <h1 className="create-title">Create New Article</h1>
      {error && <div className="create-error">Error: {error}</div>}
      <form onSubmit={handleSubmit} className="create-form">
        <div className="form-group">
          <label htmlFor="title" className="form-label">Title:</label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            className="form-input"
            required
            disabled={loading}
          />
        </div>
        <div className="form-group">
          <label htmlFor="summary" className="form-label">Summary</label>
          <textarea
            id="summary"
            value={formData.summary}
            onChange={(e) => {
              setError(null);
              setFormData(prev => ({ ...prev, summary: e.target.value }));
            }}
            className="form-textarea summary-textarea"
            placeholder="Leave empty — if a Wikipedia article matches the title, a summary will be generated automatically."
            rows={3}
            disabled={loading}
          />
        </div>
        <div className="form-group">
          <label htmlFor="tags" className="form-label">Tags</label>
          <div className="tag-input-row">
            <input
              id="tags"
              type="text"
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={handleTagKeyDown}
              className="form-input"
              placeholder="Add tag and press Enter or click Add"
              disabled={loading}
            />
            <button type="button" className="btn" onClick={handleAddTagLocal} disabled={loading || !tagInput.trim()}>
              Add
            </button>
          </div>

          <div className="tag-list">
            {tags.map((t, i) => (
              <span key={i} className="tag-chip">
                {t}
                <button type="button" className="tag-remove" onClick={() => handleRemoveTagLocal(i)} aria-label={`Remove ${t}`}>
                  ×
                </button>
              </span>
            ))}
          </div>
        </div>
        <div className="form-group">
          <label htmlFor="content" className="form-label">Content:</label>
          <MDEditor
            id="content"
            value={formData.content}
            onChange={(value) => {
              setError(null);
              setFormData(prev => ({ ...prev, content: value || '' }));
            }}
            className="form-textarea"
            height={480}
            data-color-mode="dark"
          />
        </div>
        <button type="submit" disabled={loading} className="submit-btn">
          {loading ? 'Submitting...' : 'Submit'}
        </button>
      </form>
      <Modal
        isOpen={modal.isOpen}
        onClose={closeModal}
        message={modal.message}
        type={modal.type}
      />
    </div>
  );
};

export default CreatePage;
