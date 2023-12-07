using AutoMapper;
using Autofac;

namespace SampleEFCoreApi;

public static class AutoMapperConfig
{
	public static void DependencyInjection(ContainerBuilder builder)
	{
		var mapConfig = new MapperConfiguration(config =>
		{
			config.CreateMap<Models.DtoPersonCreate, Database.Person>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.Partner, opt => opt.Ignore())
				.ForMember(dest => dest.Favorite, opt => opt.Ignore())
				.ForMember(dest => dest.Children, opt => opt.Ignore());

			config.CreateMap<Models.DtoPersonModify, Database.Person>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.Partner, opt => opt.Ignore())
				.ForMember(dest => dest.Favorite, opt => opt.Ignore())
				.ForMember(dest => dest.Children, opt => opt.Ignore());

			config.CreateMap<Models.DtoChildCreate, Database.Child>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.Partner, opt => opt.Ignore())
				.ForMember(dest => dest.Favorite, opt => opt.Ignore())
				.ForMember(dest => dest.Children, opt => opt.Ignore());

			config.CreateMap<Models.DtoChildModify, Database.Child>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.Partner, opt => opt.Ignore())
				.ForMember(dest => dest.Favorite, opt => opt.Ignore())
				.ForMember(dest => dest.Children, opt => opt.Ignore());

			config.CreateMap<Database.Person, Models.DtoPerson>()
				.ForMember(dest => dest.Partner, opt => opt.ExplicitExpansion())
				.ForMember(dest => dest.Favorite, opt => opt.ExplicitExpansion())
				.ForMember(dest => dest.Children, opt => opt.ExplicitExpansion());

			config.CreateMap<Models.DtoChild, Database.Child>()
				.ForMember(dest => dest.Partner, opt => opt.Ignore())
				.ForMember(dest => dest.Favorite, opt => opt.Ignore())
				.ForMember(dest => dest.Children, opt => opt.Ignore());

			config.CreateMap<Database.Child, Models.DtoChild>()
				.ForMember(dest => dest.Partner, opt => opt.ExplicitExpansion())
				.ForMember(dest => dest.Favorite, opt => opt.ExplicitExpansion())
				.ForMember(dest => dest.Children, opt => opt.ExplicitExpansion());

			config.CreateMap<Models.DtoGrandChild, Database.GrandChild>();

			config.CreateMap<Database.GrandChild, Models.DtoGrandChild>();

		});

		mapConfig.AssertConfigurationIsValid();

		builder.Register(x => new Mapper(mapConfig)).As<IMapper>();
	}
}
