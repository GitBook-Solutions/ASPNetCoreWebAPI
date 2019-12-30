using ASPNetCoreWebAPI.Entities;
using ASPNetCoreWebAPI.Models.Users;
using AutoMapper;

namespace ASPNetCoreWebAPI.Helpers {
    public class AutoMapperProfile : Profile {
        public AutoMapperProfile () {
            CreateMap<User, UserModel> ();
            CreateMap<RegisterModel, User> ();
            CreateMap<UpdateModel, User> ();
        }
    }
}