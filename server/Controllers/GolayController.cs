using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GolayController : ControllerBase
    {
        private readonly GolayService _golayService;

        private static readonly Random _random = new Random();

        public GolayController(GolayService golayService)
        {
            _golayService = golayService;
        }

        #region ═══════════════════════════ MATRIX ENDPOINTS ═══════════════════════════

        [HttpGet("matrix-p")]
        public ActionResult<int[][]> GetGolayMatrixP()
        {
            var matrix = _golayService.GetParityMatrix();
            return Ok(matrix);
        }

        [HttpGet("matrix-identity")]
        public ActionResult<int[][]> GetIdentityMatrix()
        {
            var matrix = _golayService.GetIdentityMatrix();
            return Ok(matrix);
        }

        [HttpGet("matrix-b")]
        public ActionResult<int[][]> GetBMatrix()
        {
            var matrix = _golayService.GetBMatrix();
            return Ok(matrix);
        }

        [HttpGet("generator-matrix")]
        public ActionResult<int[][]> GetGeneratorMatrix()
        {
            var matrix = _golayService.GetGeneratorMatrix();
            return Ok(matrix);
        }

        #endregion

        #region ═══════════════════════════ SINGLE VECTOR ENDPOINTS (Scenario 1) ═══════════════════════════

        [HttpPost("encode")]
        public ActionResult<object> Encode([FromBody] EncodeRequest request)
        {
            try
            {
                var codeword = _golayService.Encode(request.Message);
                return Ok(new
                {
                    message = request.Message,
                    messageBinary = _golayService.IntToReversedBinaryString(request.Message, 12),
                    codeword = codeword,
                    codewordBinary = _golayService.IntToReversedBinaryString(codeword, 23)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("decode")]
        public ActionResult<object> Decode([FromBody] DecodeRequest request)
        {
            try
            {
                var message = _golayService.Decode(request.Codeword);
                return Ok(new
                {
                    codeword = request.Codeword,
                    codewordBinary = _golayService.IntToReversedBinaryString(request.Codeword, 23),
                    message = message,
                    messageBinary = _golayService.IntToReversedBinaryString(message, 12)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

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
                    originalCodewordBinary = _golayService.IntToReversedBinaryString(request.Codeword, 23),
                    errorProbability = request.ErrorProbability,
                    corruptedCodeword = corruptedCodeword,
                    corruptedCodewordBinary = _golayService.IntToReversedBinaryString(corruptedCodeword, 23),
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

        [HttpPost("text/full-demo")]
        public ActionResult<object> TextFullDemo([FromBody] TextFullDemoRequest request)
        {
            try
            {
                var encodeResult = _golayService.EncodeText(request.Text);

                var channelResult = _golayService.SendTextThroughChannel(
                    encodeResult.Codewords,
                    request.ErrorProbability);

                var decodeResult = _golayService.DecodeText(
                    channelResult.CorruptedCodewords,
                    encodeResult.PaddingBits);

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

                var decodeResult = _golayService.DecodeImage(
                    encodeResult.Header,
                    channelResult.CorruptedCodewords,
                    encodeResult.PaddingBits,
                    encodeResult.PixelDataSize);

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
