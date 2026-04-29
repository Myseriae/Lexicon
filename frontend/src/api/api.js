const API_BASE_URL = '';

const fetchWrapper = async (url, options = {}) => {
    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            },
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
    return fetchWrapper('/Article');
};

export const getArticle = (id) => {
    return fetchWrapper(`/Article/${id}`);
};

export const createArticle = (article) => {
    return fetchWrapper('/Article', {
        method: 'POST',
        body: JSON.stringify(article),
    });
};

export const updateArticle = (id, article) => {
    return fetchWrapper(`/Article/${id}`, {
        method: 'PUT',
        body: JSON.stringify(article),
    });
};

export const deleteArticle = (id) => {
    return fetchWrapper(`/Article/${id}`, {
        method: 'DELETE',
    });
};

export const searchArticles = (query) => {
    return fetchWrapper(`/Article/search?query=${encodeURIComponent(query)}`);
};