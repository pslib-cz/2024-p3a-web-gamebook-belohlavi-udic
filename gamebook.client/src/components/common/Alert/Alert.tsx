import styles from './Alert.module.css';

type AlertProps = {
  message: string;
  type: 'error' | 'success' | 'info' | 'warning';
};

const Alert = ({ message, type }: AlertProps) => {
    return (
        <div className={`${styles.alert} ${styles[`alert-${type}`]}`} role="alert">
        {message}
        </div>
    );
};

export default Alert;