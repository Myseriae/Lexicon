/**
 * Decodes JWT token payload (base64 decoding, no library needed)
 * @param {string} token - JWT token
 * @returns {object|null} - Decoded payload or null if invalid
 */
export const decodeJwt = (token) => {
  try {
    if (!token) return null;

    const parts = token.split('.');
    if (parts.length !== 3) {
      console.error('Invalid JWT format: expected 3 parts');
      return null;
    }

    // Decode the payload (second part)
    const payload = parts[1];

    // Add padding if necessary (base64 requires padding to be a multiple of 4)
    const padded = payload + '='.repeat((4 - payload.length % 4) % 4);

    // Decode from base64
    const decoded = atob(padded);

    // Parse JSON
    const payloadObj = JSON.parse(decoded);

    // Map common .NET claim URIs to simpler names
    return {
      ...payloadObj,
      id: payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payloadObj.sub,
      email: payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      name: payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      role: payloadObj['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payloadObj.role
    };
  } catch (error) {
    console.error('Failed to decode JWT:', error);
    return null;
  }
};

/**
 * Gets the current user from JWT token stored in localStorage
 * @returns {object|null} - User object with id, email, role, etc or null
 */
export const getCurrentUser = () => {
  try {
    const token = localStorage.getItem('token');
    if (!token) return null;

    return decodeJwt(token);
  } catch (error) {
    console.error('Failed to get current user:', error);
    return null;
  }
};

/**
 * Gets JWT token from localStorage
 * @returns {string|null} - JWT token or null
 */
export const getToken = () => {
  return localStorage.getItem('token');
};

/**
 * Stores JWT token in localStorage
 * @param {string} token - JWT token
 */
export const setToken = (token) => {
  if (token) {
    localStorage.setItem('token', token);
  } else {
    localStorage.removeItem('token');
  }
};

/**
 * Clears the token from localStorage
 */
export const clearToken = () => {
  localStorage.removeItem('token');
};

/**
 * Checks if user is authenticated
 * @returns {boolean} - true if token exists and is valid
 */
export const isAuthenticated = () => {
  const token = getToken();
  const user = decodeJwt(token);
  return !!user && !!token;
};


