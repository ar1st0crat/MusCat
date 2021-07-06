using AutoMapper;
using AutoMapper.EquivalencyExpression;
using MusCat.Application.Dto;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Tracklist;
using MusCat.ViewModels;
using MusCat.ViewModels.Entities;

namespace MusCat
{
    public static class Mappings
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddCollectionMappers();

                cfg.AddProfile<Application.InfrastructureProfile>();

                cfg.CreateMap<Performer, PerformerViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .ForMember(m => m.Albums, opt => opt.Ignore())
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<PerformerDto, PerformerViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<Album, AlbumViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<AlbumDto, AlbumViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<Song, SongViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .ReverseMap();

                cfg.CreateMap<Song, SongDto>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .ReverseMap();

                cfg.CreateMap<Song, RadioSongViewModel>()
                   .AfterMap((src, dest) => dest.LocateAlbumImagePath(src.Album));

                cfg.CreateMap<SongViewModel, Track>()
                   .EqualityComparison((src, dest) => src.TrackNo == dest.No)
                   .ForMember(dest => dest.No, opt => opt.MapFrom(src => src.TrackNo))
                   .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                   .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.TimeLength));

                cfg.CreateMap<Track, SongViewModel>()
                   .EqualityComparison((src, dest) => src.No == dest.TrackNo)
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => -1))
                   .ForMember(dest => dest.TrackNo, opt => opt.MapFrom(src => src.No))
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                   .ForMember(dest => dest.TimeLength, opt => opt.MapFrom(src => src.Duration));

                cfg.CreateMap<Country, CountryViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id);

                cfg.CreateMap<CountryDto, CountryViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id);
            });
        }
    }
}
