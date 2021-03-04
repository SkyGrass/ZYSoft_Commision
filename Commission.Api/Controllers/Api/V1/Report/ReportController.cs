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
using System.Data.SqlClient;
using Commission.Api.RequestPayload.Report;
using Commission.Api.ViewModels.Report;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Commission.Api.Controllers.Api.V1.Report
{
    /// <summary>
    /// 
    /// </summary>
    //[CustomAuthorize]
    [Route("api/v1/report/[controller]/[action]")]
    [ApiController]
    [CustomAuthorize]
    public class ReportController : ControllerBase
    {
        private readonly DncZeusDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="mapper"></param>
        public ReportController(DncZeusDbContext dbContext, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult EmployeeList(EmployeeRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.vEmployee.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    query = query.Where(x => x.FSoftwareCode.Contains(payload.Kw.Trim()) ||
                    x.FSoftwareName.Contains(payload.Kw.Trim()));
                }
                if (payload.CustomId > 0)
                {
                    query = query.Where(x => x.FCustomId == payload.CustomId);
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

                #region  
                var entity = _dbContext.UserSalesmanMapping.FirstOrDefault(x => x.UserId == AuthContextService.CurrentUser.Guid);

                if (entity != null)
                {
                    query = query.Where(x => x.FSalesmanId == entity.SalesmanId);
                }
                #endregion

                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<vEmployee, vEmployeePointJsonModel>);
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
        public IActionResult PointList(CustomPointsRequestPayload payload)
        {
            using (_dbContext)
            {
                var query = _dbContext.vCustomPoint.AsQueryable();
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    query = query.Where(x => x.FCustomCode.Contains(payload.Kw.Trim()) ||
                    x.FCustomName.Contains(payload.Kw.Trim()));
                }
                if (payload.CustomId > 0)
                {
                    query = query.Where(x => x.FCustomId == payload.CustomId);
                }

                if (!string.IsNullOrEmpty(payload.BeginDate))
                {
                    query = query.Where(x => x.FDate.CompareTo(DateTime.Parse(payload.BeginDate)) >= 0);
                }
                if (!string.IsNullOrEmpty(payload.EndDate))
                {
                    query = query.Where(x => x.FDate.CompareTo(DateTime.Parse(string.Format(@"{0} 23:59:59", payload.EndDate))) <= 0);
                }

                var list = query.Paged(payload.CurrentPage, payload.PageSize).ToList();
                var totalCount = query.Count();
                var data = list.Select(_mapper.Map<vCustomPoint, vCustomPointJsonModel>);
                var response = ResponseModelFactory.CreateResultInstance;
                response.SetData(data, totalCount);
                return Ok(response);
            }
        }



        [HttpPost]
        public string exportExcelForEmp(EmployeeRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FSoftwareCode like '%{payload.Kw}%' OR FSoftwareName like '%{payload.Kw}%')";
                }
                if (payload.SalesmanId > 0)
                {
                    queryStr += $"AND  FSalesmanId ='{payload.SalesmanId}'";
                }
                if (payload.CustomId > 0)
                {
                    queryStr += $"AND  FCustomId ='{payload.CustomId}'";
                }
                if (!string.IsNullOrEmpty(payload.BeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.BeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.EndDate))
                {
                    queryStr += $"AND  FDate <='{payload.EndDate} 23:59:59 '";
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
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.BaseViewConfig WHERE FViewId =2
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

                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from vEmployee where 1=1 {1}", columns, queryStr));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";

                        DataSet ds = new DataSet();
                        source.TableName = "员工提成明细表";
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
                    msg = "导出数据发生异常!" + ex.Message,
                });
            }
        }

        [HttpPost]
        public string exportExcelForCus(CustomPointsRequestPayload payload)
        {
            try
            {
                string queryStr = string.Empty;
                if (!string.IsNullOrEmpty(payload.Kw))
                {
                    queryStr += $"AND (FCustomCode like '%{payload.Kw}%' OR FCustomName like '%{payload.Kw}%')";
                }

                if (payload.CustomId > 0)
                {
                    queryStr += $"AND  FCustomId ='{payload.CustomId}'";
                }
                if (!string.IsNullOrEmpty(payload.BeginDate))
                {
                    queryStr += $"AND  FDate >='{payload.BeginDate}'";
                }
                if (!string.IsNullOrEmpty(payload.EndDate))
                {
                    queryStr += $"AND  FDate <='{payload.EndDate} 23:59:59 '";
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
                    DataTable config = _dbContext.Database.SqlQuery(string.Format(@"SELECT * FROM dbo.BaseViewConfig WHERE FViewId =3
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

                    DataTable source = _dbContext.Database.SqlQuery(string.Format(@"select {0} from (SELECT   t3.Code AS FCustomCode, t3.Name AS FCustomName, SUM(t2.FPoints) AS FPoints
FROM      dbo.t_Bill AS t1 LEFT OUTER JOIN
                (SELECT FId,SUM(FPoints)FPoints FROM dbo.t_BillEntry GROUP BY FId) AS t2 ON t1.FId = t2.FId LEFT OUTER JOIN
                dbo.BaseCustom AS t3 ON t1.FCustomId = t3.Id where 1=1 {1}  GROUP BY t3.Code,t3.Name) as t where 1=1", columns, queryStr, payload.BeginDate, payload.EndDate));
                    if (source != null && source.Rows.Count > 0)
                    {
                        string errorMsg = "";

                        DataSet ds = new DataSet();
                        source.TableName = "客户积分汇总表";
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
    }
}