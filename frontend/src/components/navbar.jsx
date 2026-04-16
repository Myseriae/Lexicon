import React from 'react';
import { Link } from 'react-router-dom';

const Navbar = () => {
  return (
    <nav style={{ 
      display: 'flex', 
      justifyContent: 'space-between', 
      alignItems: 'center', 
      padding: '1rem', 
      backgroundColor: '#f8f9fa', 
      borderBottom: '1px solid #dee2e6' 
    }}>
      <Link to="/" style={{ 
        fontSize: '1.5rem', 
        fontWeight: 'bold', 
        textDecoration: 'none', 
        color: '#333' 
      }}>
        Lexicon
      </Link>
      <Link to="/create" style={{ 
        padding: '0.5rem 1rem', 
        backgroundColor: '#007bff', 
        color: 'white', 
        textDecoration: 'none', 
        borderRadius: '4px' 
      }}>
        Create
      </Link>
    </nav>
  );
};

export default Navbar;
