import { requestJson } from './httpClient';

export const login = (email, password) =>
  requestJson(
    '/api/auth/login',
    {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    },
    { skipAuth: true, skipRefresh: true }
  );

export const register = (userData) =>
  requestJson(
    '/api/auth/register',
    {
      method: 'POST',
      body: JSON.stringify(userData),
    },
    { skipAuth: true, skipRefresh: true }
  );

export const refreshToken = () =>
  requestJson(
    '/api/auth/refresh',
    {
      method: 'POST',
    },
    { skipAuth: true, skipRefresh: true }
  );

export const logout = () =>
  requestJson(
    '/api/auth/logout',
    {
      method: 'POST',
    },
    { skipAuth: true, skipRefresh: true }
  );

export const getProfile = () =>
  requestJson('/api/auth/profile', {
    method: 'GET',
  });

export const deleteAccount = (password) =>
  requestJson('/api/auth/account', {
    method: 'DELETE',
    body: JSON.stringify({ password }),
  });

export const verifyToken = () =>
  requestJson(
    '/api/auth/verify',
    {
      method: 'GET',
    },
    { skipRefresh: true }
  );
