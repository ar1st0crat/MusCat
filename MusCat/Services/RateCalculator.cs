using System;
using System.Collections.Generic;
using System.Linq;
using MusCat.Entities;

namespace MusCat.Services
{
    class RateCalculator
    {
        /// <summary>
        /// The total rate of performer's album collection is calculated based on the following statistics of album rates:
        /// 
        ///     if the number of albums is more than 2, then the worst rate and the best rate are discarded
        ///     and the total rate is an average of remaining rates
        ///     
        ///     otherwise - the total rate is simply an average of album rates
        ///     
        /// </summary>
        public byte? Calculate(IEnumerable<Album> albums)
        {
            var ratedCount = albums.Count(t => t.Rate.HasValue);

            if (ratedCount <= 0)
            {
                return null;
            }

            var sumRate = albums.Sum(t => t.Rate ?? 0);

            if (ratedCount > 2)
            {
                var minRate = albums.Min(r => r.Rate).Value;
                var maxRate = albums.Max(r => r.Rate).Value;
                sumRate -= (minRate + maxRate);
                ratedCount -= 2;
            }

            return (byte)Math.Round((double)sumRate / ratedCount,
                                              MidpointRounding.AwayFromZero);
        }
    }
}
