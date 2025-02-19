import React, { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";
import api from "../services/api";
import axios from "axios";
import '../styles/auth.css';

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    try {
      const response = await api.post("/auth/login", {
        username: username,
        passwordHash: password,
      });
      login(response.data.token);
      navigate('/main');
    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        setError(err.response.data || "Přihlášení selhalo");
      } else {
        setError("Došlo k neočekávané chybě");
      }
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-form">
        <h2 className="auth-title">Přihlášení</h2>
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
          {error && <div className="error-message">{error}</div>}
          <button type="submit" className="auth-button">
            Přihlásit se
          </button>
        </form>
        <div className="auth-switch">
          Nemáte účet?{" "}
          <button onClick={() => navigate("/register")}>
            Zaregistrovat se
          </button>
        </div>
      </div>
    </div>
  );
};

export default Login;