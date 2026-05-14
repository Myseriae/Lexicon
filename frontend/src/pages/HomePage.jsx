import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { getArticles, deleteArticle } from '../api/api';
import SpotlightCard from '../components/SpotlightCard/SpotlightCard';
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
        return <div className="loading">Loading...</div>;
    }

    if (error) {
        return <div className="error">Error: {error}</div>;
    }

    return (
        <div className="home-container">
            <h1 className="home-title">
                {tag ? `Articles tagged with "${tag}"` : 'Articles'}
            </h1>

            {tag && (
                <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
                    <button
                        className="btn"
                        onClick={() => navigate('/')}
                    >
                        Clear Filter
                    </button>
                </div>
            )}

            <div className="articles-grid">
                {articles.map(article => (
                    <SpotlightCard
                        key={article.id}
                        spotlightColor="rgba(0, 229, 255, 0.2)"
                    >
                        <div
                            onClick={() => navigate(`/article/${article.id}`)}
                            className="article-card"
                        >
                            <div className="article-card-header">
                                <h2 className="article-card-title">
                                    {article.title}
                                </h2>

                                <button
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleDelete(article.id);
                                    }}
                                    className="delete-btn"
                                    title="Delete article"
                                >
                                    <img
                                        src="/trash.svg"
                                        alt="Delete"
                                    />
                                </button>
                            </div>

                            <p className="article-author">
                                Created by {article.authorUsername}
                            </p>

                            <p className={`article-card-content ${!article.summary ? 'no-summary' : ''}`}>
                                {article.summary ? (
                                    article.summary.length > 150
                                        ? `${article.summary.substring(0, 150)}...`
                                        : article.summary
                                ) : (
                                    'No summary was found for this article. Click to view details and create a summary.'
                                )}
                            </p>

                            {/* TAGS */}
                            <div className="article-tags">
                                {article.tags?.map(tag => (
                                    <span
                                        key={tag.id}
                                        className="tag-chip"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            navigate(`/?tag=${tag.name}`);
                                        }}
                                    >
                                        #{tag.name}
                                    </span>
                                ))}
                            </div>
                        </div>
                    </SpotlightCard>
                ))}
            </div>

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