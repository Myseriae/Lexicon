import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  getArticle,
  updateArticle,
  deleteArticle,
  getCollaborators,
  getRevisions,
  rollbackArticle,
} from '../api/api';
import { addTagToArticle, removeTagFromArticle } from '../api/api';
import Modal from '../components/Modal/Modal';
import Collaborators from '../components/Collaborators/Collaborators';
import MDEditor from '@uiw/react-md-editor';
import ReactMarkdown from 'react-markdown';
import '@uiw/react-md-editor/markdown-editor.css';
import './ArticlePage.css';
import { useAuth } from '../context/AuthContext';

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
  const [activeTab, setActiveTab] = useState('article');
  const [revisions, setRevisions] = useState([]);
  const [selectedRevisionId, setSelectedRevisionId] = useState(null);
  const [revisionsLoading, setRevisionsLoading] = useState(false);
  const [revisionMessage, setRevisionMessage] = useState(null);
  const [tagInput, setTagInput] = useState('');
  const [tagLoading, setTagLoading] = useState(false);
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { currentUser: authenticatedUser } = useAuth();

  // Fetch collaborators
  const fetchCollaborators = useCallback(async () => {
    try {
      const data = await getCollaborators(id);
      setCollaborators(data);
    } catch (err) {
      console.error('Failed to fetch collaborators:', err);
    }
  }, [id]);

  const fetchArticle = useCallback(async () => {
    const data = await getArticle(id);
    setArticle(data);
    setFormData({ title: data.title, content: data.content, summary: data.summary || '' });
    return data;
  }, [id]);

  const fetchRevisions = useCallback(async () => {
    try {
      setRevisionsLoading(true);
      setRevisionMessage(null);
      const data = await getRevisions(id);
      const newestFirst = [...data].sort((a, b) => b.versionNumber - a.versionNumber);
      setRevisions(newestFirst);
      setSelectedRevisionId(prev => {
        if (prev && newestFirst.some(revision => revision.id === prev)) {
          return prev;
        }

        return newestFirst[0]?.id || null;
      });
    } catch (err) {
      setRevisionMessage({ type: 'error', text: err.message || 'Failed to load revisions' });
    } finally {
      setRevisionsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    const fetchArticleAndCollaborators = async () => {
      try {
        setError(null);
        await fetchArticle();
        // refresh collaborators if allowed
        if (canEdit) {
          fetchCollaborators();
        }
      } catch (err) {
        setError(err.message);
      }
    };

    fetchArticleAndCollaborators();
  }, [authenticatedUser, fetchArticle, fetchCollaborators]);

  useEffect(() => {
    setCurrentUser(authenticatedUser);
  }, [authenticatedUser]);

  // Check permissions when article or user changes
  useEffect(() => {
    if (article && currentUser) {
      const isAuthor = article.authorId === currentUser.id || article.authorUsername === currentUser.name;
      const isAdmin = currentUser.role === 'Admin';
      const isCollaborator = article.collaboratorIds && article.collaboratorIds.includes(currentUser.id);

      const hasEditPermission = isAuthor || isAdmin || isCollaborator;
      setCanEdit(hasEditPermission);
    } else {
      setCanEdit(false);
    }
  }, [article, currentUser]);

  useEffect(() => {
    if (!canEdit && activeTab !== 'article') {
      setActiveTab('article');
    }
  }, [activeTab, canEdit]);

  useEffect(() => {
    if (canEdit && searchParams.get('edit') === 'true') {
      setEditMode(true);
    }
  }, [canEdit, searchParams]);

  useEffect(() => {
    if (canEdit && activeTab === 'revisions') {
      fetchRevisions();
    }
  }, [activeTab, canEdit, fetchRevisions]);

  useEffect(() => {
    if (canEdit && activeTab === 'collaborators') {
      fetchCollaborators();
    }
  }, [activeTab, canEdit, fetchCollaborators]);

  const handleUpdate = async () => {
    try {
      setError(null);
      setSaving(true);
      const updated = await updateArticle(id, formData);
      // Merge the response from backend with local formData to ensure fields aren't lost
      // if the backend response is incomplete but successful.
      setArticle(prev => ({ ...prev, ...formData, ...(updated || {}) }));
      setRevisions([]);
      setSelectedRevisionId(null);
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

  const handleRollback = (revision) => {
    setModal({
      isOpen: true,
      message: `Roll back to revision ${revision.versionNumber}? The current article will be saved as a new revision first.`,
      type: 'confirm',
      onConfirm: async () => {
        try {
          setSaving(true);
          setRevisionMessage(null);
          await rollbackArticle(id, revision.id);
          await fetchArticle();
          await fetchRevisions();
          setActiveTab('article');
          setEditMode(false);
          setModal(prev => ({ ...prev, isOpen: false }));
        } catch (err) {
          setRevisionMessage({ type: 'error', text: err.message || 'Failed to roll back article' });
        } finally {
          setSaving(false);
        }
      }
    });
  };

  const selectedRevision = revisions.find(revision => revision.id === selectedRevisionId);

  const renderArticleTab = () => (
    <>
      <h1>{article.title}</h1>
      <p className="article-author">Created by {article.authorUsername}</p>
      <div className="tag-list">
        {article.tags?.map(tag => (
          <span key={tag.id} className="tag-chip">
            {tag.name}
            {canEdit && (
              <button
                type="button"
                className="tag-remove"
                onClick={async (e) => {
                  e.stopPropagation();
                  try {
                    setTagLoading(true);
                    await removeTagFromArticle(article.id, tag.id);
                    await fetchArticle();
                  } catch (err) {
                    console.error('Failed to remove tag', err);
                    setError(err.message || 'Failed to remove tag');
                  } finally {
                    setTagLoading(false);
                  }
                }}
                disabled={tagLoading}
                aria-label={`Remove tag ${tag.name}`}
              >
                ×
              </button>
            )}
          </span>
        ))}

        {canEdit && (
          <span className="tag-input-inline">
            <input
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={async (e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  if (!tagInput.trim()) return;
                  try {
                    setTagLoading(true);
                    await addTagToArticle(article.id, tagInput.trim());
                    setTagInput('');
                    await fetchArticle();
                  } catch (err) {
                    console.error('Failed to add tag', err);
                    setError(err.message || 'Failed to add tag');
                  } finally {
                    setTagLoading(false);
                  }
                }
              }}
              className="input"
              placeholder="New tag"
              disabled={tagLoading}
            />
            <button
              type="button"
              className="btn"
              onClick={async () => {
                if (!tagInput.trim()) return;
                try {
                  setTagLoading(true);
                  await addTagToArticle(article.id, tagInput.trim());
                  setTagInput('');
                  await fetchArticle();
                } catch (err) {
                  console.error('Failed to add tag', err);
                  setError(err.message || 'Failed to add tag');
                } finally {
                  setTagLoading(false);
                }
              }}
              disabled={tagLoading || !tagInput.trim()}
            >
              Add
            </button>
          </span>
        )}
      </div>
      <p className={`article-summary ${!article.summary ? 'no-summary' : ''}`}><strong>Summary:</strong> {article.summary || "Create a summary for this article."}</p>
      <div className="article-content"><ReactMarkdown>{article.content}</ReactMarkdown></div>

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
    </>
  );

  const renderEditArticle = () => (
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
      {article && (
        <MDEditor
          value={formData.content}
          onChange={(value) => setFormData({ ...formData, content: value || '' })}
          className="textarea"
          data-color-mode="dark"
        />
      )}

      <div className="article-actions">
        <button onClick={handleUpdate} disabled={saving} className="btn">
          {saving ? 'Saving...' : 'Save'}
        </button>
        <button onClick={() => setEditMode(false)} className="btn" disabled={saving}>Cancel</button>
      </div>
    </>
  );

  const renderRevisionsTab = () => (
    <div className="revisions-panel">
      <div className="revisions-header">
        <h2>Revisions</h2>
        <button onClick={fetchRevisions} className="btn btn-secondary" disabled={revisionsLoading || saving}>
          {revisionsLoading ? 'Loading...' : 'Refresh'}
        </button>
      </div>

      {revisionMessage && (
        <div className={`revision-message ${revisionMessage.type}`}>
          {revisionMessage.text}
        </div>
      )}

      {revisionsLoading && <p className="revision-empty">Loading revisions...</p>}

      {!revisionsLoading && revisions.length === 0 && (
        <p className="revision-empty">No revisions yet.</p>
      )}

      {!revisionsLoading && revisions.length > 0 && (
        <div className="revision-layout">
          <div className="revision-list" aria-label="Article revisions">
            {revisions.map(revision => (
              <button
                key={revision.id}
                type="button"
                className={`revision-list-item ${selectedRevisionId === revision.id ? 'selected' : ''}`}
                onClick={() => setSelectedRevisionId(revision.id)}
              >
                <span>Revision {revision.versionNumber}</span>
                <small>{new Date(revision.savedAt).toLocaleString()}</small>
              </button>
            ))}
          </div>

          {selectedRevision && (
            <div className="revision-preview">
              <div className="revision-preview-header">
                <div>
                  <h3>Revision {selectedRevision.versionNumber}</h3>
                  <p>{new Date(selectedRevision.savedAt).toLocaleString()}</p>
                </div>
                <button
                  onClick={() => handleRollback(selectedRevision)}
                  className="btn"
                  disabled={saving}
                >
                  {saving ? 'Rolling back...' : 'Rollback'}
                </button>
              </div>

              <div className="revision-summary">
                <strong>Summary:</strong> {selectedRevision.summary || 'No summary saved for this revision.'}
              </div>
              <div className="article-content revision-content">
                <ReactMarkdown>{selectedRevision.content}</ReactMarkdown>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );

  if (!article && !error) return <div className="loading">Loading...</div>;

  return (
    <div className="article-container">
      {error && <div className="error">Error: {error}</div>}

      {article && !editMode ? (
        <>
          <div className="article-tabs" role="tablist" aria-label="Article sections">
            <button
              type="button"
              role="tab"
              aria-selected={activeTab === 'article'}
              className={`article-tab ${activeTab === 'article' ? 'active' : ''}`}
              onClick={() => setActiveTab('article')}
            >
              Article
            </button>
            {canEdit && (
              <>
                <button
                  type="button"
                  role="tab"
                  aria-selected={activeTab === 'collaborators'}
                  className={`article-tab ${activeTab === 'collaborators' ? 'active' : ''}`}
                  onClick={() => setActiveTab('collaborators')}
                >
                  Collaborators
                </button>
                <button
                  type="button"
                  role="tab"
                  aria-selected={activeTab === 'revisions'}
                  className={`article-tab ${activeTab === 'revisions' ? 'active' : ''}`}
                  onClick={() => setActiveTab('revisions')}
                >
                  Revisions
                </button>
              </>
            )}
          </div>

          <div className="article-tab-content">
            {activeTab === 'article' && renderArticleTab()}

            {canEdit && activeTab === 'collaborators' && (
              <Collaborators
                articleId={article.id}
                collaborators={collaborators}
                authorUsername={article.authorUsername}
                isAuthor={article.authorId === currentUser?.id || article.authorUsername === currentUser?.name || currentUser?.role === 'Admin'}
                onCollaboratorAdded={fetchCollaborators}
                onCollaboratorRemoved={fetchCollaborators}
              />
            )}

            {canEdit && activeTab === 'revisions' && renderRevisionsTab()}
          </div>
        </>
      ) : article && (
        renderEditArticle()
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
