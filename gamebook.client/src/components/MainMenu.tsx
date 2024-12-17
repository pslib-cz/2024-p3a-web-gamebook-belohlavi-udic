// src/components/MainMenu.tsx
import { Link } from 'react-router-dom';
import useAuth from '../hooks/useAuth';
import { CLEAR_TOKEN } from '../providers/AuthProvider';
import './MainMenu.css';

const MainMenu = () => {
    const { state, dispatch } = useAuth();

    return (
        <nav className="main-menu">
            <Link to="/" className="menu-brand">
                GameBook
            </Link>

            <div className="menu-items">
                {state.token ? (
                    <>
                        <Link to="/game" className="menu-item">
                            Continue Game
                        </Link>
                        <Link to="/start" className="menu-item">
                            New Game
                        </Link>
                        <button
                            className="menu-button"
                            onClick={() => dispatch({ type: CLEAR_TOKEN })}
                        >
                            Sign Out
                        </button>
                    </>
                ) : (
                    <>
                        <Link to="/sign-in" className="menu-item">
                            Sign In
                        </Link>
                        <Link to="/sign-up" className="menu-item">
                            Sign Up
                        </Link>
                    </>
                )}
            </div>
        </nav>
    );
};

export default MainMenu;