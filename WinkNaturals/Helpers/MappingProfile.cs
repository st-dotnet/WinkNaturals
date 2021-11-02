using Exigo.Api.Client;
using System;
using WinkNatural.Web.Common.Utils.Enum;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNaturals.Models;
using Profile = AutoMapper.Profile;

namespace WinkNaturals.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LoginRequest, CustomerSignInModel>().ReverseMap();
            CreateMap<CustomerCreateRequest, CustomerCreateModel>().ReverseMap();
            CreateMap<ContactRequest, ContactModel>().ReverseMap();
            CreateMap<CustomerUpdateRequest, CustomerUpdateModel>().ReverseMap();
            CreateMap<CustomerValidationRequest, CustomerValidationModel>().ReverseMap();
            CreateMap<EnrollmentResponse, EnrollmentModel>().ReverseMap();

            // Create customer
            CreateMap<CustomerCreateModel, CreateCustomerRequest>()
                .ForMember(dest => dest.InsertEnrollerTree, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.MobilePhone, opt => opt.MapFrom(src => src.MobilePhone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CanLogin, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.LoginName, opt => opt.MapFrom(src => src.LoginName))
                .ForMember(dest => dest.LoginPassword, opt => opt.MapFrom(src => src.LoginPassword))
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => (int)CustomerTypeEnum.RetailCustomer))
                .ForMember(dest => dest.CustomerStatus, opt => opt.MapFrom(src => (int)CustomerStatuses.Active))
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(src => GetCSTSateTime()))
                .ForMember(dest => dest.DefaultWarehouseID, opt => opt.MapFrom(src => 1))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => "usd"))
                .ForMember(dest => dest.EnrollerID, opt => opt.MapFrom(src => src.EnrollerID == 0 ? 2 : src.EnrollerID))
                .ForMember(dest => dest.LanguageID, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.MainCountry, opt => opt.MapFrom(src => "US"))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate));

            // Signin customer
            CreateMap<CustomerSignInModel, AuthenticateCustomerRequest>()
                .ForMember(dest => dest.LoginName, opt => opt.MapFrom(src => src.LoginName))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
                      
        }

        #region private methods

        // Get CST datetime
        private static DateTime GetCSTSateTime()
        {
            try
            {
                DateTime datetimeNow = DateTime.Now;
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return TimeZoneInfo.ConvertTime(datetimeNow, cstZone);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

    }
}
