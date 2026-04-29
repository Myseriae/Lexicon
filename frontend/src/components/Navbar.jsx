import React from 'react';
import CardNav from './CardNav/CardNav';

const Navbar = () => {
  const items = [
    {
      label: "Explore",
      bgColor: "#1B1722",
      textColor: "#fff",
      links: [
        { label: "Articles", ariaLabel: "View all articles", href: "/" },
        { label: "Topics", ariaLabel: "Browse topics", href: "#" }
      ]
    },
    {
      label: "Create", 
      bgColor: "#2F293A",
      textColor: "#fff",
      links: [
        { label: "New Article", ariaLabel: "Create a new article", href: "/create" },
        { label: "Guidelines", ariaLabel: "Writing guidelines", href: "#" }
      ]
    },
    {
      label: "Support",
      bgColor: "#2F293A", 
      textColor: "#fff",
      links: [
        { label: "Email", ariaLabel: "Email us", href: "#" },
        { label: "About", ariaLabel: "About Lexicon", href: "#" }
      ]
    }
  ];

  return (
    <CardNav
      logo="/logo.svg"
      logoAlt="Lexicon Logo"
      items={items}
      baseColor="rgba(255, 255, 255, 0.1)"
      menuColor="#fff"
      buttonBgColor="#007bff"
      buttonTextColor="#fff"
      ease="power3.out"
    />
  );
};

export default Navbar;
