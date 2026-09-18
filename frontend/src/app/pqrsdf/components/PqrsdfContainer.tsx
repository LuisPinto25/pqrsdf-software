'use client';

import React, { useState } from 'react';
import { PqrsdfForm } from './PqrsdfForm';
import { ConfirmationReceipt } from './ConfirmationReceipt';
import type { MakePqrsdfResponse } from '../types/pqrsdf';

export const PqrsdfContainer: React.FC = () => {
  const [submissionResult, setSubmissionResult] = useState<MakePqrsdfResponse | null>(null);

  if (submissionResult) {
    return (
      <ConfirmationReceipt data={submissionResult} onReset={() => setSubmissionResult(null)} />
    );
  }

  return <PqrsdfForm onSuccess={setSubmissionResult} />;
};
