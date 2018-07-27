using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitKiwiForm
{
    public class TableDisplay
    {
        // 币种名称，如：Mana/ETH
        public string Tokenname { get; set; }

        // 止损点，如：5%
        public string LossPoint { get; set; }

        // 止盈点，如：10%
        public string SellPoint { get; set; }

        // 盈利点，如：2.4%
        public string Gain { get; set; }

        // 持仓比例，如：20%
        public string Hold { get; set; }

        // 订单完成情况，分为：已经盈利、正在运行、已经止损
        public string Status { get; set; }
    }
}
