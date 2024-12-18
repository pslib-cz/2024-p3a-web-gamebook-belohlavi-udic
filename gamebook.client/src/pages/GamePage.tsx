import { useEffect, useState } from "react";

function GamePage() {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [gameStarted, setGameStarted] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch("/current"); 
                if (!response.ok) {
                    throw new Error(`Error: ${response.status}`);
                }
                const result = await response.json();
                setData(result);
            } catch (err: any) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []);

    const handleStartGame = () => {
        if (data) {
            setGameStarted(true);
            console.log("Game started!", data); 
        }
    };

    if (loading) return <div>Loading...</div>;

    if (error) return <div>Error: {error}</div>;

    return (
        <div style={{ textAlign: "center", marginTop: "50px" }}>
            <h1>Game Page</h1>
            {!gameStarted ? (
                <>
                    <p>Click the button to start the game!</p>
                    <button onClick={handleStartGame}>Start Game</button>
                </>
            ) : (
                <div>
                    <h2>Game Running...</h2>
                    {}
                    <pre>{JSON.stringify(data, null, 2)}</pre>
                </div>
            )}
        </div>
    );
}

export default GamePage;
