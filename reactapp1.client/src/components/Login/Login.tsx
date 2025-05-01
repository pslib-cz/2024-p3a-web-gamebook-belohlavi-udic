import React, { useState } from "react";
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import api from "../../services/api";
import axios from "axios";
import AuthForm from "../AuthForm/AuthForm";
import FormGroup from "../UI/FormGroup/FormGroup";
import styles from "../AuthForm/AuthForm.module.css";

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

            console.log(response.data)

            navigate('/main');
        } catch (err) {
            if (axios.isAxiosError(err) && err.response) {
                setError(err.response.data || "Přihlášení selhalo"); // Corrected
            } else {
                setError("Došlo k neočekávané chybě"); // Corrected
            }
        }
    };

    const footerContent = (
        <div style={{ textAlign: 'center', marginTop: '20px' }}>
            Nemáte účet?{" "} {/* Corrected */}
            <button
                onClick={() => navigate("/register")}
                style={{
                    background: 'none',
                    border: 'none',
                    color: 'var(--accent-gold)',
                    cursor: 'pointer',
                    textDecoration: 'underline'
                }}
            >
                Zaregistrovat se
            </button>
        </div>
    );

    return (
        <AuthForm
            title="Přihlášení" // Corrected
            onSubmit={handleSubmit}
            error={error}
            submitText="Přihlásit se" // Corrected
            footer={footerContent}
        >
            <FormGroup label="Uživatelské jméno" htmlFor="username"> {/* Corrected */}
                <input
                    type="text"
                    id="username"
                    className={styles.formInput}
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                />
            </FormGroup>
            <FormGroup label="Heslo" htmlFor="password">
                <input
                    type="password"
                    id="password"
                    className={styles.formInput}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
            </FormGroup>
        </AuthForm>
    );
};

export default Login;
