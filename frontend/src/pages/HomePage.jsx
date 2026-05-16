import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { getArticles, deleteArticle } from '../api/api';
import ArticleCard from '../components/ArticleCard/ArticleCard';
import Modal from '../components/Modal/Modal';
import './HomePage.css';

const HomePage = () => {
  const [articles, setArticles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [modal, setModal] = useState({
    isOpen: false,
    message: '',
    type: '',
    onConfirm: null
  });

  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const tag = searchParams.get('tag');

  useEffect(() => {
    const fetchArticles = async () => {
      try {
        setLoading(true);

        const data = await getArticles(tag);

        setArticles(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchArticles();
  }, [tag]);

  const handleDelete = async (id) => {
    setModal({
      isOpen: true,
      message: 'Are you sure you want to delete this article?',
      type: 'confirm',
      onConfirm: async () => {
        try {
          await deleteArticle(id);

          setArticles(prev =>
            prev.filter(a => a.id !== id)
          );
        } catch (err) {
          setError(err.message);
        }
      }
    });
  };

  if (loading) {
    return <div className="home-loading">Loading...</div>;
  }

  if (error) {
    return <div className="home-error">Error: {error}</div>;
  }

  return (
    <div className="home-container">
      <main className="home-main">
        {/* Page Header */}
        <header className="home-header">
          <h1 className="home-title">
            {tag ? `Articles tagged with "${tag}"` : 'The Morning Repository'}
          </h1>
          <p className="home-tagline">
            {tag
              ? `Refining your reading to focus on "${tag}"`
              : 'A curated selection of scholarly inquiries, peer-reviewed observations, and archival deep-dives for the modern intellectual.'}
          </p>

          {tag && (
            <button
              className="home-filter-clear"
              onClick={() => navigate('/')}
            >
              Clear Filter
            </button>
          )}
        </header>

        {/* Article Feed */}
        <section className="home-feed">
          {articles.length === 0 ? (
            <div className="home-empty">
              <p>No articles found.</p>
            </div>
          ) : (
            articles.map(article => (
              <ArticleCard
                key={article.id}
                article={article}
                onDelete={handleDelete}
              />
            ))
          )}
        </section>
      </main>

      {/* Footer */}
      <footer className="home-footer">
        <div className="home-footer-content">
          <div className="home-footer-left">
            <span className="home-footer-brand">Lexicon</span>
            <span className="home-footer-copyright">© 2024 Lexicon Scholarly Press</span>
          </div>
          <div className="home-footer-links">
            <a href="#" className="home-footer-link">About</a>
            <a href="#" className="home-footer-link">Terms</a>
            <a href="#" className="home-footer-link">Library</a>
          </div>
          <div className="home-footer-social">
            <button className="home-footer-social-btn" aria-label="RSS Feed">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M4 11a9 9 0 0 1 9 9M4 4a16 16 0 0 1 16 16M5 20a3 3 0 1 1 6 0" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
            </button>
            <button className="home-footer-social-btn" aria-label="Email">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M3 8l7.89 5.26a2 2 0 0 0 2.22 0L21 8M5 19h14a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2Z" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
            </button>
          </div>
        </div>
      </footer>

      <Modal
        isOpen={modal.isOpen}
        onClose={() =>
          setModal(prev => ({
            ...prev,
            isOpen: false
          }))
        }
        message={modal.message}
        type={modal.type}
        onConfirm={modal.onConfirm}
      />
    </div>
  );
};

export default HomePage;