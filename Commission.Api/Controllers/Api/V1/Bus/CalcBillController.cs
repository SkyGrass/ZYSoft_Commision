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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
    public class CalcBillController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public CalcBillController(DncZeusDbContext dbContext, IMapper mapper, IHostingEnvironment hostingEnvironment)
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
        public IActionResult List(CalcBillRecordRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.vCalcBillRecord.AsQueryable();
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
                if (payload.Status > CommonEnum.AuditState.All)
                {
                    query = query.Where(x => x.FStatus == payload.Status);
                }
                if (payload.SalesmanId > 0)
                {
                    query = query.Where(x => x.FSalesmanId == payload.SalesmanId);
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
                var entity = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.UserId == AuthContextService.CurrentUser.Guid);

                if (entity != null)
                {
                    query = query.Where(x => x.FSalesmanId == entity.SalesmanId);
                }
                #endregion

                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<vCalcBillRecord, CalcBillRecordJsonModel>);
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
                data = string.Format(@"CA{0}", DateTime.Now.ToString("yyyyMMddHHmmss")),
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
        public IActionResult Create(SaveCalcBillForm model)
        {
            var response = ResponseModelFactory.CreateInstance;
            try
            {
                using (_dbContext)
                {
                    List<string> sqlList = new List<string>();
                    sqlList.Clear();
                    if (model.Form.FId > 0) //update
                    {
                        //delete first 
                        string delSql = string.Format(@"delete from t_calcBillEntry where FId = {0}", model.Form.FId);
                        sqlList.Add(delSql);
                        string updateSql = string.Format(@"Update t_CalcBill Set FSalesmanId ='{0}',FRemark= '{1}' Where FId ='{2}'",
                            model.Form.FSalesmanId, model.Form.FRemark, model.Form.FId);
                        sqlList.Add(updateSql);
                    }
                    else
                    {  //add
                        int newId = new IDHelper(_dbContext).GenId(typeof(T_CalcBill).GetAttributeValue((TableAttribute dna) => dna.Name));
                        model.Form.FId = newId;
                        string insertSql = string.Format(@"INSERT INTO dbo.t_CalcBill
                                                        ( FId ,
                                                          FBillNo ,
                                                          FDate ,
                                                          FSalesmanId ,
                                                          FBillerId ,
                                                          FRemark ,
                                                          FIsDeleted ,
                                                          FCreatedOn ,
                                                          FStatus
                                                        )
                                                VALUES  ( '{0}' , -- FId - int
                                                          '{1}' , -- FBillNo - varchar(50)
                                                          GETDATE() , -- FDate - datetime
                                                          '{2}' , -- FSalesmanId - int
                                                          '{3}' , -- FBillerId - int
                                                          '{4}' , -- FRemark - varchar(1000)
                                                          0 , -- FIsDeleted - bit
                                                          GETDATE() , -- FCreatedOn - datetime
                                                          0 -- FStatus - int
                                                        )", model.Form.FId, model.Form.FBillNo,
                                                        model.Form.FSalesmanId, model.Form.FSalesmanId, model.Form.FRemark);
                        sqlList.Add(insertSql);
                    }
                    //insert second
                    model.Entry.ForEach(row =>
                    {
                        sqlList.Add(string.Format(@"insert into	dbo.t_CalcBillEntry
                                                        ( FId ,
                                                          FRecordId ,
                                                          FRecordEntryId ,
                                                          FCommissionPrice ,
                                                          FExpand ,
                                                          FTotal
                                                        )
                                                VALUES  ( '{0}' , -- FId - int
                                                          '{1}' , -- FRecordId - int
                                                          '{2}' , -- FRecordEntryId - int
                                                          '{3}' , -- FCommissionPrice - decimal
                                                          '{4}' , -- FExpand - decimal
                                                          '{5}'  -- FTotal - decimal
                                                        )", model.Form.FId, row.FRecordId, row.FRecordEntryId, row.FCommissionPrice, row.FExpand, row.FTotal));

                        sqlList.Add(string.Format(@"update t_Bill set FIsCalc =1 Where FId = {0} ", row.FRecordId));
                    });

                    if (sqlList.Count > 0)
                    {
                        _dbContext.Database.BeginTransaction();
                        int effectRow = 0;
                        sqlList.ForEach(f =>
                        {
                            effectRow += _dbContext.Database.ExecuteSqlCommand(f);
                        });
                        _dbContext.Database.CommitTransaction();
                        response.SetData($"success|{model.Form.FId}");
                        response.SetSuccess();
                    }
                    else
                    {
                        response.SetFailed("未能查询到要更新的数据!");
                    }
                }
                return Ok(response);

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
                var entity = _dbContext.vCalcBillRecord.Where(x => x.FId == id).ToList();
                var response = ResponseModelFactory.CreateInstance;
                response.SetData(entity.Select(_mapper.Map<vCalcBillRecord, CalcBillRecordJsonModel>));
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
                var query = _dbContext.T_CalcBillEntry.Where(x => x.FId == id);
                var list = query.ToList();
                var data = list.Select(_mapper.Map<T_CalcBillEntry, BillRecordEntryJsonModel>);
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
        public string exportExcel(CalcBillRecordRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FBillNo like '%{payload.Kw}%')";
                }
                if (payload.SalesmanId > 0)
                {
                    queryStr += $"AND  FSalesmanId ='{payload.SalesmanId}'";
                }
                if (!string.IsNullOrEmpty(payload.BeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.BeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.EndDate))
                {
                    queryStr += $"AND  FDate <='{payload.EndDate} 23:59:59' ";
                }

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
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.BaseViewConfig WHERE FViewId =1
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

                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vCalcBillRecord where 1=1 {1}", columns, queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";

                        DataSet ds = new DataSet();
                        source.TableName = "计算单清单";
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
                DataTable dtCheck = _dbContext.Database.SqlQuery(string.Format(@"SELECT 1 FROM dbo.t_CalcBill WHERE ISNULL(FStatus,0) = 1 and FId IN ({0})", ids));
                if (dtCheck != null && dtCheck.Rows.Count > 0)
                {
                    response.SetFailed("发现记录已经被审批,无法删除!");
                    return response;
                }
                else
                {
                    List<string> sqlList = new List<string>();
                    sqlList.Clear();
                    var sql = string.Format("Delete From T_CalcBill WHERE FId IN ({0})", ids);
                    sqlList.Add(sql);

                    DataTable dt = _dbContext.Database.SqlQuery(string.Format("select ISNULL(FRecordId,0)FRecordId from T_CalcBillEntry where FId IN ({0})", ids));
                    string sourceBillId = "-1";
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sourceBillId = dt.Rows[0]["FRecordId"].ToString();
                    }

                    sql = string.Format("Delete From T_CalcBillEntry WHERE FId IN ({0})", ids);
                    sqlList.Add(sql);
                    sql = string.Format("Update T_Bill Set FIsCalc =0 WHERE FId IN ({0})", sourceBillId);
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
                var sql = string.Format("UPDATE T_CalcBill SET FStatus=@Status WHERE FId IN ({0})", parameterNames);
                parameters.Add(new SqlParameter("@Status", (int)status));
                _dbContext.Database.ExecuteSqlCommand(sql, parameters);
                var response = ResponseModelFactory.CreateInstance;
                return response;
            }
        }
    }
}