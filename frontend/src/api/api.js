import { requestJson } from './httpClient';

export const getArticles = (tag) => {
    const url = tag
        ? `/api/articles?tag=${encodeURIComponent(tag)}`
        : '/api/articles';

    return requestJson(url);
};

export const getArticle = (id) => {
    return requestJson(`/api/articles/${id}`);
};

export const createArticle = (article) => {
    return requestJson('/api/articles', {
        method: 'POST',
        body: JSON.stringify(article),
    });
};

export const updateArticle = (id, article) => {
    return requestJson(`/api/articles/${id}`, {
        method: 'PUT',
        body: JSON.stringify(article),
    });
};

export const deleteArticle = (id) => {
    return requestJson(`/api/articles/${id}`, {
        method: 'DELETE',
    });
};

export const searchArticles = (query) => {
    return requestJson(`/api/articles/search?query=${encodeURIComponent(query)}`);
};

// Collaborator endpoints
export const getCollaborators = (articleId) => {
    return requestJson(`/api/articles/${articleId}/collaborators`);
};

export const addCollaborator = (articleId, userId) => {
    return requestJson(`/api/articles/${articleId}/collaborators/${userId}`, {
        method: 'POST',
    });
};

export const addCollaboratorByUsername = (articleId, username) => {
    return requestJson(
        `/api/articles/${articleId}/collaborators/by-username/${encodeURIComponent(username)}`,
        {
            method: 'POST',
        }
    );
};

export const removeCollaborator = (articleId, userId) => {
    return requestJson(`/api/articles/${articleId}/collaborators/${userId}`, {
        method: 'DELETE',
    });
};

export const isCollaborator = (articleId, userId) => {
    return requestJson(`/api/articles/${articleId}/collaborators/${userId}/is-collaborator`);
};

// tag endpoints
export const getTags = () => {
    return requestJson('/api/tags');
};

export const addTagToArticle = (articleId, name) => {
    return requestJson(`/api/articles/${articleId}/tags`, {
        method: 'POST',
        body: JSON.stringify({ name }),
    });
};

export const removeTagFromArticle = (articleId, tagId) => {
    return requestJson(`/api/articles/${articleId}/tags/${tagId}`, {
        method: 'DELETE',
    });
};
