import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getArticle, updateArticle, deleteArticle } from '../api/api';
import './ArticlePage.css';

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
            <button onClick={handleUpdate} className="btn">Save</button>
            <button onClick={() => setEditMode(false)} className="btn">Cancel</button>
          </div>
        </>
      )}
    </div>
  );
};


export default ArticlePage;