import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import axios, { AxiosError } from 'axios';
import '../styles/auth.css';

interface RegisterError {
  message: string;
}

const Register: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError('Hesla se neshodují');
      return;
    }

    try {
      await api.post('/auth/register', { 
        Username: username, 
        PasswordHash: password 
      });
      navigate('/login');
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const axiosError = error as AxiosError<RegisterError>;
        const errorMessage = axiosError.response?.data?.message || 'Registrace selhala. Uživatelské jméno může být již obsazené.';
        setError(errorMessage);
      } else {
        setError('Došlo k neočekávané chybě');
      }
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-form">
        <h2 className="auth-title">Registrace</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Uživatelské jméno</label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Heslo</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="confirmPassword">Potvrďte heslo</label>
            <input
              type="password"
              id="confirmPassword"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>
          {error && <div className="error-message">{error}</div>}
          <button type="submit" className="auth-button">
            Zaregistrovat se
          </button>
        </form>
        <div className="auth-switch">
          Již máte účet?{" "}
          <button onClick={() => navigate("/login")}>
            Přihlásit se
          </button>
        </div>
      </div>
    </div>
  );
};

export default Register;