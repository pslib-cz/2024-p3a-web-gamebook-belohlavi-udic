import { FC } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const FrontPage: FC = () => {
    const { state } = useAuth();

    return (
        <div className="front-page">
            <h1>Gamebook</h1>
            {state.token ? (
                <Link to="/game">Hrát</Link>
            ) : (
                <div>
                    <Link to="/sign-in">Přihlásit</Link>
                    <Link to="/sign-up">Registrace</Link>
                </div>
            )}
        </div>
    );
};

export default FrontPage;