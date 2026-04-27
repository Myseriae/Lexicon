import React from 'react';
import './Modal.css';

const Modal = ({ isOpen, onClose, message, type, onConfirm }) => {
  if (!isOpen) return null;

  const handleConfirm = () => {
    if (onConfirm) onConfirm();
    onClose();
  };

  const handleCancel = () => {
    onClose();
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <p className="modal-message">{message}</p>
        <div className="modal-actions">
          {type === 'confirm' ? (
            <>
              <button onClick={handleConfirm} className="btn btn-confirm">Yes</button>
              <button onClick={handleCancel} className="btn btn-cancel">No</button>
            </>
          ) : (
            <button onClick={handleCancel} className="btn">OK</button>
          )}
        </div>
      </div>
    </div>
  );
};

export default Modal;
