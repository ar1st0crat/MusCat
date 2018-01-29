using System.Collections.Generic;

namespace MusCat.Core.Interfaces
{
    public interface IRateCalculator
    {
        byte? Calculate(IEnumerable<byte?> rates);
    }
}