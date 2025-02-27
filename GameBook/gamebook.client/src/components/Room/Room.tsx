import React, { useState, useEffect } from "react";
import { useGame } from "../../contexts/GameContext";
import { useNavigate } from 'react-router-dom';
import { MoveRight, MoveLeft, ArrowUp, 
         MoveHorizontal, RefreshCw, Home, Swords } from "lucide-react";
import HealthBar from '../HealthBar/HealthBar';
import BearHealthBar from '../BearHealthBar/BearHealthBar';
import styles from './Room.module.css';
import { Exit } from "../../types";

const Room: React.FC = () => {
    const navigate = useNavigate();
    const gameContext = useGame();
    const [buttonPosition, setButtonPosition] = useState<{ top: number; left: number }>({ top: 0, left: 0 });

    // Initialize button position
    useEffect(() => {
        if (gameContext?.currentRoom?.name === "Jeskyně s medvědem" && gameContext.isFighting) {
            randomizeButtonPosition();
        }
    }, [gameContext?.currentRoom?.name, gameContext?.isFighting]);

    // Function to randomize button position
    const randomizeButtonPosition = () => {
        const containerWidth = window.innerWidth - 200; // Subtracting button width
        const containerHeight = window.innerHeight - 100; // Subtracting button height
        
        // Safe margins to keep button visible
        const minTop = 200; // Keep it below title area
        const maxTop = containerHeight - 100;
        const minLeft = 20;
        const maxLeft = containerWidth - 200;
        
        // Generate random position within safe bounds
        const newTop = Math.floor(Math.random() * (maxTop - minTop) + minTop);
        const newLeft = Math.floor(Math.random() * (maxLeft - minLeft) + minLeft);
        
        setButtonPosition({ top: newTop, left: newLeft });
    };

    // Combined handler for attack and button movement
    const handleAttackAndMove = async () => {
        if (gameContext?.attackBear) {
            await gameContext.attackBear();
            randomizeButtonPosition();
        }
    };

    if (!gameContext) {
        return <div className={styles.loading}>Načítání...</div>;
    }

    const { 
        currentRoom, 
        player, 
        movePlayer, 
        isLoading, 
        error, 
        resetGame, 
        bearHP,
        isFighting 
    } = gameContext;

    if (isLoading) return <div className={styles.loading}>Načítání místnosti...</div>;
    if (error) return <div className={styles.error}>{error}</div>;
    if (!currentRoom) return <div className={styles.error}>Místnost není k dispozici.</div>;

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
                return <MoveRight className={styles.directionIcon} size={32} />;
            case 'Jít na západ':
            case 'Vrátit se':
            case 'Otočit se a jít zpět':
                return <MoveLeft className={styles.directionIcon} size={32} />;
            case 'Přeskočit past':
                return <ArrowUp className={styles.directionIcon} size={32} />;
            default:
                return <MoveHorizontal className={styles.directionIcon} size={32} />;
        }
    };

    return (
        <div className={styles.container} style={{
            backgroundImage: currentRoom.imageUrl ? `url('${currentRoom.imageUrl}')` : undefined,
        }}>
            {player && <HealthBar hp={player.hp} maxHp={100} />}
            {isBearRoom && isFighting && <BearHealthBar hp={bearHP} maxHp={500} />}
            
            <div className={styles.card}>
                <h2 className={styles.title}>{currentRoom.name}</h2>
                <p className={styles.description}>{currentRoom.description}</p>

                {!isGameOver && !isVictory && (
                    <div>
                        {isBearRoom && isFighting ? (
                            <>
                                <p className={styles.warningText}>SOUBOJ S MEDVĚDEM!</p>
                                <p className={styles.combatDescription}>
                                    Každou sekundu ztrácíš 10 HP! Útok způsobí 40-60 poškození medvědovi.
                                </p>
                                <div className={styles.bearFightContainer}>
                                    <button 
                                        onClick={handleAttackAndMove} 
                                        className={styles.fightButton}
                                        style={{
                                            position: 'absolute',
                                            top: `${buttonPosition.top}px`,
                                            left: `${buttonPosition.left}px`,
                                            zIndex: 100
                                        }}
                                    >
                                        <Swords className={styles.fightIcon} size={24} />
                                        <span>Útok! (-40-60 HP)</span>
                                    </button>
                                </div>
                            </>
                        ) : (
                            currentRoom.exits && currentRoom.exits.length > 0 && (
                                <>
                                    <p className={styles.warningText}>VYBER SI POKRAČOVÁNÍ HRY</p>
                                    <div className={styles.exitsGrid}>
                                        {currentRoom.exits.map((exit: Exit) => (
                                            <button
                                                key={exit.direction}
                                                onClick={() => movePlayer(exit.targetRoomId)}
                                                className={styles.directionCard}
                                            >
                                                <div className={styles.directionContent}>
                                                    {getDirectionIcon(exit.direction)}
                                                    <div className={styles.directionText}>
                                                        <div className={styles.directionMain}>{exit.direction}</div>
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

                <div className={styles.gameEndContainer}>
                    {isGameOver && (
                        <button onClick={resetGame} className={styles.restartButton}>
                            <RefreshCw className={styles.restartIcon} size={24} />
                            <span>Začít znovu</span>
                        </button>
                    )}
                    
                    {isVictory && (
                        <>
                            <button onClick={handleReturnToMenu} className={styles.menuButton}>
                                <Home className={styles.menuIcon} size={24} />
                                <span>Zpět do menu</span>
                            </button>
                            <button onClick={resetGame} className={styles.restartButton}>
                                <RefreshCw className={styles.restartIcon} size={24} />
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