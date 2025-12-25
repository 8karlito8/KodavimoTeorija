import styles from './BinaryDisplay.module.css';

interface BinaryDisplayProps {
  value: string;
  bitCount: 12 | 23;
  highlightPositions?: number[];
  label?: string;
  showPositions?: boolean;
  displayMode?: 'normal' | 'errorPattern';
}

export default function BinaryDisplay({
  value,
  bitCount,
  highlightPositions = [],
  label,
  showPositions = false,
  displayMode = 'normal'
}: BinaryDisplayProps) {
  // Normalize value to correct length
  const normalizedValue = value.padStart(bitCount, '0').slice(0, bitCount);

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
              const isHighlighted = highlightPositions.includes(position);
              const isErrorBit = displayMode === 'errorPattern' && bitValue === '1';

              return (
                <div
                  key={position}
                  className={`${styles.bit} ${bitValue === '1' ? styles.active : ''} ${
                    isHighlighted ? styles.highlighted : ''
                  } ${isErrorBit ? styles.errorBit : ''}`}
                  title={`Position ${position}: ${bitValue}`}
                >
                  {bitValue}
                  {showPositions && (
                    <div className={styles.position}>{position}</div>
                  )}
                </div>
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
}
