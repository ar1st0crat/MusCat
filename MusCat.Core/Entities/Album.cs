using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Album
    {
        public long ID { get; set; }
        public long PerformerID { get; set; }
        public string Name { get; set; }
        public short ReleaseYear { get; set; }
        public string TotalTime { get; set; }
        public string Info { get; set; }
        public byte? Rate { get; set; }
        public virtual Performer Performer { get; set; }
        public virtual ICollection<Song> Songs { get; set; }

        public Album()
        {
            Songs = new HashSet<Song>();
        }
    }
}
