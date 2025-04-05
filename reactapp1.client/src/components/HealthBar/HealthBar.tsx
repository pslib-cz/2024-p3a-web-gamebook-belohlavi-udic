import React from 'react';
import styles from './HealthBar.module.css';

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
        <div className={styles.container}>
            <div className={styles.label}>{hp} / {maxHp} HP</div>
            <div className={styles.outer}>
                <div
                    className={styles.inner}
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