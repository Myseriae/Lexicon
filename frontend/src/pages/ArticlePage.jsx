import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getArticle, updateArticle, deleteArticle } from '../api/api';

const ArticlePage = () => {
  const { id } = useParams();

  const [article, setArticle] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({ title: '', content: '' });

  useEffect(() => {
    const fetchArticle = async () => {
      const data = await getArticle(id);
      setArticle(data);
      setFormData({ title: data.title, content: data.content });
    };

    fetchArticle();
  }, [id]);

  const handleUpdate = async () => {
    await updateArticle(id, formData);
    setArticle(formData);
    setEditMode(false);
  };

  const handleDelete = async () => {
    if (!window.confirm('Delete this article?')) return;
    await deleteArticle(id);
    window.location.href = '/';
  };

  if (!article) return <div style={{ color: '#fff' }}>Loading...</div>;

  return (
    <div style={{ padding: '2rem', color: '#fff', maxWidth: '800px', margin: '0 auto' }}>
      
      {!editMode ? (
        <>
          <h1>{article.title}</h1>
          <p style={{ color: '#ccc', marginTop: '1rem' }}>{article.content}</p>

          <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem' }}>
            <button onClick={() => setEditMode(true)} style={btnStyle}>Edit</button>
            <button onClick={handleDelete} style={deleteBtn}>Delete</button>
          </div>
        </>
      ) : (
        <>
          <input
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            style={inputStyle}
          />
          <textarea
            value={formData.content}
            onChange={(e) => setFormData({ ...formData, content: e.target.value })}
            style={textareaStyle}
          />

          <div style={{ marginTop: '1rem', display: 'flex', gap: '1rem' }}>
            <button onClick={handleUpdate} style={btnStyle}>Save</button>
            <button onClick={() => setEditMode(false)} style={btnStyle}>Cancel</button>
          </div>
        </>
      )}
    </div>
  );
};

const btnStyle = {
  padding: '0.5rem 1rem',
  backgroundColor: '#00cfff',
  border: 'none',
  color: '#000',
  borderRadius: '5px',
  cursor: 'pointer'
};

const deleteBtn = {
  ...btnStyle,
  backgroundColor: '#ff4d4d',
  color: '#fff'
};

const inputStyle = {
  width: '100%',
  padding: '0.5rem',
  marginBottom: '1rem',
  backgroundColor: '#222',
  border: '1px solid #444',
  color: '#fff'
};

const textareaStyle = {
  ...inputStyle,
  minHeight: '150px'
};

export default ArticlePage;