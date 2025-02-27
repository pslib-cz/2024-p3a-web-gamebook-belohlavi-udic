import React, { ReactNode } from 'react';
import styles from './AuthForm.module.css';

interface AuthFormProps {
  title: string;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void | Promise<void>;
  error: string | null;
  children: ReactNode;
  submitText: string;
  footer: ReactNode;
}

const AuthForm: React.FC<AuthFormProps> = ({
  title,
  onSubmit,
  error,
  children,
  submitText,
  footer
}) => {
  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <h2 className={styles.title}>{title}</h2>
        <form className={styles.form} onSubmit={onSubmit}>
          {children}
          {error && <div className={styles.errorMessage}>{error}</div>}
          <button type="submit" className={styles.button}>
            {submitText}
          </button>
        </form>
        {footer}
      </div>
    </div>
  );
};

export default AuthForm;