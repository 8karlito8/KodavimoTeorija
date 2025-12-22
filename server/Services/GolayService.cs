using server.Data;

namespace server.Services
{
    /// <summary>
    /// ╔═══════════════════════════════════════════════════════════════════════════╗
    /// ║                    GOLAY CODE SERVICE                                      ║
    /// ║                                                                            ║
    /// ║  Implements encoding and decoding for Golay (23,12,7) code                ║
    /// ║  with syndrome-based error correction using IMLD algorithm                ║
    /// ╚═══════════════════════════════════════════════════════════════════════════╝
    ///
    /// GOLAY CODE BASICS:
    /// ==================
    /// - Encodes 12 information bits into 23 coded bits
    /// - Adds 11 parity bits for error detection/correction
    /// - Can correct up to 3 bit errors (perfect code)
    /// - Minimum distance: 7 (hence can detect 6 errors, correct 3)
    ///
    /// ENCODING FORMULA: c = m × G (in GF(2), which is binary/XOR arithmetic)
    /// Where:
    ///   m = message vector (12 bits)
    ///   G = generator matrix (12×23) = [I | P̂]
    ///   c = codeword vector (23 bits)
    ///
    /// DECODING ALGORITHM (from literatura12.pdf):
    /// ==========================================
    /// Uses Algorithm 3.7.1 which relies on Algorithm 3.6.1 (IMLD for C24)
    ///
    /// The key insight is that C23 is obtained by "puncturing" C24 (removing
    /// the last bit). So to decode C23, we:
    /// 1. Extend the received word to 24 bits (add parity bit)
    /// 2. Decode using IMLD algorithm for C24
    /// 3. Remove the last bit to get the C23 codeword
    ///
    /// REFERENCES:
    /// ===========
    /// - literatura12.pdf: §3.5-3.7 (pages 82-89)
    /// - Algorithm 3.6.1: IMLD for C24 (page 85)
    /// - Algorithm 3.7.1: Decoding for C23 (page 88)
    /// </summary>
    public class GolayService
    {
        private readonly GolayMatrices _matrices;
        private static readonly Random _random = new Random();

        public GolayService(GolayMatrices matrices)
        {
            _matrices = matrices;
        }

        #region ═══════════════════════════ PUBLIC API METHODS ═══════════════════════════

        public int[][] GetParityMatrix()
        {
            return _matrices.GetParityMatrix();
        }

        public int[][] GetIdentityMatrix()
        {
            return _matrices.GetIdentityMatrix();
        }

        public int[][] GetBMatrix()
        {
            return _matrices.GetBMatrix();
        }

        /// <summary>
        /// Constructs the Generator Matrix G = [I | P̂] for C23
        ///
        /// SYSTEMATIC CODE STRUCTURE:
        /// ==========================
        /// The generator matrix has a special form: G = [I₁₂ | P̂₁₂ₓ₁₁]
        /// - First 12 columns: Identity matrix (I₁₂)
        ///   This makes the code "systematic" - the message appears directly in the codeword
        /// - Last 11 columns: Parity matrix (P̂₁₂ₓ₁₁)
        ///   These columns determine the 11 parity check bits
        ///
        /// RESULT: 12×23 matrix
        /// Row i represents: "How to encode when bit i of the message is 1"
        /// </summary>
        public int[][] GetGeneratorMatrix()
        {
            var identity = _matrices.IdentityMatrix;
            var parity = _matrices.ParityMatrix;
            var generator = new int[12][];

            for (int i = 0; i < 12; i++)
            {
                generator[i] = new int[23];

                // Copy identity matrix (first 12 columns)
                // These ensure the message bits appear directly in positions 0-11
                for (int j = 0; j < 12; j++)
                {
                    generator[i][j] = identity[i][j];
                }

                // Concatenate parity matrix (columns 12-22)
                // These generate the 11 parity check bits
                for (int j = 0; j < 11; j++)
                {
                    generator[i][j + 12] = parity[i][j];
                }
            }

            return generator;
        }

        /// <summary>
        /// Encodes a 12-bit message into a 23-bit Golay codeword.
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ ENCODING PROCESS                                                    │
        /// │                                                                     │
        /// │   Message (12 bits)    Generator Matrix G (12×23)                  │
        /// │   [m₀ m₁ ... m₁₁]  ×  [I₁₂ | P̂]  =  Codeword (23 bits)           │
        /// │                                       [m₀ m₁ ... m₁₁ | p₀ ... p₁₀] │
        /// │                        ↑              ↑               ↑            │
        /// │                        │              │               │            │
        /// │                   12×23 matrix    Message bits    Parity bits      │
        /// └─────────────────────────────────────────────────────────────────────┘
        ///
        /// ALGORITHM: Matrix-Vector Multiplication in GF(2)
        /// ================================================
        /// c = m × G, where all arithmetic is modulo 2 (XOR)
        ///
        /// For each bit position i in the message (i = 0 to 11):
        ///   - If message bit i is 1:
        ///     - XOR row i of generator matrix into the codeword
        ///
        /// WHY IT WORKS:
        /// In GF(2), addition = XOR. Each row of G represents the contribution
        /// of one message bit. XORing rows where message bits are 1 gives the
        /// final codeword.
        ///
        /// EXAMPLE:
        /// Message: 000000000101 (decimal 5, bits 0 and 2 are set)
        /// Result: XOR(row[0], row[2]) = codeword
        /// </summary>
        /// <param name="message">12-bit message (0-4095)</param>
        /// <returns>23-bit encoded codeword</returns>
        public int Encode(int message)
        {
            // ─────────────────────────────────────────────────────────────────
            // VALIDATION: Ensure message is exactly 12 bits
            // (1 << 12) = 2^12 = 4096, so valid range is 0-4095
            // ─────────────────────────────────────────────────────────────────
            if (message < 0 || message >= (1 << 12))
            {
                throw new ArgumentException("Message must be 12 bits (0-4095)", nameof(message));
            }

            int codeword = 0;
            var generator = GetGeneratorMatrix();

            // ─────────────────────────────────────────────────────────────────
            // MATRIX MULTIPLICATION in GF(2):
            // For each bit position in the message, if that bit is 1,
            // XOR the corresponding row of G into the codeword
            // ─────────────────────────────────────────────────────────────────
            for (int i = 0; i < 12; i++)
            {
                // Check if bit i of message is 1
                // (message & (1 << i)) isolates bit i
                if ((message & (1 << i)) != 0)
                {
                    // XOR row i of generator matrix into codeword
                    for (int j = 0; j < 23; j++)
                    {
                        if (generator[i][j] == 1)
                        {
                            // Flip bit j in codeword (XOR with 1)
                            codeword ^= 1 << j;
                        }
                    }
                }
            }

            return codeword;
        }

