import React, { createContext, useContext, useReducer, useCallback } from 'react';

interface Challenge {
    id: number;
    type: string;
    description: string;
    successOutcome: string;
    failureOutcome: string;
}

interface ChallengeState {
    currentChallenge: Challenge | null;
    isActive: boolean;
    loading: boolean;
    error: string | null;
    outcome: 'success' | 'failure' | null;
}

type ChallengeAction =
    | { type: 'SET_CHALLENGE'; challenge: Challenge }
    | { type: 'START_CHALLENGE' }
    | { type: 'END_CHALLENGE'; outcome: 'success' | 'failure' }
    | { type: 'SET_LOADING'; loading: boolean }
    | { type: 'SET_ERROR'; error: string | null }
    | { type: 'RESET' };

const initialState: ChallengeState = {
    currentChallenge: null,
    isActive: false,
    loading: false,
    error: null,
    outcome: null
};

function challengeReducer(state: ChallengeState, action: ChallengeAction): ChallengeState {
    switch (action.type) {
        case 'SET_CHALLENGE':
            return { ...state, currentChallenge: action.challenge, error: null };
        case 'START_CHALLENGE':
            return { ...state, isActive: true, outcome: null };
        case 'END_CHALLENGE':
            return { ...state, isActive: false, outcome: action.outcome };
        case 'SET_LOADING':
            return { ...state, loading: action.loading };
        case 'SET_ERROR':
            return { ...state, error: action.error };
        case 'RESET':
            return initialState;
        default:
            return state;
    }
}

interface ChallengeContextType {
    state: ChallengeState;
    startChallenge: (challengeId: number) => Promise<void>;
    attemptChallenge: () => Promise<void>;
}

const ChallengeContext = createContext<ChallengeContextType | undefined>(undefined);

export function ChallengeProvider({ children }: { children: React.ReactNode }) {
    const [state, dispatch] = useReducer(challengeReducer, initialState);

    const startChallenge = useCallback(async (challengeId: number) => {
        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const response = await fetch(`/api/challenges/${challengeId}`);
            if (!response.ok) throw new Error('Failed to load challenge');

            const challenge = await response.json();
            dispatch({ type: 'SET_CHALLENGE', challenge });
            dispatch({ type: 'START_CHALLENGE' });
        } catch (error) {
            dispatch({ type: 'SET_ERROR', error: error instanceof Error ? error.message : 'Unknown error' });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, []);

    const attemptChallenge = useCallback(async () => {
        if (!state.currentChallenge) return;

        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const response = await fetch(`/api/challenges/${state.currentChallenge.id}/attempt`, {
                method: 'POST'
            });

            if (!response.ok) throw new Error('Failed to process challenge attempt');

            const result = await response.json();
            dispatch({ type: 'END_CHALLENGE', outcome: result.success ? 'success' : 'failure' });
        } catch (error) {
            dispatch({ type: 'SET_ERROR', error: error instanceof Error ? error.message : 'Unknown error' });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, [state.currentChallenge]);

    return (
        <ChallengeContext.Provider value={{ state, startChallenge, attemptChallenge }}>
            {children}
        </ChallengeContext.Provider>
    );
}

export function useChallenge() {
    const context = useContext(ChallengeContext);
    if (!context) {
        throw new Error('useChallenge must be used within a ChallengeProvider');
    }
    return context;
}