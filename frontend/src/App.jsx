import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { useState } from 'react';
import HomePage from './pages/HomePage';
import CreatePage from './pages/CreatePage';
import Navbar from './components/Navbar.jsx';
import ArticlePage from './pages/ArticlePage';
import LoginPage from './pages/LoginPage.jsx';
import RegisterPage from './pages/RegisterPage.jsx';

function App() {
  const [searchQuery, setSearchQuery] = useState('');

  return (
    <Router>
      <div className="app-container">

        <div className="app-content">
          <Navbar onSearch={setSearchQuery} />

          <div className="app-routes-container">
            <Routes>
              <Route path="/" element={<HomePage searchQuery={searchQuery} />} />
              <Route path="/create" element={<CreatePage />} />
              <Route path="/article/:id" element={<ArticlePage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
            </Routes>
          </div>
        </div>
      </div>
    </Router>
  );
}

export default App;