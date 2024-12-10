import useAuth from '../hooks/useAuth';

const requireAuth = (Component: React.ComponentType) => {
    return (props: any) => {
        const { state } = useAuth();
        if (!state.token) {
            return <p>Musíte se přihlásit!</p>;
        }
        return <Component {...props} />;
    };
}

export default requireAuth;