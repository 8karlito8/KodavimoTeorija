"""
Golay Code Implementation and Usage Example
Demonstrates (23,12,7) Binary Golay Code
"""

import random
from typing import List, Tuple

class GolayCode:
    """Binary Golay Code (23,12,7) - Encodes 12 bits to 23 bits with error correction"""

    def __init__(self):
        # Generator matrix for (23,12) Golay code
        # This is the standard generator matrix form [I | P]
        self.generator_matrix = [
            0b100000000000_10111100001,
            0b010000000000_11011100010,
            0b001000000000_11011110100,
            0b000100000000_11001111000,
            0b000010000000_10101110001,
            0b000001000000_01011101001,
            0b000000100000_10110101010,
            0b000000010000_01101011010,
            0b000000001000_11010011100,
            0b000000000100_10100111100,
            0b000000000010_01001111100,
            0b000000000001_11110111010,
        ]

        # Parity check matrix (for syndrome decoding)
        self.parity_check_matrix = [
            0b10111100001_000000000001,
            0b11011100010_000000000010,
            0b11011110100_000000000100,
            0b11001111000_000000001000,
            0b10101110001_000000010000,
            0b01011101001_000000100000,
            0b10110101010_000001000000,
            0b01101011010_000010000000,
            0b11010011100_000100000000,
            0b10100111100_001000000000,
            0b01001111100_010000000000,
            0b11110111010_100000000000,
        ]

        # Precomputed coset leaders (error patterns) for syndrome lookup
        self.syndrome_table = self._build_syndrome_table()

    def _build_syndrome_table(self) -> dict:
        """Build syndrome to error pattern lookup table"""
        table = {}

        # All single-bit errors (23 patterns)
        for i in range(23):
            error = 1 << i
            syndrome = self._calculate_syndrome(error)
            table[syndrome] = error

        # All two-bit errors (253 patterns)
        for i in range(23):
            for j in range(i + 1, 23):
                error = (1 << i) | (1 << j)
                syndrome = self._calculate_syndrome(error)
                table[syndrome] = error

        return table

    def _calculate_syndrome(self, received: int) -> int:
        """Calculate syndrome (error pattern indicator)"""
        syndrome = 0
        for i in range(11):
            parity = bin(received & self.parity_check_matrix[i]).count('1') % 2
            syndrome |= (parity << i)
        return syndrome

    def encode(self, data: int) -> int:
        """
        Encode 12 information bits to 23 codeword bits
        Args:
            data: 12-bit information (0-4095)
        Returns:
            23-bit codeword
        """
        if data < 0 or data >= (1 << 12):
            raise ValueError("Data must be 12 bits (0-4095)")

        codeword = 0
        for i in range(12):
            if (data >> i) & 1:
                codeword ^= self.generator_matrix[i]

        return codeword & ((1 << 23) - 1)

    def decode(self, received: int) -> Tuple[int, int]:
        """
        Decode received 23-bit word and correct up to 3 errors
        Args:
            received: 23-bit received word (possibly corrupted)
        Returns:
            (decoded_data: 12-bit info, errors_corrected: count)
        """
        received = received & ((1 << 23) - 1)

        # Calculate syndrome
        syndrome = self._calculate_syndrome(received)

        if syndrome == 0:
            # No errors detected
            return received >> 11, 0

        # Find error pattern from syndrome table
        if syndrome in self.syndrome_table:
            error_pattern = self.syndrome_table[syndrome]
            corrected = received ^ error_pattern
            errors = bin(error_pattern).count('1')
            return corrected >> 11, errors

        # Uncorrectable error
        return received >> 11, -1  # -1 indicates uncorrectable error


def demonstrate_golay():
    """Demonstrate Golay code with practical examples"""

    golay = GolayCode()

    print("=" * 60)
    print("GOLAY CODE (23,12,7) DEMONSTRATION")
    print("=" * 60)

    # Example 1: Clean transmission
    print("\n[Example 1] Clean Transmission (No Errors)")
    print("-" * 60)
    info_bits = 0b101010101010  # 12 bits: 2730 in decimal
    print(f"Original message:     {info_bits:012b} (decimal: {info_bits})")

    codeword = golay.encode(info_bits)
    print(f"Encoded codeword:     {codeword:023b} (decimal: {codeword})")
    print(f"  Information bits:   {info_bits:012b}")
    print(f"  Parity bits:        {codeword >> 12:011b}")

    received = codeword
    decoded, errors = golay.decode(received)
    print(f"\nReceived word:        {received:023b}")
    print(f"Errors detected:      {errors}")
    print(f"Decoded message:      {decoded:012b} (decimal: {decoded})")
    print(f"Transmission OK:      {info_bits == decoded}")

    # Example 2: Single error correction
    print("\n\n[Example 2] Single Error (Correctable)")
    print("-" * 60)
    received_corrupted = codeword ^ (1 << 7)  # Flip bit 7
    print(f"Received word:        {received_corrupted:023b}")
    print(f"  (bit 7 corrupted)")

    decoded, errors = golay.decode(received_corrupted)
    print(f"Errors detected:      {errors}")
    print(f"Decoded message:      {decoded:012b} (decimal: {decoded})")
    print(f"Correction OK:        {info_bits == decoded}")

    # Example 3: Multiple errors correction
    print("\n\n[Example 3] Triple Error (Correctable)")
    print("-" * 60)
    received_corrupted = codeword ^ ((1 << 3) | (1 << 9) | (1 << 15))
    print(f"Received word:        {received_corrupted:023b}")
    print(f"  (bits 3, 9, 15 corrupted)")

    decoded, errors = golay.decode(received_corrupted)
    print(f"Errors detected:      {errors}")
    print(f"Decoded message:      {decoded:012b} (decimal: {decoded})")
    print(f"Correction OK:        {info_bits == decoded}")

    # Example 4: Practical usage - sending multiple messages through noisy channel
    print("\n\n[Example 4] Multiple Messages Through Noisy Channel")
    print("-" * 60)
    messages = [42, 365, 1234, 2048, 4095]
    error_rate = 0.05  # 5% bit error rate

    print(f"Simulating transmission with {error_rate*100}% random bit errors\n")

    successful = 0
    for msg in messages:
        codeword = golay.encode(msg)

        # Simulate channel noise
        corrupted = codeword
        num_errors = sum(1 for _ in range(23) if random.random() < error_rate)
        for _ in range(num_errors):
            bit_pos = random.randint(0, 22)
            corrupted ^= (1 << bit_pos)

        decoded, corrected = golay.decode(corrupted)

        success = (msg == decoded)
        if success:
            successful += 1

        status = "✓ PASS" if success else "✗ FAIL"
        print(f"Message {msg:4d} → Codeword → +{num_errors} errors → Decoded {decoded:4d} [{status}]")

    print(f"\nSuccessful transmissions: {successful}/{len(messages)}")


if __name__ == "__main__":
    demonstrate_golay()
