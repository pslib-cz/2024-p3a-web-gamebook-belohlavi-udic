import React from 'react';
import styles from './BearHealthBar.module.css';

interface BearHealthBarProps {
    hp: number;
    maxHp: number;
}

const BearHealthBar: React.FC<BearHealthBarProps> = ({ hp, maxHp }) => {
    const percentage = (hp / maxHp) * 100;
    
    return (
        <div className={styles.container}>
            <div className={styles.label}>Medvěd HP: {hp}/{maxHp}</div>
            <div className={styles.outer}>
                <div 
                    className={styles.inner}
                    style={{
                        width: `${percentage}%`
                    }}
                />
            </div>
        </div>
    );
};

export default BearHealthBar;