        /// <summary>
        /// Decodes a 23-bit Golay codeword with error correction.
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ DECODING PROCESS (Algorithm 3.7.1 from literatura12.pdf)            │
        /// │                                                                     │
        /// │   Received word w (23 bits) - may contain up to 3 errors            │
        /// │           │                                                         │
        /// │           ▼                                                         │
        /// │   ┌───────────────────┐                                             │
        /// │   │ Step 1: Extend    │  Add parity bit to make 24-bit word         │
        /// │   │ to C24 (w0 or w1) │  Choose the one with ODD weight             │
        /// │   └─────────┬─────────┘                                             │
        /// │             │                                                       │
        /// │             ▼                                                       │
        /// │   ┌───────────────────┐                                             │
        /// │   │ Step 2: Decode    │  Use Algorithm 3.6.1 (IMLD for C24)         │
        /// │   │ using IMLD        │  Find error pattern u, compute c = w + u    │
        /// │   └─────────┬─────────┘                                             │
        /// │             │                                                       │
        /// │             ▼                                                       │
        /// │   ┌───────────────────┐                                             │
        /// │   │ Step 3: Puncture  │  Remove last bit to get C23 codeword        │
        /// │   │ back to C23       │  Extract message from first 12 bits         │
        /// │   └─────────┬─────────┘                                             │
        /// │             │                                                       │
        /// │             ▼                                                       │
        /// │   Decoded message (12 bits) - original message recovered            │
        /// └─────────────────────────────────────────────────────────────────────┘
        ///
        /// WHY THIS WORKS:
        /// ===============
        /// C23 is obtained from C24 by puncturing (removing the last bit).
        /// Every C23 codeword, when extended with a parity bit, becomes a C24 codeword.
        /// All C24 codewords have EVEN weight. So:
        /// - If received word has odd weight → 0 or 2 errors in first 23 bits
        /// - If received word has even weight → 1 or 3 errors in first 23 bits
        ///
        /// By choosing the 24-bit extension with odd weight, we ensure that
        /// the error pattern has weight ≤ 3 in the first 23 positions.
        /// </summary>
        /// <param name="codeword">23-bit received word (may contain errors)</param>
        /// <returns>Decoded 12-bit message with errors corrected</returns>
        public int Decode(int codeword)
        {
            // ─────────────────────────────────────────────────────────────────
            // VALIDATION: Ensure codeword is exactly 23 bits
            // ─────────────────────────────────────────────────────────────────
            if (codeword < 0 || codeword >= (1 << 23))
            {
                throw new ArgumentException("Codeword must be 23 bits (0-8388607)", nameof(codeword));
            }

            // ─────────────────────────────────────────────────────────────────
            // Algorithm 3.7.1, Step 1: Form w0 or w1, whichever has ODD weight
            // ─────────────────────────────────────────────────────────────────
            //
            // w0 = received word with 0 appended (24 bits)
            // w1 = received word with 1 appended (24 bits)
            //
            // From page 88: "Form w0 or w1, whichever has odd weight"
            // We need odd weight because the IMLD algorithm expects this.
            // ─────────────────────────────────────────────────────────────────

            int[] w23 = IntToBitArray(codeword, 23);
            int weight23 = CalculateWeight(w23);

            // Algorithm 3.7.1: Form w0 or w1, whichever has odd weight
            int[] w24 = new int[24];
            Array.Copy(w23, w24, 23);

            if (weight23 % 2 == 0) {
                w24[23] = 1;  // Append 1 to make odd weight
            }
            else {
                w24[23] = 0;  // Append 0 to keep odd weight
            }

            // ─────────────────────────────────────────────────────────────────
            // Algorithm 3.7.1, Step 2: Decode wi using Algorithm 3.6.1
            // ─────────────────────────────────────────────────────────────────
            var (correctedCodeword24, errorPattern, success) = DecodeC24_IMLD(w24);

            if (!success)
            {
                // More than 3 errors - cannot correct, return best effort
                // Extract message from first 12 bits of received word
                return codeword & ((1 << 12) - 1);
            }

            // ─────────────────────────────────────────────────────────────────
            // Algorithm 3.7.1, Step 3: Remove the last digit from c
            // ─────────────────────────────────────────────────────────────────
            // The corrected 24-bit codeword has the message in positions 0-11
            // (because G = [I | B] is in systematic form)
            // ─────────────────────────────────────────────────────────────────

            int message = 0;
            for (int i = 0; i < 12; i++)
            {
                if (correctedCodeword24[i] == 1)
                {
                    message |= 1 << i;
                }
            }

            return message;
        }

