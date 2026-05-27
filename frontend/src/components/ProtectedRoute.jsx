import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function ProtectedRoute({ children }) {
  const { isAuthenticated, isInitializing } = useAuth();
  if (isInitializing) return null; // wait for session restore before redirecting
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return children;
}
