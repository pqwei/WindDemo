using System;
using System.Collections.Generic;
using System.Text;

namespace WindDemo
{
    public class WindModel
    {

        /// <summary>
        /// 日期
        /// </summary>
        public System.DateTime Time { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public System.String WindCode { get; set; }

        /// <summary>
        /// 开价
        /// </summary>
        public System.Int32 Open { get; set; }

        /// <summary>
        /// 收价
        /// </summary>
        public System.Int32 Close { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public System.Int32 High { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public System.Int32 Low { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public System.Int64 Volume { get; set; }
        /// <summary>
        /// 成交额
        /// </summary>
        public System.Int64 Amount { get; set; }
    }
}
