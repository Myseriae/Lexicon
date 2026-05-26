import { getAccessToken } from '../auth/tokenStore';

const API_BASE_URL = '';

let refreshHandler = null;
let refreshRequest = null;

const parseResponseBody = async (response) => {
  const contentType = response.headers.get('content-type') || '';

  if (contentType.includes('application/json')) {
    return response.json();
  }

  const text = await response.text();
  return text || null;
};

const createError = (response, data) => {
  const message =
    data?.message ||
    data?.title ||
    (typeof data === 'string' && data) ||
    `Request failed with status ${response.status}`;

  const error = new Error(message);
  error.status = response.status;
  error.data = data;
  return error;
};

const runRefresh = async () => {
  if (!refreshHandler) {
    return false;
  }

  if (!refreshRequest) {
    refreshRequest = (async () => {
      try {
        await refreshHandler();
        return true;
      } catch {
        return false;
      } finally {
        refreshRequest = null;
      }
    })();
  }

  return refreshRequest;
};

export const registerRefreshHandler = (handler) => {
  refreshHandler = handler;
};

export const requestJson = async (
  url,
  options = {},
  { skipAuth = false, skipRefresh = false } = {}
) => {
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (!skipAuth) {
    const token = getAccessToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
  }

  const response = await fetch(`${API_BASE_URL}${url}`, {
    credentials: 'include',
    ...options,
    headers,
  });

  const data = await parseResponseBody(response);

  if (response.ok) {
    return data;
  }

  if (response.status === 401 && !skipRefresh && !skipAuth) {
    const refreshed = await runRefresh();

    if (refreshed) {
      return requestJson(url, options, { skipAuth, skipRefresh: true });
    }
  }

  throw createError(response, data);
};
