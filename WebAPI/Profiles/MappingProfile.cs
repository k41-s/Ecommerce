using AutoMapper;
using Ecommerce.core.DTOs;
using WebAPI.Models;

namespace WebAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Country, CountryDTO>().ReverseMap();

            // Category mappings
            CreateMap<Category, CategoryDTO>().ReverseMap();

            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.CountryNames, opt => opt.MapFrom(src => src.Countries.Select(a => a.Name)));

            CreateMap<ProductDTO, Product>()
                .ForMember(dest => dest.Countries, opt => opt.Ignore());

            CreateMap<User, AuthenticatedUserDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname));

            CreateMap<AuthenticatedUserDTO, User>();

            CreateMap<User, UserDTO>().ReverseMap();

            // If you want to map RegisterUserDTO to User for registration logic
            CreateMap<RegisterUserDTO, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Hash password manually in service/controller
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User")); // Assign default role

            // CustomerOrder mappings
            CreateMap<CustomerOrder, OrderDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name + " " + src.User.Surname));

            CreateMap<OrderDTO, CustomerOrder>()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UserWithOrdersDTO, User>().ReverseMap();
        }
    }
}
