import React from "react";
import { useGame } from "../context/GameContext";
import { useNavigate } from 'react-router-dom';
import { MoveRight, MoveLeft, ArrowUp, 
         MoveHorizontal, RefreshCw, Home, Swords } from "lucide-react";
import HealthBar from './HealthBar';
import BearHealthBar from './BearHealthBar';
import './Room.css';

interface Exit {
    id: number;
    direction: string;
    targetRoomId: number;
}

const Room: React.FC = () => {
    const navigate = useNavigate();
    const gameContext = useGame();

    if (!gameContext) {
        return <div className="loading">Načítání...</div>;
    }

    const { 
        currentRoom, 
        player, 
        movePlayer, 
        isLoading, 
        error, 
        resetGame, 
        attackBear,
        bearHP,
        isFighting 
    } = gameContext;

    if (isLoading) return <div className="loading">Načítání místnosti...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!currentRoom) return <div className="error">Místnost není k dispozici.</div>;

    const isGameOver = currentRoom.name === "Konec hry";
    const isVictory = currentRoom.name === "Bezpečná zóna";
    const isBearRoom = currentRoom.name === "Jeskyně s medvědem";

    const handleReturnToMenu = () => {
        navigate('/main');
    };

    const getDirectionIcon = (direction: string) => {
        switch (direction) {
            case 'Jít na východ':
            case 'Přejít most rychle':
                return <MoveRight className="direction-icon" size={32} />;
            case 'Jít na západ':
            case 'Vrátit se':
            case 'Otočit se a jít zpět':
                return <MoveLeft className="direction-icon" size={32} />;
            case 'Přeskočit past':
                return <ArrowUp className="direction-icon" size={32} />;
            default:
                return <MoveHorizontal className="direction-icon" size={32} />;
        }
    };

    return (
        <div className="room-container" style={{
            backgroundImage: currentRoom.imageUrl ? `url('${currentRoom.imageUrl}')` : undefined,
        }}>
            {player && <HealthBar hp={player.hp} maxHp={100} />}
            {isBearRoom && isFighting && <BearHealthBar hp={bearHP} maxHp={500} />}
            
            <div className="room-card">
                <h2 className="room-title">{currentRoom.name}</h2>
                <p className="room-description">{currentRoom.description}</p>

                {!isGameOver && !isVictory && (
                    <div className="exits-container">
                        {isBearRoom && isFighting ? (
                            <>
                                <p className="warning-text">SOUBOJ S MEDVĚDEM!</p>
                                <p className="combat-description">
                                    Každou sekundu ztrácíš 10 HP! Útok způsobí 40-60 poškození medvědovi.
                                </p>
                                <div className="bear-fight-controls">
                                    <button onClick={attackBear} className="fight-button">
                                        <Swords className="fight-icon" size={24} />
                                        <span>Útok! (-40-60 HP)</span>
                                    </button>
                                </div>
                            </>
                        ) : (
                            currentRoom.exits && currentRoom.exits.length > 0 && (
                                <>
                                    <p className="warning-text">VYBER SI POKRAČOVÁNÍ HRY</p>
                                    <div className="exits-grid">
                                        {currentRoom.exits.map((exit: Exit) => (
                                            <button
                                                key={exit.direction}
                                                onClick={() => movePlayer(exit.targetRoomId)}
                                                className="direction-card"
                                            >
                                                <div className="direction-content">
                                                    {getDirectionIcon(exit.direction)}
                                                    <div className="direction-text">
                                                        <div className="direction-main">{exit.direction}</div>
                                                    </div>
                                                </div>
                                            </button>
                                        ))}
                                    </div>
                                </>
                            )
                        )}
                    </div>
                )}

                <div className="game-end-container">
                    {isGameOver && (
                        <button onClick={resetGame} className="restart-button">
                            <RefreshCw className="restart-icon" size={24} />
                            <span>Začít znovu</span>
                        </button>
                    )}
                    
                    {isVictory && (
                        <>
                            <button onClick={handleReturnToMenu} className="menu-button">
                                <Home className="menu-icon" size={24} />
                                <span>Zpět do menu</span>
                            </button>
                            <button onClick={resetGame} className="restart-button">
                                <RefreshCw className="restart-icon" size={24} />
                                <span>Hrát znovu</span>
                            </button>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Room;