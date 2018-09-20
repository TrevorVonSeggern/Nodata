using AutoMapper;

namespace NoData.Tests.SharedExampleClasses.Automapper
{
    public static class AutoMapperConfig
    {
        public static IMapper CreateConfig()
        {
            var mapConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Database.Entity.Person, Database.Models.DtoPerson>()
                    // .ForMember(dest => dest.Partner, opt => { opt.ExplicitExpansion(); opt.MapFrom(x => x.Partner); }).MaxDepth(4).PreserveReferences()
                    // .ForMember(dest => dest.Partner, opt => opt.MapFrom(x => x.Partner)).MaxDepth(4).PreserveReferences()
                    .ForMember(dest => dest.Partner, opt => opt.ExplicitExpansion()).MaxDepth(4).PreserveReferences()
                    .ForMember(dest => dest.Favorite, opt => opt.ExplicitExpansion())
                    .ForMember(dest => dest.Children, opt => opt.ExplicitExpansion())
                    ;


                config.CreateMap<Database.Entity.Child, Database.Models.DtoChild>()
                    .ForMember(dest => dest.Partner, opt => opt.ExplicitExpansion())
                    .ForMember(dest => dest.Favorite, opt => opt.ExplicitExpansion())
                    .ForMember(dest => dest.Children, opt => opt.ExplicitExpansion());

                config.CreateMap<Database.Entity.GrandChild, Database.Models.DtoGrandChild>();
            });

            mapConfig.AssertConfigurationIsValid();

            return new Mapper(mapConfig);
        }
    }
}