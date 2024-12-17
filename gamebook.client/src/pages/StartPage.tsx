import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGame } from '../contexts/GameContext';
import { Alert } from '../components/common';

export default function StartPage() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();
    const { loadRoom } = useGame();

    const startNewGame = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await fetch('/api/players/current', {
                method: 'GET',
            });

            if (!response.ok) {
                throw new Error('Nepoda?ilo se za?ít novou hru');
            }

            const data = await response.json();
            await loadRoom(data.currentRoomID);
            navigate(`/room/${data.currentRoomID}`);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Nastala neznámá chyba');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-[80vh] flex items-center justify-center">
            <div className="max-w-2xl w-full text-center p-8">
                <h1 className="text-4xl font-bold text-green-400 mb-6">
                    Vítej v dobrodružství
                </h1>

                <div className="bg-gray-800 p-8 rounded-lg shadow-xl mb-8">
                    <p className="text-gray-300 text-lg mb-6">
                        Vydej se na nebezpe?nou cestu plnou výzev, souboj? a tajemství.
                        Tvá rozhodnutí ur?í tv?j osud.
                    </p>

                    {error && (
                        <div className="mb-6">
                            <Alert type="error" message={error} />
                        </div>
                    )}

                    <div className="flex flex-col gap-4">
                        <button
                            onClick={startNewGame}
                            disabled={loading}
                            className="
                px-8 py-4 
                bg-green-600 hover:bg-green-700 
                text-white text-xl font-bold rounded-lg 
                transition-all duration-200
                transform hover:scale-105
                disabled:opacity-50 disabled:cursor-not-allowed
                disabled:hover:scale-100
              "
                        >
                            {loading ? (
                                <span className="flex items-center justify-center">
                                    <svg className="animate-spin h-5 w-5 mr-3" viewBox="0 0 24 24">
                                        <circle
                                            className="opacity-25"
                                            cx="12"
                                            cy="12"
                                            r="10"
                                            stroke="currentColor"
                                            strokeWidth="4"
                                        />
                                        <path
                                            className="opacity-75"
                                            fill="currentColor"
                                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                        />
                                    </svg>
                                    Na?ítání...
                                </span>
                            ) : (
                                'Za?ít nové dobrodružství'
                            )}
                        </button>

                        <p className="text-gray-500 text-sm">
                            Pozor: Za?átek nové hry resetuje tv?j p?edchozí postup.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}