using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Album
    {
        public int Id { get; set; }
        public int PerformerId { get; set; }
        public string Name { get; set; }
        public short ReleaseYear { get; set; }
        public string TotalTime { get; set; }
        public string Info { get; set; }
        public byte? Rate { get; set; }
        public Performer Performer { get; set; }
        public ICollection<Song> Songs { get; set; }

        public Album()
        {
            Songs = new HashSet<Song>();
        }
    }
}
