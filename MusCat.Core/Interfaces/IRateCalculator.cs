using System.Collections.Generic;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces
{
    public interface IRateCalculator
    {
        byte? Calculate(IEnumerable<Album> albums);
    }
}