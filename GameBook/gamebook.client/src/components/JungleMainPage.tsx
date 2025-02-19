import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './JungleMainPage.css';

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
    <div className="jungle-container">
      <div className="background-image" />

      <button className="logout-button" onClick={handleLogout}>
        Odhlásit se
      </button>

      <div className="content-overlay">
        <h1 className="game-title">ÚTĚK Z DŽUNGLE</h1>
        <p className="game-subtitle">Cílem hry je utéct z neprozkoumané džungle.</p>

        <div className="buttons-container">
          <button onClick={startGame} className="game-button play-button">
            Hrát
          </button>

          <button onClick={() => setShowInfo(true)} className="game-button info-button">
            Info o hře
          </button>
        </div>
      </div>

      {showInfo && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h2 className="modal-title">O hře</h2>
            <p className="modal-text">
              Vítejte v adventure hře Útěk z džungle! Vaším úkolem je najít cestu ven 
              z nebezpečné džungle. Budete čelit různým překážkám a rozhodnutím, 
              která ovlivní váš osud. Vybírejte moudře!
            </p>
            <button onClick={() => setShowInfo(false)} className="modal-close">
              Zavřít
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default JungleMainPage;