using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GolayController : ControllerBase
    {
        private readonly GolayService _golayService;

        public GolayController(GolayService golayService)
        {
            _golayService = golayService;
        }

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

        [HttpGet("generator-matrix")]
        public ActionResult<int[][]> GetGeneratorMatrix()
        {
            var matrix = _golayService.GetGeneratorMatrix();
            return Ok(matrix);
        }

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
    }

    public record EncodeRequest(int Message);
    public record DecodeRequest(int Codeword);
}
