using server.Data;

namespace server.Services
{
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

        public int[][] GetGeneratorMatrix()
        {
            var identity = _matrices.IdentityMatrix;
            var parity = _matrices.ParityMatrix;
            var generator = new int[12][];

            for (int i = 0; i < 12; i++)
            {
                generator[i] = new int[23];

                for (int j = 0; j < 12; j++)
                {
                    generator[i][j] = identity[i][j];
                }

                for (int j = 0; j < 11; j++)
                {
                    generator[i][j + 12] = parity[i][j];
                }
            }

            return generator;
        }

        public int Encode(int message)
        {
            if (message < 0 || message >= (1 << 12))
            {
                throw new ArgumentException("Message must be 12 bits (0-4095)", nameof(message));
            }

            int codeword = 0;
            var generator = GetGeneratorMatrix();

            for (int i = 0; i < 12; i++)
            {
                if ((message & (1 << i)) != 0)
                {
                    for (int j = 0; j < 23; j++)
                    {
                        if (generator[i][j] == 1)
                        {
                            codeword ^= 1 << j;
                        }
                    }
                }
            }

            return codeword;
        }

        public int Decode(int codeword)
        {
            if (codeword < 0 || codeword >= (1 << 23))
            {
                throw new ArgumentException("Codeword must be 23 bits (0-8388607)", nameof(codeword));
            }

            int[] w23 = IntToBitArray(codeword, 23);
            int weight23 = CalculateWeight(w23);

            int[] w24 = new int[24];
            Array.Copy(w23, w24, 23);

            if (weight23 % 2 == 0) {
                w24[23] = 1;  // Append 1 to make odd weight
            }
            else {
                w24[23] = 0;  // Append 0 to keep odd weight
            }

            var (correctedCodeword24, errorPattern, success) = DecodeC24_IMLD(w24);

            if (!success)
            {
                return codeword & ((1 << 12) - 1);
            }

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

        private (int[] correctedCodeword, int[] errorPattern, bool success) DecodeC24_IMLD(int[] w24)
        {
            var B = _matrices.BMatrix;

            int[] s1 = ComputeSyndromeS1(w24);
            int weightS1 = CalculateWeight(s1);

            if (weightS1 <= 3)
            {
                int[] errorPattern = new int[24];
                Array.Copy(s1, errorPattern, 12);

                int[] corrected = XorVectors(w24, errorPattern);
                int[] testSyndrome = ComputeSyndromeS1(corrected);
                if (CalculateWeight(testSyndrome) == 0)
                {
                    return (corrected, errorPattern, true);
                }
            }

            for (int i = 0; i < 12; i++)
            {
                int[] s1PlusBi = XorVectors(s1, B[i]);
                if (CalculateWeight(s1PlusBi) <= 2)
                {
                    int[] errorPattern = new int[24];
                    Array.Copy(s1PlusBi, errorPattern, 12);
                    errorPattern[12 + i] = 1;

                    int[] corrected = XorVectors(w24, errorPattern);
                    int[] testSyndrome = ComputeSyndromeS1(corrected);
                    if (CalculateWeight(testSyndrome) == 0)
                    {
                        return (corrected, errorPattern, true);
                    }
                }
            }

            int[] s2 = ComputeSyndromeS2(s1);
            int weightS2 = CalculateWeight(s2);

            if (weightS2 <= 3)
            {
                int[] errorPattern = new int[24];
                Array.Copy(s2, 0, errorPattern, 12, 12);

                int[] corrected = XorVectors(w24, errorPattern);
                int[] testSyndrome = ComputeSyndromeS1(corrected);
                if (CalculateWeight(testSyndrome) == 0)
                {
                    return (corrected, errorPattern, true);
                }
            }

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

            return (w24, new int[24], false);
        }

        private int[] ComputeSyndromeS1(int[] w24)
        {
            var B = _matrices.BMatrix;

            int[] w1 = new int[12];
            int[] w2 = new int[12];
            Array.Copy(w24, 0, w1, 0, 12);
            Array.Copy(w24, 12, w2, 0, 12);

            int[] w2B = MultiplyVectorByMatrix(w2, B);

            return XorVectors(w1, w2B);
        }

        private int[] ComputeSyndromeS2(int[] s1)
        {
            var B = _matrices.BMatrix;
            return MultiplyVectorByMatrix(s1, B);
        }

        #endregion

        #region ═══════════════════════════ TEXT PROCESSING ═══════════════════════════
        public TextEncodeResult EncodeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            var bits = new List<int>();
            foreach (byte b in bytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }

            int originalBitCount = bits.Count;
            int paddingBits = (12 - (bits.Count % 12)) % 12;
            for (int i = 0; i < paddingBits; i++)
            {
                bits.Add(0);
            }

            var messages = new List<int>();
            var codewords = new List<int>();

            for (int i = 0; i < bits.Count; i += 12)
            {
                int message = 0;
                for (int j = 0; j < 12; j++)
                {
                    message |= bits[i + j] << j;
                }
                messages.Add(message);

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

                for (int i = 0; i < 12; i++)
                {
                    bits.Add((result.DecodedMessage >> i) & 1);
                }
            }

            if (paddingBits > 0)
            {
                bits.RemoveRange(bits.Count - paddingBits, paddingBits);
            }

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

        public ImageEncodeResult EncodeImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 54)
            {
                throw new ArgumentException("Invalid BMP image data", nameof(imageData));
            }

            if (imageData[0] != 'B' || imageData[1] != 'M')
            {
                throw new ArgumentException("Not a valid BMP file (missing BM signature)", nameof(imageData));
            }

            int headerSize = BitConverter.ToInt32(imageData, 10);
            byte[] header = new byte[headerSize];
            Array.Copy(imageData, header, headerSize);

            byte[] pixelData = new byte[imageData.Length - headerSize];
            Array.Copy(imageData, headerSize, pixelData, 0, pixelData.Length);

            var bits = new List<int>();
            foreach (byte b in pixelData)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }

            int originalBitCount = bits.Count;
            int paddingBits = (12 - (bits.Count % 12)) % 12;
            for (int i = 0; i < paddingBits; i++)
            {
                bits.Add(0);
            }

            var codewords = new List<int>();
            for (int i = 0; i < bits.Count; i += 12)
            {
                int message = 0;
                for (int j = 0; j < 12; j++)
                {
                    message |= bits[i + j] << j;
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

            if (paddingBits > 0)
            {
                bits.RemoveRange(bits.Count - paddingBits, paddingBits);
            }

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

        private int[] IntToBitArray(int value, int length)
        {
            int[] bits = new int[length];
            for (int i = 0; i < length; i++)
            {
                bits[i] = (value >> i) & 1;
            }
            return bits;
        }

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

        private int[] XorVectors(int[] a, int[] b)
        {
            int length = Math.Min(a.Length, b.Length);
            int[] result = new int[Math.Max(a.Length, b.Length)];

            for (int i = 0; i < length; i++)
            {
                result[i] = a[i] ^ b[i];
            }

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
