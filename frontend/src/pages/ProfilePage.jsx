import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getProfile } from '../api/authApi';
import { useAuth } from '../context/AuthContext';
import './ProfilePage.css';

const ProfilePage = () => {
  const [profile, setProfile] = useState(null);
  const [password, setPassword] = useState('');
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();
  const { logout, deleteAccount } = useAuth();

  useEffect(() => {
    let isMounted = true;

    const loadProfile = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getProfile();
        if (isMounted) {
          setProfile(data);
        }
      } catch (err) {
        if (isMounted) {
          setError(err.message || 'Failed to load profile');
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    loadProfile();

    return () => {
      isMounted = false;
    };
  }, []);

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  const handleDeleteAccount = async (event) => {
    event.preventDefault();
    if (!confirmingDelete) {
      setConfirmingDelete(true);
      return;
    }

    try {
      setSubmitting(true);
      setError(null);
      await deleteAccount(password);
      navigate('/');
    } catch (err) {
      setError(err.message || 'Failed to delete account');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <div className="profile-status">Loading profile...</div>;
  }

  return (
    <div className="profile-container">
      <header className="profile-header">
        <div>
          <h1>Profile</h1>
          <p>Manage your account and articles.</p>
        </div>
        <button type="button" className="btn btn-secondary" onClick={handleLogout}>
          Logout
        </button>
      </header>

      {error && <div className="profile-error">{error}</div>}

      {profile && (
        <>
          <section className="profile-section">
            <h2>Account</h2>
            <div className="account-details">
              <div>
                <span>Username</span>
                <strong>{profile.userName}</strong>
              </div>
              <div>
                <span>Email</span>
                <strong>{profile.email}</strong>
              </div>
            </div>
          </section>

          <section className="profile-section">
            <div className="section-heading">
              <h2>Your articles</h2>
              <Link to="/create" className="btn">
                Create
              </Link>
            </div>

            {profile.articles.length === 0 ? (
              <p className="profile-empty">You have not created any articles yet.</p>
            ) : (
              <div className="profile-article-list">
                {profile.articles.map((article) => (
                  <article key={article.id} className="profile-article-row">
                    <div>
                      <h3>{article.title}</h3>
                      <p>
                        {article.summary ||
                          'No summary has been added for this article yet.'}
                      </p>
                    </div>
                    <div className="profile-article-actions">
                      <Link to={`/article/${article.id}`} className="btn btn-secondary">
                        View
                      </Link>
                      <Link to={`/article/${article.id}?edit=true`} className="btn">
                        Edit
                      </Link>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </section>

          <section className="profile-section danger-section">
            <h2>Delete account</h2>
            <p>
              Your account will be anonymized and disabled. Your articles will remain
              available as authored by Deleted User.
            </p>
            <form onSubmit={handleDeleteAccount} className="delete-account-form">
              <label htmlFor="delete-password">Password</label>
              <input
                id="delete-password"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Confirm your password"
                required
                disabled={submitting}
              />
              {confirmingDelete && (
                <p className="delete-confirmation">
                  Press Delete account again to permanently disable this login.
                </p>
              )}
              <button
                type="submit"
                className="btn btn-delete"
                disabled={submitting || !password}
              >
                {submitting ? 'Deleting...' : 'Delete account'}
              </button>
            </form>
          </section>
        </>
      )}
    </div>
  );
};

export default ProfilePage;
