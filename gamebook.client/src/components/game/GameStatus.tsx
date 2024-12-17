import './GameStatus.css';

export const GameStatus = () => {
    return (
        <div className="game-status">
            <div className="health-indicator">
                <span>?? 100 HP</span>
            </div>
            <div className="location-indicator">
                <span>??? Aktu�ln� pozice</span>
            </div>
        </div>
    );
};