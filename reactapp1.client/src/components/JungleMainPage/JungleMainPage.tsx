import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import styles from './JungleMainPage.module.css';

const JungleMainPage: React.FC = () => {
    const navigate = useNavigate();
    const { logout } = useAuth();
    const [showInfo, setShowInfo] = useState(false);

    const startGame = () => {
        navigate('/game');
    };

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <div className={styles.container}>
            <div className={styles.backgroundImage} />

            <button className={styles.logoutButton} onClick={handleLogout}>
                Odhl�sit se
            </button>

            <div className={styles.contentOverlay}>
                <h1 className={styles.title}>�T�K Z D�UNGLE</h1>
                <p className={styles.subtitle}>C�lem hry je ut�ct z neprozkouman� d�ungle.</p>

                <div className={styles.buttonsContainer}>
                    <button onClick={startGame} className={styles.playButton}>
                        Hr�t
                    </button>

                    <button onClick={() => setShowInfo(true)} className={styles.infoButton}>
                        Info o h�e
                    </button>
                </div>
            </div>

            {showInfo && (
                <div className={styles.modalOverlay}>
                    <div className={styles.modalContent}>
                        <h2 className={styles.modalTitle}>O h�e</h2>
                        <p className={styles.modalText}>
                            V�tejte v adventure h�e �t�k z d�ungle! Va��m �kolem je naj�t cestu ven
                            z nebezpe�n� d�ungle. Budete �elit r�zn�m p�ek�k�m a rozhodnut�m,
                            kter� ovlivn� v� osud. Vyb�rejte moud�e!
                        </p>
                        <button onClick={() => setShowInfo(false)} className={styles.modalClose}>
                            Zav��t
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default JungleMainPage;