import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { GameProvider } from './context/GameContext';
import LoginPage from './components/Login';
import RegisterPage from './components/Register';
import GamePage from './pages/GamePage';
import JungleMainPage from './components/JungleMainPage';

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <GameProvider>
          <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
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

const PrivateRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isLoggedIn } = useAuth();
  return isLoggedIn ? <>{children}</> : <Navigate to="/login" replace />;
};

export default App;