using Newtonsoft.Json;
using System.Xml.Serialization;

namespace MusCat.Application.Dto
{
    public class SongDto
    {
        public int Id;
        public byte TrackNo { get; set; }
        public string Name { get; set; }
        public string TimeLength { get; set; }
        public byte? Rate { get; set; }
        [JsonIgnore]
        [XmlIgnore]
        public AlbumDto Album { get; set; }
    }
}
