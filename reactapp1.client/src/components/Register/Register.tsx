import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import axios, { AxiosError } from 'axios';
import AuthForm from '../AuthForm/AuthForm';
import FormGroup from '../UI/FormGroup/FormGroup';
import styles from '../AuthForm/AuthForm.module.css';

// interface RegisterError {
//     message: string;
// }

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
            setError('Hesla se neshodují'); // Corrected
            return;
        }

        try {
            // Upravené požadavky na backend - malá písmena // Corrected
            await api.post('/auth/register', {
                username: username,      // změněno z Username // Corrected
                passwordHash: password   // změněno z PasswordHash // Corrected
            });
            navigate('/login');
        } catch (error) {
            if (axios.isAxiosError(error)) {
                console.error("Response error:", error.response?.data);
                const axiosError = error as AxiosError<any>;
                const errorMessage = typeof axiosError.response?.data === 'string'
                    ? axiosError.response.data
                    : 'Registrace selhala. Uživatelské jméno může být již obsazené.'; // Corrected
                setError(errorMessage);
            } else {
                setError('Došlo k neočekávané chybě'); // Corrected
            }
        }
    };

    const footerContent = (
        <div style={{ textAlign: 'center', marginTop: '20px' }}>
            Již máte účet?{" "} {/* Corrected */}
            <button
                onClick={() => navigate("/login")}
                style={{
                    background: 'none',
                    border: 'none',
                    color: 'var(--accent-gold)',
                    cursor: 'pointer',
                    textDecoration: 'underline'
                }}
            >
                Přihlásit se {/* Corrected */}
            </button>
        </div>
    );

    return (
        <AuthForm
            title="Registrace"
            onSubmit={handleSubmit}
            error={error}
            submitText="Zaregistrovat se"
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
            <FormGroup label="Potvrďte heslo" htmlFor="confirmPassword"> {/* Corrected */}
                <input
                    type="password"
                    id="confirmPassword"
                    className={styles.formInput}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    required
                />
            </FormGroup>
        </AuthForm>
    );
};

export default Register;
