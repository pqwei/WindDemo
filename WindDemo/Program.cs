using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using WAPIWrapperCSharp;
using ZSWind.Api;

namespace WindDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //var api = Wind.GetApi();//获取WindAPI
            //WindData wd = api.wsi($"IC2001.CFE", "high,open,low,close,volume,amt", $"2020-01-02 09:30:00", $"2020-01-02 10:00:00", "");
            
            //从WindData.txt读取伪数据
            string path = Path.Combine(AppContext.BaseDirectory, "WindData.txt");
            string json = File.ReadAllText(path);
            var wd = JsonConvert.DeserializeObject<WindData>(json);

            var response = Wind.ConvertToSAFSData(wd);//WindData转化为安全数据
            if (response.Code==0)
            {
                var data = Wind.ToDynamicList(response.Data);//DataTable转化为dynamic列表
                var result = data.Select(o=>new { o.Code,o.windcode,o.high }).ToList();//取出需要的数据并处理
            }
            else
            {
                throw new Exception("WindApi调用失败");
            }
        }
    }
}
