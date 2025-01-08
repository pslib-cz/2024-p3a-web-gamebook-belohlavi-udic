import React, { createContext, useContext, useReducer, useCallback } from 'react';
import { GameService } from '../service/gameService';
import useAuth from '../hooks/useAuth';

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
    updateHP: (newHp: number) => void;
}

const GameContext = createContext<GameContextType | undefined>(undefined);

export function GameProvider({ children }: { children: React.ReactNode }) {
    const [state, dispatch] = useReducer(gameReducer, initialState);
    const { state: authState } = useAuth();

    const handleApiError = useCallback((error: unknown, navigate: (path: string) => void): string => {
        let errorMessage = 'An unknown error occurred';
        if (error instanceof Error) {
            errorMessage = error.message;
            if (errorMessage.includes('Unauthorized')) {
                navigate('/sign-in');
                return 'Session expired. Please log in again.';
            }
        } else if (typeof error === 'string') {
            errorMessage = error;
        }
        return errorMessage;
    }, []);

    const loadRoom = useCallback(async (roomId: number) => {
        if (!authState.token) {
            console.warn("Authentication token is missing.");
            return;
        }

        dispatch({ type: 'SET_LOADING', loading: true });
        try {
            const roomData = await GameService.getRoomById(roomId, authState.token);
            dispatch({ type: 'SET_ROOM', room: roomData });

            const connectionsData = await GameService.getConnections(roomId, authState.token);
            dispatch({ type: 'SET_CONNECTIONS', connections: connectionsData });

            dispatch({ type: 'SET_ERROR', error: null });
        } catch (error) {
            const errorMessage = handleApiError(error, (path) => {
                // Pøesmìrování na /sign-in v pøípadì chyby autentizace
                window.location.href = path;
            });
            dispatch({ type: 'SET_ERROR', error: errorMessage });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, [authState.token, handleApiError]);

    const makeMove = useCallback(async (connectionId: number) => {
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
            const errorMessage = handleApiError(error, (path) => {
                // Pøesmìrování na /sign-in v pøípadì chyby autentizace
                window.location.href = path;
            });
            dispatch({ type: 'SET_ERROR', error: errorMessage });
        } finally {
            dispatch({ type: 'SET_LOADING', loading: false });
        }
    }, [authState.token, loadRoom, handleApiError]);

    const updateHP = useCallback((newHp: number) => {
        dispatch({ type: 'UPDATE_HP', hp: newHp });
    }, []);

    return (
        <GameContext.Provider value={{ state, dispatch, loadRoom, makeMove, updateHP }}>
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