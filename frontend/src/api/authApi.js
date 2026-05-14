import { setToken, clearToken } from '../utils/jwtUtils';

const API_BASE_URL = '';

const fetchWrapper = async (url, options = {}) => {
  try {
    const response = await fetch(`${API_BASE_URL}${url}`, {
      credentials: 'include',
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
    throw new Error(error.message || 'Network error occurred');
  }
};

/**
 * Login user with email and password
 * @param {string} email - User email
 * @param {string} password - User password
 * @returns {object} - Response with JWT token
 */
export const login = async (email, password) => {
  const data = await fetchWrapper('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });

  // Store token in localStorage if provided
  if (data?.accessToken) {
    setToken(data.accessToken);
  }

  return data;
};

/**
 * Register a new user
 * @param {object} userData - User data (email, password, name, etc)
 * @returns {object} - Response with JWT token
 */
export const register = async (userData) => {
  const data = await fetchWrapper('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(userData),
  });

  // Store token in localStorage if provided
  if (data?.accessToken) {
    setToken(data.accessToken);
  }

  return data;
};

/**
 * Logout user
 */
export const logout = () => {
  clearToken();
};

/**
 * Refresh token (call backend to get new token)
 * @returns {object} - Response with new JWT token
 */
export const refreshToken = async () => {
  const data = await fetchWrapper('/api/auth/refresh', {
    method: 'POST',
  });

  // Store new token in localStorage if provided
  if (data?.accessToken) {
    setToken(data.accessToken);
  }

  return data;
};

/**
 * Verify token is valid
 * @returns {object} - User info if token is valid
 */
export const verifyToken = async () => {
  return await fetchWrapper('/api/auth/verify', {
    method: 'GET',
  });
};

