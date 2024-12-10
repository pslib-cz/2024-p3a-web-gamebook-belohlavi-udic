import useAuth from '../hooks/useAuth';

const TokenPage = () => {
    const { state } = useAuth();
    return <pre>{JSON.stringify(state)}</pre>;
    }

export default TokenPage;
