import React from 'react';
import './HealthBar.css';

interface BearHealthBarProps {
    hp: number;
    maxHp: number;
}

const BearHealthBar: React.FC<BearHealthBarProps> = ({ hp, maxHp }) => {
    const percentage = (hp / maxHp) * 100;
    
    return (
        <div className="health-bar-container bear-health">
            <div className="health-bar-label">Medvěd HP: {hp}/{maxHp}</div>
            <div className="health-bar-outer">
                <div 
                    className="health-bar-inner"
                    style={{
                        width: `${percentage}%`,
                        backgroundColor: '#e74c3c'
                    }}
                />
            </div>
        </div>
    );
};

export default BearHealthBar;