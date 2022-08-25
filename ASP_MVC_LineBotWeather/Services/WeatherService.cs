using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC_LineBotWeather.Services
{
    public class WeatherService
    {
        #region 檢查搜尋縣市是否存在
        public Dictionary<string, object> SearchCheck(string SearchData)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (SearchData.Substring(0, 3) == "天氣 ")
            {
                int checker = 0;
                if (SearchData.Substring(3, 1) == "台")
                {
                    SearchData.Replace("台", "臺");
                }
                string PureSearchData = SearchData.Substring(3, 3);
                string[] County = new string[] { "宜蘭縣", "花蓮縣", "臺東縣", "澎湖縣", "金門縣", "連江縣", "臺北市", "新北市", "桃園市", "臺中市", "臺南市", "高雄市", "基隆市", "新竹縣", "新竹市", "苗栗縣", "彰化縣", "南投縣", "雲林縣", "嘉義縣", "嘉義市", "屏東縣" };

                for (int i = 0; i < County.Count(); i++)
                {
                    if (County[i] == PureSearchData)
                    {
                        result.Add("Status", true);
                        result.Add("SearchData", PureSearchData);
                        result.Add("Msg", "搜尋縣市存在");
                        break;
                    }
                    else
                        checker += 1;
                }
                if (checker == County.Count())
                {
                    string msg = "搜尋縣市不存在" +
                        "\n\n臺灣各縣市如下：";
                    for(int j = 0; j < County.Length; j++)
                    {
                        msg += "\n" + County[j].ToString();
                    }
                    result.Add("Status", false);
                    result.Add("Msg", msg);
                }
            }
            else
            {
                result.Add("Status", false);
                result.Add("Msg", "搜尋格式錯誤！");
            }
            return result;
        }
        #endregion
    }
}