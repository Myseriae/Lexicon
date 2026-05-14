import { getToken } from '../utils/jwtUtils';

const API_BASE_URL = '';

const fetchWrapper = async (url, options = {}) => {
    try {
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers,
        };

        // Add authorization token if available
        const token = getToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(`${API_BASE_URL}${url}`, {
            headers,
            ...options,
        });

        const contentType = response.headers.get('content-type');

        let data = null;
        if (contentType && contentType.includes('application/json')) {
            data = await response.json();
        }

        if (!response.ok) {
            throw new Error(
                data?.message || `Request failed with status ${response.status}`
            );
        }

        return data;
    } catch (error) {
        // Network error or unexpected crash
        throw new Error(error.message || 'Network error occurred');
    }
};

export const getArticles = () => {
    return fetchWrapper('/api/articles');
};

export const getArticle = (id) => {
    return fetchWrapper(`/api/articles/${id}`);
};

export const createArticle = (article) => {
    return fetchWrapper('/api/articles', {
        method: 'POST',
        body: JSON.stringify(article),
    });
};

export const updateArticle = (id, article) => {
    return fetchWrapper(`/api/articles/${id}`, {
        method: 'PUT',
        body: JSON.stringify(article),
    });
};

export const deleteArticle = (id) => {
    return fetchWrapper(`/api/articles/${id}`, {
        method: 'DELETE',
    });
};

export const searchArticles = (query) => {
    return fetchWrapper(`/api/articles/search?query=${encodeURIComponent(query)}`);
};

// Collaborator endpoints
export const getCollaborators = (articleId) => {
    return fetchWrapper(`/api/articles/${articleId}/collaborators`);
};

export const addCollaborator = (articleId, userId) => {
    return fetchWrapper(`/api/articles/${articleId}/collaborators/${userId}`, {
        method: 'POST',
    });
};

export const addCollaboratorByUsername = (articleId, username) => {
    return fetchWrapper(
        `/api/articles/${articleId}/collaborators/by-username/${encodeURIComponent(username)}`,
        {
            method: 'POST',
        }
    );
};

export const removeCollaborator = (articleId, userId) => {
    return fetchWrapper(`/api/articles/${articleId}/collaborators/${userId}`, {
        method: 'DELETE',
    });
};

export const isCollaborator = (articleId, userId) => {
    return fetchWrapper(`/api/articles/${articleId}/collaborators/${userId}/is-collaborator`);
};
