using AutoMapper;
using Ecommerce.core.DTOs;
using WebApp.ViewModels;

namespace WebApp.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Country
            CreateMap<CountryVM, CountryDTO>().ReverseMap();

            // CustomerOrder
            CreateMap<OrderVM, OrderDTO>().ReverseMap();

            // Login (usually one-way, no reverse mapping needed)
            CreateMap<LoginViewModel, LoginDTO>();
            CreateMap<UserDTO, ProfileViewModel>().ReverseMap();

            // Product
            CreateMap<ProductVM, ProductDTO>().ReverseMap();

            // MyOrder
            CreateMap<MyOrderVM, OrderDTO>().ReverseMap();

            // Register (usually one-way, for sending data to API)
            CreateMap<RegisterViewModel, RegisterUserDTO>();

            // Category
            CreateMap<CategoryVM, CategoryDTO>().ReverseMap();

            CreateMap<UserWithOrdersDTO, UserWithOrdersVM>().ReverseMap();

            CreateMap<OrderDTO, OrderInfoViewModel>().ReverseMap();

            CreateMap<CountryDTO, CountryVM>().ReverseMap();
        }
    }
}
