import React, { createContext, useState, useEffect, useRef, ReactNode, useContext } from 'react';
import api from '../services/api';

interface Exit {
    id: number;
    direction: string;
    targetRoomId: number;
}

interface Room {
    id: number;
    name: string;
    description: string;
    imagePath: string;
    imageUrl?: string;
    exits: Exit[];
}

interface Player {
    id: number;
    hp: number;
    currentRoomId: number;
}

interface GameContextType {
    currentRoom: Room | null;
    player: Player | null;
    isLoading: boolean;
    error: string | null;
    movePlayer: (roomId: number) => Promise<void>;
    resetGame: () => Promise<void>;
    updatePlayerHp: (newHp: number) => Promise<void>;
    attackBear: () => Promise<void>;
    bearHP: number;
    isFighting: boolean;
}

export const GameContext = createContext<GameContextType | null>(null);

export const GameProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [currentRoom, setCurrentRoom] = useState<Room | null>(null);
    const [player, setPlayer] = useState<Player | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [bearHP, setBearHP] = useState(500);
    const [isFighting, setIsFighting] = useState(false);
    const intervalRef = useRef<number | null>(null);

    const updatePlayerHp = async (newHp: number) => {
        if (!player) return;
        try {
            const response = await api.put(`/players/${player.id}/hp`, { hp: newHp });
            setPlayer(response.data);
            
            if (newHp <= 0) {
                if (intervalRef.current) {
                    window.clearInterval(intervalRef.current);
                }
                await movePlayer(9); 
            }
        } catch (err) {
            console.error('Error updating HP:', err);
            setError('Failed to update HP');
        }
    };

    const fetchRoomData = async (roomId: number) => {
        try {
            setIsLoading(true);
            const response = await api.get<Room>(`/rooms/${roomId}`);
            
            if (response.data) {
                const processedRoom = {
                    ...response.data,
                    imageUrl: `https://localhost:7227${response.data.imagePath}`
                };
                setCurrentRoom(processedRoom);
                
                
                if (processedRoom.name === "Jeskyně s medvědem") {
                    setIsFighting(true);
                    setBearHP(500);
                } else {
                    setIsFighting(false);
                }
                
                setError(null);
            }
        } catch (err) {
            console.error('Error fetching room:', err);
            setError('Failed to load room data');
        } finally {
            setIsLoading(false);
        }
    };

    const movePlayer = async (roomId: number) => {
        if (!player) return;

        try {
            setIsLoading(true);
            await api.put(`/players/${player.id}/move/${roomId}`);
            const playerResponse = await api.get<Player>(`/players/${player.id}`);
            setPlayer(playerResponse.data);
            await fetchRoomData(roomId);
        } catch (err) {
            console.error('Error moving player:', err);
            setError('Failed to move player');
        } finally {
            setIsLoading(false);
        }
    };

    const attackBear = async () => {
        if (!player || !isFighting) return;
        
        const damage = Math.floor(Math.random() * 21) + 40;
        const newBearHP = Math.max(0, bearHP - damage);
        setBearHP(newBearHP);

        if (newBearHP <= 0) {
            if (intervalRef.current) {
                window.clearInterval(intervalRef.current);
            }
            setIsFighting(false);
            await movePlayer(7);
        }
    };

    const resetGame = async () => {
        try {
            setIsLoading(true);
            if (intervalRef.current) {
                window.clearInterval(intervalRef.current);
            }
            await api.put(`/players/1/reset`);
            const playerResponse = await api.get<Player>(`/players/1`);
            setPlayer(playerResponse.data);
            await fetchRoomData(1);
            setBearHP(500);
            setIsFighting(false);
            setError(null);
        } catch (err) {
            console.error('Error resetting game:', err);
            setError('Failed to reset game');
        } finally {
            setIsLoading(false);
        }
    };

  
    useEffect(() => {
        if (currentRoom?.name === "Jeskyně s medvědem" && player && player.hp > 0) {
            
            intervalRef.current = window.setInterval(async () => {
                if (player) {
                    const newHp = player.hp - 10;
                    await updatePlayerHp(newHp);
                }
            }, 1000);
        }

        
        return () => {
            if (intervalRef.current) {
                window.clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        };
    }, [currentRoom?.name, player?.hp]);

    
    useEffect(() => {
        const initializeGame = async () => {
            try {
                setIsLoading(true);
                const playerResponse = await api.get<Player>('/players/1');
                setPlayer(playerResponse.data);
                await fetchRoomData(playerResponse.data.currentRoomId);
            } catch (err) {
                console.error('Error initializing game:', err);
                setError('Failed to initialize game');
            } finally {
                setIsLoading(false);
            }
        };

        initializeGame();

        return () => {
            if (intervalRef.current) {
                window.clearInterval(intervalRef.current);
            }
        };
    }, []);

    return (
        <GameContext.Provider value={{
            currentRoom,
            player,
            isLoading,
            error,
            movePlayer,
            resetGame,
            updatePlayerHp,
            attackBear,
            bearHP,
            isFighting
        }}>
            {children}
        </GameContext.Provider>
    );
};

export const useGame = (): GameContextType => {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error('useGame must be used within a GameProvider');
    }
    return context;
};