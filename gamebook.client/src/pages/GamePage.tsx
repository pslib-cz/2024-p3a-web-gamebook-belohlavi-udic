import { useState, useEffect } from 'react';
import useAuth from '../hooks/useAuth';
import { requireAuth } from '../hocs/requireAuth';
import { GameStatus } from '../components/game/GameStatus';
import { RoomView } from '../components/game/RoomView';
import { Alert } from '../components/common/Alert/Alert';
import '../styles/Game.css';

interface Room {
    id: number;
    name: string;
    description: string;
}

interface Connection {
    id: number;
    roomId1: number;
    roomId2: number;
    connectionType: string;
}

const GamePage = () => {
    const { state } = useAuth();
    const [currentRoom, setCurrentRoom] = useState<Room | null>(null);
    const [connections, setConnections] = useState<Connection[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadRoomData();
    }, []);

    const loadRoomData = async () => {
        try {
            setLoading(true);
            const response = await fetch('/api/rooms/current', {
                headers: {
                    'Authorization': `Bearer ${state.token}`
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load room data');
            }

            const data = await response.json();
            setCurrentRoom(data.room);
            setConnections(data.connections);
        } catch (err) {
            setError('Nepodařilo se načíst herní data');
        } finally {
            setLoading(false);
        }
    };

    const handleMove = async (connectionId: number) => {
        try {
            setLoading(true);
            const response = await fetch(`/api/connections/${connectionId}/move`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${state.token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Failed to move');
            }

            await loadRoomData();
        } catch (err) {
            setError('Nepodařilo se přesunout do další místnosti');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return <div className="game-loading">Načítám...</div>;
    }

    if (error) {
        return <Alert message={error} type="error" />;
    }

    if (!currentRoom) {
        return <div className="game-error">Místnost nenalezena</div>;
    }

    return (
        <div className="game-page">
            <GameStatus />
            <RoomView
                room={currentRoom}
                connections={connections}
                onMove={handleMove}
            />
        </div>
    );
};

export default requireAuth(GamePage);