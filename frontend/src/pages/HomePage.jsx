import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getArticles, deleteArticle } from '../api/api';
import SpotlightCard from '../components/SpotlightCard/SpotlightCard';
import Modal from '../components/Modal/Modal';
import './HomePage.css';

const HomePage = () => {
    const [articles, setArticles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [modal, setModal] = useState({ isOpen: false, message: '', type: '', onConfirm: null });

    const navigate = useNavigate();

    useEffect(() => {
        const fetchArticles = async () => {
            try {
                const data = await getArticles();
                setArticles(data);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchArticles();
    }, []);

    const handleDelete = async (id) => {
        setModal({
            isOpen: true,
            message: 'Are you sure you want to delete this article?',
            type: 'confirm',
            onConfirm: async () => {
                try {
                    await deleteArticle(id);
                    setArticles(articles.filter(article => article.id !== id));
                } catch (err) {
                    alert('Failed to delete article: ' + err.message);
                }
            }
        });
    };

    if (loading) return <div className="loading">Loading...</div>;
    if (error) return <div className="error">Error: {error}</div>;

    return (
        <div className="home-container">
            <h1 className="home-title">Articles</h1>

            <div className="articles-grid">
                {articles.map(article => (
                    <SpotlightCard key={article.id} spotlightColor="rgba(0, 229, 255, 0.2)">
                        
                        <div 
                            onClick={() => navigate(`/article/${article.id}`)}
                            className="article-card"
                        >
                            <div className="article-card-header">
                                <h2 className="article-card-title">{article.title}</h2>

                                <button 
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleDelete(article.id);
                                    }}
                                    className="delete-btn"
                                    title="Delete article"
                                >
                                    <img src="/trash.svg" alt="Delete" />
                                </button>
                            </div>

                            <p className="article-card-content">{article.content}</p>
                        </div>

                    </SpotlightCard>
                ))}
            </div>

            <Modal
                isOpen={modal.isOpen}
                onClose={() => setModal({ ...modal, isOpen: false })}
                message={modal.message}
                type={modal.type}
                onConfirm={modal.onConfirm}
            />
        </div>
    );
};

export default HomePage;