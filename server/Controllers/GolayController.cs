using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers
{
    /// <summary>
    /// ╔═══════════════════════════════════════════════════════════════════════════╗
    /// ║                    GOLAY CODE API CONTROLLER                              ║
    /// ║                                                                            ║
    /// ║  REST API endpoints for Golay (23,12,7) encoding, decoding, and          ║
    /// ║  channel simulation with support for vectors, text, and images           ║
    /// ╚═══════════════════════════════════════════════════════════════════════════╝
    ///
    /// ENDPOINTS OVERVIEW:
    /// ===================
    ///
    /// MATRICES:
    ///   GET  /golay/matrix-p          - Get parity matrix P̂ (12×11)
    ///   GET  /golay/matrix-identity   - Get identity matrix I (12×12)
    ///   GET  /golay/matrix-b          - Get B matrix (12×12) for C24
    ///   GET  /golay/generator-matrix  - Get generator matrix G (12×23)
    ///
    /// SINGLE VECTOR (Scenario 1):
    ///   POST /golay/encode            - Encode 12-bit message to 23-bit codeword
    ///   POST /golay/decode            - Decode 23-bit codeword (with error correction)
    ///   POST /golay/decode-detailed   - Decode with full algorithm details
    ///   POST /golay/channel           - Simulate BSC channel errors
    ///
    /// TEXT PROCESSING (Scenario 2):
    ///   POST /golay/text/encode       - Encode text to codewords
    ///   POST /golay/text/channel      - Send codewords through channel
    ///   POST /golay/text/decode       - Decode codewords to text
    ///   POST /golay/text/full-demo    - Complete encode→channel→decode pipeline
    ///
    /// IMAGE PROCESSING (Scenario 3):
    ///   POST /golay/image/encode      - Encode BMP image
    ///   POST /golay/image/channel     - Send encoded image through channel
    ///   POST /golay/image/decode      - Decode image from codewords
    ///   POST /golay/image/full-demo   - Complete encode→channel→decode pipeline
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class GolayController : ControllerBase
    {
        private readonly GolayService _golayService;

        // Static Random for BSC simulation (initialized once per Tasks.md requirement)
        private static readonly Random _random = new Random();

        public GolayController(GolayService golayService)
        {
            _golayService = golayService;
        }

        #region ═══════════════════════════ MATRIX ENDPOINTS ═══════════════════════════

        /// <summary>
        /// GET /golay/matrix-p
        /// Returns the parity matrix P̂ (12×11) used for Golay (23,12) code.
        /// </summary>
        [HttpGet("matrix-p")]
        public ActionResult<int[][]> GetGolayMatrixP()
        {
            var matrix = _golayService.GetParityMatrix();
            return Ok(matrix);
        }

        /// <summary>
        /// GET /golay/matrix-identity
        /// Returns the identity matrix I (12×12).
        /// </summary>
        [HttpGet("matrix-identity")]
        public ActionResult<int[][]> GetIdentityMatrix()
        {
            var matrix = _golayService.GetIdentityMatrix();
            return Ok(matrix);
        }

        /// <summary>
        /// GET /golay/matrix-b
        /// Returns the B matrix (12×12) used for extended Golay code C24.
        /// This matrix is essential for syndrome-based decoding.
        /// </summary>
        [HttpGet("matrix-b")]
        public ActionResult<int[][]> GetBMatrix()
        {
            var matrix = _golayService.GetBMatrix();
            return Ok(matrix);
        }

        /// <summary>
        /// GET /golay/generator-matrix
        /// Returns the generator matrix G = [I | P̂] (12×23).
        /// </summary>
        [HttpGet("generator-matrix")]
        public ActionResult<int[][]> GetGeneratorMatrix()
        {
            var matrix = _golayService.GetGeneratorMatrix();
            return Ok(matrix);
        }

        #endregion

        #region ═══════════════════════════ SINGLE VECTOR ENDPOINTS (Scenario 1) ═══════════════════════════

        /// <summary>
        /// POST /golay/encode
        ///
        /// Encodes a 12-bit message into a 23-bit Golay codeword.
        ///
        /// Request:  { "message": 42 }
        /// Response: { "message": 42, "messageBinary": "000000101010",
        ///             "codeword": 5678901, "codewordBinary": "10101..." }
        /// </summary>
        [HttpPost("encode")]
        public ActionResult<object> Encode([FromBody] EncodeRequest request)
        {
            try
            {
                var codeword = _golayService.Encode(request.Message);
                return Ok(new
                {
                    message = request.Message,
                    messageBinary = Convert.ToString(request.Message, 2).PadLeft(12, '0'),
                    codeword = codeword,
                    codewordBinary = Convert.ToString(codeword, 2).PadLeft(23, '0')
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/decode
        ///
        /// Decodes a 23-bit codeword with error correction (up to 3 errors).
        /// Uses Algorithm 3.7.1 from literatura12.pdf.
        ///
        /// Request:  { "codeword": 5678901 }
        /// Response: { "codeword": 5678901, "message": 42, ... }
        /// </summary>
        [HttpPost("decode")]
        public ActionResult<object> Decode([FromBody] DecodeRequest request)
        {
            try
            {
                var message = _golayService.Decode(request.Codeword);
                return Ok(new
                {
                    codeword = request.Codeword,
                    codewordBinary = Convert.ToString(request.Codeword, 2).PadLeft(23, '0'),
                    message = message,
                    messageBinary = Convert.ToString(message, 2).PadLeft(12, '0')
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/decode-detailed
        ///
        /// Decodes with full details about the syndrome-based algorithm.
        /// Useful for educational purposes and debugging.
        ///
        /// Returns: syndromes, error pattern, positions, correction status, etc.
        /// </summary>
        [HttpPost("decode-detailed")]
        public ActionResult<DecodeResult> DecodeDetailed([FromBody] DecodeRequest request)
        {
            try
            {
                var result = _golayService.DecodeWithDetails(request.Codeword);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/channel
        ///
        /// Simulates sending a codeword through a Binary Symmetric Channel (BSC).
        /// Each bit has an independent probability of being flipped.
        ///
        /// Request:  { "codeword": 5678901, "errorProbability": 0.1 }
        /// Response: { "originalCodeword": ..., "corruptedCodeword": ...,
        ///             "errorCount": 2, "errorPositions": [0, 14], "canCorrect": true }
        /// </summary>
        [HttpPost("channel")]
        public ActionResult<object> SimulateChannel([FromBody] ChannelRequest request)
        {
            try
            {
                var (corruptedCodeword, errorPositions, errorCount) =
                    _golayService.SimulateChannel(request.Codeword, request.ErrorProbability);

                return Ok(new
                {
                    originalCodeword = request.Codeword,
                    originalCodewordBinary = Convert.ToString(request.Codeword, 2).PadLeft(23, '0'),
                    errorProbability = request.ErrorProbability,
                    corruptedCodeword = corruptedCodeword,
                    corruptedCodewordBinary = Convert.ToString(corruptedCodeword, 2).PadLeft(23, '0'),
                    errorCount = errorCount,
                    errorPositions = errorPositions,
                    canCorrect = errorCount <= 3,
                    status = errorCount <= 3
                        ? $"✓ {errorCount} error(s) - Can be corrected by Golay code"
                        : $"✗ {errorCount} error(s) - Exceeds correction capability (max 3)"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region ═══════════════════════════ TEXT ENDPOINTS (Scenario 2) ═══════════════════════════

        /// <summary>
        /// POST /golay/text/encode
        ///
        /// Encodes text into Golay codewords.
        /// Text → UTF-8 bytes → bits → 12-bit chunks → 23-bit codewords
        ///
        /// Request:  { "text": "Hello, World!" }
        /// Response: { "originalText": "...", "codewords": [...], "paddingBits": 8, ... }
        /// </summary>
        [HttpPost("text/encode")]
        public ActionResult<TextEncodeResult> EncodeText([FromBody] TextEncodeRequest request)
        {
            try
            {
                var result = _golayService.EncodeText(request.Text);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/text/channel
        ///
        /// Sends encoded text codewords through a noisy channel.
        ///
        /// Request:  { "codewords": [...], "errorProbability": 0.05 }
        /// Response: { "corruptedCodewords": [...], "totalErrors": 15, ... }
        /// </summary>
        [HttpPost("text/channel")]
        public ActionResult<TextChannelResult> SendTextThroughChannel([FromBody] TextChannelRequest request)
        {
            try
            {
                var result = _golayService.SendTextThroughChannel(request.Codewords, request.ErrorProbability);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/text/decode
        ///
        /// Decodes codewords back to text with error correction.
        ///
        /// Request:  { "codewords": [...], "paddingBits": 8 }
        /// Response: { "decodedText": "Hello, World!", "correctedErrors": 15, ... }
        /// </summary>
        [HttpPost("text/decode")]
        public ActionResult<TextDecodeResult> DecodeText([FromBody] TextDecodeRequest request)
        {
            try
            {
                var result = _golayService.DecodeText(request.Codewords, request.PaddingBits);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/text/full-demo
        ///
        /// Complete text transmission demo: encode → channel → decode
        /// Shows both with and without error correction for comparison.
        ///
        /// Request:  { "text": "Hello", "errorProbability": 0.1 }
        /// Response: {
        ///   "original": "Hello",
        ///   "withoutCode": { "received": "He??o", "errors": 5 },
        ///   "withCode": { "decoded": "Hello", "errorsFound": 5, "errorsCorrected": 5 }
        /// }
        /// </summary>
        [HttpPost("text/full-demo")]
        public ActionResult<object> TextFullDemo([FromBody] TextFullDemoRequest request)
        {
            try
            {
                // Step 1: Encode the text
                var encodeResult = _golayService.EncodeText(request.Text);

                // Step 2: Send through channel (with error correction code)
                var channelResult = _golayService.SendTextThroughChannel(
                    encodeResult.Codewords,
                    request.ErrorProbability);

                // Step 3: Decode with error correction
                var decodeResult = _golayService.DecodeText(
                    channelResult.CorruptedCodewords,
                    encodeResult.PaddingBits);

                // Step 4: Also show what happens WITHOUT error correction
                // (just send raw bytes through channel)
                var rawBytes = System.Text.Encoding.UTF8.GetBytes(request.Text);
                var corruptedBytes = new byte[rawBytes.Length];
                int rawErrors = 0;

                for (int i = 0; i < rawBytes.Length; i++)
                {
                    byte corrupted = rawBytes[i];
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (_random.NextDouble() < request.ErrorProbability)
                        {
                            corrupted ^= (byte)(1 << bit);
                            rawErrors++;
                        }
                    }
                    corruptedBytes[i] = corrupted;
                }

                string corruptedText;
                try
                {
                    corruptedText = System.Text.Encoding.UTF8.GetString(corruptedBytes);
                }
                catch
                {
                    corruptedText = "[CORRUPTED - Invalid UTF-8]";
                }

                return Ok(new
                {
                    original = new
                    {
                        text = request.Text,
                        byteCount = rawBytes.Length,
                        bitCount = rawBytes.Length * 8
                    },
                    channel = new
                    {
                        errorProbability = request.ErrorProbability,
                        expectedErrorsPerBit = $"{request.ErrorProbability * 100:F1}%"
                    },
                    withoutCode = new
                    {
                        description = "Text sent directly through channel (NO error correction)",
                        receivedText = corruptedText,
                        bitErrors = rawErrors,
                        status = rawErrors > 0 ? "✗ Text corrupted" : "✓ No errors"
                    },
                    withCode = new
                    {
                        description = "Text encoded with Golay code (WITH error correction)",
                        codewordCount = encodeResult.MessageCount,
                        totalBitsSent = encodeResult.MessageCount * 23,
                        totalBitErrors = channelResult.TotalErrors,
                        errorsCorrected = decodeResult.CorrectedErrors,
                        uncorrectableBlocks = decodeResult.UncorrectableBlocks,
                        decodedText = decodeResult.DecodedText,
                        status = decodeResult.UncorrectableBlocks == 0
                            ? "✓ All errors corrected!"
                            : $"✗ {decodeResult.UncorrectableBlocks} block(s) had >3 errors"
                    },
                    comparison = new
                    {
                        withoutCodeMatch = request.Text == corruptedText,
                        withCodeMatch = request.Text == decodeResult.DecodedText,
                        conclusion = request.Text == decodeResult.DecodedText && request.Text != corruptedText
                            ? "Golay code successfully protected the message!"
                            : request.Text == corruptedText
                                ? "Channel was clean - no errors occurred"
                                : "Too many errors for Golay to correct"
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region ═══════════════════════════ IMAGE ENDPOINTS (Scenario 3) ═══════════════════════════

        /// <summary>
        /// POST /golay/image/encode
        ///
        /// Encodes a BMP image into Golay codewords.
        /// Header is preserved (not encoded), only pixel data is encoded.
        ///
        /// Request: BMP file as byte array
        /// Response: { "header": [...], "codewords": [...], "paddingBits": 4, ... }
        /// </summary>
        [HttpPost("image/encode")]
        public async Task<ActionResult<ImageEncodeResult>> EncodeImage()
        {
            try
            {
                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var imageData = ms.ToArray();

                var result = _golayService.EncodeImage(imageData);

                // Don't include full header in response (too large), just metadata
                return Ok(new
                {
                    headerSize = result.HeaderSize,
                    pixelDataSize = result.PixelDataSize,
                    bitCount = result.BitCount,
                    paddingBits = result.PaddingBits,
                    codewordCount = result.CodewordCount,
                    codewords = result.Codewords,
                    header = Convert.ToBase64String(result.Header)  // Base64 encoded header
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/image/channel
        ///
        /// Sends encoded image codewords through a noisy channel.
        /// </summary>
        [HttpPost("image/channel")]
        public ActionResult<ImageChannelResult> SendImageThroughChannel([FromBody] ImageChannelRequest request)
        {
            try
            {
                var result = _golayService.SendImageThroughChannel(request.Codewords, request.ErrorProbability);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/image/decode
        ///
        /// Decodes codewords back to BMP image with error correction.
        ///
        /// Returns the reconstructed image as a downloadable BMP file.
        /// </summary>
        [HttpPost("image/decode")]
        public ActionResult DecodeImage([FromBody] ImageDecodeRequest request)
        {
            try
            {
                byte[] header = Convert.FromBase64String(request.HeaderBase64);
                var result = _golayService.DecodeImage(
                    header,
                    request.Codewords,
                    request.PaddingBits,
                    request.OriginalPixelSize);

                // Return as downloadable BMP file
                return File(result.ImageData, "image/bmp", "decoded.bmp");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /golay/image/full-demo
        ///
        /// Complete image transmission demo with comparison.
        /// Accepts a BMP file and returns both:
        /// - Image sent without error correction (corrupted)
        /// - Image sent with Golay code (corrected)
        /// </summary>
        [HttpPost("image/full-demo")]
        public async Task<ActionResult> ImageFullDemo([FromQuery] double errorProbability = 0.05)
        {
            try
            {
                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var imageData = ms.ToArray();

                // Step 1: Encode
                var encodeResult = _golayService.EncodeImage(imageData);

                // Step 2: Send through channel
                var channelResult = _golayService.SendImageThroughChannel(
                    encodeResult.Codewords,
                    errorProbability);

                // Step 3: Decode with error correction
                var decodeResult = _golayService.DecodeImage(
                    encodeResult.Header,
                    channelResult.CorruptedCodewords,
                    encodeResult.PaddingBits,
                    encodeResult.PixelDataSize);

                // Step 4: Create corrupted image WITHOUT error correction
                int headerSize = encodeResult.HeaderSize;
                var corruptedWithoutCode = new byte[imageData.Length];
                Array.Copy(imageData, corruptedWithoutCode, headerSize); // Preserve header

                var random = new Random();
                int rawErrors = 0;
                for (int i = headerSize; i < imageData.Length; i++)
                {
                    byte corrupted = imageData[i];
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if (random.NextDouble() < errorProbability)
                        {
                            corrupted ^= (byte)(1 << bit);
                            rawErrors++;
                        }
                    }
                    corruptedWithoutCode[i] = corrupted;
                }

                // Return JSON with base64 encoded images
                return Ok(new
                {
                    stats = new
                    {
                        originalSize = imageData.Length,
                        headerSize = headerSize,
                        pixelDataSize = encodeResult.PixelDataSize,
                        codewordCount = encodeResult.CodewordCount,
                        errorProbability = errorProbability,
                        channelErrors = channelResult.TotalErrors,
                        correctedErrors = decodeResult.CorrectedErrors,
                        uncorrectableBlocks = decodeResult.UncorrectableBlocks,
                        rawBitErrors = rawErrors
                    },
                    images = new
                    {
                        original = Convert.ToBase64String(imageData),
                        corruptedWithoutCode = Convert.ToBase64String(corruptedWithoutCode),
                        decodedWithCode = Convert.ToBase64String(decodeResult.ImageData)
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion
    }

    #region ═══════════════════════════ REQUEST DTOs ═══════════════════════════

    // Single vector requests
    public record EncodeRequest(int Message);
    public record DecodeRequest(int Codeword);
    public record ChannelRequest(int Codeword, double ErrorProbability);

    // Text requests
    public record TextEncodeRequest(string Text);
    public record TextChannelRequest(int[] Codewords, double ErrorProbability);
    public record TextDecodeRequest(int[] Codewords, int PaddingBits);
    public record TextFullDemoRequest(string Text, double ErrorProbability);

    // Image requests
    public record ImageChannelRequest(int[] Codewords, double ErrorProbability);
    public record ImageDecodeRequest(
        string HeaderBase64,
        int[] Codewords,
        int PaddingBits,
        int OriginalPixelSize);

    #endregion
}
