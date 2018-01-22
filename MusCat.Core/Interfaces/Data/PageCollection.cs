using System;
using System.Collections.Generic;

namespace MusCat.Core.Interfaces.Data
{
    public class PageCollection<T>
    {
        public IReadOnlyCollection<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalPages => (int)(Math.Ceiling((decimal)TotalItems / ItemsPerPage));
    }
}
