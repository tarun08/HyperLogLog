using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace HyperLogLog
{
    public class HyperLogLog
    {
        private readonly int p;         // precision (log2(m))
        private readonly int m;         // number of registers
        private readonly byte[] registers;

        public HyperLogLog(int precision = 14)
        {
            if (precision < 4 || precision > 18)
                throw new ArgumentException("Precision must be between 4 and 18.");

            p = precision;
            m = 1 << p; // m = 2^p
            registers = new byte[m];
        }

        private static ulong Hash64(string value)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
            // take first 8 bytes for 64-bit
            return BitConverter.ToUInt64(bytes, 0);
        }

        public void Add(string value)
        {
            ulong x = Hash64(value);
            int j = (int)(x & (ulong)(m - 1)); // lowest p bits = index
            ulong w = x >> p;                  // remaining bits
            int rho = LeadingZeros(w, 64 - p) + 1;
            if (rho > registers[j])
                registers[j] = (byte)rho;
        }

        /// <summary>
        /// Estimate distinct count.
        /// </summary>
        public double Count()
        {
            double Z = 0.0;
            foreach (var r in registers)
                Z += Math.Pow(2.0, -r);

            double alpha;
            if (m == 16) alpha = 0.673;
            else if (m == 32) alpha = 0.697;
            else if (m == 64) alpha = 0.709;
            else alpha = 0.7213 / (1 + 1.079 / m);

            double E = alpha * m * m / Z;

            // Small-range correction (linear counting)
            int V = registers.Count(r => r == 0);
            if (E <= (5.0 / 2.0) * m && V > 0)
            {
                return m * Math.Log((double)m / V);
            }

            return E;
        }

        private static int LeadingZeros(ulong x, int bits)
        {
            if (x == 0) return bits;
            return bits - (int)BitOperations.Log2(x) - 1;
        }
    }

}
