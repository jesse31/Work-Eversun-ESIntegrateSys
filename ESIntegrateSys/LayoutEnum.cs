using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESIntegrateSys
{
    public static class LayoutMapping
    {
        // 部門與 LayoutEnum 的對應關係
        public static readonly Dictionary<string, LayoutEnum> DeptToLayoutMap = new Dictionary<string, LayoutEnum>
        {
            { "IT", LayoutEnum._AdminLayout },
            { "QE", LayoutEnum._MaterialLayout },
            { "IE", LayoutEnum._QuoteLayout },
            { "SD", LayoutEnum._QuoteLayout },
            { "S", LayoutEnum._MaterialLayout },
            { "", LayoutEnum._Layout },
        };

        // 獲取 LayoutEnum 根據部門名稱
        public static LayoutEnum GetLayoutForDept(string dept)
        {
            // 防止 dept 為 null 導致 Dictionary.TryGetValue 報錯
            if (string.IsNullOrEmpty(dept))
            {
                return LayoutEnum._Layout;
            }

            // 如果部門存在於字典中，返回對應的 LayoutEnum，否則返回一個預設值
            return DeptToLayoutMap.TryGetValue(dept, out LayoutEnum layout) ? layout : LayoutEnum._Layout;
        }
    }

    public enum LayoutEnum
    {
        _AdminLayout,
        _MaterialLayout,
        _QuoteLayout,
        _WoTSLayout,
        _Layout
    }
}