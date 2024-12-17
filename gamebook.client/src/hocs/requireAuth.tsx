import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import useAuth from '../hooks/useAuth';

export const requireAuth = <P extends object>(
    WrappedComponent: React.ComponentType<P>
) => {
    return function WithAuthWrapper(props: P) {
        const { state } = useAuth();
        const navigate = useNavigate();

        useEffect(() => {
            if (!state.token) {
                navigate('/sign-in');
            }
        }, [state.token, navigate]);

        if (!state.token) {
            return null;
        }

        return <WrappedComponent {...props} />;
    };
};

export default requireAuth;