import React, { useState } from 'react';
import { addCollaboratorByUsername, removeCollaborator } from '../../api/api';
import './Collaborators.css';

const Collaborators = ({
  articleId,
  collaborators = [],
  authorId,
  isAuthor,
  onCollaboratorAdded,
  onCollaboratorRemoved
}) => {
  const [showAddForm, setShowAddForm] = useState(false);
  const [newCollaboratorUsername, setNewCollaboratorUsername] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const handleAddCollaborator = async () => {
    if (!newCollaboratorUsername.trim()) {
      setError('Please enter a username');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccess(null);

      await addCollaboratorByUsername(articleId, newCollaboratorUsername);

      setSuccess(`Collaborator added successfully!`);
      setNewCollaboratorUsername('');
      setShowAddForm(false);

      if (onCollaboratorAdded) {
        onCollaboratorAdded();
      }
    } catch (err) {
      setError(err.message || 'Failed to add collaborator');
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveCollaborator = async (userId) => {
    if (!window.confirm(`Remove this collaborator?`)) {
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccess(null);

      await removeCollaborator(articleId, userId);

      setSuccess('Collaborator removed successfully!');

      if (onCollaboratorRemoved) {
        onCollaboratorRemoved();
      }
    } catch (err) {
      setError(err.message || 'Failed to remove collaborator');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="collaborators-section">
      <h3>Collaborators</h3>

      {error && <div className="collaborators-error">{error}</div>}
      {success && <div className="collaborators-success">{success}</div>}

      <div className="collaborators-list">
        {/* Show Author */}
        <div className="collaborator-item">
          <span className="collaborator-info">
            <span className="collaborator-name">{authorId}</span>
            <span className="collaborator-badge author-badge">Author</span>
          </span>
        </div>

        {/* Show Collaborators */}
        {collaborators.map((collaborator) => (
          <div key={collaborator.userId} className="collaborator-item">
            <span className="collaborator-info">
              <span className="collaborator-name">{collaborator.userName}</span>
              <span className="collaborator-id">({collaborator.userId})</span>
            </span>

            {isAuthor && (
              <button
                onClick={() => handleRemoveCollaborator(collaborator.userId)}
                className="btn-remove-collaborator"
                disabled={loading}
                title="Remove collaborator"
              >
                ✕
              </button>
            )}
          </div>
        ))}
      </div>

      {isAuthor && (
        <div className="add-collaborator-section">
          {!showAddForm ? (
            <button
              onClick={() => setShowAddForm(true)}
              className="btn-add-collaborator"
              disabled={loading}
            >
              + Add Collaborator
            </button>
          ) : (
            <div className="add-collaborator-form">
              <input
                type="text"
                value={newCollaboratorUsername}
                onChange={(e) => setNewCollaboratorUsername(e.target.value)}
                placeholder="Enter username"
                disabled={loading}
                className="collaborator-input"
              />
              <button
                onClick={handleAddCollaborator}
                className="btn-confirm"
                disabled={loading}
              >
                {loading ? 'Adding...' : 'Add'}
              </button>
              <button
                onClick={() => {
                  setShowAddForm(false);
                  setNewCollaboratorUsername('');
                  setError(null);
                }}
                className="btn-cancel"
                disabled={loading}
              >
                Cancel
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default Collaborators;
