import React, { useState, useEffect } from 'react';
import { getArticles, deleteArticle } from '../api/api';
import SpotlightCard from '../components/SpotlightCard/SpotlightCard';

const HomePage = () => {
    const [articles, setArticles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

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
        if (!window.confirm('Are you sure you want to delete this article?')) return;

        try {
            await deleteArticle(id);
            setArticles(articles.filter(article => article.id !== id));
        } catch (err) {
            alert('Failed to delete article: ' + err.message);
        }
    };

    if (loading) return <div>Loading...</div>;
    if (error) return <div>Error: {error}</div>;

    return (
        <div style={{ padding: '2rem', minHeight: '100vh', color: '#fff' }}>
            <h1 style={{ textAlign: 'center' }}>Articles</h1>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '2rem' }}>
                {articles.map(article => (
                    <SpotlightCard key={article.id} spotlightColor="rgba(0, 229, 255, 0.2)">
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                            <h2 style={{ color: '#fff', marginBottom: '1rem' }}>{article.title}</h2>
                            <button 
                                onClick={() => handleDelete(article.id)}
                                style={{
                                    background: 'none',
                                    border: 'none',
                                    cursor: 'pointer',
                                    padding: '5px',
                                    borderRadius: '4px',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center'
                                }}
                                title="Delete article"
                            >
                                <img src="/trash.svg" alt="Delete" style={{ width: '50px', height: '50px', filter: 'invert(1)' }} />
                            </button>
                        </div>
                        <p style={{ color: '#ccc', lineHeight: '1.6' }}>{article.content}</p>
                    </SpotlightCard>
                ))}
            </div>
        </div>
    );
};

export default HomePage;
