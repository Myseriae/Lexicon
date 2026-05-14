import React, { useEffect, useState } from 'react';
import {useNavigate, useParams} from 'react-router-dom';
import { getArticle, updateArticle, deleteArticle, getCollaborators } from '../api/api';
import { getCurrentUser } from '../utils/jwtUtils';
import Modal from '../components/Modal/Modal';
import Collaborators from '../components/Collaborators/Collaborators';
import './ArticlePage.css';

const ArticlePage = () => {
  const { id } = useParams();

  const [article, setArticle] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({ title: '', content: '', summary: '' });
  const [modal, setModal] = useState({ isOpen: false, message: '', type: '', onConfirm: null });
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [canEdit, setCanEdit] = useState(false);
  const [collaborators, setCollaborators] = useState([]);
  const navigate = useNavigate();

  // Fetch collaborators
  const fetchCollaborators = async () => {
    try {
      const data = await getCollaborators(id);
      setCollaborators(data);
    } catch (err) {
      console.error('Failed to fetch collaborators:', err);
    }
  };

  useEffect(() => {
    const fetchArticleAndCollaborators = async () => {
      try {
        setError(null);
        const data = await getArticle(id);
        setArticle(data);
        setFormData({ title: data.title, content: data.content, summary: data.summary || '' });

        // Fetch collaborators if the user is logged in
        const user = getCurrentUser();
        if (user) {
          const collabData = await getCollaborators(id);
          setCollaborators(collabData);
        }
      } catch (err) {
        setError(err.message);
      }
    };

    fetchArticleAndCollaborators();
  }, [id]);

  // Get current user on component mount
  useEffect(() => {
    const user = getCurrentUser();
    setCurrentUser(user);
  }, []);

  // Check permissions when article or user changes
  useEffect(() => {
    if (article && currentUser) {
      const isAuthor = currentUser.id === article.authorId;
      const isAdmin = currentUser.role === 'Admin';
      const isCollaborator = article.collaboratorIds && article.collaboratorIds.includes(currentUser.id);

      const hasEditPermission = isAuthor || isAdmin || isCollaborator;
      setCanEdit(hasEditPermission);
    }
  }, [article, currentUser]);


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
          <p className="article-author">Created by {article.authorUsername}</p>
          <p className={`article-summary ${!article.summary ? 'no-summary' : ''}`}><strong>Summary:</strong> {article.summary || "Create a summary for this article."}</p>
          <p className="article-content">{article.content}</p>

           <div className="article-actions">
              {canEdit && (
                <>
                  <button onClick={() => setEditMode(true)} className="btn" disabled={saving}>Edit</button>
                  <button onClick={handleDelete} className="btn btn-delete" disabled={saving}>Delete</button>
                </>
              )}
              {!canEdit && currentUser && (
                <p className="no-permission">You don't have permission to edit this article</p>
              )}
              {!currentUser && (
                <p className="no-permission">Please log in to edit articles</p>
              )}
            </div>

            {/* Show collaborators section to author and collaborators */}
            {article && currentUser && (canEdit) && (
              <Collaborators
                articleId={article.id}
                collaborators={collaborators}
                authorId={article.authorId}
                isAuthor={currentUser.id === article.authorId}
                onCollaboratorAdded={fetchCollaborators}
                onCollaboratorRemoved={fetchCollaborators}
              />
            )}
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