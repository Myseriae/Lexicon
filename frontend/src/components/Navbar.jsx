import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { isAuthenticated, logout } from '../api/authApi';
import './Navbar.css';

const Navbar = ({ onSearch }) => {
  const navigate = useNavigate();
  const loggedIn = isAuthenticated();
  const [searchValue, setSearchValue] = useState('');

  // When user submits search (Enter or button), navigate to home with search param
  const handleSearch = () => {
    const q = (searchValue || '').trim();
    if (q) {
      navigate(`/?search=${encodeURIComponent(q)}`);
      if (onSearch) onSearch(q);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

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

      <div className="navbar-search">
        <div className="search-container">
          {/* single clickable search button used instead of two icons */}

          <input
            type="text"
            className="search-input"
            placeholder="Search articles, content, tags..."
            value={searchValue}
            onChange={(e) => setSearchValue(e.target.value)}
            onKeyDown={handleKeyDown}
          />

          <button
            type="button"
            className="search-button"
            onClick={handleSearch}
            aria-label="Search"
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path d="M21 21l-4.35-4.35" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              <circle cx="11" cy="11" r="6" stroke="currentColor" strokeWidth="2" />
            </svg>
          </button>
        </div>
      </div>

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