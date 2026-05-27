import React, { useState } from 'react';
import { createArticle } from '../api/api';
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
      await createArticle(formData);
      console.log('Form submitted successfully');
      setFormData({
        title: '',
        summary: '',
        content: ''
      });
      setModal({ isOpen: true, message: 'Article created successfully!', type: 'success' });
    } catch (err) {
      console.error('Error submitting form:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
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
