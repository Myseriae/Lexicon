import {
  createContext,
  startTransition,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import {
  clearAccessToken,
  getAccessToken,
  setAccessToken,
  subscribeToAccessToken,
} from '../auth/tokenStore';
import {
  login as loginRequest,
  logout as logoutRequest,
  refreshToken as refreshTokenRequest,
  register as registerRequest,
  deleteAccount as deleteAccountRequest,
} from '../api/authApi';
import { registerRefreshHandler } from '../api/httpClient';
import { decodeJwt } from '../utils/jwtUtils';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [accessToken, setAccessTokenState] = useState(() => getAccessToken());
  const [isInitializing, setIsInitializing] = useState(true);
  const refreshRequestRef = useRef(null);
  const refreshSessionRef = useRef(null);

  useEffect(() => {
    return subscribeToAccessToken((token) => {
      startTransition(() => {
        setAccessTokenState(token);
      });
    });
  }, []);

  refreshSessionRef.current = async () => {
    if (!refreshRequestRef.current) {
      refreshRequestRef.current = (async () => {
        try {
          const response = await refreshTokenRequest();
          if (!response?.accessToken) {
            throw new Error('Refresh response did not include an access token.');
          }

          setAccessToken(response.accessToken);
          return response;
        } catch (error) {
          clearAccessToken();
          throw error;
        } finally {
          refreshRequestRef.current = null;
        }
      })();
    }

    return refreshRequestRef.current;
  };

  useEffect(() => {
    registerRefreshHandler(() => refreshSessionRef.current());

    return () => {
      registerRefreshHandler(null);
    };
  }, []);

  useEffect(() => {
    let isMounted = true;

    const restoreSession = async () => {
      try {
        await refreshSessionRef.current();
      } catch {
        clearAccessToken();
      } finally {
        if (isMounted) {
          setIsInitializing(false);
        }
      }
    };

    restoreSession();

    return () => {
      isMounted = false;
    };
  }, []);

  const login = async (email, password) => {
    const response = await loginRequest(email, password);
    setAccessToken(response.accessToken);
    return response;
  };

  const register = async (userData) => {
    const response = await registerRequest(userData);
    setAccessToken(response.accessToken);
    return response;
  };

  const logout = async () => {
    try {
      await logoutRequest();
    } finally {
      clearAccessToken();
    }
  };

  const deleteAccount = async (password) => {
    try {
      await deleteAccountRequest(password);
    } finally {
      clearAccessToken();
    }
  };

  const currentUser = decodeJwt(accessToken);

  const value = {
    accessToken,
    currentUser,
    isAuthenticated: !!accessToken && !!currentUser,
    isInitializing,
    login,
    logout,
    deleteAccount,
    refreshSession: refreshSessionRef.current,
    register,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
}
