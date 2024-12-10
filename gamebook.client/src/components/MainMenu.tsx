import useAuth from '../hooks/useAuth';
import { CLEAR_TOKEN } from '../providers/AuthProvider';
import { Link } from 'react-router-dom';

const MainMenu = () => {
    const { state, dispatch } = useAuth();
    return (
        <menu>
            <li><Link to="/">Titulní stránka</Link></li>
            <li><Link to="/token">Token</Link></li>
            <li><Link to="/start">Start</Link></li>
            {!state.token ?
            <>
            <li><Link to="/sign-in">Přihlášení</Link></li>
            <li><Link to="/sign-up">Registrace</Link></li>
            </>
            :
            <button onClick={() => dispatch({ type: CLEAR_TOKEN })}>Odhlásit</button>
            }  
        </menu>
    );
}

export default MainMenu;