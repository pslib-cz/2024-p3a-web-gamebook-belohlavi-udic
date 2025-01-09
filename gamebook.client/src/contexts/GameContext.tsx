import React, { createContext, useContext, useReducer, useCallback } from 'react';
import { GameService } from '../service/gameService';
import useAuth from '../hooks/useAuth';
import { useNavigate } from 'react-router-dom';

// Interfaces for data structures
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

// Actions for the reducer
type GameAction =
    | { type: 'SET_ROOM'; room: Room }
    | { type: 'SET_CONNECTIONS'; connections: Connection[] }
    | { type: 'UPDATE_HP'; hp: number }
    | { type: 'SET_STATUS'; status: string }
    | { type: 'SET_LOADING'; loading: boolean }
    | { type: 'SET_ERROR'; error: string | null };

// Initial state for the game
const initialState: GameState = {
    currentRoom: null,
    hp: 100,
    status: 'active',
    connections: [],
    roomData: null,
    loading: false,
    error: null
};

// Reducer function to handle state changes
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

// Interface for the context
interface GameContextType {
    state: GameState;
    dispatch: React.Dispatch<GameAction>;
    loadRoom: (roomId: number) => Promise<void>;
    makeMove: (connectionId: number) => Promise<void>;
    updateHP: (newHp: number) => void;
}

// Creating the context
const GameContext = createContext<GameContextType | undefined>(undefined);

// GameProvider component
export function GameProvider({ children }: { children: React.ReactNode }) {
    const [state, dispatch] = useReducer(gameReducer, initialState);
    const { state: authState } = useAuth();
    const navigate = useNavigate();

    const handleApiError = useCallback((error: unknown): string => {
        let errorMessage = 'An unknown error occurred';
        if (error instanceof Error) {
            errorMessage = error.message;
            if (errorMessage.includes('Unauthorized')) {
                // If unauthorized, remove the token and redirect to sign-in
                localStorage.removeItem('access_token');
                navigate('/sign-in');
                return 'Session expired. Please log in again.';
            }
        } else if (typeof error === 'string') {
            errorMessage = error;
        }

        console.error("GameContext: API Error Handled:", errorMessage);

        return errorMessage;
    }, [navigate]);

    const loadRoom = useCallback(async (roomId: number) => {
        console.log("GameContext: loadRoom called with roomId:", roomId);

        if (!authState.token) {
            console.warn("GameContext: Authentication token is missing.");
            return;
        }

        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const roomData = await GameService.getRoomById(roomId, authState.token);
            dispatch({ type: 'SET_ROOM', room: roomData });

            const connectionsData = await GameService.getConnections(roomId, authState.token);

            // Debugging: Log the connections data
            console.log("GameContext: Connections data received:", connectionsData);

            // Check if connectionsData is an array before dispatching
            if (Array.isArray(connectionsData)) {
                dispatch({ type: 'SET_CONNECTIONS', connections: connectionsData });
            } else {
                console.error("GameContext: Invalid connections data received (not an array).");
                dispatch({ type: 'SET_ERROR', error: 'Invalid connections data received.' });
            }

            dispatch({ type: 'SET_ERROR', error: null });
        } catch (error) {
            const errorMessage = handleApiError(error);
            dispatch({ type: 'SET_ERROR', error: errorMessage });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, [authState.token, handleApiError]);

    const makeMove = useCallback(async (connectionId: number) => {
        console.log("GameContext: makeMove called with connectionId:", connectionId);
        if (!authState.token) {
            console.warn("Authentication token is missing.");
            return;
        }

        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const moveResult = await GameService.movePlayer(connectionId, authState.token);
            dispatch({ type: 'UPDATE_HP', hp: moveResult.playerHp });
            dispatch({ type: 'SET_STATUS', status: moveResult.playerStatus });

            await loadRoom(moveResult.newRoomId);
            dispatch({ type: 'SET_ERROR', error: null });
        } catch (error) {
            const errorMessage = handleApiError(error);
            dispatch({ type: 'SET_ERROR', error: errorMessage });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, [authState.token, loadRoom, handleApiError]);

    const updateHP = useCallback((newHp: number) => {
        console.log("GameContext: updateHP called with newHp:", newHp);
        dispatch({ type: 'UPDATE_HP', hp: newHp });
    }, []);

    return (
        <GameContext.Provider value={{ state, dispatch, loadRoom, makeMove, updateHP }}>
            {children}
        </GameContext.Provider>
    );
}

// Custom hook to use the game context
export function useGame() {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error('useGame must be used within a GameProvider');
    }
    return context;
}