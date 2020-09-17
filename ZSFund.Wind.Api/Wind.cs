using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WAPIWrapperCSharp;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Dynamic;
using Microsoft.CSharp;
using ZSFund.LogService;

namespace ZSFund.Wind.Api
{
    /// <summary>
    /// WindApi辅助类
    /// </summary>
    public static class Wind
    {
        /// <summary>
        /// 获取WindAPI,三次登录失败返回null
        /// </summary>
        /// <returns></returns>
        public static WindAPI GetApi()
        {
            WindAPI w = new WindAPI();
            for (int i = 0; i < 3; i++)
            {
                //登录WFT
                int nRetCode = w.start();
                if (0 != nRetCode)//登录失败
                {
                    var strErrorMsg = w.getErrorMsg(nRetCode);
                    Logger.Write("登录WFT失败", strErrorMsg, string.Empty, string.Empty, LogLevel.Error, 0);
                }
                else
                {
                    return w;
                }
            }
            return null;
        }

        /// <summary>
        /// WindData转化为安全数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataResponse ConvertToSAFSData(WindData data)
        {
            DataResponse sdata = new DataResponse()
            {
                Data = new List<dynamic>()
            };
            try
            {
                if (data.errorCode == 0)
                {
                    List<dynamic> tempdata = null;
                    if (data.data is JArray)
                        tempdata = (data.data as IEnumerable<dynamic>)?.ToList();
                    if (data.data is double[])
                        tempdata = (data.data as double[]).Select(o => (dynamic)o)?.ToList();
                    if (data.data is string[])
                        tempdata = (data.data as string[]).Select(o => (dynamic)o)?.ToList();
                    if (data.data is object[])
                        tempdata = (data.data as object[]).Select(o => (dynamic)o)?.ToList();
                    if (tempdata == null)
                    {
                        throw new Exception("WindData.data转化失败");
                    }

                    var fields = data.fieldList;
                    var fieldCount = fields.Length;
                    if (tempdata.Count < data.timeList.Length * data.codeList.Length * fieldCount)
                    {
                        throw new Exception("WindData.data数据不完整");
                    }

                    int index = 0;
                    foreach (var date in data.timeList)
                    {
                        for (int i = 0; i < data.codeList.Length; i++)
                        {
                            dynamic model = new ExpandoObject();
                            ((IDictionary<string, dynamic>)model).Add("Code", data.codeList[i]);
                            ((IDictionary<string, dynamic>)model).Add("Time", date);
                            for (int j = 0; j < fields.Length; j++)
                            {
                                ((IDictionary<string, dynamic>)model).Add(fields[j], tempdata[index++]);
                            }

                            sdata.Data.Add(model);
                        }
                    }
                }
                else if (data.data != null)
                {
                    sdata.Code = data.errorCode;
                    var arr = data.data as Array;
                    if (arr.Length == 1)
                    {
                        sdata.Message = arr.GetValue(0).ToString();
                    }
                    else
                    {
                        sdata.Message = data.data.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                sdata.Code = 1;
                sdata.Message = $"Wind数据解析失败{ex}";
                Logger.Write("Wind数据解析失败", ex.ToString(), string.Empty, string.Empty, LogLevel.Error, 0);
            }

            return sdata;
        }

        /// <summary>
        /// DataTable转化为dynamic列表
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<dynamic> ToDynamicList(this DataTable dt)
        {
            List<dynamic> result = new List<dynamic>();
            try
            {
                if (dt == null) return result;
                if (dt.Rows.Count < 1) return result;
                var columnNames = new List<string>();
                foreach (var column in dt.Columns)
                {
                    columnNames.Add(column.ToString());
                }
                foreach (DataRow row in dt.Rows)
                {
                    dynamic p = new ExpandoObject();
                    foreach (var columnName in columnNames)
                    {
                        ((IDictionary<string, object>)p).Add(columnName, row.Field<dynamic>(columnName)?.ToString());
                    }
                    result.Add(p);
                };
                dt.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Write("DataTable转化为dynamic列表失败", ex.ToString(), string.Empty, string.Empty, LogLevel.Error, 0);
            }
            return result;
        }
    }

    /// <summary>
    /// 安全数据模型
    /// </summary>
    public class DataResponse
    {
        /// <summary>
        /// 成功的话返回0
        /// </summary>
        public int Code { get; set; }

        public string Message { get; set; }

        public List<dynamic> Data { get; set; }
    }
}
