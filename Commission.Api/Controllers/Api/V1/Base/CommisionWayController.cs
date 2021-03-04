using AutoMapper;
using Commission.Api.Entities;
using Commission.Api.Entities.Enums;
using Commission.Api.Extensions;
using Commission.Api.Extensions.AuthContext;
using Commission.Api.Extensions.CustomException;
using Commission.Api.Models.Response;
using Commission.Api.RequestPayload.Base.CommisionWay;
using Commission.Api.Utils;
using Commission.Api.ViewModels.Base.CommisionWay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
    public class CommisionWayController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public CommisionWayController(DncZeusDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult List(CommisionWayRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.BaseCommisionWay.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    query = query.Where(x => x.Code.Contains(payload.Kw.Trim()) || x.Name.Contains(payload.Kw.Trim()));
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
                var data = list.Select(_mapper.Map<BaseCommisionWay, CommisionWayJsonModel>);
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
        public IActionResult Select(CommisionWaySelectRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.BaseCommisionWay.AsQueryable();
                var list = query.ToList();
                var data = list.Select(_mapper.Map<BaseCommisionWay, CommisionWaySelectJsonModel>);
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
                var query = _dbContext.BaseCommisionWay.AsQueryable();
                var list = query.ToList();
                var data = list.Select(_mapper.Map<BaseCommisionWay, CommisionWaySelectJsonModel>);
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
        [HttpGet("/api/v1/base/area/find_list_by_kw/{kw}")]
        public IActionResult FindByKeyword(string kw)
        {
            var response = ResponseModelFactory.CreateResultInstance;
            if (string.IsNullOrEmpty(kw))
            {
                response.SetFailed("没有查询到数据");
                return Ok(response);
            }
            using (_dbContext)
            {

                var query = _dbContext.DncIcon.Where(x => x.Code.Contains(kw));

                var list = query.ToList();
                var data = list.Select(x => new { x.Code, x.Color, x.Size });

                response.SetData(data);
                return Ok(response);
            }
        }

        /// <summary>
        /// 创建病区
        /// </summary>
        /// <param name="model">病区视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Create(CommisionWayCreateViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (model.Code.Trim().Length <= 0)
            {
                response.SetFailed("请输入提成比例方式编码");
                return Ok(response);
            }
            if (model.Name.Trim().Length <= 0)
            {
                response.SetFailed("请输入提成比例方式名称");
                return Ok(response);
            }
            using (_dbContext)
            {
                if (_dbContext.BaseCommisionWay.Count(x => x.Code == model.Code) > 0)
                {
                    response.SetFailed("提成比例方式已存在");
                    return Ok(response);
                }
                var entity = _mapper.Map<CommisionWayCreateViewModel, BaseCommisionWay>(model);
                entity.Id = new IDHelper(_dbContext).GenId(typeof(BaseCommisionWay).GetAttributeValue((TableAttribute dna) => dna.Name));
                entity.CreatedOn = DateTime.Now;
                entity.CreatedByUserGuid = AuthContextService.CurrentUser.Guid;
                entity.CreatedByUserName = AuthContextService.CurrentUser.DisplayName;
                _dbContext.BaseCommisionWay.Add(entity);
                _dbContext.SaveChanges();

                response.SetSuccess();
                return Ok(response);
            }
        }

        /// <summary>
        /// 编辑病区
        /// </summary>
        /// <param name="id">病区ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        public IActionResult Edit(int id)
        {
            using (_dbContext)
            {
                var entity = _dbContext.BaseCommisionWay.FirstOrDefault(x => x.Id == id);
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(_mapper.Map<BaseCommisionWay, CommisionWayCreateViewModel>(entity));
                return Ok(response);
            }
        }

        /// <summary>
        /// 保存编辑后的病区信息
        /// </summary>
        /// <param name="model">病区视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Edit(CommisionWayCreateViewModel model)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (model.Code.Trim().Length <= 0)
            {
                response.SetFailed("请输入提成比例方式编码");
                return Ok(response);
            }
            if (model.Name.Trim().Length <= 0)
            {
                response.SetFailed("请输入提成比例方式名称");
                return Ok(response);
            }
            using (_dbContext)
            {
                if (_dbContext.BaseCommisionWay.Count(x => x.Code == model.Code && x.Id != model.Id) > 0)
                {
                    response.SetFailed("提成比例方式已存在");
                    return Ok(response);
                }
                var entity = _dbContext.BaseCommisionWay.FirstOrDefault(x => x.Id == model.Id);
                entity.Code = model.Code;
                entity.Name = model.Name;
                entity.IsDeleted = model.IsDeleted;
                entity.ModifiedByUserGuid = AuthContextService.CurrentUser.Guid;
                entity.ModifiedByUserName = AuthContextService.CurrentUser.DisplayName;
                entity.ModifiedOn = DateTime.Now;
                entity.Status = model.Status;
                entity.Description = model.Description;
                _dbContext.SaveChanges();
                response.SetSuccess();
                return Ok(response);
            }
        }

        /// <summary>
        /// 删除病区
        /// </summary>
        /// <param name="ids">病区ID,多个以逗号分隔</param>
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
        /// 恢复病区
        /// </summary>
        /// <param name="ids">病区ID,多个以逗号分隔</param>
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


        /// <summary>
        /// 批量操作
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ids">病区ID,多个以逗号分隔</param>
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
        /// 删除病区
        /// </summary>
        /// <param name="isDeleted"></param>
        /// <param name="ids">病区ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateIsDelete(CommonEnum.IsDeleted isDeleted, string ids)
        {
            using (_dbContext)
            {
                var response = ResponseModelFactory.CreateInstance;
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE BaseCommisionWay SET IsDeleted=@IsDeleted WHERE Id IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@IsDeleted", (int)isDeleted));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                response.SetSuccess();
                return response;
            }
        }

        /// <summary>
        /// 删除病区
        /// </summary>
        /// <param name="status">病区状态</param>
        /// <param name="ids">病区ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateStatus(UserStatus status, string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE BaseCommisionWay SET Status=@Status WHERE Id IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@Status", (int)status));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }
    }
}