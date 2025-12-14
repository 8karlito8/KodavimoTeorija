using server.Data;

namespace server.Services
{
    public class GolayService
    {
        private readonly GolayMatrices _matrices;

        public GolayService(GolayMatrices matrices)
        {
            _matrices = matrices;
        }

        public int[][] GetParityMatrix()
        {
            return _matrices.GetParityMatrix();
        }

        public int[][] GetIdentityMatrix()
        {
            return _matrices.GetIdentityMatrix();
        }

        public int[][] GetGeneratorMatrix()
        {
            var identity = _matrices.IdentityMatrix;
            var parity = _matrices.ParityMatrix;
            var generator = new int[12][];

            for (int i = 0; i < 12; i++)
            {
                generator[i] = new int[23];

                // Copy identity matrix (first 12 columns)
                for (int j = 0; j < 12; j++)
                {
                    generator[i][j] = identity[i][j];
                }

                // Copy parity matrix (last 11 columns)
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
            // For each bit in the message
            for (int i = 0; i < 12; i++)
            {
                if ((message & (1 << i)) != 0)
                {
                    // XOR the corresponding row of generator matrix
                    for (int j = 0; j < 23; j++)
                    {
                        if (generator[i][j] == 1)
                        {
                            codeword ^= (1 << j);
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
            // Simple decoding: extract first 12 bits (systematic code)
            // For now, no error correction implemented
            int message = codeword & ((1 << 12) - 1);
            return message;
        }
    }
}
