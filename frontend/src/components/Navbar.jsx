import React, { useState, useEffect } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getTags } from '../api/api';
import './Navbar.css';

const Navbar = ({ onSearch }) => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [searchValue, setSearchValue] = useState('');
      useEffect(() => {
        // Sync navbar state with URL - if URL is cleared, reset search inputs
        const search = searchParams.get('search');
        const tags = searchParams.get('tags');
        const tag = searchParams.get('tag');
    
        // If no search/tag params in URL, reset the inputs
        if (!search && !tags && !tag) {
          setSearchValue('');
          setSelectedTags([]);
        }
          }, [searchParams.toString()]);

  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [availableTags, setAvailableTags] = useState([]);
  const [selectedTags, setSelectedTags] = useState([]);
  const { isAuthenticated, isInitializing, logout } = useAuth();

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
    if (isAuthenticated) {
      navigate('/create');
    } else {
      navigate('/register');
    }
  };

  const toggleDropdown = async () => {
    const next = !dropdownOpen;
    setDropdownOpen(next);
    if (next && availableTags.length === 0) {
      try {
        const tags = await getTags();
        setAvailableTags(tags || []);
      } catch (err) {
        console.error('Failed to load tags', err);
      }
    }
  };

  const toggleSelectTag = (tagName) => {
    setSelectedTags(prev => {
      if (prev.includes(tagName)) return prev.filter(t => t !== tagName);
      return [...prev, tagName];
    });
  };

  const applyTagSearch = () => {
    if (selectedTags.length === 0) return;
    navigate(`/?tags=${encodeURIComponent(selectedTags.join(','))}`);
    setDropdownOpen(false);
  };

  const clearTagSelection = () => {
    setSelectedTags([]);
    navigate('/');
    setDropdownOpen(false);
  };

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  const handleBrandClick = (e) => {
    e.preventDefault();
    setSearchValue('');
    setSelectedTags([]);
    setDropdownOpen(false);
    navigate('/');
  };

  return (
    <nav className="navbar">
      <div className="navbar-left">
        <a href="/" onClick={handleBrandClick} className="navbar-brand">Lexicon</a>

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
            placeholder="Search articles..."
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
            🔍
          </button>
        </div>

        <div className="tag-search">
          <button type="button" className="btn" onClick={toggleDropdown} aria-expanded={dropdownOpen} aria-haspopup="listbox">
            Search by tags
          </button>

          {dropdownOpen && (
            <div className="tag-dropdown" role="dialog" aria-label="Search by tags">
              <div className="tag-list-scroll">
                {availableTags.length === 0 ? (
                  <div className="loading">Loading tags...</div>
                ) : (
                  availableTags.map(t => (
                    <label key={t.id} className="tag-checkbox">
                      <input
                        type="checkbox"
                        checked={selectedTags.includes(t.name)}
                        onChange={() => toggleSelectTag(t.name)}
                      />
                      <span>{t.name}</span>
                    </label>
                  ))
                )}
              </div>
              <div className="tag-actions">
                <button type="button" className="btn" onClick={applyTagSearch} disabled={selectedTags.length === 0}>Apply</button>
                <button type="button" className="btn btn-secondary" onClick={clearTagSelection}>Clear</button>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="navbar-right">
        {isAuthenticated ? (
          <button className="logout-button" onClick={handleLogout}>
            Logout
          </button>
        ) : !isInitializing ? (
          <>
            <Link to="/login" className="login-button">
              Login
            </Link>
            <Link to="/register" className="register-button">
              Register
            </Link>
          </>
        ) : null}
      </div>
    </nav>
  );
};

export default Navbar;
