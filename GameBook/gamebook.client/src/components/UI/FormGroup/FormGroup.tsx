import React, { ReactNode } from 'react';
import styles from '../../AuthForm/AuthForm.module.css';

interface FormGroupProps {
  label: string;
  htmlFor: string;
  children: ReactNode;
}

const FormGroup: React.FC<FormGroupProps> = ({ label, htmlFor, children }) => {
  return (
    <div className={styles.formGroup}>
      <label htmlFor={htmlFor} className={styles.formLabel}>
        {label}
      </label>
      {children}
    </div>
  );
};

export default FormGroup;