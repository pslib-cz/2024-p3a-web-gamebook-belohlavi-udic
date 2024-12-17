import './GameStatus.css';

export const GameStatus = () => {
    return (
        <div className="game-status">
            <div className="health-indicator">
                <span>?? 100 HP</span>
            </div>
            <div className="location-indicator">
                <span>??? Aktuální pozice</span>
            </div>
        </div>
    );
};