        /// <summary>
        /// Decodes with detailed information about what happened.
        /// Useful for educational purposes and debugging.
        /// </summary>
        public DecodeResult DecodeWithDetails(int codeword)
        {
            if (codeword < 0 || codeword >= (1 << 23))
            {
                throw new ArgumentException("Codeword must be 23 bits (0-8388607)", nameof(codeword));
            }

            int[] w23 = IntToBitArray(codeword, 23);
            int weight23 = CalculateWeight(w23);

            int[] w24 = new int[24];
            Array.Copy(w23, w24, 23);

            int appendedBit;
            if (weight23 % 2 == 0)
            {
                w24[23] = 1;
                appendedBit = 1;
            }
            else
            {
                w24[23] = 0;
                appendedBit = 0;
            }

            var (correctedCodeword24, errorPattern, success) = DecodeC24_IMLD(w24);

            // Find error positions
            var errorPositions = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (errorPattern[i] == 1)
                {
                    errorPositions.Add(i);
                }
            }

            int message;
            if (success)
            {
                message = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (correctedCodeword24[i] == 1)
                    {
                        message |= 1 << i;
                    }
                }
            }
            else
            {
                message = codeword & ((1 << 12) - 1);
            }

            // Compute syndromes for educational display
            int[] s1 = ComputeSyndromeS1(w24);
            int[] s2 = ComputeSyndromeS2(s1);

