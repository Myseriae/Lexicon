import React from 'react';
import { useNavigate } from 'react-router-dom';
import './ArticleCard.css';

const ArticleCard = ({ article, onDelete }) => {
  const navigate = useNavigate();

  const handleCardClick = () => {
    navigate(`/article/${article.id}`);
  };

  const handleTagClick = (e, tagName) => {
    e.stopPropagation();
    navigate(`/?tag=${tagName}`);
  };

  const handleDeleteClick = (e) => {
    e.stopPropagation();
    if (onDelete) {
      onDelete(article.id);
    }
  };

  return (
    <article className="article-card" onClick={handleCardClick}>
      <div className="article-card-content">
        {/* Metadata */}
        <div className="article-metadata">
          {article.tags && article.tags.length > 0 && (
            <span className="article-tag-label">
              {article.tags[0]?.name || 'Uncategorized'}
            </span>
          )}
        </div>

        {/* Title */}
        <h2 className="article-title">{article.title}</h2>

        {/* Summary */}
        <p className="article-summary">
          {article.summary ? (
            article.summary.length > 200
              ? `${article.summary.substring(0, 200)}...`
              : article.summary
          ) : (
            <em>No summary available. Click to view full article.</em>
          )}
        </p>

        {/* Footer: Author and Delete */}
        <div className="article-footer">
          <span className="article-author">
            By {article.authorUsername}
          </span>

          <button
            className="article-delete-btn"
            onClick={handleDeleteClick}
            title="Delete article"
            aria-label="Delete article"
          >
            <svg
              width="20"
              height="20"
              viewBox="0 0 20 20"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M5 5v12a2 2 0 002 2h6a2 2 0 002-2V5M2 5h16M8 5V3a1 1 0 011-1h2a1 1 0 011 1v2"
                stroke="currentColor"
                strokeWidth="1.5"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </button>
        </div>

        {/* Tags */}
        {article.tags && article.tags.length > 0 && (
          <div className="article-tags">
            {article.tags.map(tag => (
              <span
                key={tag.id}
                className="article-tag-chip"
                onClick={(e) => handleTagClick(e, tag.name)}
              >
                #{tag.name}
              </span>
            ))}
          </div>
        )}
      </div>
    </article>
  );
};

export default ArticleCard;

