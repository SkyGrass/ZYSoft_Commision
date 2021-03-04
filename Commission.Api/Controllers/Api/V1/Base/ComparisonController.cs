using AutoMapper;
using Commission.Api.Entities;
using Commission.Api.Entities.Enums;
using Commission.Api.Extensions;
using Commission.Api.Extensions.AuthContext;
using Commission.Api.Extensions.CustomException;
using Commission.Api.Models.Response;
using Commission.Api.RequestPayload.Base.Comparison;
using Commission.Api.Utils;
using Commission.Api.ViewModels.Base.Comparison;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Commission.Api.Controllers.Api.V1.Base
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v1/base/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class ComparisonController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public ComparisonController(DncZeusDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult List(ComparisonRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.BaseComparison.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    List<int> ids = _dbContext.BaseSoftware.AsQueryable().
                        Where(f => f.Name.Contains(payload.Kw.Trim())
                        || f.Code.Contains(payload.Kw.Trim())).ToList().Select(s => s.Id).ToList();

                    query = query.Where(x => ids.Contains(x.SoftwareId));
                }
                if (payload.IsDeleted > CommonEnum.IsDeleted.All)
                {
                    query = query.Where(x => x.IsDeleted == payload.IsDeleted);
                }
                if (payload.Status > CommonEnum.Status.All)
                {
                    query = query.Where(x => x.Status == payload.Status);
                }
                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<BaseComparison, ComparisonJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data, totalCount);
                return Ok(response);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Select(ComparisonSelectRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.BaseComparison.AsQueryable();
                var list = query.ToList();
                var data = list.Select(_mapper.Map<BaseComparison, ComparisonSelectJsonModel>);
                return Ok(new
                {
                    state = "success",
                    data,
                    message = ""
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SelectAll()
        {
            using (_dbContext)
            {
                var query = _dbContext.BaseComparison.AsQueryable();
                var list = query.ToList();
                var data = list.Select(_mapper.Map<BaseComparison, ComparisonSelectJsonModel>);
                return Ok(new
                {
                    state = "success",
                    data,
                    message = ""
                });
            }
        }


        /// <summary>
        /// 创建提成比例对照
        /// </summary>
        /// <param name="model">提成比例对照视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Create(ComparisonCreateViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            using (_dbContext)
            {
                var entity = _mapper.Map<ComparisonCreateViewModel, BaseComparison>(model);
                entity.Id = new IDHelper(_dbContext).GenId(typeof(BaseComparison).GetAttributeValue((TableAttribute dna) => dna.Name));
                entity.CreatedOn = DateTime.Now;
                entity.CreatedByUserGuid = AuthContextService.CurrentUser.Guid;
                entity.CreatedByUserName = AuthContextService.CurrentUser.DisplayName;
                _dbContext.BaseComparison.Add(entity);
                _dbContext.SaveChanges();

                response.SetSuccess();
                return Ok(response);
            }
        }

        /// <summary>
        /// 编辑提成比例对照
        /// </summary>
        /// <param name="id">提成比例对照ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        public IActionResult Edit(int id)
        {
            using (_dbContext)
            {
                var entity = _dbContext.BaseComparison.FirstOrDefault(x => x.Id == id);
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(_mapper.Map<BaseComparison, ComparisonCreateViewModel>(entity));
                return Ok(response);
            }
        }

        /// <summary>
        /// 保存编辑后的提成比例对照信息
        /// </summary>
        /// <param name="model">提成比例对照视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Edit(ComparisonCreateViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            using (_dbContext)
            {
                var entity = _dbContext.BaseComparison.FirstOrDefault(x => x.Id == model.Id);
                entity.SoftwareId = model.SoftwareId;
                entity.CommisionWayId = model.CommisionWayId;
                entity.Proportion = model.Proportion;
                entity.IsDeleted = model.IsDeleted;
                entity.ModifiedByUserGuid = AuthContextService.CurrentUser.Guid;
                entity.ModifiedByUserName = AuthContextService.CurrentUser.DisplayName;
                entity.ModifiedOn = DateTime.Now;
                entity.Status = model.Status;
                entity.Description = model.Description;
                entity.StartPeriod = model.StartPeriod;
                entity.EndPeriod = model.EndPeriod;
                _dbContext.SaveChanges();
                response.SetSuccess();
                return Ok(response);
            }
        }

        /// <summary>
        /// 删除提成比例对照
        /// </summary>
        /// <param name="ids">提成比例对照ID,多个以逗号分隔</param>
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
        /// 恢复提成比例对照
        /// </summary>
        /// <param name="ids">提成比例对照ID,多个以逗号分隔</param>
        /// <returns></returns>
        [HttpGet("{ids}")]
        [ProducesResponseType(200)]
        public IActionResult Recover(string ids)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (ConfigurationManager.AppSettings.IsTrialVersion)
            {
                response.SetIsTrial();
                return Ok(response);
            }
            response = UpdateIsDelete(CommonEnum.IsDeleted.No, ids);
            return Ok(response);
        }

        [HttpGet("{ids}")]
        [ProducesResponseType(200)]
        public IActionResult Reset(string ids)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (ConfigurationManager.AppSettings.IsTrialVersion)
            {
                response.SetIsTrial();
                return Ok(response);
            }
            response = ResetAppPwd(ids);
            return Ok(response);
        }

        private ResponseModel ResetAppPwd(string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE BaseComparison SET AppPwd=@AppPwd WHERE Id IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@AppPwd", MD5Helper.GenerateMD5("12345")));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ids">提成比例对照ID,多个以逗号分隔</param>
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
                    }
                    else
                    {
                        response = UpdateIsDelete(CommonEnum.IsDeleted.Yes, ids);
                    }
                    break;
                case "recover":
                    response = UpdateIsDelete(CommonEnum.IsDeleted.No, ids);
                    break;
                case "forbidden":
                    if (ConfigurationManager.AppSettings.IsTrialVersion)
                    {
                        response.SetIsTrial();
                    }
                    else
                    {
                        response = UpdateStatus(UserStatus.Forbidden, ids);
                    }
                    break;
                case "normal":
                    response = UpdateStatus(UserStatus.Normal, ids);
                    break;
                default:
                    break;
            }
            return Ok(response);
        }


        /// <summary>
        /// 删除提成比例对照
        /// </summary>
        /// <param name="isDeleted"></param>
        /// <param name="ids">提成比例对照ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateIsDelete(CommonEnum.IsDeleted isDeleted, string ids)
        {
            using (_dbContext)
            {
                var response = ResponseModelFactory.CreateInstance;
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE BaseComparison SET IsDeleted=@IsDeleted WHERE Id IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@IsDeleted", (int)isDeleted));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                response.SetSuccess();
                return response;
            }
        }

        /// <summary>
        /// 删除提成比例对照
        /// </summary>
        /// <param name="status">提成比例对照状态</param>
        /// <param name="ids">提成比例对照ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateStatus(UserStatus status, string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE BaseComparison SET Status=@Status WHERE Id IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@Status", (int)status));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }
    }
}