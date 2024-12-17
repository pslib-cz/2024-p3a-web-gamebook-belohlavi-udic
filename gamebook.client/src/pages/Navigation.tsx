import { Link } from 'react-router-dom';
import useAuth from '../hooks/useAuth';
import { CLEAR_TOKEN } from '../providers/AuthProvider';

export const Navigation = () => {
    const { state, dispatch } = useAuth();

    const handleSignOut = () => {
        dispatch({ type: CLEAR_TOKEN });
    };

    return (
        <nav className="navigation">
            <Link to="/" className="nav-brand">GameBook</Link>
            <div className="nav-links">
                {state.token ? (
                    <>
                        <Link to="/game">Hrát</Link>
                        <button onClick={handleSignOut}>Odhlásit</button>
                    </>
                ) : (
                    <>
                        <Link to="/sign-in">Přihlásit</Link>
                        <Link to="/sign-up">Registrace</Link>
                    </>
                )}
            </div>
        </nav>
    );
};

export default Navigation;