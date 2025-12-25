import { flipBit } from '../utils/binaryUtils';
import styles from './BinaryInput.module.css';

interface BinaryInputProps {
  value: string;
  bitCount: 12 | 23;
  onChange: (binary: string) => void;
  highlightPositions?: number[];
  disabled?: boolean;
  label?: string;
  variant?: 'edit' | 'error';
  originalValue?: string;
}

export default function BinaryInput({
  value,
  bitCount,
  onChange,
  highlightPositions = [],
  disabled = false,
  label,
  variant = 'edit',
  originalValue
}: BinaryInputProps) {
  // Normalize value to correct length
  const normalizedValue = value.padStart(bitCount, '0').slice(0, bitCount);
  const normalizedOriginal = originalValue?.padStart(bitCount, '0').slice(0, bitCount);

  const handleBitClick = (position: number) => {
    if (disabled) return;

    try {
      const newBinary = flipBit(normalizedValue, position);
      onChange(newBinary);
    } catch (error) {
      console.error(`Error flipping bit at position ${position}:`, error);
    }
  };

  // Group bits into chunks of 4 for readability
  const bitGroups: number[][] = [];
  for (let i = bitCount - 1; i >= 0; i -= 4) {
    const group: number[] = [];
    for (let j = Math.min(3, i); j >= 0; j--) {
      const position = i - j;
      if (position >= 0) {
        group.push(position);
      }
    }
    if (group.length > 0) {
      bitGroups.push(group);
    }
  }

  return (
    <div className={styles.container}>
      {label && <div className={styles.label}>{label}</div>}

      <div className={styles.bitContainer}>
        {bitGroups.map((group, groupIdx) => (
          <div key={groupIdx} className={styles.bitGroup}>
            {group.map(position => {
              const bitIndex = bitCount - 1 - position;
              const bitValue = normalizedValue[bitIndex];
              const originalBitValue = normalizedOriginal?.[bitIndex];
              const isManuallyChanged = normalizedOriginal && bitValue !== originalBitValue;
              const isHighlighted = highlightPositions.includes(position);

              return (
                <button
                  key={position}
                  type="button"
                  className={`${styles.bit} ${bitValue === '1' ? styles.active : ''} ${
                    isHighlighted ? styles.highlighted : ''
                  } ${variant === 'edit' ? styles.editVariant : styles.errorVariant} ${
                    isManuallyChanged ? styles.manuallyChanged : ''
                  }`}
                  onClick={() => handleBitClick(position)}
                  disabled={disabled}
                  title={`Position ${position}: ${bitValue} (click to toggle)`}
                  aria-label={`Bit ${position}, value ${bitValue}`}
                >
                  {bitValue}
                </button>
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
}
