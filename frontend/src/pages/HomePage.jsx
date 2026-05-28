import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { getArticles, searchArticles, deleteArticle } from '../api/api';
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
    const tagsParam = searchParams.get('tags');
    const search = searchParams.get('search');
    const selectedTags = tagsParam ? tagsParam.split(',').map(t => t.trim()).filter(Boolean) : [];

    useEffect(() => {
        const fetchArticles = async () => {
            try {
                setLoading(true);

                let data;
                if (search && search.trim()) {
                    data = await searchArticles(search);
                } else if (selectedTags.length > 0) {
                    // backend does not support multi-tag query, fetch all and filter client-side
                    const all = await getArticles();
                    const lowered = selectedTags.map(t => t.toLowerCase());
                    data = all.filter(a => (a.tags || []).some(tag => lowered.includes(tag.name.toLowerCase())));
                } else if (tag) {
                    data = await getArticles(tag);
                } else {
                    data = await getArticles();
                }

                setArticles(data);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchArticles();
    }, [tag, search, tagsParam]);


    if (loading) {
        return <div className="loading">Loading...</div>;
    }

    if (error) {
        return <div className="error">Error: {error}</div>;
    }

    return (
        <div className="home-container">
            <h1 className="home-title">
                {selectedTags.length > 0
                    ? `Articles tagged with "${selectedTags.join(', ')}"`
                    : tag
                        ? `Articles tagged with "${tag}"`
                        : 'Articles'
                }
            </h1>

            {(selectedTags.length > 0 || tag) && (
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