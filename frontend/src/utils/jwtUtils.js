import { getAccessToken } from '../auth/tokenStore';

/**
 * Decodes JWT token payload (base64 decoding, no library needed)
 * @param {string} token
 * @returns {object|null}
 */
export const decodeJwt = (token) => {
  try {
    if (!token) {
      return null;
    }

    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }

    const payload = parts[1];
    const padded = payload + '='.repeat((4 - (payload.length % 4)) % 4);
    const decoded = atob(padded);
    const payloadObj = JSON.parse(decoded);

    return {
      ...payloadObj,
      id:
        payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        payloadObj.sub,
      email:
        payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      name: payloadObj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      role:
        payloadObj['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        payloadObj.role,
    };
  } catch (error) {
    console.error('Failed to decode JWT:', error);
    return null;
  }
};

export const getCurrentUser = () => decodeJwt(getAccessToken());
