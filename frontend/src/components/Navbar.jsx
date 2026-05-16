import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { isAuthenticated, logout } from '../api/authApi';
import './Navbar.css';

const Navbar = ({ onSearch }) => {
  const navigate = useNavigate();
  const loggedIn = isAuthenticated();
  const [searchValue, setSearchValue] = useState('');

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (onSearch) {
        onSearch(searchValue);
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchValue, onSearch]);

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
      <div className="navbar-content">
        <div className="navbar-left">
          <Link to="/" className="navbar-brand">Lexicon</Link>
        </div>

        <div className="navbar-center">
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
              placeholder="Search the library..."
              value={searchValue}
              onChange={(e) => setSearchValue(e.target.value)}
            />
          </div>
        </div>

        <div className="navbar-right">
          <button className="create-button" onClick={handleCreateClick}>
            Create
          </button>

          {loggedIn ? (
            <button className="logout-button" onClick={handleLogout}>
              Logout
            </button>
          ) : (
            <>
              <Link to="/login" className="login-button">
                Login
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;