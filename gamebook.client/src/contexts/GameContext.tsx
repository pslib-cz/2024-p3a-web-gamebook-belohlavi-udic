import React, { createContext, useContext, useReducer, useCallback } from 'react';

interface Room {
    id: number;
    name: string;
    description: string;
    exits?: string;
}

interface Connection {
    id: number;
    roomId1: number;
    roomId2: number;
    connectionType: string;
}

interface GameState {
    currentRoom: number | null;
    hp: number;
    status: string;
    connections: Connection[];
    roomData: Room | null;
    loading: boolean;
    error: string | null;
}

type GameAction =
    | { type: 'SET_ROOM'; room: Room }
    | { type: 'SET_CONNECTIONS'; connections: Connection[] }
    | { type: 'UPDATE_HP'; hp: number }
    | { type: 'SET_STATUS'; status: string }
    | { type: 'SET_LOADING'; loading: boolean }
    | { type: 'SET_ERROR'; error: string | null };

const initialState: GameState = {
    currentRoom: null,
    hp: 100,
    status: 'active',
    connections: [],
    roomData: null,
    loading: false,
    error: null
};

function gameReducer(state: GameState, action: GameAction): GameState {
    switch (action.type) {
        case 'SET_ROOM':
            return { ...state, roomData: action.room, currentRoom: action.room.id };
        case 'SET_CONNECTIONS':
            return { ...state, connections: action.connections };
        case 'UPDATE_HP':
            return { ...state, hp: action.hp };
        case 'SET_STATUS':
            return { ...state, status: action.status };
        case 'SET_LOADING':
            return { ...state, loading: action.loading };
        case 'SET_ERROR':
            return { ...state, error: action.error };
        default:
            return state;
    }
}

interface GameContextType {
    state: GameState;
    dispatch: React.Dispatch<GameAction>;
    loadRoom: (roomId: number) => Promise<void>;
    makeMove: (connectionId: number) => Promise<void>;
}

const GameContext = createContext<GameContextType | undefined>(undefined);

export function GameProvider({ children }: { children: React.ReactNode }) {
    const [state, dispatch] = useReducer(gameReducer, initialState);

    const handleApiError = (error: unknown): string => {
        if (error instanceof Error) return error.message;
        if (typeof error === 'string') return error;
        return 'An unknown error occurred';
    };

    const loadRoom = useCallback(async (roomId: number) => {
        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const response = await fetch(`/api/rooms/${roomId}`);
            if (!response.ok) throw new Error('Failed to load room');
            const roomData = await response.json();
            dispatch({ type: 'SET_ROOM', room: roomData });

            const connectionsResponse = await fetch(`/api/connections?roomId=${roomId}`);
            if (!connectionsResponse.ok) throw new Error('Failed to load connections');
            const connectionsData = await connectionsResponse.json();
            dispatch({ type: 'SET_CONNECTIONS', connections: connectionsData });

            dispatch({ type: 'SET_ERROR', error: null });
        } catch (error) {
            dispatch({ type: 'SET_ERROR', error: handleApiError(error) });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, []);

    const makeMove = useCallback(async (connectionId: number) => {
        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const response = await fetch('/api/players/current/move', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ connectionId }),
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to make move');
            }

            const data = await response.json();
            dispatch({ type: 'UPDATE_HP', hp: data.currentHP });
            dispatch({ type: 'SET_STATUS', status: data.status });

            await loadRoom(data.newRoomId);
            dispatch({ type: 'SET_ERROR', error: null });
        } catch (error) {
            dispatch({ type: 'SET_ERROR', error: handleApiError(error) });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, [loadRoom]);

    return (
        <GameContext.Provider value={{ state, dispatch, loadRoom, makeMove }}>
            {children}
        </GameContext.Provider>
    );
}

export function useGame() {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error('useGame must be used within a GameProvider');
    }
    return context;
}