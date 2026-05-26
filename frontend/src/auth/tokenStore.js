let accessToken = null;
const listeners = new Set();

const notify = () => {
  for (const listener of listeners) {
    listener(accessToken);
  }
};

export const getAccessToken = () => accessToken;

export const setAccessToken = (token) => {
  accessToken = token || null;
  notify();
};

export const clearAccessToken = () => {
  accessToken = null;
  notify();
};

export const subscribeToAccessToken = (listener) => {
  listeners.add(listener);

  return () => {
    listeners.delete(listener);
  };
};
