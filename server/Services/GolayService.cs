using server.Data;

namespace server.Services
{
    /// <summary>
    /// Pagrindinė Golay (23,12,7) klaidų taisymo kodo implementacija.
    /// Vykdo kodavimą, dekodavimą, kanalo simuliaciją, teksto ir vaizdų apdorojimą.
    /// </summary>
    public class GolayService
    {
        private readonly GolayMatrices _matrices;

        // KRITIŠKAI SVARBU: Static Random generatorius inicializuojamas tik vieną kartą.
        // Užtikrina, kad greitai iškviesti SimulateChannel metodai gautų skirtingas klaidas.
        // Jei būtų ne-static, laiku pagrįstas seed'inimas sukurtų identiškus klaidų šablonus.
        private static readonly Random _random = new Random();

        /// <summary>
        /// Konstruktorius.
        /// Parametrai: matrices - GolayMatrices objektas su matricomis P̂ ir B
        /// </summary>
        public GolayService(GolayMatrices matrices)
        {
            _matrices = matrices;
        }

        #region ═══════════════════════════ PUBLIC API METHODS ═══════════════════════════

        /// <summary>
        /// Grąžina parities matricą P̂ (12×11).
        /// Parametrai: nėra
        /// Grąžina: 12×11 matricą GF(2) lauke
        /// </summary>
        public int[][] GetParityMatrix()
        {
            return _matrices.GetParityMatrix();
        }

        /// <summary>
        /// Grąžina identiteto matricą I₁₂ (12×12).
        /// Parametrai: nėra
        /// Grąžina: 12×12 identiteto matricą
        /// </summary>
        public int[][] GetIdentityMatrix()
        {
            return _matrices.GetIdentityMatrix();
        }

        /// <summary>
        /// Grąžina B matricą (12×12) išplėstam C24 kodui.
        /// Parametrai: nėra
        /// Grąžina: 12×12 B matricą
        /// </summary>
        public int[][] GetBMatrix()
        {
            return _matrices.GetBMatrix();
        }

        /// <summary>
        /// Sukonstruoja ir grąžina generatoriaus matricą G = [I₁₂ | P̂].
        /// Sisteminė kodo forma - pranešimas matomas pirmose 12 pozicijų.
        /// Parametrai: nėra
        /// Grąžina: 12×23 generatoriaus matricą
        /// </summary>
        public int[][] GetGeneratorMatrix()
        {
            var identity = _matrices.IdentityMatrix;
            var parity = _matrices.ParityMatrix;
            var generator = new int[12][];

            // Kiekviena eilutė: [I₁₂ | P̂]
            // Pirmi 12 stulpelių - identiteto matrica
            // Paskutiniai 11 stulpelių - parities matrica
            for (int i = 0; i < 12; i++)
            {
                generator[i] = new int[23];

                // Kopijuoti identiteto dalį (stulpeliai 0-11)
                for (int j = 0; j < 12; j++)
                {
                    generator[i][j] = identity[i][j];
                }

                // Kopijuoti parities dalį (stulpeliai 12-22)
                for (int j = 0; j < 11; j++)
                {
                    generator[i][j + 12] = parity[i][j];
                }
            }

            return generator;
        }

