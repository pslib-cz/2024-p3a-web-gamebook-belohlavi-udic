import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { GameProvider } from './contexts/GameContext';
import Login from './components/Login/Login';
import Register from './components/Register/Register';
import GamePage from './pages/GamePage';
import JungleMainPage from './components/JungleMainPage/JungleMainPage';
import './styles/global.css';

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <GameProvider>
          <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/main" element={
              <PrivateRoute>
                <JungleMainPage />
              </PrivateRoute>
            } />
            <Route path="/game" element={
              <PrivateRoute>
                <GamePage />
              </PrivateRoute>
            } />
          </Routes>
        </GameProvider>
      </AuthProvider>
    </BrowserRouter>
  );
};

interface PrivateRouteProps {
  children: React.ReactNode;
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children }) => {
  const { isLoggedIn } = useAuth();
  return isLoggedIn ? <>{children}</> : <Navigate to="/login" replace />;
};

export default App;