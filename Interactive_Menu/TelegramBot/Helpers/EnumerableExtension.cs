using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.Helpers
{
    internal static class EnumerableExtension
    {
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (batchSize <= 0)
                throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

            if (batchNumber < 0)
                throw new ArgumentException("Batch number must be non-negative", nameof(batchNumber));

            return source.Skip(batchNumber * batchSize).Take(batchSize);
        }
    }
}
