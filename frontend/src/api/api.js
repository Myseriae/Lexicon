const API_BASE_URL = 'http://localhost:5149'; // Replace with your actual API base URL

export const getArticles = async () => {
    const response = await fetch(`${API_BASE_URL}/Article`);
    if (!response.ok) {
        throw new Error('Failed to fetch articles');
    }
    return response.json();
};

export const getArticle = async (articleId) => {
    const response = await fetch(`${API_BASE_URL}/Article/${articleId}`);
    if (!response.ok) {
        throw new Error('Failed to fetch article');
    }
    return response.json();
};

export const createArticle = async (article) => {
    const response = await fetch(`${API_BASE_URL}/Article`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(article),
    });
    if (!response.ok) {
        throw new Error('Failed to create article');
    }
    return response.json();
};

export const deleteArticle = async (articleId) => {
    const response = await fetch(`${API_BASE_URL}/Article/${articleId}`, {
        method: 'DELETE',
    });
    if (!response.ok) {
        throw new Error('Failed to delete article');
    }
};