        /// <summary>
        /// Užkoduoja 12-bitų pranešimą į 23-bitų kodo žodį naudojant Golay (23,12,7) kodą.
        /// Algoritmas: c = m × G, kur G - generatoriaus matrica, daugyba GF(2) lauke.
        /// Parametrai:
        ///   message - 12-bitų pranešimas (sveikasis skaičius 0-4095)
        /// Grąžina:
        ///   23-bitų kodo žodį (sveikasis skaičius 0-8388607)
        /// Išmeta:
        ///   ArgumentException jei pranešimas ne 12 bitų
        /// </summary>
        public int Encode(int message)
        {
            if (message < 0 || message >= (1 << 12))
            {
                throw new ArgumentException("Message must be 12 bits (0-4095)", nameof(message));
            }

            int codeword = 0;
            var generator = GetGeneratorMatrix();

            // Matricos-vektoriaus daugyba GF(2) lauke
            // Kiekvienas pranešimo bitas i, jei lygus 1, prideda generatoriaus i-tą eilutę į rezultatą
            // Sudėtis GF(2) = XOR
            for (int i = 0; i < 12; i++)
            {
                if ((message & (1 << i)) != 0)
                {
                    // Pridėti i-tą generatoriaus eilutę (XOR)
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

        /// <summary>
        /// Dekoduoja 23-bitų kodo žodį į 12-bitų pranešimą naudojant Algoritmą 3.7.1.
        /// Taiso iki 3 bitų klaidas.
        /// Algoritmas (literatura12.pdf, p. 88):
        ///   1. Išplėsti C23 žodį į C24 (pridėti parities bitą)
        ///   2. Dekoduoti C24 naudojant IMLD (Algoritmas 3.6.1)
        ///   3. Ekstraktuoti pranešimą iš pirmų 12 bitų
        /// Parametrai:
        ///   codeword - 23-bitų gautas žodis (gali turėti klaidų)
        /// Grąžina:
        ///   12-bitų dekoduotą pranešimą
        /// Išmeta:
        ///   ArgumentException jei kodo žodis ne 23 bitai
        /// </summary>
        public int Decode(int codeword)
        {
            if (codeword < 0 || codeword >= (1 << 23))
            {
                throw new ArgumentException("Codeword must be 23 bits (0-8388607)", nameof(codeword));
            }

            // Žingsnis 1: Konvertuoti į bitų masyvą ir apskaičiuoti svorį
            int[] w23 = IntToBitArray(codeword, 23);
            int weight23 = CalculateWeight(w23);

            // Žingsnis 2: Išplėsti į 24 bitus (algoritmas 3.7.1)
            // Pridedamas parities bitas taip, kad bendras svoris būtų nelyginis
            int[] w24 = new int[24];
            Array.Copy(w23, w24, 23);

            if (weight23 % 2 == 0) {
                w24[23] = 1;  // Pridėti 1, kad svoris būtų nelyginis
            }
            else {
                w24[23] = 0;  // Pridėti 0, kad svoris liktų nelyginis
            }

            // Žingsnis 3: Dekoduoti C24 žodį naudojant IMLD algoritmą
            var (correctedCodeword24, errorPattern, success) = DecodeC24_IMLD(w24);

            // Žingsnis 4: Jei nepavyko ištaisyti, grąžinti pirmųjų 12 bitų kaip pranešimą
            if (!success)
            {
                return codeword & ((1 << 12) - 1);
            }

            // Žingsnis 5: Ekstraktuoti pranešimą iš pirmų 12 bitų (sisteminė forma)
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
        /// Dekoduoja kodo žodį ir grąžina detalią informaciją apie procesą.
        /// Naudojama mokymuisi ir demonstravimui - rodo sindromas, klaidų šabloną, ir kt.
        /// Parametrai:
        ///   codeword - 23-bitų gautas žodis
        /// Grąžina:
        ///   DecodeResult objektą su detalia dekodavimo informacija:
        ///     - Sindromas S1 ir S2
        ///     - Klaidų šablonas ir pozicijos
        ///     - Ištaisytas kodo žodis
        ///     - Dekoduotas pranešimas
        ///     - Sėkmės statusas
        /// </summary>
        public DecodeResult DecodeWithDetails(int codeword)
        {
            if (codeword < 0 || codeword >= (1 << 23))
            {
                throw new ArgumentException("Codeword must be 23 bits (0-8388607)", nameof(codeword));
            }

            // Konvertuoti į bitų masyvą ir apskaičiuoti svorį
            int[] w23 = IntToBitArray(codeword, 23);
            int weight23 = CalculateWeight(w23);

            // Išplėsti į 24 bitus pridedant parities bitą
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

            // Dekoduoti naudojant IMLD algoritmą
            var (correctedCodeword24, errorPattern, success) = DecodeC24_IMLD(w24);

            // Rasti klaidų pozicijas C23 kodo žodyje (bitai 0-22)
            // PASTABA: errorPattern[23] yra C24 parities bitas - jo NEREIKIA raportuoti
            // Raportuojame masyvų indeksus tiesiogiai - frontend juos transformuos
            var errorPositions = new List<int>();
            for (int i = 0; i < 23; i++)  // Tik C23 pozicijos (0-22)
            {
                if (errorPattern[i] == 1)
                {
                    errorPositions.Add(i);  // Frontend atlieka pozicijų transformaciją
                }
            }

            // Ekstraktuoti pranešimą
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
                // Jei nepavyko ištaisyti, grąžinti pirmųjų 12 bitų
                message = codeword & ((1 << 12) - 1);
            }

            // Apskaičiuoti sindromas mokomajam tikslui
            int[] s1 = ComputeSyndromeS1(w24);
            int[] s2 = ComputeSyndromeS2(s1);

            // Konvertuoti į bitų masyvus nuosekliam reversavimui
            int[] originalCodewordArray = IntToBitArray(codeword, 23);
            int[] decodedMessageArray = IntToBitArray(message, 12);

            // Sukurti detalų rezultatų objektą
            return new DecodeResult
            {
                OriginalCodeword = codeword,
                OriginalCodewordBinary = string.Join("", originalCodewordArray.Reverse()),
                ExtendedWord = BitArrayToInt(w24),
                ExtendedWordBinary = string.Join("", w24.Reverse()),
                AppendedBit = appendedBit,
                SyndromeS1 = string.Join("", s1.Reverse()),
                SyndromeS1Weight = CalculateWeight(s1),
                SyndromeS2 = string.Join("", s2.Reverse()),
                SyndromeS2Weight = CalculateWeight(s2),
                ErrorPattern = string.Join("", errorPattern.Take(23).Reverse()),
                ErrorCount = errorPositions.Count,
                ErrorPositions = errorPositions.ToArray(),
                CorrectedCodeword = BitArrayToInt(correctedCodeword24.Take(23).ToArray()),
                CorrectedCodewordBinary = string.Join("", correctedCodeword24.Take(23).Reverse()),
                DecodedMessage = message,
                DecodedMessageBinary = string.Join("", decodedMessageArray.Reverse()),
                Success = success,
                Status = success
                    ? $"Successfully corrected {errorPositions.Count} error(s)"
                    : "Too many errors to correct (>3)"
            };
        }

        /// <summary>
        /// Simuliuoja dvejetainį simetrinį kanalą (Binary Symmetric Channel - BSC).
        /// Kiekvienas bitas apverčiamas nepriklausomai su tikimybe errorProbability.
        /// Parametrai:
        ///   codeword - 23-bitų kodo žodis
        ///   errorProbability - klaidų tikimybė vienam bitui (0.0-1.0)
        /// Grąžina:
        ///   Tuple su:
        ///     - corruptedCodeword: sugadintas kodo žodis
        ///     - errorPositions: klaidų pozicijos masyve
        ///     - errorCount: klaidų skaičius
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

            // Kiekvienam bitui nepriklausomai tikrinti ar apversti
            for (int i = 0; i < 23; i++)
            {
                if (_random.NextDouble() < errorProbability)
                {
                    corruptedCodeword ^= 1 << i;  // Apversti bitą i (XOR)
                    errorPositionsList.Add(i);
                }
            }

            return (corruptedCodeword, errorPositionsList.ToArray(), errorPositionsList.Count);
        }

        #endregion

        #region ═══════════════════════════ IMLD ALGORITHM (3.6.1) ═══════════════════════════

        /// <summary>
        /// Dekoduoja C24 kodo žodį naudojant IMLD (Iterative Majority Logic Decoding) algoritmą.
        /// Implementuoja Algoritmą 3.6.1 iš literatura12.pdf (p. 85).
        /// Algoritmo žingsniai:
        ///   1. Apskaičiuoti sindromą s₁ = w₁ + w₂B
        ///   2. Jei wt(s₁) ≤ 3: klaidų šablonas u = [s₁, 0]
        ///   3. Kitu atveju: tikrinti s₁ + bᵢ kiekvienai B eilutei bᵢ
        ///   4. Jei nepavyko: apskaičiuoti s₂ = s₁B ir kartoti
        /// Parametrai:
        ///   w24 - 24-bitų gautas žodis (nelyginė svoris)
        /// Grąžina:
        ///   Tuple su:
        ///     - correctedCodeword: ištaisytas kodo žodis
        ///     - errorPattern: klaidų šablonas (24 bitai)
        ///     - success: ar pavyko ištaisyti
        /// </summary>
        private (int[] correctedCodeword, int[] errorPattern, bool success) DecodeC24_IMLD(int[] w24)
        {
            var B = _matrices.BMatrix;

            // Žingsnis 1: Apskaičiuoti pirmą sindromą s₁ = w₁ + w₂B
            int[] s1 = ComputeSyndromeS1(w24);
            int weightS1 = CalculateWeight(s1);

            // Žingsnis 2: Jei wt(s₁) ≤ 3, klaidų šablonas u = [s₁, 0]
            // Klaidos pirmoje pusėje (w₁)
            if (weightS1 <= 3)
            {
                int[] errorPattern = new int[24];
                Array.Copy(s1, errorPattern, 12);  // u₁ = s₁, u₂ = 0

                int[] corrected = XorVectors(w24, errorPattern);
                int[] testSyndrome = ComputeSyndromeS1(corrected);
                if (CalculateWeight(testSyndrome) == 0)
                {
                    return (corrected, errorPattern, true);
                }
            }

            // Žingsnis 3: Tikrinti s₁ + bᵢ visoms B eilutėms bᵢ
            // Jei wt(s₁ + bᵢ) ≤ 2, klaidų šablonas u = [s₁ + bᵢ, eᵢ]
            for (int i = 0; i < 12; i++)
            {
                int[] s1PlusBi = XorVectors(s1, B[i]);
                if (CalculateWeight(s1PlusBi) <= 2)
                {
                    int[] errorPattern = new int[24];
                    Array.Copy(s1PlusBi, errorPattern, 12);  // u₁ = s₁ + bᵢ
                    errorPattern[12 + i] = 1;  // u₂ = eᵢ (vienetinis vektorius i pozicijoje)

                    int[] corrected = XorVectors(w24, errorPattern);
                    int[] testSyndrome = ComputeSyndromeS1(corrected);
                    if (CalculateWeight(testSyndrome) == 0)
                    {
                        return (corrected, errorPattern, true);
                    }
                }
            }

            // Žingsnis 4: Apskaičiuoti antrą sindromą s₂ = s₁B
            int[] s2 = ComputeSyndromeS2(s1);
            int weightS2 = CalculateWeight(s2);

            // Žingsnis 5: Jei wt(s₂) ≤ 3, klaidų šablonas u = [0, s₂]
            // Klaidos antroje pusėje (w₂)
            if (weightS2 <= 3)
            {
                int[] errorPattern = new int[24];
                Array.Copy(s2, 0, errorPattern, 12, 12);  // u₁ = 0, u₂ = s₂

                int[] corrected = XorVectors(w24, errorPattern);
                int[] testSyndrome = ComputeSyndromeS1(corrected);
                if (CalculateWeight(testSyndrome) == 0)
                {
                    return (corrected, errorPattern, true);
                }
            }

            // Žingsnis 6: Tikrinti s₂ + bᵢ visoms B eilutėms bᵢ
            // Jei wt(s₂ + bᵢ) ≤ 2, klaidų šablonas u = [eᵢ, s₂ + bᵢ]
            for (int i = 0; i < 12; i++)
            {
                int[] s2PlusBi = XorVectors(s2, B[i]);
                if (CalculateWeight(s2PlusBi) <= 2)
                {
                    int[] errorPattern = new int[24];
                    errorPattern[i] = 1;  // u₁ = eᵢ (vienetinis vektorius i pozicijoje)
                    Array.Copy(s2PlusBi, 0, errorPattern, 12, 12);  // u₂ = s₂ + bᵢ

                    // Validuoti: patikrinti ar šis klaidų šablonas duoda teisingą kodo žodį
                    int[] corrected = XorVectors(w24, errorPattern);
                    int[] testSyndrome = ComputeSyndromeS1(corrected);
                    if (CalculateWeight(testSyndrome) == 0)
                    {
                        return (corrected, errorPattern, true);
                    }
                }
            }

            // Žingsnis 7: Nepavyko ištaisyti (>3 klaidos)
            return (w24, new int[24], false);
        }

        /// <summary>
        /// Apskaičiuoja sindromą S₁ = w₁ + w₂B.
        /// Sindromas priklauso tik nuo klaidų šablono, ne nuo pradinio kodo žodžio.
        /// Parametrai:
        ///   w24 - 24-bitų gautas žodis
        /// Grąžina:
        ///   12-bitų sindromą s₁
        /// </summary>
        private int[] ComputeSyndromeS1(int[] w24)
        {
            var B = _matrices.BMatrix;

            // Padalinti w24 į dvi puses: w₁ (0-11) ir w₂ (12-23)
            int[] w1 = new int[12];
            int[] w2 = new int[12];
            Array.Copy(w24, 0, w1, 0, 12);
            Array.Copy(w24, 12, w2, 0, 12);

            // Apskaičiuoti w₂B (matricos daugyba GF(2))
            int[] w2B = MultiplyVectorByMatrix(w2, B);

            // Grąžinti s₁ = w₁ + w₂B (sudėtis GF(2) = XOR)
            return XorVectors(w1, w2B);
        }

        /// <summary>
        /// Apskaičiuoja sindromą S₂ = S₁B.
        /// Naudojamas algoritmo 3.6.1 žingsniuose 4-6.
        /// Parametrai:
        ///   s1 - pirmasis sindromas (12 bitų)
        /// Grąžina:
        ///   12-bitų sindromą s₂
        /// </summary>
        private int[] ComputeSyndromeS2(int[] s1)
        {
            var B = _matrices.BMatrix;
            return MultiplyVectorByMatrix(s1, B);
        }

        #endregion

        #region ═══════════════════════════ TEXT PROCESSING ═══════════════════════════

        /// <summary>
        /// Užkoduoja tekstą į Golay kodo žodžių masyvą.
        /// Procesas: tekstas → UTF-8 baitai → bitai → 12-bitų blokai → kodo žodžiai.
        /// Parametrai:
        ///   text - UTF-8 tekstas (gali būti kelių eilučių)
        /// Grąžina:
        ///   TextEncodeResult su:
        ///     - Pranešimų masyvu (12-bitų blokai)
        ///     - Kodo žodžių masyvu (23-bitų blokai)
        ///     - Padding informacija (TARNYBINĖ INFORMACIJA - nesiunčiama per kanalą)
        /// </summary>
        public TextEncodeResult EncodeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }

            // Konvertuoti tekstą į UTF-8 baitus
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            // Konvertuoti baitus į bitų srautą (LSB-first)
            var bits = new List<int>();
            foreach (byte b in bytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }

            // Apskaičiuoti padding (užpildymas nuliais iki 12 kartotinio)
            // SVARBU: paddingBits yra tarnybinė informacija - NESIUNČIAMA per kanalą
            int originalBitCount = bits.Count;
            int paddingBits = (12 - (bits.Count % 12)) % 12;
            for (int i = 0; i < paddingBits; i++)
            {
                bits.Add(0);
            }

            // Suskaidyti bitus į 12-bitų blokus ir užkoduoti
            var messages = new List<int>();
            var codewords = new List<int>();

            for (int i = 0; i < bits.Count; i += 12)
            {
                // Sudėti 12 bitų į vienetą pranešimą
                int message = 0;
                for (int j = 0; j < 12; j++)
                {
                    message |= bits[i + j] << j;
                }
                messages.Add(message);

                // Užkoduoti pranešimą
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
        /// Siunčia kodo žodžių masyvą per BSC kanalą.
        /// Kiekvienas kodo žodis sugadinamas nepriklausomai.
        /// Parametrai:
        ///   codewords - kodo žodžių masyvas
        ///   errorProbability - klaidų tikimybė vienam bitui (0.0-1.0)
        /// Grąžina:
        ///   TextChannelResult su:
        ///     - Sugadintų kodo žodžių masyvu
        ///     - Klaidų pozicijomis kiekvienam kodo žodžiui
        ///     - Bendru klaidų skaičiumi
        /// </summary>
        public TextChannelResult SendTextThroughChannel(int[] codewords, double errorProbability)
        {
            var corruptedCodewords = new List<int>();
            var allErrorPositions = new List<int[]>();
            int totalErrors = 0;

            // Kiekvieną kodo žodį siųsti per kanalą nepriklausomai
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
        /// Dekoduoja kodo žodžių masyvą atgal į tekstą.
        /// Procesas: kodo žodžiai → 12-bitų blokai → bitai → UTF-8 baitai → tekstas.
        /// Parametrai:
        ///   codewords - kodo žodžių masyvas (gali turėti klaidų)
        ///   paddingBits - užpildymo bitų skaičius (tarnybinė informacija iš kodavimo)
        /// Grąžina:
        ///   TextDecodeResult su:
        ///     - Dekoduotu tekstu
        ///     - Ištaisytų klaidų skaičiumi
        ///     - Neištaisomų blokų skaičiumi (>3 klaidos bloke)
        /// </summary>
        public TextDecodeResult DecodeText(int[] codewords, int paddingBits)
        {
            var messages = new List<int>();
            var bits = new List<int>();
            int correctedErrors = 0;
            int uncorrectableBlocks = 0;

            // Dekoduoti kiekvieną kodo žodį
            foreach (int codeword in codewords)
            {
                var result = DecodeWithDetails(codeword);
                messages.Add(result.DecodedMessage);

                // Skaičiuoti statistiką
                if (result.Success)
                {
                    correctedErrors += result.ErrorCount;
                }
                else
                {
                    uncorrectableBlocks++;
                }

                // Konvertuoti pranešimą į bitus (LSB-first)
                for (int i = 0; i < 12; i++)
                {
                    bits.Add((result.DecodedMessage >> i) & 1);
                }
            }

            // Pašalinti padding bitus (naudojant išsaugotą metadata)
            if (paddingBits > 0)
            {
                bits.RemoveRange(bits.Count - paddingBits, paddingBits);
            }

            // Konvertuoti bitus atgal į baitus (8 bitai = 1 baitas)
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

            // Konvertuoti baitus į UTF-8 tekstą
            string decodedText;
            try
            {
                decodedText = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            }
            catch
            {
                // Jei baitai nesudaro teisingos UTF-8 sekos (per daug klaidų)
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
        /// Užkoduoja 24-bitų BMP vaizdą į Golay kodo žodžių masyvą.
        /// SVARBU: BMP antraštė išsaugoma kaip tarnybinė informacija ir NESIUNČIAMA per kanalą.
        /// Koduojami tik pikselių duomenys.
        /// Parametrai:
        ///   imageData - BMP failo baitai
        /// Grąžina:
        ///   ImageEncodeResult su:
        ///     - Antraše (tarnybinė informacija)
        ///     - Kodo žodžių masyvu (tik pikseliai)
        ///     - Padding informacija
        /// </summary>
        public ImageEncodeResult EncodeImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 54)
            {
                throw new ArgumentException("Invalid BMP image data", nameof(imageData));
            }

            // Patikrinti BMP signatūrą "BM"
            if (imageData[0] != 'B' || imageData[1] != 'M')
            {
                throw new ArgumentException("Not a valid BMP file (missing BM signature)", nameof(imageData));
            }

            // Nuskaityti antraštės dydį iš BMP failo (offsetas 10, 4 baitai)
            int headerSize = BitConverter.ToInt32(imageData, 10);

            // Išskirti antraštę (TARNYBINĖ INFORMACIJA - nesiunčiama per kanalą)
            byte[] header = new byte[headerSize];
            Array.Copy(imageData, header, headerSize);

            // Išskirti pikselių duomenis (bus koduojami)
            byte[] pixelData = new byte[imageData.Length - headerSize];
            Array.Copy(imageData, headerSize, pixelData, 0, pixelData.Length);

            // Konvertuoti pikselių duomenis į bitus
            var bits = new List<int>();
            foreach (byte b in pixelData)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }

            // Pridėti padding iki 12 kartotinio
            int originalBitCount = bits.Count;
            int paddingBits = (12 - (bits.Count % 12)) % 12;
            for (int i = 0; i < paddingBits; i++)
            {
                bits.Add(0);
            }

            // Suskaidyti į 12-bitų blokus ir užkoduoti
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

        /// <summary>
        /// Siunčia vaizdo kodo žodžius per BSC kanalą.
        /// Analogiškas SendTextThroughChannel, bet optimizuotas vaizdams.
        /// Parametrai:
        ///   codewords - kodo žodžių masyvas
        ///   errorProbability - klaidų tikimybė (0.0-1.0)
        /// Grąžina:
        ///   ImageChannelResult su sugadintais kodo žodžiais ir statistika
        /// </summary>
        public ImageChannelResult SendImageThroughChannel(int[] codewords, double errorProbability)
        {
            var corruptedCodewords = new List<int>();
            int totalErrors = 0;

            // Kiekvieną kodo žodį siųsti per kanalą
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
        /// Dekoduoja kodo žodžius atgal į BMP vaizdą.
        /// Procesas: kodo žodžiai → pikselių bitai → pikselių baitai → BMP vaizdas (su antraše).
        /// Parametrai:
        ///   header - BMP antraštė (tarnybinė informacija iš kodavimo)
        ///   codewords - kodo žodžių masyvas (gali turėti klaidų)
        ///   paddingBits - užpildymo bitų skaičius (tarnybinė informacija)
        ///   originalPixelSize - originalus pikselių duomenų dydis baitais
        /// Grąžina:
        ///   ImageDecodeResult su:
        ///     - Dekoduotu BMP vaizdu (antraštė + pikseliai)
        ///     - Ištaisytų klaidų skaičiumi
        ///     - Neištaisomų blokų skaičiumi
        /// </summary>
        public ImageDecodeResult DecodeImage(byte[] header, int[] codewords, int paddingBits, int originalPixelSize)
        {
            var bits = new List<int>();
            int correctedErrors = 0;
            int uncorrectableBlocks = 0;

            // Dekoduoti kiekvieną kodo žodį
            foreach (int codeword in codewords)
            {
                var result = DecodeWithDetails(codeword);

                // Skaičiuoti statistiką
                if (result.Success)
                {
                    correctedErrors += result.ErrorCount;
                }
                else
                {
                    uncorrectableBlocks++;
                }

                // Konvertuoti 12-bitų pranešimą į bitus (LSB-first)
                for (int i = 0; i < 12; i++)
                {
                    bits.Add((result.DecodedMessage >> i) & 1);
                }
            }

            // Pašalinti padding bitus
            if (paddingBits > 0)
            {
                bits.RemoveRange(bits.Count - paddingBits, paddingBits);
            }

            // Konvertuoti bitus atgal į baitus (8 bitai = 1 baitas)
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

            // Sujungti antraštę su dekoduotais pikseliais
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
        /// Konvertuoja sveikąjį skaičių į bitų masyvą (LSB-first formatu).
        /// Pavyzdys: 5 (dešimtainis) → [1,0,1,0,0,0...] (bitas 0 yra LSB)
        /// Parametrai:
        ///   value - sveikasis skaičius
        ///   length - bitų masyvo ilgis
        /// Grąžina:
        ///   Bitų masyvą (kiekvienas elementas 0 arba 1)
        /// </summary>
        private int[] IntToBitArray(int value, int length)
        {
            int[] bits = new int[length];
            for (int i = 0; i < length; i++)
            {
                // Ekstraktuoti i-tą bitą (LSB = 0)
                bits[i] = (value >> i) & 1;
            }
            return bits;
        }

        /// <summary>
        /// Konvertuoja sveikąjį skaičių į apverstą dvejetainę eilutę (MSB-first atvaizdavimui).
        /// SVARBU: Naudojama nuosekliam dvejetainių eilučių formatui visame backend'e.
        /// Parametrai:
        ///   value - sveikasis skaičius
        ///   bitCount - bitų skaičius
        /// Grąžina:
        ///   Dvejetainę eilutę (pvz., "10100111...")
        /// </summary>
        public string IntToReversedBinaryString(int value, int bitCount)
        {
            int[] bits = IntToBitArray(value, bitCount);
            return string.Join("", bits.Reverse());
        }

        /// <summary>
        /// Konvertuoja bitų masyvą atgal į sveikąjį skaičių.
        /// Atvirkštinė operacija IntToBitArray.
        /// Parametrai:
        ///   bits - bitų masyvas (LSB-first)
        /// Grąžina:
        ///   Sveikąjį skaičių
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
        /// Apskaičiuoja Hamming svorį (vienetų skaičių) bitų masyve.
        /// Naudojama sindromų analizei ir klaidų skaičiavimui.
        /// Parametrai:
        ///   bits - bitų masyvas
        /// Grąžina:
        ///   Vienetų skaičių
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
        /// Sudeda du bitų masyvus GF(2) lauke (XOR operacija).
        /// Sudėtis GF(2): 0+0=0, 0+1=1, 1+0=1, 1+1=0 (XOR)
        /// Parametrai:
        ///   a - pirmasis bitų masyvas
        ///   b - antrasis bitų masyvas (gali būti skirtingo ilgio)
        /// Grąžina:
        ///   Rezultatų masyvą (ilgis = max(a.Length, b.Length))
        /// </summary>
        private int[] XorVectors(int[] a, int[] b)
        {
            int length = Math.Min(a.Length, b.Length);
            int[] result = new int[Math.Max(a.Length, b.Length)];

            // XOR bendrai daliai
            for (int i = 0; i < length; i++)
            {
                result[i] = a[i] ^ b[i];
            }

            // Kopijuoti likusią ilgesnio masyvo dalį
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
        /// Daugina vektorių iš matricos GF(2) lauke.
        /// Operacija: result[j] = Σ(vector[i] AND matrix[i][j]) (daugyba=AND, sudėtis=XOR)
        /// Parametrai:
        ///   vector - eilutės vektorius (1×n)
        ///   matrix - matrica (n×m)
        /// Grąžina:
        ///   Rezultatų vektorių (1×m)
        /// </summary>
        private int[] MultiplyVectorByMatrix(int[] vector, int[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            int[] result = new int[cols];

            // Kiekvienam stulpeliui j
            for (int j = 0; j < cols; j++)
            {
                int sum = 0;
                // Sumuoti (XOR) visus vector[i] AND matrix[i][j]
                for (int i = 0; i < rows && i < vector.Length; i++)
                {
                    sum ^= vector[i] & matrix[i][j];  // Daugyba GF(2) = AND, sudėtis = XOR
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

    public class TextChannelResult
    {
        public int[] OriginalCodewords { get; set; } = Array.Empty<int>();
        public int[] CorruptedCodewords { get; set; } = Array.Empty<int>();
        public int[][] ErrorPositions { get; set; } = Array.Empty<int[]>();
        public int TotalErrors { get; set; }
        public double ErrorProbability { get; set; }
    }

    public class TextDecodeResult
    {
        public string DecodedText { get; set; } = "";
        public int[] Messages { get; set; } = Array.Empty<int>();
        public int CorrectedErrors { get; set; }
        public int UncorrectableBlocks { get; set; }
        public int TotalBlocks { get; set; }
    }

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

    public class ImageChannelResult
    {
        public int[] OriginalCodewords { get; set; } = Array.Empty<int>();
        public int[] CorruptedCodewords { get; set; } = Array.Empty<int>();
        public int TotalErrors { get; set; }
        public double ErrorProbability { get; set; }
    }

    public class ImageDecodeResult
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public int CorrectedErrors { get; set; }
        public int UncorrectableBlocks { get; set; }
        public int TotalBlocks { get; set; }
    }

    #endregion
}
