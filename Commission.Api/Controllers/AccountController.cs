
using Commission.Api.Entities;
using Commission.Api.Extensions;
using Commission.Api.Extensions.AuthContext;
using Commission.Api.ViewModels.Rbac.DncMenu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public AccountController(DncZeusDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Profile()
        {
            var response = ResponseModelFactory.CreateInstance;
            using (_dbContext)
            {
                var guid = AuthContextService.CurrentUser.Guid;
                var user = _dbContext.DncUser.FirstOrDefaultAsync(x => x.FGuid == guid).Result;

                var menus = _dbContext.DncMenu.Where(x => x.IsDeleted == IsDeleted.No && x.Status == Status.Normal).ToList();

                var sqlPermission = @"SELECT P.Code AS PermissionCode,P.ActionCode AS PermissionActionCode,P.Name AS PermissionName,P.Type AS PermissionType,M.Name AS MenuName,M.Guid AS MenuGuid,M.Alias AS MenuAlias,M.IsDefaultRouter FROM DncRolePermissionMapping AS RPM 
LEFT JOIN DncPermission AS P ON P.Code = RPM.PermissionCode
INNER JOIN DncMenu AS M ON M.Guid = P.MenuGuid
WHERE P.IsDeleted=0 AND P.Status=1 AND EXISTS (SELECT 1 FROM DncUserRoleMapping AS URM WHERE URM.UserGuid={0} AND URM.RoleCode=RPM.RoleCode)";
                if (user.UserType == UserType.SuperAdministrator)
                {
                    sqlPermission = @"SELECT P.Code AS PermissionCode,P.ActionCode AS PermissionActionCode,P.Name AS PermissionName,P.Type AS PermissionType,M.Name AS MenuName,M.Guid AS MenuGuid,M.Alias AS MenuAlias,M.IsDefaultRouter FROM DncPermission AS P 
INNER JOIN DncMenu AS M ON M.Guid = P.MenuGuid
WHERE P.IsDeleted=0 AND P.Status=1";
                }
                var permissions = _dbContext.DncPermissionWithMenu.FromSql(sqlPermission, user.FGuid).ToList();

                var pagePermissions = permissions.GroupBy(x => x.MenuAlias).ToDictionary(g => g.Key, g => g.Select(x => x.PermissionActionCode).Distinct());
                response.SetData(new
                {
                    access = new string[] { },
                    avator = user.Avatar,
                    user_guid = user.FGuid,
                    user_name = user.LoginName,
                    user_realname = user.DisplayName,
                    user_dept = user.UserDept,
                    user_type = user.UserType,
                    user_pwd = user.Password,
                    permissions = pagePermissions,
                    emp_code = user.EmpCode,
                });
            }

            return Ok(response);
        }

        private List<string> FindParentMenuAlias(List<DncMenu> menus, Guid? parentGuid)
        {
            var pages = new List<string>();
            var parent = menus.FirstOrDefault(x => x.Guid == parentGuid);
            if (parent != null)
            {
                if (!pages.Contains(parent.Alias))
                {
                    pages.Add(parent.Alias);
                }
                else
                {
                    return pages;
                }
                if (parent.ParentGuid != Guid.Empty)
                {
                    pages.AddRange(FindParentMenuAlias(menus, parent.ParentGuid));
                }
            }

            return pages.Distinct().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Menu()
        {
            var strSql = @"SELECT DISTINCT M.* FROM DncRolePermissionMapping AS RPM 
LEFT JOIN DncPermission AS P ON P.Code = RPM.PermissionCode
INNER JOIN DncMenu AS M ON M.Guid = P.MenuGuid
WHERE P.IsDeleted=0 AND P.Status=1 AND P.Type=0 AND M.IsDeleted=0 AND M.Status=1 
AND EXISTS (SELECT 1 FROM DncUserRoleMapping AS URM WHERE URM.UserGuid={0} AND URM.RoleCode=RPM.RoleCode)";

            if (AuthContextService.CurrentUser.UserType == UserType.SuperAdministrator)
            {
                //如果是超级管理员
                strSql = @"SELECT * FROM DncMenu WHERE IsDeleted=0 AND Status=1";
            }
            var menus = _dbContext.DncMenu.FromSql(strSql, AuthContextService.CurrentUser.Guid).ToList();
            var rootMenus = _dbContext.DncMenu.Where(x => x.IsDeleted == IsDeleted.No && x.Status == Status.Normal && x.ParentGuid == Guid.Empty).ToList();
            foreach (var root in rootMenus)
            {
                if (menus.Exists(x => x.Guid == root.Guid))
                {
                    continue;
                }
                if (menus.Exists(x => x.ParentGuid == root.Guid)) menus.Add(root);
            }
            menus = menus.OrderBy(x => x.Sort).ThenBy(x => x.CreatedOn).ToList();
            var menu = MenuItemHelper.LoadMenuTree(menus, "0");
            return Ok(menu);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class MenuItemHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="menus"></param>
        /// <param name="selectedGuid"></param>
        /// <returns></returns>
        public static List<MenuItem> BuildTree(this List<MenuItem> menus, string selectedGuid = null)
        {
            var lookup = menus.ToLookup(x => x.ParentId);

            List<MenuItem> Build(string pid)
            {
                return lookup[pid]
                    .Select(x => new MenuItem()
                    {
                        Guid = x.Guid,
                        ParentId = x.ParentId,
                        Children = Build(x.Guid),
                        Component = x.Component ?? "Main",
                        Name = x.Name,
                        Path = x.Path,
                        Meta = new MenuMeta
                        {
                            BeforeCloseFun = x.Meta.BeforeCloseFun,
                            HideInMenu = x.Meta.HideInMenu,
                            Icon = x.Meta.Icon,
                            NotCache = x.Meta.NotCache,
                            Title = x.Meta.Title,
                            Permission = x.Meta.Permission
                        }
                    }).ToList();
            }

            var result = Build(selectedGuid);
            return result;
        }

        public static List<MenuItem> LoadMenuTree(List<DncMenu> menus, string selectedGuid = null)
        {
            var temp = menus.Select(x => new MenuItem
            {
                Guid = x.Guid.ToString(),
                ParentId = x.ParentGuid != null && ((Guid)x.ParentGuid) == Guid.Empty ? "0" : x.ParentGuid?.ToString(),
                Name = x.Alias,
                Path = $"/{x.Url}",
              
                Component = x.Component,
                Meta = new MenuMeta
                {
                    BeforeCloseFun = x.BeforeCloseFun ?? "",
                    HideInMenu = x.HideInMenu == YesOrNo.Yes,
                    Icon = x.Icon,
                    NotCache = x.NotCache == YesOrNo.Yes,
                    Title = x.Name
                }
            }).ToList();
            var tree = temp.BuildTree(selectedGuid);
            return tree;
        }
    }
}