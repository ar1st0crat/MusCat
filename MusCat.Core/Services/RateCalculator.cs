﻿using System;
using System.Collections.Generic;
using System.Linq;
using MusCat.Core.Interfaces;

namespace MusCat.Core.Services
{
    public class RateCalculator : IRateCalculator
    {
        /// <summary>
        /// The total rate of performer's album collection is calculated 
        /// based on the following statistics of album rates:
        /// 
        ///     if the number of albums is more than 2, 
        ///     then the worst rate and the best rate are discarded
        ///     and the total rate is an average of remaining rates
        ///     
        ///     otherwise - the total rate is simply an average of album rates
        /// </summary>
        public byte? Calculate(IEnumerable<byte?> rates)
        {
            var ratedCount = rates.Count(r => r.HasValue);

            if (ratedCount <= 0)
            {
                return null;
            }

            var sumRate = rates.Sum(r => r ?? 0);

            if (ratedCount > 2)
            {
                var minRate = rates.Min().Value;
                var maxRate = rates.Max().Value;
                sumRate -= (minRate + maxRate);
                ratedCount -= 2;
            }

            return (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);
        }
    }
}
