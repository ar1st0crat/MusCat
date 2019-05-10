using Newtonsoft.Json;
using System.Xml.Serialization;

namespace MusCat.Application.Dto
{
    public class AlbumDto
    {
        public int Id { get; set; }
        public int PerformerId { get; set; }
        public string Name { get; set; }
        public short ReleaseYear { get; set; }
        public string TotalTime { get; set; }
        public string Info { get; set; }
        public byte? Rate { get; set; }
        [JsonIgnore]
        [XmlIgnore]
        public PerformerDto Performer { get; set; }
    }
}
