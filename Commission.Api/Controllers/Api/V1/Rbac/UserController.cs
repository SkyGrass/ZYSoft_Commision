﻿using AutoMapper;
using Commission.Api.Entities;
using Commission.Api.Entities.Enums;
using Commission.Api.Extensions;
using Commission.Api.Extensions.AuthContext;
using Commission.Api.Extensions.CustomException;
using Commission.Api.Extensions.DataAccess;
using Commission.Api.Models.Response;
using Commission.Api.RequestPayload.Rbac.User;
using Commission.Api.Utils;
using Commission.Api.ViewModels.Rbac.DncUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;

namespace Commission.Api.Controllers.Api.V1.Rbac
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v1/rbac/[controller]/[action]")]
    [ApiController]
    //[CustomAuthorize]
    public class UserController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public UserController(DncZeusDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult List(UserRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.DncUser.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    query = query.Where(x => x.LoginName.Contains(payload.Kw.Trim()) || x.DisplayName.Contains(payload.Kw.Trim()));
                }
                if (payload.IsDeleted > CommonEnum.IsDeleted.All)
                {
                    query = query.Where(x => x.IsDeleted == payload.IsDeleted);
                }
                if (payload.Status > UserStatus.All)
                {
                    query = query.Where(x => x.Status == payload.Status);
                }

                if (payload.FirstSort != null)
                {
                    query = query.OrderBy(payload.FirstSort.Field, payload.FirstSort.Direct == "DESC");
                }
                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<DncUser, UserJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data, totalCount);
                return Ok(response);
            }
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="model">用户视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Create(UserCreateViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (model.LoginName.Trim().Length <= 0)
            {
                response.SetFailed("请输入登录名称");
                return Ok(response);
            }
            using (_dbContext)
            {
                if (_dbContext.DncUser.Count(x => x.LoginName == model.LoginName) > 0)
                {
                    response.SetFailed("登录名已存在");
                    return Ok(response);
                }
                var entity = _mapper.Map<UserCreateViewModel, DncUser>(model);
                entity.CreatedOn = DateTime.Now;
                entity.FGuid = Guid.NewGuid();
                entity.Status = model.Status;
                entity.Password = DEShelper.DESEncrypt((model.Password ?? "").Trim());
                _dbContext.DncUser.Add(entity);
                _dbContext.SaveChanges();

                response.SetSuccess();
                return Ok(response);
            }
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="guid">用户GUID</param>
        /// <returns></returns>
        [HttpGet("{guid}")]
        [ProducesResponseType(200)]
        public IActionResult Edit(Guid guid)
        {
            using (_dbContext)
            {
                var entity = _dbContext.DncUser.FirstOrDefault(x => x.FGuid == guid);
                entity.Password = DEShelper.DESDecrypt((entity.Password ?? "").Trim());
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(_mapper.Map<DncUser, UserEditViewModel>(entity));
                return Ok(response);
            }
        }

        /// <summary>
        /// 保存编辑后的用户信息
        /// </summary>
        /// <param name="model">用户视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Edit(UserEditViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (ConfigurationManager.AppSettings.IsTrialVersion)
            {
                response.SetIsTrial();
                return Ok(response);
            }
            using (_dbContext)
            {
                var entity = _dbContext.DncUser.FirstOrDefault(x => x.FGuid == model.FGuid);
                if (entity == null)
                {
                    response.SetFailed("用户不存在");
                    return Ok(response);
                }
                entity.DisplayName = model.DisplayName;
                entity.IsDeleted = model.IsDeleted;
                entity.IsLocked = model.IsLocked;
                entity.ModifiedByUserGuid = AuthContextService.CurrentUser.Guid;
                entity.ModifiedByUserName = AuthContextService.CurrentUser.DisplayName;
                entity.ModifiedOn = DateTime.Now;
                entity.Password = DEShelper.DESEncrypt((model.Password ?? "").Trim());
                entity.Status = model.Status;
                entity.UserType = model.UserType;
                entity.Description = model.Description;
                entity.UserDept = model.UserDept;
                entity.EmpCode = model.EmpCode;
                _dbContext.SaveChanges();
                response = ResponseModelFactory.CreateInstance;
                return Ok(response);
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="ids">用户GUID,多个以逗号分隔</param>
        /// <returns></returns>
        [HttpGet("{ids}")]
        [ProducesResponseType(200)]
        public IActionResult Delete(string ids)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (ConfigurationManager.AppSettings.IsTrialVersion)
            {
                response.SetIsTrial();
                return Ok(response);
            }
            response = UpdateIsDelete(CommonEnum.IsDeleted.Yes, ids);
            return Ok(response);
        }

        /// <summary>
        /// 恢复用户
        /// </summary>
        /// <param name="ids">用户GUID,多个以逗号分隔</param>
        /// <returns></returns>
        [HttpGet("{ids}")]
        [ProducesResponseType(200)]
        public IActionResult Recover(string ids)
        {
            var response = UpdateIsDelete(CommonEnum.IsDeleted.No, ids);
            return Ok(response);
        }

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ids">用户ID,多个以逗号分隔</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult Batch(string command, string ids)
        {
            var response = ResponseModelFactory.CreateInstance;
            switch (command)
            {
                case "delete":
                    if (ConfigurationManager.AppSettings.IsTrialVersion)
                    {
                        response.SetIsTrial();
                        return Ok(response);
                    }
                    response = UpdateIsDelete(CommonEnum.IsDeleted.Yes, ids);
                    break;
                case "recover":
                    response = UpdateIsDelete(CommonEnum.IsDeleted.No, ids);
                    break;
                case "forbidden":
                    if (ConfigurationManager.AppSettings.IsTrialVersion)
                    {
                        response.SetIsTrial();
                        return Ok(response);
                    }
                    response = UpdateStatus(UserStatus.Forbidden, ids);
                    break;
                case "normal":
                    response = UpdateStatus(UserStatus.Normal, ids);
                    break;
                default:
                    break;
            }
            return Ok(response);
        }

        [HttpPost]
        public IActionResult SelectAll()
        {
            using (_dbContext)
            {
                var query = _dbContext.DncUser.AsQueryable();
                var list = query.ToList();
                var data = list.Select(_mapper.Map<DncUser, UserSelectJsonModel>);
                return Ok(new
                {
                    state = "success",
                    data,
                    message = ""
                });
            }
        }
        #region 用户-角色
        /// <summary>
        /// 保存用户-角色的关系映射数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("/api/v1/rbac/user/save_roles")]
        public IActionResult SaveRoles(SaveUserRolesViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            var roles = model.AssignedRoles.Select(x => new DncUserRoleMapping
            {
                UserGuid = model.UserGuid,
                CreatedOn = DateTime.Now,
                RoleCode = x.Trim()
            }).ToList();
            _dbContext.Database.ExecuteSqlCommand("DELETE FROM DncUserRoleMapping WHERE UserGuid={0}", model.UserGuid);
            var success = true;
            if (roles.Count > 0)
            {
                _dbContext.DncUserRoleMapping.AddRange(roles);
                success = _dbContext.SaveChanges() > 0;
            }

            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                response.SetFailed("保存用户角色数据失败");
            }
            return Ok(response);
        }
        #endregion

        #region 用户-业务员
        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="guid">用户GUID</param>
        /// <returns></returns>
        [HttpGet("{guid}")]
        [ProducesResponseType(200)]
        public IActionResult Bind(Guid guid)
        {
            using (_dbContext)
            {
                var entity = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.UserId == guid);
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(_mapper.Map<UserSalesmanMapping, UserSalesmanViewModel>(entity));
                return Ok(response);
            }
        }

        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Bind(UserSalesmanCreateViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            using (_dbContext)
            {

                var entity = _dbContext.DncUser.FirstOrDefault(x => x.FGuid == model.UserId);
                if (entity == null)
                {
                    response.SetFailed("用户不存在");
                    return Ok(response);
                }
                if (model.SalesmanId > 0)
                {
                    var entity1 = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.SalesmanId == model.SalesmanId);
                    if (model.Id < 0)
                    {
                        if (entity1 != null)
                        {
                            response.SetFailed("业务员已存在绑定关系,请先进行解绑!");
                            return Ok(response);
                        }
                        else
                        {
                            entity1 = _mapper.Map<UserSalesmanCreateViewModel, UserSalesmanMapping>(model);
                        }
                    }
                    else
                    {
                        entity1 = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.UserId == model.UserId);
                        if (!entity1.SalesmanId.Equals(model.SalesmanId))
                        {
                            _dbContext.Entry(entity1).State = EntityState.Deleted;
                            _dbContext.SaveChanges();
                        }
                    }

                    entity1.Id = new IDHelper(_dbContext).GenId(typeof(UserSalesmanMapping).GetAttributeValue((TableAttribute dna) => dna.Name));
                    entity1.UserId = model.UserId;
                    entity1.SalesmanId = model.SalesmanId;
                    entity1.CreatedOn = DateTime.Now;
                    entity1.CreatedByUserGuid = AuthContextService.CurrentUser.Guid;
                    entity1.CreatedByUserName = AuthContextService.CurrentUser.DisplayName;
                    _dbContext.UserSalesmanMapping.Add(entity1);
                    _dbContext.SaveChanges();
                }
                else
                {
                    _dbContext.Database.ExecuteSqlCommand(string.Format(@"delete from UserSalesmanMapping where Id = '{0}'", model.Id));
                }
                response = ResponseModelFactory.CreateInstance;
                return Ok(response);

            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="isDeleted"></param>
        /// <param name="ids">用户ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateIsDelete(CommonEnum.IsDeleted isDeleted, string ids)
        {
            using (_dbContext)
            {
                //var idList = ids.Split(",").ToList();
                ////idList.ForEach(x => {
                ////  _dbContext.Database.ExecuteSqlCommand($"UPDATE DncUser SET IsDeleted=1 WHERE Id = {x}");
                ////});
                //_dbContext.Database.ExecuteSqlCommand($"UPDATE DncUser SET IsDeleted={(int)isDeleted} WHERE Id IN ({ids})");
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE DncUser SET IsDeleted=@IsDeleted WHERE FGuid IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@IsDeleted", (int)isDeleted));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="status">用户状态</param>
        /// <param name="ids">用户ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateStatus(UserStatus status, string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE DncUser SET Status=@Status WHERE FGuid IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@Status", (int)status));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }
        #endregion
    }
}