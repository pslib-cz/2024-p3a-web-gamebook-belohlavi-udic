import React from 'react';
import './HealthBar.css';

interface HealthBarProps {
    hp: number;
    maxHp: number;
}

const HealthBar: React.FC<HealthBarProps> = ({ hp, maxHp }) => {
    const percentage = (hp / maxHp) * 100;
    
    const getHealthColor = () => {
        if (percentage > 70) return '#4CAF50';
        if (percentage > 30) return '#FFC107';
        return '#f44336';
    };

    return (
        <div className="health-bar-container">
            <div className="health-bar-label">{hp} / {maxHp} HP</div>
            <div className="health-bar-outer">
                <div 
                    className="health-bar-inner"
                    style={{
                        width: `${percentage}%`,
                        backgroundColor: getHealthColor()
                    }}
                />
            </div>
        </div>
    );
};

export default HealthBar;