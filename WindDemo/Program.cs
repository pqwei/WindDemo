using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using WAPIWrapperCSharp;
using ZSFund.Wind.Api;

namespace WindDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //从WindData.txt读取伪数据
            string path = Path.Combine(AppContext.BaseDirectory, "WindData.txt");
            string json = File.ReadAllText(path);
            var wd = JsonConvert.DeserializeObject<WindData>(json);

            //var api = Wind.GetApi();//获取WindAPI,三次登录失败返回null
            //WindData wd = api.wsi($"IC2001.CFE", "high,open,low,close,volume,amt", $"2020-01-02 09:30:00",
            //    $"2020-01-02 10:00:00", "");

            var response = Wind.ConvertToSAFSData(wd);//WindData转化为安全数据
            if (response.Code == 0)
            {
                var result = response.Data.Select(o => new { o.Code, o.windcode, o.high })
                    .ToList();//取出需要的数据并处理

                List<string> fields = new List<string> { "Code", "windcode", "high" };
                foreach (var field in fields)
                {
                    var model = response.Data.FirstOrDefault() as IDictionary<string, dynamic>;
                    var value = model[field];//根据字段名动态获取

                    var models = response.Data.Select(o => o as IDictionary<string, dynamic>).ToList();
                    value = models[0][field];
                }
            }
            else
            {
                throw new Exception($"WindApi调用失败:{response.Message}");
            }
        }
    }
}
