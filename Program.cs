namespace HyperLogLog
{
    internal class Program
    {
        static void Main()
        {
            int n = 500000;
            int step = 1000;
            var rand = new Random();

            var hll = new HyperLogLog(14); // precision = 14 (~0.8% error)
            var seen = new HashSet<string>();

            Console.WriteLine("Items\tTrueCount\tHLL_Estimate\tError(%)");

            for (int i = 1; i <= n; i++)
            {
                string value = rand.Next(1, n * 2).ToString();
                seen.Add(value);
                hll.Add(value);

                if (i % step == 0)
                {
                    int trueCount = seen.Count;
                    double est = hll.Count();
                    double error = (est - trueCount) / trueCount * 100.0;
                    Console.WriteLine($"{i}\t{trueCount}\t\t{est:F0}\t\t{error:F2}");
                }
            }
        }
    }
}
