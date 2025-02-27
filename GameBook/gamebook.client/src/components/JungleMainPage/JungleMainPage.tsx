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
        Odhlásit se
      </button>

      <div className={styles.contentOverlay}>
        <h1 className={styles.title}>ÚTĚK Z DŽUNGLE</h1>
        <p className={styles.subtitle}>Cílem hry je utéct z neprozkoumané džungle.</p>

        <div className={styles.buttonsContainer}>
          <button onClick={startGame} className={styles.playButton}>
            Hrát
          </button>

          <button onClick={() => setShowInfo(true)} className={styles.infoButton}>
            Info o hře
          </button>
        </div>
      </div>

      {showInfo && (
        <div className={styles.modalOverlay}>
          <div className={styles.modalContent}>
            <h2 className={styles.modalTitle}>O hře</h2>
            <p className={styles.modalText}>
              Vítejte v adventure hře Útěk z džungle! Vaším úkolem je najít cestu ven 
              z nebezpečné džungle. Budete čelit různým překážkám a rozhodnutím, 
              která ovlivní váš osud. Vybírejte moudře!
            </p>
            <button onClick={() => setShowInfo(false)} className={styles.modalClose}>
              Zavřít
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default JungleMainPage;