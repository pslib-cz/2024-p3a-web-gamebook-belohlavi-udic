import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Alert } from "../components/common";
import "../styles/Auth.css";

const SignUpPage = () => {
    const [error, setError] = useState<Error | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const navigate = useNavigate();

    const registerUser = async (email: string, password: string) => {
        setLoading(true);
        try {
            const response = await fetch("/api/account/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email, password }),
            });

            if (!response.ok) {
                throw new Error("Registrace nebyla úspěšná");
            }

            navigate("/sign-in");
        } catch (error) {
            if (error instanceof Error) {
                setError(error);
            } else {
                setError(new Error("Registrace se nezdařila"));
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-page">
            <div className="auth-container">
                <h1>Registrace</h1>
                {error && <Alert message={error.message} type="error" />}
                <form
                    onSubmit={(event) => {
                        event.preventDefault();
                        const form = event.target as HTMLFormElement;
                        const email = form.email.value;
                        const password = form.password.value;
                        registerUser(email, password);
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
                            minLength={6}
                        />
                    </div>
                    <button
                        type="submit"
                        className="auth-button"
                        disabled={loading}
                    >
                        {loading ? "Registrace..." : "Registrovat"}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default SignUpPage;