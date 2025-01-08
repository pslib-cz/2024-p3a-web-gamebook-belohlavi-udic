import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useAuth from "../hooks/useAuth";
import { GameService } from "../service/gameService";

function GamePage() {
    const { state: authState } = useAuth();
    const navigate = useNavigate();
    const [gameStarted, setGameStarted] = useState(false);

    const handleStartGame = async () => {
        if (authState.token) {
            setGameStarted(true);
            try {
                const newGame = await GameService.startNewGame(authState.token);
                const initialRoomId = newGame.initialRoomId;

                navigate(`/room/${initialRoomId}`);
            } catch (error) {
                console.error("Error starting the game:", error);
            }
        } else {
            console.error('Not logged in, cannot start a new game');
        }
    };

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
                </div>
            )}
        </div>
    );
}

export default GamePage;