import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Alert } from "../components/common";
import useAuth from "../hooks/useAuth";
import { SET_TOKEN } from "../providers/AuthProvider";
import "../styles/Auth.css";

const SignInPage = () => {
    const [error, setError] = useState<Error | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const { dispatch } = useAuth();
    const navigate = useNavigate();

    const loginUser = async (email: string, password: string) => {
        setLoading(true);
        try {
            const response = await fetch("/api/account/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email, password }),
            });

            if (!response.ok) {
                throw new Error("Přihlášení nebylo úspěšné");
            }

            const data = await response.json();
            dispatch({ type: SET_TOKEN, token: data.accessToken });
            navigate("/game");
        } catch (error) {
            if (error instanceof Error) {
                setError(error);
            } else {
                setError(new Error("Přihlášení se nezdařilo"));
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-page">
            <div className="auth-container">
                <h1>Přihlášení</h1>
                {error && <Alert message={error.message} type="error" />}
                <form
                    onSubmit={(event) => {
                        event.preventDefault();
                        const form = event.target as HTMLFormElement;
                        const email = form.email.value;
                        const password = form.password.value;
                        loginUser(email, password);
                    }}
                    className="auth-form"
                >
                    <div className="form-group">
                        <label htmlFor="email">Email</label>
                        <input
                            type="email"
                            id="email"
                            name="email"
                            required
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="password">Heslo</label>
                        <input
                            type="password"
                            id="password"
                            name="password"
                            required
                        />
                    </div>
                    <button
                        type="submit"
                        className="auth-button"
                        disabled={loading}
                    >
                        {loading ? "Přihlašování..." : "Přihlásit"}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default SignInPage;