//import './App.css'
import './App.css'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import HomePage from './pages/HomePage'
import CreatePage from './pages/CreatePage'
import Navbar from './components/navbar'
import LightRays from './components/LightRays/LightRays'
import ArticlePage from './pages/ArticlePage'

function App() {
  return (
    <Router>
      <div className="app-container">
        <div className="app-background">
          <LightRays
            raysOrigin="top-center"
            raysColor="#00ffff"
            raysSpeed={1.5}
            lightSpread={0.8}
            rayLength={1.2}
            followMouse={true}
            mouseInfluence={0.1}
            noiseAmount={0.1}
            distortion={0.05}
          />
        </div>
        <div className="app-content">
          <Navbar />
          <div className="app-routes-container">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/create" element={<CreatePage />} />
              <Route path="/article/:id" element={<ArticlePage />} />
            </Routes>
          </div>
        </div>
      </div>
    </Router>
  )
}

export default App
