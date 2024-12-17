import { createContext, FC, PropsWithChildren, useReducer } from 'react';

export const SET_TOKEN = 'SET_TOKEN';
export const CLEAR_TOKEN = 'CLEAR_TOKEN';

type AuthState = {
    token: string | null;
};

export type AuthAction =
    | { type: typeof SET_TOKEN; token: string }
    | { type: typeof CLEAR_TOKEN };

export type AuthContextType = {
    state: AuthState;
    dispatch: React.Dispatch<AuthAction>;
};

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
    switch (action.type) {
        case SET_TOKEN:
            return { ...state, token: action.token };
        case CLEAR_TOKEN:
            return { ...state, token: null };
        default:
            return state;
    }
};

const initialState: AuthState = {
    token: null
};

export const AuthContext = createContext<AuthContextType>({
    state: initialState,
    dispatch: () => null
});

export const AuthProvider: FC<PropsWithChildren> = ({ children }) => {
    const [state, dispatch] = useReducer(authReducer, initialState);

    return (
        <AuthContext.Provider value={{ state, dispatch }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthProvider;