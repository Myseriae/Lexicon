import React, { useState } from 'react';
import { createArticle } from '../api/api';

const CreatePage = () => {
  const [formData, setFormData] = useState({
    title: '',
    content: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleChange = (e) => {
    const { name, value } = e.target;
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
        content: ''
      });
      alert('Article created successfully!');
    } catch (err) {
      console.error('Error submitting form:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '2rem', color: '#fff' }}>
      <h1 style={{ color: '#fff' }}>Create New Article</h1>
      {error && <div style={{ color: '#ff4d4d', marginBottom: '1rem' }}>Error: {error}</div>}
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', maxWidth: '400px', gap: '1rem' }}>
        <div>
          <label htmlFor="title" style={{ display: 'block', marginBottom: '0.5rem' }}>Title:</label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            style={{ width: '100%', padding: '0.5rem', backgroundColor: '#222', border: '1px solid #444', color: '#fff', borderRadius: '4px' }}
            required
            disabled={loading}
          />
        </div>
        <div>
          <label htmlFor="content" style={{ display: 'block', marginBottom: '0.5rem' }}>Content:</label>
          <textarea
            id="content"
            name="content"
            value={formData.content}
            onChange={handleChange}
            style={{ width: '100%', padding: '0.5rem', minHeight: '100px', backgroundColor: '#222', border: '1px solid #444', color: '#fff', borderRadius: '4px' }}
            required
            disabled={loading}
          />
        </div>
        <button type="submit" disabled={loading} style={{ 
          padding: '0.5rem', 
          backgroundColor: loading ? '#6c757d' : '#28a745', 
          color: 'white', 
          border: 'none', 
          borderRadius: '4px', 
          cursor: loading ? 'not-allowed' : 'pointer' 
        }}>
          {loading ? 'Submitting...' : 'Submit'}
        </button>
      </form>
    </div>
  );
};

export default CreatePage;
