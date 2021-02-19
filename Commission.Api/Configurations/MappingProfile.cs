

using AutoMapper;
using Commission.Api.Entities;
using Commission.Api.ViewModels.Base.CommisionWay; 
using Commission.Api.ViewModels.Base.Custom;
using Commission.Api.ViewModels.Base.Comparison;
using Commission.Api.ViewModels.Base.Software;
using Commission.Api.ViewModels.Base.Salesman;
using Commission.Api.ViewModels.Rbac.DncIcon;
using Commission.Api.ViewModels.Rbac.DncMenu;
using Commission.Api.ViewModels.Rbac.DncPermission;
using Commission.Api.ViewModels.Rbac.DncRole;
using Commission.Api.ViewModels.Rbac.DncUser;

namespace Commission.Api.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public MappingProfile()
        {
            #region DncUser
            CreateMap<DncUser, UserJsonModel>();
            CreateMap<UserCreateViewModel, DncUser>();
            CreateMap<UserEditViewModel, DncUser>();
            #endregion

            #region DncRole
            CreateMap<DncRole, RoleJsonModel>();
            CreateMap<RoleCreateViewModel, DncRole>();
            #endregion

            #region DncMenu
            CreateMap<DncMenu, MenuJsonModel>();
            CreateMap<MenuCreateViewModel, DncMenu>();
            CreateMap<MenuEditViewModel, DncMenu>();
            CreateMap<DncMenu, MenuEditViewModel>();
            #endregion

            #region DncIcon
            CreateMap<DncIcon, IconCreateViewModel>();
            CreateMap<IconCreateViewModel, DncIcon>();
            #endregion

            #region DncPermission
            CreateMap<DncPermission, PermissionJsonModel>()
                .ForMember(d => d.MenuName, s => s.MapFrom(x => x.Menu.Name))
                .ForMember(d => d.PermissionTypeText, s => s.MapFrom(x => x.Type.ToString()));
            CreateMap<PermissionCreateViewModel, DncPermission>();
            CreateMap<PermissionEditViewModel, DncPermission>();
            CreateMap<DncPermission, PermissionEditViewModel>();
            #endregion

            #region BaseCustom
            CreateMap<BaseCustom, CustomCreateViewModel>();
            CreateMap<CustomCreateViewModel, BaseCustom>();
            #endregion

            #region BaseSalesman
            CreateMap<BaseSalesman, SalesmanCreateViewModel>();
            CreateMap<SalesmanCreateViewModel, BaseSalesman>();
            #endregion

            #region BaseSoftware
            CreateMap<BaseSoftware, SoftwareCreateViewModel>();
            CreateMap<SoftwareCreateViewModel, BaseSoftware>();
            #endregion 

            #region BaseCommisionWay
            CreateMap<BaseCommisionWay, CommisionWayCreateViewModel>();
            CreateMap<CommisionWayCreateViewModel, BaseCommisionWay>();
            #endregion

            #region BaseComparison
            CreateMap<BaseComparison, ComparisonCreateViewModel>();
            CreateMap<ComparisonCreateViewModel, BaseComparison>();
            #endregion

            #region BaseComparison
            CreateMap<UserSalesmanMapping, UserSalesmanCreateViewModel>();
            CreateMap<UserSalesmanCreateViewModel, UserSalesmanMapping>();
            #endregion

        }
    }
}
