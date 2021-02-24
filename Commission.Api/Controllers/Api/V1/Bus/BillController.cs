using AutoMapper;
using Commission.Api.Entities;
using Commission.Api.Entities.Enums;
using Commission.Api.Extensions;
using Commission.Api.Extensions.AuthContext;
using Commission.Api.Extensions.CustomException;
using Commission.Api.Models.Response;
using Commission.Api.RequestPayload.Bus;
using Commission.Api.Utils;
using Commission.Api.ViewModels.Bus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.Controllers.Api.V1.Bus
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v1/bus/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class BillController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public BillController(DncZeusDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult List(BillRecordRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.vBillRecord.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    query = query.Where(x => x.FBillNo.Contains(payload.Kw.Trim()) ||
                    x.FRemark.Contains(payload.Kw.Trim()));
                }
                if (payload.IsDeleted > CommonEnum.IsDeleted.All)
                {
                    query = query.Where(x => (CommonEnum.IsDeleted)(x.FIsDeleted ? 1 : 0) ==
                        payload.IsDeleted);
                }
                if (payload.IsCommission > CommonEnum.YesOrNo.All)
                {
                    query = query.Where(x => ((CommonEnum.YesOrNo)(x.FIsCommission ? 1 : 0)) == payload.IsCommission);
                }
                if (payload.Status > CommonEnum.AuditState.All)
                {
                    query = query.Where(x => x.FStatus == payload.Status);
                }
                if (payload.CustomId > 0)
                {
                    query = query.Where(x => x.FCustomId == payload.CustomId);
                }
                if (payload.SalesmanId > 0)
                {
                    query = query.Where(x => x.FSalesmanId == payload.SalesmanId);
                }
                if (payload.ConfirmerId > 0)
                {
                    query = query.Where(x => x.FConfirmerId == payload.ConfirmerId);
                }
                if (!string.IsNullOrEmpty(payload.BeginDate))
                {
                    query = query.Where(x => x.FDate.CompareTo(DateTime.Parse(payload.BeginDate)) >= 0);
                }
                if (!string.IsNullOrEmpty(payload.EndDate))
                {
                    query = query.Where(x => x.FDate.CompareTo(DateTime.Parse(string.Format(@"{0} 23:59:59", payload.EndDate))) <= 0);
                }

                #region 一般用户
                if (AuthContextService.CurrentUser.UserType == UserType.GeneralUser)
                {
                    var entity = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.UserId == AuthContextService.CurrentUser.Guid);

                    if (entity != null)
                    {
                        query = query.Where(x => x.FSalesmanId == entity.SalesmanId);
                    }
                    else
                    {
                        query = query.Where(x => x.FSalesmanId == -1); //没有绑定的一般用户
                    }
                }
                #endregion

                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<vBillRecord, BillRecordJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data, totalCount);
                return Ok(response);
            }
        }

        public IActionResult GenBillNo()
        {
            return Ok(new
            {
                state = "success",
                data = string.Format(@"SO{0}", DateTime.Now.ToString("yyyyMMddHHmmss")),
                message = ""
            });
        }

        /// <summary>
        /// 创建床位
        /// </summary>
        /// <param name="model">床位视图实体</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Create(SaveForm model)
        {
            var response = ResponseModelFactory.CreateInstance;
            try
            {
                List<BillEntryCreateModel> listEntry = _mapper.Map<List<T_BillEntry>, List<BillEntryCreateModel>>(model.Entry);
                int newId = 0;
                if (model.Form.FId > 0)
                {
                    _dbContext.Entry(model.Form).State = EntityState.Modified;
                    var entry = _dbContext.T_BillEntry.Where(f => f.FId == model.Form.FId).ToList();
                    entry.ForEach(ele =>
                    {
                        _dbContext.Entry(ele).State = EntityState.Modified;
                    });
                }
                else
                {
                    newId = new IDHelper(_dbContext).GenId(typeof(T_Bill).GetAttributeValue((TableAttribute dna) => dna.Name));
                    model.Form.FBillerId = model.Form.FSalesmanId;
                    model.Form.FIsDeleted = false;
                    model.Form.FStatus = CommonEnum.AuditState.No;
                    model.Form.FCreatedOn = DateTime.Now;
                    model.Form.FId = newId;
                }
                listEntry.ForEach(f =>
                {
                    f.FId = model.Form.FId;
                });


                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    if (newId > 0)
                    {
                        var form = _dbContext.T_Bill.Where(f => f.FId == model.Form.FId).FirstOrDefault();
                        form.FBillerId = model.Form.FSalesmanId;
                        form.FCustomId = model.Form.FCustomId;
                        form.FSalesmanId = model.Form.FSalesmanId;
                        form.FConfirmerId = model.Form.FConfirmerId;
                        form.FDate = model.Form.FDate;
                        form.FIsCommission = model.Form.FIsCommission;
                        form.FRemark = model.Form.FRemark;
                        form.FIsDeleted = false;
                        form.FStatus = model.Form.FStatus;
                        form.FCreatedOn = model.Form.FCreatedOn;
                    }
                    else
                    {
                        _dbContext.T_Bill.Add(model.Form);
                    }
                    listEntry.ForEach(f =>
                    {
                        _dbContext.T_BillEntry.Add(new T_BillEntry()
                        {
                            FContractPrice = f.FContractPrice,
                            FDcRate = f.FDcRate,
                            FId = f.FId,
                            FModule = f.FModule,
                            FPoints = f.FPoints,
                            FStandardPrice = f.FStandardPrice,
                            FSoftwareId = f.FSoftwareId
                        }); ;
                    });
                    _dbContext.SaveChanges();
                    transaction.Commit();

                    response.SetData($"success|{model.Form.FId}");
                    response.SetSuccess();
                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                _dbContext.Database.RollbackTransaction();
                response.SetData("error");
                response.SetFailed();
                return Ok(response);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ids">图标ID,多个以逗号分隔</param>
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

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        public IActionResult Edit(int id)
        {
            using (_dbContext)
            {
                var entity = _dbContext.vBillRecord.FirstOrDefault(x => x.FId == id);
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(_mapper.Map<vBillRecord, BillRecordJsonModel>(entity));
                return Ok(response);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        public IActionResult EditEntry(int id)
        {
            using (_dbContext)
            {
                var response = ResponseModelFactory.CreateInstance;
                var query = _dbContext.T_BillEntry.Where(x => x.FId == id);
                var list = query.ToList();
                var data = list.Select(_mapper.Map<T_BillEntry, BillRecordEntryJsonModel>);
                response.SetData(data);
                return Ok(response);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        public IActionResult Audit(string id)
        {
            var response = ResponseModelFactory.CreateInstance;
            if (ConfigurationManager.AppSettings.IsTrialVersion)
            {
                response.SetIsTrial();
                return Ok(response);
            }
            response = UpdateAuditStatus(AuditState.Yes, id);
            return Ok(response);
        }

        /// <summary>
        /// 删除图标
        /// </summary>
        /// <param name="isDeleted"></param>
        /// <param name="ids">图标ID字符串,多个以逗号隔开</param>
        /// <returns></returns>
        private ResponseModel UpdateIsDelete(CommonEnum.IsDeleted isDeleted, string ids)
        {
            using (_dbContext)
            {
                var response = ResponseModelFactory.CreateInstance;
                DataTable dtCheck = _dbContext.Database.SqlQuery(string.Format(@"SELECT 1 FROM dbo.vBillRecord WHERE ISNULL(FStatus,0) = 1", ids));
                if (dtCheck != null && dtCheck.Rows.Count > 0)
                {
                    response.SetFailed("发现记录已经被审批,无法删除!");
                    return response;
                }
                else
                {
                    var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                    var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                    var sql = string.Format("UPDATE T_Bill SET FIsDeleted=@IsDeleted WHERE FId IN ({0})", parameterNames);
                    parameters.Add(new SqlParameter("@IsDeleted", (int)isDeleted));
                    _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                    response.SetSuccess();
                    return response;
                }
            }
        }

        private ResponseModel UpdateAuditStatus(AuditState status, string ids)
        {
            using (_dbContext)
            {
                var parameters = ids.Split(",").Select((id, index) => new SqlParameter(string.Format("@p{0}", index), id)).ToList();
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                var sql = string.Format("UPDATE T_Bill SET FStatus=@Status WHERE FId IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@Status", (int)status));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }
    }
}