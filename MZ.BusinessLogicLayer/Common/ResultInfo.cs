using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLogicLayer
{
    public class ResultInfo
    {
        /// <summary>
        /// 状态
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object data { get; set; }
    }
}
