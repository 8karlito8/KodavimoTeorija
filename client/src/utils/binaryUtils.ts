/**
 * Binary Utilities for Golay Code Demo
 *
 * Provides conversion and manipulation functions for binary/decimal values
 * used in the error-correcting code demonstration.
 */

/**
 * Convert decimal number to binary string with padding.
 *
 * @param decimal - The decimal number to convert (must be non-negative)
 * @param bitCount - Number of bits in output (12 for message, 23 for codeword)
 * @returns Binary string with leading zeros (e.g., "000000101010")
 * @throws {Error} If decimal exceeds bitCount range or is negative
 *
 * @example
 * decimalToBinary(42, 12)  // Returns "000000101010"
 * decimalToBinary(7, 12)   // Returns "000000000111"
 * decimalToBinary(4095, 12) // Returns "111111111111"
 */
export function decimalToBinary(decimal: number, bitCount: number): string {
  if (decimal < 0) {
    throw new Error(`Value ${decimal} must be non-negative`);
  }

  const maxValue = Math.pow(2, bitCount) - 1;
  if (decimal > maxValue) {
    throw new Error(`Value ${decimal} exceeds ${bitCount}-bit range (max: ${maxValue})`);
  }

  return decimal.toString(2).padStart(bitCount, '0');
}

/**
 * Convert binary string to decimal number.
 *
 * @param binary - Binary string (e.g., "101010")
 * @returns Decimal number
 * @throws {Error} If binary string contains invalid characters
 *
 * @example
 * binaryToDecimal("101010")  // Returns 42
 * binaryToDecimal("000000101010")  // Returns 42
 * binaryToDecimal("111111111111")  // Returns 4095
 */
export function binaryToDecimal(binary: string): number {
  if (!/^[01]+$/.test(binary)) {
    throw new Error('Binary string must contain only 0 and 1');
  }

  return parseInt(binary, 2);
}

/**
 * Validate binary string format.
 *
 * @param binary - Binary string to validate
 * @param expectedLength - Expected bit count (optional)
 * @returns Validation result with error message if invalid
 *
 * @example
 * validateBinary("101010")  // { valid: true }
 * validateBinary("10201")   // { valid: false, error: "Only 0 and 1 allowed" }
 * validateBinary("1010", 12) // { valid: false, error: "Expected 12 bits, got 4" }
 */
export function validateBinary(
  binary: string,
  expectedLength?: number
): { valid: boolean; error?: string } {
  if (!binary || binary.length === 0) {
    return { valid: false, error: 'Binary string cannot be empty' };
  }

  if (!/^[01]+$/.test(binary)) {
    return { valid: false, error: 'Only 0 and 1 allowed' };
  }

  if (expectedLength !== undefined && binary.length !== expectedLength) {
    return {
      valid: false,
      error: `Expected ${expectedLength} bits, got ${binary.length}`
    };
  }

  return { valid: true };
}

/**
 * Flip a bit at a specific position in a binary string.
 *
 * Positions are 0-indexed from the RIGHT (LSB = position 0).
 *
 * @param binary - Binary string
 * @param position - Bit position to flip (0 = rightmost bit)
 * @returns New binary string with flipped bit
 * @throws {Error} If position is out of range
 *
 * @example
 * flipBit("101010", 0)  // Returns "101011" (flip rightmost bit)
 * flipBit("101010", 1)  // Returns "101000" (flip second bit from right)
 * flipBit("101010", 3)  // Returns "101110" (flip fourth bit from right)
 */
export function flipBit(binary: string, position: number): string {
  if (position < 0 || position >= binary.length) {
    throw new Error(`Position ${position} out of range for ${binary.length}-bit string`);
  }

  const bits = binary.split('');
  const index = bits.length - 1 - position;
  bits[index] = bits[index] === '0' ? '1' : '0';
  return bits.join('');
}

/**
 * Set a bit at a specific position to a specific value.
 *
 * Positions are 0-indexed from the RIGHT (LSB = position 0).
 *
 * @param binary - Binary string
 * @param position - Bit position to set (0 = rightmost bit)
 * @param value - Value to set (0 or 1)
 * @returns New binary string with bit set to value
 * @throws {Error} If position is out of range or value is invalid
 *
 * @example
 * setBit("101010", 0, 1)  // Returns "101011"
 * setBit("101010", 1, 0)  // Returns "101000"
 */
export function setBit(binary: string, position: number, value: 0 | 1): string {
  if (position < 0 || position >= binary.length) {
    throw new Error(`Position ${position} out of range for ${binary.length}-bit string`);
  }

  if (value !== 0 && value !== 1) {
    throw new Error(`Value must be 0 or 1, got ${value}`);
  }

  const bits = binary.split('');
  const index = bits.length - 1 - position;
  bits[index] = value.toString();
  return bits.join('');
}

/**
 * Get the bit value at a specific position.
 *
 * Positions are 0-indexed from the RIGHT (LSB = position 0).
 *
 * @param binary - Binary string
 * @param position - Bit position (0 = rightmost bit)
 * @returns Bit value (0 or 1)
 * @throws {Error} If position is out of range
 *
 * @example
 * getBit("101010", 0)  // Returns 0
 * getBit("101010", 1)  // Returns 1
 */
export function getBit(binary: string, position: number): 0 | 1 {
  if (position < 0 || position >= binary.length) {
    throw new Error(`Position ${position} out of range for ${binary.length}-bit string`);
  }

  const index = binary.length - 1 - position;
  return binary[index] === '1' ? 1 : 0;
}

/**
 * Pad or trim binary string to exact length.
 *
 * @param binary - Binary string
 * @param length - Desired length
 * @returns Binary string of exact length (padded with leading zeros or trimmed from left)
 *
 * @example
 * normalizeBinary("1010", 8)    // Returns "00001010"
 * normalizeBinary("11110000", 4) // Returns "0000" (trimmed from left)
 */
export function normalizeBinary(binary: string, length: number): string {
  if (binary.length > length) {
    // Trim from left
    return binary.slice(binary.length - length);
  } else {
    // Pad with leading zeros
    return binary.padStart(length, '0');
  }
}
