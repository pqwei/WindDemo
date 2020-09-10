﻿using System;
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

namespace ZSFund.Wind.Api
{
    /// <summary>
    /// WindApi辅助类
    /// </summary>
    public static class Wind
    {
        /// <summary>
        /// 获取WindAPI
        /// </summary>
        /// <returns></returns>
        public static WindAPI GetApi()
        {
            WindAPI w = new WindAPI();
            Login(w);
            return w;
        }

        /// <summary>
        /// WindApi登陆
        /// </summary>
        /// <param name="w"></param>
        private static void Login(WindAPI w)
        {
            string strErrorMsg;

            //登录WFT
            int nRetCode = w.start();
            if (0 != nRetCode)//登录失败
            {
                strErrorMsg = w.getErrorMsg(nRetCode);
                throw new Exception($"登录WFT失败{strErrorMsg}");
            }
            return;
        }

        /// <summary>
        /// WindData转化为安全数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataResponse ConvertToSAFSData(WindData data)
        {
            DataResponse sdata = new DataResponse();
            try
            {
                if (data.errorCode == 0)
                {
                    string[] tempdata = new string[] { };
                    if (data.data is JArray)
                        tempdata = ((JArray)data.data).ToObject<string[]>().ToArray();
                    if (data.data is double[])
                        tempdata = (data.data as double[]).Select(o => o.ToString()).ToArray();
                    if (data.data is string[])
                        tempdata = (data.data as string[]).Select(o => o == null ? "" : o.ToString()).ToArray();
                    if (data.data is object[])
                        tempdata = (data.data as object[]).Select(o => o == null ? "" : o.ToString()).ToArray();

                    DataTable dt = new DataTable();
                    dt.TableName = "WindData";
                    dt.Columns.Add("Code");
                    dt.Columns.Add("Time", typeof(DateTime));

                    foreach (var field in data.fieldList)
                    {
                        dt.Columns.Add(field);
                    }

                    var i = 0;
                    var fieldCount = data.fieldList.Length;
                    foreach (var date in data.timeList)
                    {
                        foreach (var code in data.codeList)
                        {
                            var dataList = new List<object>() { code, date };
                            var d = tempdata.Skip(i * fieldCount).Take(fieldCount).ToArray();
                            dataList.AddRange(d);
                            dt.Rows.Add(dataList.ToArray());
                            i++;
                        }
                    }

                    sdata.Data = dt;
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
                throw new Exception("Wind数据解析失败", ex);
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
                throw new Exception("DataTable转化为dynamic列表失败", ex);
            }
            return result;
        }
    }

    /// <summary>
    /// 安全数据模型
    /// </summary>
    public class DataResponse
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public DataTable Data { get; set; }
    }
}