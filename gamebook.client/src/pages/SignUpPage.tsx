import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Alert } from "../components/common";

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
            const data = await response.json();
            navigate("/sign-in");
        } catch (error) {
            if (error instanceof Error) {
                setError(error);
            }
            else
            {
                setError(new Error("Registrace nebyla úspěšná"));
            }
        }
        finally {
            setLoading(false);
        }
    };
    
    return (
        <div>
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
        >
            <div>
                <label htmlFor="email">Email</label>
                <input type="email" id="email" name="email" required />
            </div>
            <div>
                <label htmlFor="password">Password</label>
                <input type="password" id="password" name="password" required />
            </div>
            <button type="submit" disabled={loading}>
                Registrace
            </button>
        </form>
        </div>
    );
}

export default SignUpPage;