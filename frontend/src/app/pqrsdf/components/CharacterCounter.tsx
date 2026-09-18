'use client';

import React from 'react';

interface CharacterCounterProps {
  currentLength: number;
  maxLength: number;
  minLength?: number;
}

export const CharacterCounter: React.FC<CharacterCounterProps> = ({
  currentLength,
  maxLength,
  minLength,
}) => {
  const isNearLimit = maxLength - currentLength <= 20;
  const isOverLimit = currentLength > maxLength;
  const isUnderMin = minLength !== undefined && currentLength > 0 && currentLength < minLength;

  let colorClass = 'text-gray-500';
  if (isOverLimit) {
    colorClass = 'text-red-600 font-semibold';
  } else if (isNearLimit) {
    colorClass = 'text-amber-600';
  } else if (isUnderMin) {
    colorClass = 'text-blue-600';
  }

  return (
    <div
      className={`text-xs mt-1 text-right transition-colors duration-150 ${colorClass}`}
      aria-live="polite"
    >
      <span>
        {currentLength} / {maxLength} caracteres
      </span>
      {minLength && currentLength < minLength && (
        <span className="ml-2 font-medium">(mínimo {minLength})</span>
      )}
    </div>
  );
};
