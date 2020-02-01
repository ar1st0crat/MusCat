using AutoMapper;
using AutoMapper.EquivalencyExpression;
using MusCat.Application.Dto;
using MusCat.Core.Entities;
using System;

namespace MusCat.Application
{
    public static class Auto
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.AddProfile<InfrastructureProfile>();
            });

            return config.CreateMapper();
        });

        public static IMapper Mapper => Lazy.Value;
    }

    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<Performer, PerformerDto>()
               .EqualityComparison((src, dest) => src.Id == dest.Id)
               .ReverseMap();

            CreateMap<Album, AlbumDto>()
               .EqualityComparison((src, dest) => src.Id == dest.Id)
               .ReverseMap();

            CreateMap<Song, SongDto>()
               .EqualityComparison((src, dest) => src.Id == dest.Id)
               .ReverseMap();

            CreateMap<Country, CountryDto>()
               .EqualityComparison((src, dest) => src.Id == dest.Id)
               .ReverseMap();
        }
    }
}
