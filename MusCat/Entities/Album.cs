using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusCat.Entities
{
    public class Album
    {
        public Album()
        {
            Songs = new HashSet<Song>();
        }

        public long PerformerID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; }

        public short ReleaseYear { get; set; }

        [StringLength(6)]
        public string TotalTime { get; set; }

        [Column(TypeName = "text")]
        public string Info { get; set; }

        public byte? Rate { get; set; }

        public virtual Performer Performer { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
    }
}
