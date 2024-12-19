import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useGame } from '../contexts/GameContext';
import { Alert } from '../components/common';
import useAuth from '../hooks/useAuth';
import { GameService } from '../service/gameService';

export default function RoomPage() {
    const { roomId } = useParams();
    const navigate = useNavigate();
    const { state, loadRoom } = useGame();
    const { state: authState } = useAuth();

    useEffect(() => {
        if (roomId) {
            const roomIdNumber = parseInt(roomId, 10);
            if (!isNaN(roomIdNumber)) {
                loadRoom(roomIdNumber);
            }
            else {
                console.error(`Invalid roomId ${roomId}`);
                navigate('/not-found'); // Handle invalid routes
            }

        }
    }, [roomId, loadRoom, navigate]);

    const handleAction = async (connectionId: number) => {
        if (!authState.token) {
            console.error("No token found!");
            return;
        }
        try {
            await GameService.movePlayer(connectionId, authState.token);
            if (state.roomData?.id)
                await loadRoom(state.roomData.id);
            else
                console.log("cannot move - the room was not loaded yet.")


        }
        catch (error) {
            if (error instanceof Error) {
                console.log(error.message);
            }
        }
    };

    if (state.loading) {
        return (
            <div className="flex justify-center items-center min-h-[50vh]">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-green-500"></div>
            </div>
        );
    }

    if (!state.roomData) {
        return <Alert message="Místnost nebyla nalezena" type="error" />;
    }

    return (
        <div className="game-container relative min-h-[80vh] flex flex-col">

            <div className="absolute top-4 left-4 z-10">
                <div className="bg-gray-800 p-2 rounded-lg shadow-lg">
                    <div className="text-sm text-gray-300 mb-1">Health</div>
                    <div className="w-32 h-4 bg-gray-700 rounded-full overflow-hidden">
                        <div
                            className="h-full bg-green-500 transition-all duration-300"
                            style={{ width: `${state.hp}%` }}
                        />
                    </div>
                </div>
            </div>


            <div className="flex-1 flex flex-col items-center justify-center p-8 text-center">
                <h1 className="text-3xl font-bold text-green-400 mb-6">
                    {state.roomData.name}
                </h1>

                <div className="max-w-2xl bg-gray-800 p-6 rounded-lg shadow-lg mb-8">
                    <p className="text-gray-300 text-lg">
                        {state.roomData.description}
                    </p>
                </div>


                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 w-full max-w-2xl">
                    {state.connections.map((connection) => (
                        <button
                            key={connection.id}
                            onClick={() => handleAction(connection.id)}
                            className="
                    px-6 py-3 
                    bg-green-600 hover:bg-green-700 
                    text-white font-medium rounded-lg 
                    transition-colors duration-200
                    shadow-lg hover:shadow-xl
                    disabled:opacity-50 disabled:cursor-not-allowed
                "
                            disabled={state.loading || state.hp <= 0}
                        >
                            {connection.connectionType}
                        </button>
                    ))}
                </div>


                {state.status === 'dead' && (
                    <div className="mt-8">
                        <Alert
                            type="error"
                            message="Zem?el jsi! Hra skon?ila."
                        />
                        <button
                            onClick={() => navigate('/start')}
                            className="mt-4 px-6 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg"
                        >
                            Za?ít znovu
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}