            return new DecodeResult
            {
                OriginalCodeword = codeword,
                OriginalCodewordBinary = Convert.ToString(codeword, 2).PadLeft(23, '0'),
                ExtendedWord = BitArrayToInt(w24),
                ExtendedWordBinary = string.Join("", w24),
                AppendedBit = appendedBit,
                SyndromeS1 = string.Join("", s1),
                SyndromeS1Weight = CalculateWeight(s1),
                SyndromeS2 = string.Join("", s2),
                SyndromeS2Weight = CalculateWeight(s2),
                ErrorPattern = string.Join("", errorPattern),
                ErrorCount = errorPositions.Count,
                ErrorPositions = errorPositions.ToArray(),
                CorrectedCodeword = BitArrayToInt(correctedCodeword24.Take(23).ToArray()),
                CorrectedCodewordBinary = string.Join("", correctedCodeword24.Take(23)),
                DecodedMessage = message,
                DecodedMessageBinary = Convert.ToString(message, 2).PadLeft(12, '0'),
                Success = success,
                Status = success
                    ? $"Successfully corrected {errorPositions.Count} error(s)"
                    : "Too many errors to correct (>3)"
            };
        }

        /// <summary>
        /// Simulates sending a codeword through a Binary Symmetric Channel (BSC).
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ BINARY SYMMETRIC CHANNEL (BSC)                                      │
        /// │                                                                     │
        /// │              ┌─────────────────┐                                    │
        /// │              │                 │                                    │
        /// │    0 ───────►│  1-p      p    │───────► 0                           │
        /// │              │      ╲   ╱      │                                    │
        /// │              │       ╲ ╱       │         p = error probability      │
        /// │              │        ╳        │                                    │
        /// │              │       ╱ ╲       │                                    │
        /// │    1 ───────►│  p      1-p    │───────► 1                           │
        /// │              │                 │                                    │
        /// │              └─────────────────┘                                    │
        /// │                                                                     │
        /// │  Each bit independently flips with probability p                    │
        /// └─────────────────────────────────────────────────────────────────────┘
        ///
        /// IMPLEMENTATION:
        /// For each of the 23 bits:
        /// 1. Generate random number in [0, 1)
        /// 2. If random < errorProbability: flip the bit
        ///
        /// NOTE: Uses static Random to ensure independent errors across
        /// multiple calls (see comment on _random field).
        /// </summary>
        public (int corruptedCodeword, int[] errorPositions, int errorCount)
            SimulateChannel(int codeword, double errorProbability)
        {
            if (codeword < 0 || codeword >= (1 << 23))
            {
                throw new ArgumentException("Codeword must be 23 bits (0-8388607)", nameof(codeword));
            }

            if (errorProbability < 0.0 || errorProbability > 1.0)
            {
                throw new ArgumentException("Error probability must be between 0.0 and 1.0", nameof(errorProbability));
            }

            int corruptedCodeword = codeword;
            var errorPositionsList = new List<int>();

            // Use the static _random to ensure independence between calls
            for (int i = 0; i < 23; i++)
            {
                if (_random.NextDouble() < errorProbability)
                {
                    corruptedCodeword ^= 1 << i;  // Flip bit i
                    errorPositionsList.Add(i);
                }
            }

            return (corruptedCodeword, errorPositionsList.ToArray(), errorPositionsList.Count);
        }

        #endregion

        #region ═══════════════════════════ IMLD ALGORITHM (3.6.1) ═══════════════════════════

        /// <summary>
        /// Algorithm 3.6.1 (IMLD for C24) from literatura12.pdf, page 85
        ///
        /// IMLD = Iterative Majority Logic Decoding
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ ALGORITHM 3.6.1 - IMLD FOR C24                                      │
        /// │                                                                     │
        /// │ Input: 24-bit word w (with odd weight)                              │
        /// │ Output: Error pattern u = [u₁, u₂] where u₁, u₂ are 12-bit vectors  │
        /// │                                                                     │
        /// │ The error pattern u tells us which bits were flipped during         │
        /// │ transmission. The original codeword is: v = w ⊕ u                   │
        /// │                                                                     │
        /// │ STEPS:                                                              │
        /// │ ──────                                                              │
        /// │ 1. Compute syndrome s₁ = wH = u₁ + u₂B                              │
        /// │                                                                     │
        /// │ 2. If wt(s₁) ≤ 3 then u = [s₁, 0]                                   │
        /// │    (All errors are in first 12 bits)                                │
        /// │                                                                     │
        /// │ 3. If wt(s₁ + bᵢ) ≤ 2 for some row bᵢ of B                          │
        /// │    then u = [s₁ + bᵢ, eᵢ]                                           │
        /// │    (Errors in first 12 bits plus one error at position i+12)        │
        /// │                                                                     │
        /// │ 4. Compute second syndrome s₂ = s₁B                                 │
        /// │                                                                     │
        /// │ 5. If wt(s₂) ≤ 3 then u = [0, s₂]                                   │
        /// │    (All errors are in last 12 bits)                                 │
        /// │                                                                     │
        /// │ 6. If wt(s₂ + bᵢ) ≤ 2 for some row bᵢ of B                          │
        /// │    then u = [eᵢ, s₂ + bᵢ]                                           │
        /// │    (One error at position i plus errors in last 12 bits)            │
        /// │                                                                     │
        /// │ 7. If u not determined → request retransmission (>3 errors)         │
        /// └─────────────────────────────────────────────────────────────────────┘
        ///
        /// MATHEMATICAL BACKGROUND:
        /// ========================
        /// For extended Golay C24:
        /// - Generator matrix: G = [I | B] (12×24)
        /// - Parity check matrix: H = [B | I]ᵀ (24×12)
        ///
        /// A valid codeword c satisfies: cH = 0
        ///
        /// If we receive w = c + e (codeword + error), then:
        ///   syndrome s = wH = (c + e)H = cH + eH = 0 + eH = eH
        ///
        /// The syndrome depends ONLY on the error pattern, not the original codeword!
        /// This is the key insight that makes syndrome decoding work.
        ///
        /// For error pattern e = [u₁ | u₂]:
        ///   s = eH = [u₁ | u₂] × [B | I]ᵀ = u₁B + u₂
        ///
        /// So if we know s and can guess u₂ (or u₁), we can find the other part.
        /// </summary>
        private (int[] correctedCodeword, int[] errorPattern, bool success) DecodeC24_IMLD(int[] w24)
        {
            var B = _matrices.BMatrix;

            // ─────────────────────────────────────────────────────────────────
            // STEP 1: Compute first syndrome s₁ = wH
            // ─────────────────────────────────────────────────────────────────
            //
            // For w = [w₁ | w₂] (each 12 bits) and H = [B | I]ᵀ:
            //   s₁ = wH = w₁B + w₂
            //
            // This is equivalent to: s₁ = w × H where H is the parity check matrix
            // ─────────────────────────────────────────────────────────────────
            int[] s1 = ComputeSyndromeS1(w24);
            int weightS1 = CalculateWeight(s1);

            // ─────────────────────────────────────────────────────────────────
            // STEP 2: If wt(s₁) ≤ 3 then u = [s₁, 0]
            // ─────────────────────────────────────────────────────────────────
            //
            // If the syndrome has weight ≤ 3, TEST if error pattern [s₁, 0] is valid.
            // We validate by checking if (w + u) produces syndrome = 0.
            // ─────────────────────────────────────────────────────────────────
            if (weightS1 <= 3)
            {
                int[] errorPattern = new int[24];
                Array.Copy(s1, errorPattern, 12);  // u₁ = s₁
                // u₂ = 0 (already initialized)

                // Validate: check if this error pattern produces a valid codeword
                int[] corrected = XorVectors(w24, errorPattern);
                int[] testSyndrome = ComputeSyndromeS1(corrected);
                if (CalculateWeight(testSyndrome) == 0)
                {
                    return (corrected, errorPattern, true);
                }
                // Otherwise, continue to next step
            }

            // ─────────────────────────────────────────────────────────────────
            // STEP 3: Check if wt(s₁ + bᵢ) ≤ 2 for some row bᵢ of B
            // ─────────────────────────────────────────────────────────────────
            //
            // Try adding each row of B to s₁. If the result has weight ≤ 2,
            // then u₁ = s₁ + bᵢ and u₂ = eᵢ (unit vector with 1 at position i)
            //
            // This handles the case where there's one error in the last 12 bits
            // (at position i) and at most 2 errors in the first 12 bits.
            // ─────────────────────────────────────────────────────────────────
            for (int i = 0; i < 12; i++)
            {
                int[] s1PlusBi = XorVectors(s1, B[i]);
                if (CalculateWeight(s1PlusBi) <= 2)
                {
                    int[] errorPattern = new int[24];
                    Array.Copy(s1PlusBi, errorPattern, 12);  // u₁ = s₁ + bᵢ
                    errorPattern[12 + i] = 1;  // u₂ = eᵢ

                    // Validate: check if this error pattern produces a valid codeword
                    int[] corrected = XorVectors(w24, errorPattern);
                    int[] testSyndrome = ComputeSyndromeS1(corrected);
                    if (CalculateWeight(testSyndrome) == 0)
                    {
                        return (corrected, errorPattern, true);
                    }
                }
            }

            // ─────────────────────────────────────────────────────────────────
            // STEP 4: Compute second syndrome s₂ = s₁B
            // ─────────────────────────────────────────────────────────────────
            //
            // From the theory: s₁ = u₁B + u₂, so s₁B = u₁B² + u₂B = u₁ + u₂B
            // (using the property B² = I)
            //
            // But we can also derive: s₂ = (u₁ + u₂B)B = u₁B + u₂B² = u₁B + u₂
            // Actually, s₂ = s₁B relates to the error pattern differently.
            // ─────────────────────────────────────────────────────────────────
            int[] s2 = ComputeSyndromeS2(s1);
            int weightS2 = CalculateWeight(s2);

            // ─────────────────────────────────────────────────────────────────
            // STEP 5: If wt(s₂) ≤ 3 then u = [0, s₂]
            // ─────────────────────────────────────────────────────────────────
            //
            // If the second syndrome has weight ≤ 3, TEST if error pattern [0, s₂] is valid.
            // ─────────────────────────────────────────────────────────────────
            if (weightS2 <= 3)
            {
                int[] errorPattern = new int[24];
                // u₁ = 0 (already initialized)
                Array.Copy(s2, 0, errorPattern, 12, 12);  // u₂ = s₂

                // Validate: check if this error pattern produces a valid codeword
                int[] corrected = XorVectors(w24, errorPattern);
                int[] testSyndrome = ComputeSyndromeS1(corrected);
                if (CalculateWeight(testSyndrome) == 0)
                {
                    return (corrected, errorPattern, true);
                }
                // Otherwise, continue to next step
            }

            // ─────────────────────────────────────────────────────────────────
            // STEP 6: Check if wt(s₂ + bᵢ) ≤ 2 for some row bᵢ of B
            // ─────────────────────────────────────────────────────────────────
            //
            // This handles the case where there's one error in the first 12 bits
            // (at position i) and at most 2 errors in the last 12 bits.
            // ─────────────────────────────────────────────────────────────────
            for (int i = 0; i < 12; i++)
            {
                int[] s2PlusBi = XorVectors(s2, B[i]);
                if (CalculateWeight(s2PlusBi) <= 2)
                {
                    int[] errorPattern = new int[24];
                    errorPattern[i] = 1;  // u₁ = eᵢ
                    Array.Copy(s2PlusBi, 0, errorPattern, 12, 12);  // u₂ = s₂ + bᵢ

                    // Validate: check if this error pattern produces a valid codeword
                    int[] corrected = XorVectors(w24, errorPattern);
                    int[] testSyndrome = ComputeSyndromeS1(corrected);
                    if (CalculateWeight(testSyndrome) == 0)
                    {
                        return (corrected, errorPattern, true);
                    }
                }
            }

            // ─────────────────────────────────────────────────────────────────
            // STEP 7: Cannot decode - more than 3 errors
            // ─────────────────────────────────────────────────────────────────
            //
            // If we reach here, the received word has more than 3 errors.
            // The Golay code cannot correct this many errors.
            // Return the original word with no correction applied.
            // ─────────────────────────────────────────────────────────────────
            return (w24, new int[24], false);
        }

        /// <summary>
        /// Computes the first syndrome s₁ = wH for a 24-bit word.
        ///
        /// For w = [w₁ | w₂] (each part is 12 bits) and H = [B | I]ᵀ:
        ///   s₁ = w × H = w₁ × B + w₂
        ///
        /// In GF(2), this means:
        /// - Multiply the first 12 bits of w by matrix B
        /// - XOR with the last 12 bits of w
        /// </summary>
        private int[] ComputeSyndromeS1(int[] w24)
        {
            var B = _matrices.BMatrix;

            // Extract w₁ (first 12 bits) and w₂ (last 12 bits)
            int[] w1 = new int[12];
            int[] w2 = new int[12];
            Array.Copy(w24, 0, w1, 0, 12);
            Array.Copy(w24, 12, w2, 0, 12);

            // Compute w₂ × B (matrix-vector multiplication in GF(2))
            int[] w2B = MultiplyVectorByMatrix(w2, B);

            // s₁ = w₁ + w₂B (XOR in GF(2))
            return XorVectors(w1, w2B);
        }

        /// <summary>
        /// Computes the second syndrome s₂ = s₁ × B
        /// </summary>
        private int[] ComputeSyndromeS2(int[] s1)
        {
            var B = _matrices.BMatrix;
            return MultiplyVectorByMatrix(s1, B);
        }

        #endregion

        #region ═══════════════════════════ TEXT PROCESSING ═══════════════════════════

        /// <summary>
        /// Encodes a text string using Golay code.
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ TEXT ENCODING PROCESS                                               │
        /// │                                                                     │
        /// │  "Hello" → UTF-8 bytes → bit stream → 12-bit chunks → encode each   │
        /// │                                                                     │
        /// │  Character: 'H' 'e' 'l' 'l' 'o'                                     │
        /// │  UTF-8:     72  101 108 108 111  (5 bytes = 40 bits)                │
        /// │                                                                     │
        /// │  Padding: Add zeros to make total bits divisible by 12              │
        /// │  40 bits → 48 bits (add 8 zeros) → 4 message blocks                 │
        /// │                                                                     │
        /// │  Each 12-bit block → 23-bit codeword                                │
        /// │  4 blocks × 23 bits = 92 bits total                                 │
        /// │                                                                     │
        /// │  NOTE: Padding count is stored as metadata (not sent through        │
        /// │  the channel) to allow correct reconstruction during decoding.      │
        /// └─────────────────────────────────────────────────────────────────────┘
        /// </summary>
        public TextEncodeResult EncodeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }

            // Convert text to bytes (UTF-8)
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            // Convert bytes to bit array (LSB first to match IntToBitArray)
            var bits = new List<int>();
            foreach (byte b in bytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }

            // Calculate padding needed to make length divisible by 12
            int originalBitCount = bits.Count;
            int paddingBits = (12 - (bits.Count % 12)) % 12;
            for (int i = 0; i < paddingBits; i++)
            {
                bits.Add(0);
            }

            // Split into 12-bit messages and encode each
            var messages = new List<int>();
            var codewords = new List<int>();

            for (int i = 0; i < bits.Count; i += 12)
            {
                // Convert 12 bits to integer (LSB first to match IntToBitArray)
                int message = 0;
                for (int j = 0; j < 12; j++)
                {
                    message |= bits[i + j] << j;
                }
                messages.Add(message);

                // Encode the message
                int codeword = Encode(message);
                codewords.Add(codeword);
            }

            return new TextEncodeResult
            {
                OriginalText = text,
                ByteCount = bytes.Length,
                BitCount = originalBitCount,
                PaddingBits = paddingBits,
                MessageCount = messages.Count,
                Messages = messages.ToArray(),
                Codewords = codewords.ToArray()
            };
        }

        /// <summary>
        /// Sends encoded text through a noisy channel.
        /// </summary>
        public TextChannelResult SendTextThroughChannel(int[] codewords, double errorProbability)
        {
            var corruptedCodewords = new List<int>();
            var allErrorPositions = new List<int[]>();
            int totalErrors = 0;

            foreach (int codeword in codewords)
            {
                var (corrupted, errorPos, errorCount) = SimulateChannel(codeword, errorProbability);
                corruptedCodewords.Add(corrupted);
                allErrorPositions.Add(errorPos);
                totalErrors += errorCount;
            }

            return new TextChannelResult
            {
                OriginalCodewords = codewords,
                CorruptedCodewords = corruptedCodewords.ToArray(),
                ErrorPositions = allErrorPositions.ToArray(),
                TotalErrors = totalErrors,
                ErrorProbability = errorProbability
            };
        }

        /// <summary>
        /// Decodes text from codewords (with or without errors).
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ TEXT DECODING PROCESS                                              │
        /// │                                                                     │
        /// │  Received codewords → decode each → 12-bit messages → bit stream   │
        /// │       → remove padding → bytes → UTF-8 string                      │
        /// │                                                                     │
        /// │  If errors ≤ 3 per codeword: successfully corrected               │
        /// │  If errors > 3: some characters may be corrupted                   │
        /// └─────────────────────────────────────────────────────────────────────┘
        /// </summary>
        public TextDecodeResult DecodeText(int[] codewords, int paddingBits)
        {
            var messages = new List<int>();
            var bits = new List<int>();
            int correctedErrors = 0;
            int uncorrectableBlocks = 0;

            foreach (int codeword in codewords)
            {
                var result = DecodeWithDetails(codeword);
                messages.Add(result.DecodedMessage);

                if (result.Success)
                {
                    correctedErrors += result.ErrorCount;
                }
                else
                {
                    uncorrectableBlocks++;
                }

                // Convert 12-bit message to bits (LSB first to match IntToBitArray)
                for (int i = 0; i < 12; i++)
                {
                    bits.Add((result.DecodedMessage >> i) & 1);
                }
            }

            // Remove padding bits
            if (paddingBits > 0)
            {
                bits.RemoveRange(bits.Count - paddingBits, paddingBits);
            }

            // Convert bits to bytes (LSB first to match byte extraction)
            var bytes = new List<byte>();
            for (int i = 0; i < bits.Count; i += 8)
            {
                if (i + 8 <= bits.Count)
                {
                    byte b = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        b |= (byte)(bits[i + j] << j);
                    }
                    bytes.Add(b);
                }
            }

            // Convert bytes to string
            string decodedText;
            try
            {
                decodedText = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            }
            catch
            {
                decodedText = "[DECODING ERROR: Invalid UTF-8 sequence]";
            }

            return new TextDecodeResult
            {
                DecodedText = decodedText,
                Messages = messages.ToArray(),
                CorrectedErrors = correctedErrors,
                UncorrectableBlocks = uncorrectableBlocks,
                TotalBlocks = codewords.Length
            };
        }

        #endregion

        #region ═══════════════════════════ IMAGE PROCESSING ═══════════════════════════

        /// <summary>
        /// Encodes BMP image data using Golay code.
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ IMAGE ENCODING PROCESS                                             │
        /// │                                                                     │
        /// │  BMP file structure:                                               │
        /// │  ┌──────────────────────────────────────────────────────┐          │
        /// │  │ HEADER (54 bytes) │ PIXEL DATA (variable)           │          │
        /// │  │ - NOT encoded     │ - Split into 12-bit chunks      │          │
        /// │  │ - Sent as-is      │ - Each chunk → 23-bit codeword  │          │
        /// │  └──────────────────────────────────────────────────────┘          │
        /// │                                                                     │
        /// │  From Tasks.md:                                                     │
        /// │  "paveiksliukų antraštėse esančią informaciją irgi laikykime       │
        /// │   tarnybine ir jos neiškraipykime"                                 │
        /// │  (Image header information should be treated as service info       │
        /// │   and not corrupted)                                               │
        /// │                                                                     │
        /// │  24-bit BMP: Each pixel = 3 bytes (B, G, R)                        │
        /// │  Pixel data is encoded, header is preserved                        │
        /// └─────────────────────────────────────────────────────────────────────┘
        /// </summary>
        public ImageEncodeResult EncodeImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 54)
            {
                throw new ArgumentException("Invalid BMP image data", nameof(imageData));
            }

            // Verify BMP signature
            if (imageData[0] != 'B' || imageData[1] != 'M')
            {
                throw new ArgumentException("Not a valid BMP file (missing BM signature)", nameof(imageData));
            }

            // Extract header (first 54 bytes for standard BMP)
            // Header offset is stored at bytes 10-13
            int headerSize = BitConverter.ToInt32(imageData, 10);
            byte[] header = new byte[headerSize];
            Array.Copy(imageData, header, headerSize);

            // Extract pixel data
            byte[] pixelData = new byte[imageData.Length - headerSize];
            Array.Copy(imageData, headerSize, pixelData, 0, pixelData.Length);

            // Convert pixel data to bits (LSB first to match IntToBitArray)
            var bits = new List<int>();
            foreach (byte b in pixelData)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }

            // Pad to make divisible by 12
            int originalBitCount = bits.Count;
            int paddingBits = (12 - (bits.Count % 12)) % 12;
            for (int i = 0; i < paddingBits; i++)
            {
                bits.Add(0);
            }

            // Encode 12-bit blocks
            var codewords = new List<int>();
            for (int i = 0; i < bits.Count; i += 12)
            {
                int message = 0;
                for (int j = 0; j < 12; j++)
                {
                    message |= bits[i + j] << j;  // LSB first to match IntToBitArray
                }
                codewords.Add(Encode(message));
            }

            return new ImageEncodeResult
            {
                Header = header,
                HeaderSize = headerSize,
                PixelDataSize = pixelData.Length,
                BitCount = originalBitCount,
                PaddingBits = paddingBits,
                CodewordCount = codewords.Count,
                Codewords = codewords.ToArray()
            };
        }

        /// <summary>
        /// Sends encoded image data through a noisy channel.
        /// Note: Only pixel data is corrupted, header is preserved.
        /// </summary>
        public ImageChannelResult SendImageThroughChannel(int[] codewords, double errorProbability)
        {
            var corruptedCodewords = new List<int>();
            int totalErrors = 0;

            foreach (int codeword in codewords)
            {
                var (corrupted, _, errorCount) = SimulateChannel(codeword, errorProbability);
                corruptedCodewords.Add(corrupted);
                totalErrors += errorCount;
            }

            return new ImageChannelResult
            {
                OriginalCodewords = codewords,
                CorruptedCodewords = corruptedCodewords.ToArray(),
                TotalErrors = totalErrors,
                ErrorProbability = errorProbability
            };
        }

        /// <summary>
        /// Decodes image data from codewords.
        /// </summary>
        public ImageDecodeResult DecodeImage(byte[] header, int[] codewords, int paddingBits, int originalPixelSize)
        {
            var bits = new List<int>();
            int correctedErrors = 0;
            int uncorrectableBlocks = 0;

            foreach (int codeword in codewords)
            {
                var result = DecodeWithDetails(codeword);

                if (result.Success)
                {
                    correctedErrors += result.ErrorCount;
                }
                else
                {
                    uncorrectableBlocks++;
                }

                // Convert 12-bit message to bits (LSB first to match IntToBitArray)
                for (int i = 0; i < 12; i++)
                {
                    bits.Add((result.DecodedMessage >> i) & 1);
                }
            }

            // Remove padding
            if (paddingBits > 0)
            {
                bits.RemoveRange(bits.Count - paddingBits, paddingBits);
            }

            // Convert bits to bytes (LSB first to match byte extraction)
            var pixelBytes = new List<byte>();
            for (int i = 0; i < bits.Count && pixelBytes.Count < originalPixelSize; i += 8)
            {
                if (i + 8 <= bits.Count)
                {
                    byte b = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        b |= (byte)(bits[i + j] << j);
                    }
                    pixelBytes.Add(b);
                }
            }

            // Reconstruct image
            byte[] imageData = new byte[header.Length + pixelBytes.Count];
            Array.Copy(header, imageData, header.Length);
            Array.Copy(pixelBytes.ToArray(), 0, imageData, header.Length, pixelBytes.Count);

            return new ImageDecodeResult
            {
                ImageData = imageData,
                CorrectedErrors = correctedErrors,
                UncorrectableBlocks = uncorrectableBlocks,
                TotalBlocks = codewords.Length
            };
        }

        #endregion

        #region ═══════════════════════════ HELPER METHODS ═══════════════════════════

        /// <summary>
        /// Converts an integer to a bit array representation.
        ///
        /// Example: IntToBitArray(5, 12) returns [1,0,1,0,0,0,0,0,0,0,0,0]
        ///          (bit 0 = 1, bit 2 = 1, rest are 0)
        ///
        /// Note: Bit 0 is at index 0 (LSB first representation in the array)
        /// </summary>
        private int[] IntToBitArray(int value, int length)
        {
            int[] bits = new int[length];
            for (int i = 0; i < length; i++)
            {
                bits[i] = (value >> i) & 1;
            }
            return bits;
        }

        /// <summary>
        /// Converts a bit array back to an integer.
        ///
        /// Example: BitArrayToInt([1,0,1,0,0,0,0,0,0,0,0,0]) returns 5
        /// </summary>
        private int BitArrayToInt(int[] bits)
        {
            int value = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == 1)
                {
                    value |= 1 << i;
                }
            }
            return value;
        }

        /// <summary>
        /// Calculates the Hamming weight (number of 1s) in a bit array.
        ///
        /// ┌─────────────────────────────────────────────────────────────────────┐
        /// │ HAMMING WEIGHT                                                     │
        /// │                                                                     │
        /// │ The Hamming weight of a binary vector is the count of 1s.         │
        /// │                                                                     │
        /// │ Examples:                                                          │
        /// │   wt([0,0,0,0]) = 0                                                │
        /// │   wt([1,0,1,0]) = 2                                                │
        /// │   wt([1,1,1,1]) = 4                                                │
        /// │                                                                     │
        /// │ In Golay decoding, weight is used to determine the error pattern: │
        /// │   - wt(s) ≤ 3 means we found the error pattern                    │
        /// │   - wt(s + bᵢ) ≤ 2 means error includes position i               │
        /// └─────────────────────────────────────────────────────────────────────┘
        /// </summary>
        private int CalculateWeight(int[] bits)
        {
            int weight = 0;
            foreach (int bit in bits)
            {
                if (bit == 1)
                {
                    weight++;
                }
            }
            return weight;
        }

        /// <summary>
        /// XORs two bit arrays (vector addition in GF(2)).
        ///
        /// In GF(2) (binary field):
        ///   0 + 0 = 0
        ///   0 + 1 = 1
        ///   1 + 0 = 1
        ///   1 + 1 = 0  ← This is the key difference from normal addition!
        ///
        /// XOR is used for:
        /// - Adding vectors: u + v
        /// - Computing syndromes: s = wH
        /// - Correcting errors: c = w ⊕ e
        /// </summary>
        private int[] XorVectors(int[] a, int[] b)
        {
            int length = Math.Min(a.Length, b.Length);
            int[] result = new int[Math.Max(a.Length, b.Length)];

            for (int i = 0; i < length; i++)
            {
                result[i] = a[i] ^ b[i];
            }

            // Copy remaining elements if lengths differ
            if (a.Length > length)
            {
                Array.Copy(a, length, result, length, a.Length - length);
            }
            else if (b.Length > length)
            {
                Array.Copy(b, length, result, length, b.Length - length);
            }

            return result;
        }

        /// <summary>
        /// Multiplies a vector by a matrix in GF(2).
        ///
        /// For vector v (length n) and matrix M (n×m):
        ///   result[j] = Σᵢ (v[i] × M[i][j]) mod 2
        ///
        /// In GF(2), multiplication is AND and addition is XOR:
        ///   result[j] = v[0]×M[0][j] ⊕ v[1]×M[1][j] ⊕ ... ⊕ v[n-1]×M[n-1][j]
        /// </summary>
        private int[] MultiplyVectorByMatrix(int[] vector, int[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            int[] result = new int[cols];

            for (int j = 0; j < cols; j++)
            {
                int sum = 0;
                for (int i = 0; i < rows && i < vector.Length; i++)
                {
                    sum ^= vector[i] & matrix[i][j];  // AND then XOR
                }
                result[j] = sum;
            }

            return result;
        }

        #endregion
    }

    #region ═══════════════════════════ RESULT CLASSES ═══════════════════════════

    /// <summary>
    /// Detailed result from decoding operation
    /// </summary>
    public class DecodeResult
    {
        public int OriginalCodeword { get; set; }
        public string OriginalCodewordBinary { get; set; } = "";
        public int ExtendedWord { get; set; }
        public string ExtendedWordBinary { get; set; } = "";
        public int AppendedBit { get; set; }
        public string SyndromeS1 { get; set; } = "";
        public int SyndromeS1Weight { get; set; }
        public string SyndromeS2 { get; set; } = "";
        public int SyndromeS2Weight { get; set; }
        public string ErrorPattern { get; set; } = "";
        public int ErrorCount { get; set; }
        public int[] ErrorPositions { get; set; } = Array.Empty<int>();
        public int CorrectedCodeword { get; set; }
        public string CorrectedCodewordBinary { get; set; } = "";
        public int DecodedMessage { get; set; }
        public string DecodedMessageBinary { get; set; } = "";
        public bool Success { get; set; }
        public string Status { get; set; } = "";
    }

    /// <summary>
    /// Result from text encoding operation
    /// </summary>
    public class TextEncodeResult
    {
        public string OriginalText { get; set; } = "";
        public int ByteCount { get; set; }
        public int BitCount { get; set; }
        public int PaddingBits { get; set; }
        public int MessageCount { get; set; }
        public int[] Messages { get; set; } = Array.Empty<int>();
        public int[] Codewords { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// Result from sending text through channel
    /// </summary>
    public class TextChannelResult
    {
        public int[] OriginalCodewords { get; set; } = Array.Empty<int>();
        public int[] CorruptedCodewords { get; set; } = Array.Empty<int>();
        public int[][] ErrorPositions { get; set; } = Array.Empty<int[]>();
        public int TotalErrors { get; set; }
        public double ErrorProbability { get; set; }
    }

    /// <summary>
    /// Result from text decoding operation
    /// </summary>
    public class TextDecodeResult
    {
        public string DecodedText { get; set; } = "";
        public int[] Messages { get; set; } = Array.Empty<int>();
        public int CorrectedErrors { get; set; }
        public int UncorrectableBlocks { get; set; }
        public int TotalBlocks { get; set; }
    }

    /// <summary>
    /// Result from image encoding operation
    /// </summary>
    public class ImageEncodeResult
    {
        public byte[] Header { get; set; } = Array.Empty<byte>();
        public int HeaderSize { get; set; }
        public int PixelDataSize { get; set; }
        public int BitCount { get; set; }
        public int PaddingBits { get; set; }
        public int CodewordCount { get; set; }
        public int[] Codewords { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// Result from sending image through channel
    /// </summary>
    public class ImageChannelResult
    {
        public int[] OriginalCodewords { get; set; } = Array.Empty<int>();
        public int[] CorruptedCodewords { get; set; } = Array.Empty<int>();
        public int TotalErrors { get; set; }
        public double ErrorProbability { get; set; }
    }

    /// <summary>
    /// Result from image decoding operation
    /// </summary>
    public class ImageDecodeResult
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public int CorrectedErrors { get; set; }
        public int UncorrectableBlocks { get; set; }
        public int TotalBlocks { get; set; }
    }

    #endregion
}
