import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { isAuthenticated, logout } from '../api/authApi';
import './Navbar.css';

const Navbar = () => {
  const navigate = useNavigate();

  // derive auth state directly (no stale state issues)
  const loggedIn = isAuthenticated();

  const handleCreateClick = () => {
    if (loggedIn) {
      navigate('/create');
    } else {
      navigate('/register');
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
       <nav className="navbar">
         {/* Left Section */}
         <div className="navbar-left">
           <Link to="/" className="navbar-brand">Lexicon</Link>

          <button className="create-button" onClick={handleCreateClick}>
            <svg
                width="20"
                height="20"
                viewBox="0 0 20 20"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                className="plus-icon"
            >
              <path
                  d="M10 4V16M4 10H16"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
              />
            </svg>
            Create
          </button>
        </div>

        {/* Center Search */}
        <div className="navbar-search">
          <div className="search-container">
            <svg
                width="20"
                height="20"
                viewBox="0 0 20 20"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                className="search-icon"
            >
              <circle
                  cx="8.5"
                  cy="8.5"
                  r="6.5"
                  stroke="currentColor"
                  strokeWidth="2"
                  fill="none"
              />
              <path
                  d="M13 13L18 18"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
              />
            </svg>

            <input
                type="text"
                className="search-input"
                placeholder="Search articles, topics..."
                disabled
            />
          </div>
        </div>

        {/* Right Auth */}
        <div className="navbar-right">
          {loggedIn ? (
              <button className="logout-button" onClick={handleLogout}>
                Logout
              </button>
          ) : (
              <>
                <Link to="/login" className="login-button">
                  Login
                </Link>
                <Link to="/register" className="register-button">
                  Register
                </Link>
              </>
          )}
        </div>
      </nav>
  );
};

export default Navbar;