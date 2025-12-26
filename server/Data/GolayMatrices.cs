namespace server.Data
{
    /// <summary>
    /// Contains the matrices used for Golay code encoding and decoding.
    ///
    /// GOLAY CODE OVERVIEW:
    /// =====================
    ///
    /// C23 - Golay (23,12,7) code:
    ///   - Length: n = 23 bits
    ///   - Dimension: k = 12 bits (message)
    ///   - Minimum distance: d = 7
    ///   - Can correct: t = 3 errors (floor((d-1)/2) = 3)
    ///
    /// C24 - Extended Golay (24,12,8) code:
    ///   - Length: n = 24 bits (adds 1 parity bit to C23)
    ///   - Dimension: k = 12 bits (message)
    ///   - Minimum distance: d = 8
    ///   - All codewords have even weight
    ///   - Used as intermediate step for decoding C23
    ///
    /// MATRIX RELATIONSHIPS (from literatura12.pdf):
    /// ==============================================
    ///
    /// For C23:
    ///   Generator matrix G = [I₁₂ | P̂] where P̂ is 12×11
    ///
    /// For C24 (extended):
    ///   B matrix (12×12) = P̂ with column of 1s added
    ///   Generator matrix G = [I₁₂ | B]
    ///   Parity check matrix H = [B | I₁₂]^T (24×12)
    ///
    /// Key property: B² = I (B is its own inverse in GF(2))
    ///              BB^T = I
    ///              GH = 0
    /// </summary>
    public class GolayMatrices
    {
        // ═══════════════════════════════════════════════════════════════════════
        // IDENTITY MATRIX I₁₂ (12×12)
        // ═══════════════════════════════════════════════════════════════════════
        //
        // The identity matrix is used in the systematic form of the generator
        // matrix G = [I | P]. When we encode a message m, the first 12 bits of
        // the codeword are the message itself (systematic code property).
        //
        // This means: c = m × G = m × [I | P] = [m | mP]
        //            └── First 12 bits are the original message
        // ═══════════════════════════════════════════════════════════════════════
        public readonly int[][] IdentityMatrix =
        [
            [1,0,0,0,0,0,0,0,0,0,0,0],
            [0,1,0,0,0,0,0,0,0,0,0,0],
            [0,0,1,0,0,0,0,0,0,0,0,0],
            [0,0,0,1,0,0,0,0,0,0,0,0],
            [0,0,0,0,1,0,0,0,0,0,0,0],
            [0,0,0,0,0,1,0,0,0,0,0,0],
            [0,0,0,0,0,0,1,0,0,0,0,0],
            [0,0,0,0,0,0,0,1,0,0,0,0],
            [0,0,0,0,0,0,0,0,1,0,0,0],
            [0,0,0,0,0,0,0,0,0,1,0,0],
            [0,0,0,0,0,0,0,0,0,0,1,0],
            [0,0,0,0,0,0,0,0,0,0,0,1],
        ];

        // ═══════════════════════════════════════════════════════════════════════
        // PARITY MATRIX P̂ (12×11) for Golay (23,12) code
        // ═══════════════════════════════════════════════════════════════════════
        //
        // This is the P̂ matrix from literatura12.pdf, used for the punctured
        // Golay code C23. It's obtained from the B matrix by removing the last
        // column (the column of all 1s).
        //
        // Generator matrix for C23: G = [I₁₂ | P̂] (12×23)
        //
        // The matrix has a cyclic structure in its first 11 rows (B₁), where
        // each row is a cyclic shift of the previous one.
        // Row 12 is all 1s (the "j" vector).
        // ═══════════════════════════════════════════════════════════════════════
        public readonly int[][] ParityMatrix =
        [
            // Row 0:  B₁ first row - 11011100010
            [1,1,0,1,1,1,0,0,0,1,0],
            // Row 1:  Cyclic shift - 10111000101
            [1,0,1,1,1,0,0,0,1,0,1],
            // Row 2:  Cyclic shift - 01110001011
            [0,1,1,1,0,0,0,1,0,1,1],
            // Row 3:  Cyclic shift - 11100010110
            [1,1,1,0,0,0,1,0,1,1,0],
            // Row 4:  Cyclic shift - 11000101101
            [1,1,0,0,0,1,0,1,1,0,1],
            // Row 5:  Cyclic shift - 10001011011
            [1,0,0,0,1,0,1,1,0,1,1],
            // Row 6:  Cyclic shift - 00010110111
            [0,0,0,1,0,1,1,0,1,1,1],
            // Row 7:  Cyclic shift - 00101101110
            [0,0,1,0,1,1,0,1,1,1,0],
            // Row 8:  Cyclic shift - 01011011100
            [0,1,0,1,1,0,1,1,1,0,0],
            // Row 9:  Cyclic shift - 10110111000
            [1,0,1,1,0,1,1,1,0,0,0],
            // Row 10: Cyclic shift - 01101110001
            [0,1,1,0,1,1,1,0,0,0,1],
            // Row 11: All ones (j vector)
            [1,1,1,1,1,1,1,1,1,1,1],
        ];

        // ═══════════════════════════════════════════════════════════════════════
        // B MATRIX (12×12) for Extended Golay (24,12) code
        // ═══════════════════════════════════════════════════════════════════════
        //
        // From literatura12.pdf, page 82:
        //
        // B = [ B₁  | j^T ]  where B₁ is 11×11 (cyclic structure)
        //     [ j   | 0   ]        j is all-ones vector (11 elements)
        //                         j^T is all-ones column (11 elements)
        //
        // In our representation (12×12):
        // - First 11 columns: same as ParityMatrix
        // - Last column: [1,1,1,1,1,1,1,1,1,1,1,0]^T (all 1s except last)
        //
        // IMPORTANT PROPERTIES:
        // - B is symmetric: B = B^T
        // - B² = I (B is self-inverse in GF(2))
        // - Every row has odd weight (7 or 11)
        // - Used for syndrome-based decoding
        //
        // The B matrix structure (from page 82):
        // ┌                                     ┐
        // │ 1 1 0 1 1 1 0 0 0 1 0 | 1 │  Row 0  │
        // │ 1 0 1 1 1 0 0 0 1 0 1 | 1 │  Row 1  │
        // │ 0 1 1 1 0 0 0 1 0 1 1 | 1 │  Row 2  │
        // │ 1 1 1 0 0 0 1 0 1 1 0 | 1 │  Row 3  │
        // │ 1 1 0 0 0 1 0 1 1 0 1 | 1 │  Row 4  │
        // │ 1 0 0 0 1 0 1 1 0 1 1 | 1 │  Row 5  │
        // │ 0 0 0 1 0 1 1 0 1 1 1 | 1 │  Row 6  │
        // │ 0 0 1 0 1 1 0 1 1 1 0 | 1 │  Row 7  │
        // │ 0 1 0 1 1 0 1 1 1 0 0 | 1 │  Row 8  │
        // │ 1 0 1 1 0 1 1 1 0 0 0 | 1 │  Row 9  │
        // │ 0 1 1 0 1 1 1 0 0 0 1 | 1 │  Row 10 │
        // │ 1 1 1 1 1 1 1 1 1 1 1 | 0 │  Row 11 │
        // └                                     ┘
        // ═══════════════════════════════════════════════════════════════════════
        public readonly int[][] BMatrix =
        [
            [1,1,0,1,1,1,0,0,0,1,0,1],  // Row 0:  weight 7
            [1,0,1,1,1,0,0,0,1,0,1,1],  // Row 1:  weight 7
            [0,1,1,1,0,0,0,1,0,1,1,1],  // Row 2:  weight 7
            [1,1,1,0,0,0,1,0,1,1,0,1],  // Row 3:  weight 7
            [1,1,0,0,0,1,0,1,1,0,1,1],  // Row 4:  weight 7
            [1,0,0,0,1,0,1,1,0,1,1,1],  // Row 5:  weight 7
            [0,0,0,1,0,1,1,0,1,1,1,1],  // Row 6:  weight 7
            [0,0,1,0,1,1,0,1,1,1,0,1],  // Row 7:  weight 7
            [0,1,0,1,1,0,1,1,1,0,0,1],  // Row 8:  weight 7
            [1,0,1,1,0,1,1,1,0,0,0,1],  // Row 9:  weight 7
            [0,1,1,0,1,1,1,0,0,0,1,1],  // Row 10: weight 7
            [1,1,1,1,1,1,1,1,1,1,1,0],  // Row 11: weight 11
        ];

        /// <summary>
        /// Grąžina parities matricą P̂ (12×11) C23 kodui.
        /// Naudojama generatoriaus matricos konstrukcijoje: G = [I₁₂ | P̂].
        /// Parametrai: nėra
        /// Grąžina:
        ///   12×11 parities matricą GF(2) lauke
        ///   Kiekviena eilutė yra cikliniu poslinkiu gautasiš pirmos eilutės
        /// </summary>
        public int[][] GetParityMatrix()
        {
            return ParityMatrix;
        }

        /// <summary>
        /// Grąžina identiteto matricą I₁₂ (12×12).
        /// Naudojama sisteminės formos generatoriaus matricoje.
        /// Parametrai: nėra
        /// Grąžina:
        ///   12×12 identiteto matricą (įstrižainėje vienetai, kitur nuliai)
        /// </summary>
        public int[][] GetIdentityMatrix()
        {
            return IdentityMatrix;
        }

        /// <summary>
        /// Grąžina B matricą (12×12) išplėstam Golay C24 kodui.
        /// KRITIŠKAI SVARBU sindromų dekodavimui (algoritmas 3.6.1).
        /// Savybės:
        ///   - B² = I (B yra pati sau atvirkštinė GF(2) lauke)
        ///   - B = B^T (simetrinė)
        ///   - Kiekvienos eilutės svoris nelyginis (7 arba 11)
        /// Parametrai: nėra
        /// Grąžina:
        ///   12×12 B matricą
        /// </summary>
        public int[][] GetBMatrix()
        {
            return BMatrix;
        }
    }
}
