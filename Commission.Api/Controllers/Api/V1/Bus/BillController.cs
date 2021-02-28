﻿using AutoMapper;
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
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;

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
        private readonly IHostingEnvironment _hostingEnvironment;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public BillController(DncZeusDbContext dbContext, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
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

        [HttpGet("{ids}")]
        [ProducesResponseType(200)]
        public IActionResult CalcListEntry(string ids)
        {
            using (_dbContext)
            {
                var response = ResponseModelFactory.CreateInstance;

                var sql = string.Format("select * from vBillRecord  where fid in ({0})", ids);
                var list = _dbContext.Database.SqlQuery(sql).ToList<vBillRecord>();
                var data = list.Select(_mapper.Map<vBillRecord, BillRecordJsonModel>); response.SetData(data);
                return Ok(response);

            }
        }

        [HttpPost]
        public IActionResult CalcList(BillRecordListRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.vBillRecordList.AsQueryable();
                var response = ResponseModelFactory.CreateResultInstance;
                if (payload.SalesmanId <= 0)
                {
                    response.SetData("未指定要查询的业务员信息!");
                    response.SetFailed();
                    return Ok(response);
                }

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

                query = query.Where(x => x.FIsCalc == false);

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
                var data = list.Select(_mapper.Map<vBillRecordList, BillRecordListJsonModel>);

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
        public IActionResult Create(SaveBillForm model)
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
                    if (newId <= 0)
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

        [HttpPost]
        public string exportExcel(BillRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += string.Format(@" AND (FBillNo like '%{0}%' OR FRemark like '%{0}%')", payload.Kw.Trim());
                }
                if (payload.IsDeleted > CommonEnum.IsDeleted.All)
                {
                    queryStr += string.Format(@" AND  FIsDeleted = {0}", payload.IsDeleted.GetHashCode());
                }
                if (payload.IsCommission > CommonEnum.YesOrNo.All)
                {
                    queryStr += string.Format(@" AND  FIsCommission = {0}", payload.IsCommission.GetHashCode());
                }
                if (payload.Status > CommonEnum.AuditState.All)
                {
                    queryStr += string.Format(@" AND  FStatus = {0}", payload.Status.GetHashCode());
                }
                if (payload.CustomId > 0)
                {
                    queryStr += string.Format(@" AND  FCustomId = {0}", payload.CustomId);
                }
                if (payload.SalesmanId > 0)
                {
                    queryStr += string.Format(@" AND  FSalesmanId = {0}", payload.SalesmanId);
                }
                if (payload.ConfirmerId > 0)
                {
                    queryStr += string.Format(@" AND  FConfirmerId = {0}", payload.ConfirmerId);
                }
                if (!string.IsNullOrEmpty(payload.BeginDate))
                {
                    queryStr += string.Format(@" AND  FDate >= '{0}'", payload.BeginDate);
                }
                if (!string.IsNullOrEmpty(payload.EndDate))
                {
                    queryStr += string.Format(@" AND  FDate <= '{0}'", string.Format(@"{0} 23:59:59", payload.EndDate));
                }

                #region 一般用户
                if (AuthContextService.CurrentUser.UserType == UserType.GeneralUser)
                {
                    var entity = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.UserId == AuthContextService.CurrentUser.Guid);

                    if (entity != null)
                    {
                        queryStr += string.Format(@" AND  FSalesmanId = {0}", payload.SalesmanId);
                    }
                    else
                    {
                        queryStr += string.Format(@" AND  FSalesmanId = -1");
                    }
                }
                #endregion

                var fileType = "xlsx";
                var path = string.Format(@"{0}/excels", _hostingEnvironment.WebRootPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = RandomHelper.GetRandomizer(10, true, false, true, false);
                path = string.Format(@"{0}/{1}.{2}", path, fileName, fileType);

                using (_dbContext)
                {
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.BaseViewConfig WHERE FViewId =4
                AND ISNULL(FIsClose,0)=0 ORDER BY FNo"));
                    string columns = string.Empty;
                    if (config != null)
                    {
                        foreach (DataRow dr in config.Rows)
                        {
                            columns += $"{dr["FColName"]} as {dr["FLabelName"]},";
                        }

                        if (columns.EndsWith(","))
                        {
                            columns = columns.Substring(0, columns.Length - 1);
                        }
                    }
                    else
                    {
                        columns = "*";
                    }

                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vBillRecord where 1=1 {1}", columns, queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";

                        DataSet ds = new DataSet();
                        source.TableName = "销售录入清单";
                        ds.Tables.Add(source);
                        List<int> result = new ExcelHelper(path).DataTableToExcel(ds, true, ref errorMsg);
                        if (result.Count > 0)
                        {
                            var fileBase64 = ExcelHelper.File2Base64(path);
                            if (!string.IsNullOrEmpty(fileBase64))
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "success",
                                    msg = "",
                                    data = new
                                    {
                                        filename = string.Format(@"{0}.{1}", fileName, fileType),
                                        file = fileBase64
                                    }
                                });
                            }
                            else
                            {
                                return JsonConvert.SerializeObject(new
                                {
                                    state = "error",
                                    msg = "文件转化失败!"
                                });
                            }
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                state = "error",
                                msg = errorMsg
                            });
                        }
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            state = "error",
                            msg = "没有查询到数据!",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    state = "error",
                    msg = "导出数据发生异常!",
                });
            }
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
                DataTable dtCheck = _dbContext.Database.SqlQuery(string.Format(@"SELECT 1 FROM dbo.vBillRecord WHERE (ISNULL(FStatus,0) = 1
                OR ISNULL(FIsCalc,0) =1) AND FId IN ({0}) ", ids));
                if (dtCheck != null && dtCheck.Rows.Count > 0)
                {
                    response.SetFailed("发现记录已经被审批或已被计算提成,无法删除!");
                    return response;
                }
                else
                {
                    List<string> sqlList = new List<string>();
                    sqlList.Clear();
                    var sql = string.Format("Delete From T_Bill WHERE FId IN ({0})", ids);
                    sqlList.Add(sql); 
                    sql = string.Format("Delete From T_BillEntry WHERE FId IN ({0})", ids);
                    sqlList.Add(sql);

                    _dbContext.Database.BeginTransaction();
                    int effectRow = 0;
                    sqlList.ForEach(f =>
                    {
                        effectRow += _dbContext.Database.ExecuteSqlCommand(f);
                    });
                    _dbContext.Database.CommitTransaction();
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