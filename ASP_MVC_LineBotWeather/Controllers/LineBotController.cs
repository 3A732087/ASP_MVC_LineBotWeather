using ASP_MVC_LineBotWeather.Services;
using isRock.LineBot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;

namespace ASP_MVC_LineBotWeather.Controllers
{
    public class LineBotController : ApiController
    {

        [HttpPost]
        public IHttpActionResult Post()
        {
            string ChannelAccessToken = WebConfigurationManager.AppSettings["accessToken"];
            string WeatherAPI = WebConfigurationManager.AppSettings["weatherAPI"];

            WeatherService weatherService = new WeatherService();

            try
            {

                //取得 http Post RawData(should be JSON)
                string postData = Request.Content.ReadAsStringAsync().Result;
                //剖析JSON
                var ReceivedMessage = isRock.LineBot.Utility.Parsing(postData);

                Dictionary<string, object> checkResult = weatherService.SearchCheck(ReceivedMessage.events[0].message.text);

                if (checkResult["Status"].Equals(true))
                {
                    string JsonStr = "";
                    string locationName = checkResult["SearchData"].ToString();
                    string Url = $"https://opendata.cwb.gov.tw/api/v1/rest/datastore/F-C0032-001?Authorization={WeatherAPI}&format=JSON&locationName={locationName}&elementName=MinT,MaxT,PoP";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                    request.Method = "GET";

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            JsonStr = sr.ReadToEnd();
                        }
                    }

                    if (!string.IsNullOrEmpty(JsonStr))
                    {
                        var resultData = JsonConvert.DeserializeObject<Root>(JsonStr);
                        if (resultData.success.ToString() == "true")
                        {
                            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(ChannelAccessToken);  //傳入Channel access token
                            List<WeatherElement> Data = new List<WeatherElement>();
                            List<Time> res = new List<Time>();
                            Data = resultData.records.location[0].weatherElement;

                            for (int i = 0; i < 3; i++)
                            {
                                foreach (var time in Data)
                                {
                                    res.Add(time.time[i]);
                                }
                            }

                            //順序：降雨機率、最低溫、最高溫
                            Uri MessageImg = new Uri("圖片網址");

                            Column Columns = new Column();
                            CarouselTemplate carouselTemplate = new CarouselTemplate();

                            //組訊息
                            string title = "";
                            string content = "";

                            var actions = new List<isRock.LineBot.TemplateActionBase>();
                            actions.Add(new isRock.LineBot.UriAction() { label = "詳細內容", uri = new Uri("https://www.cwb.gov.tw/V8/C/W/County/index.html") });


                            for (int j = 0; j < res.Count(); j += 3)
                            {
                                title = Convert.ToDateTime(res[j].startTime).ToString("MM-dd HH:mm") + " ~ " + Convert.ToDateTime(res[j].endTime).ToString("MM-dd HH:mm");
                                content = "溫度" + res[j + 1].parameter.parameterName + " ~ " + res[j + 2].parameter.parameterName + "°C" +
                                    " \n" + "降雨機率" + res[j].parameter.parameterName + "%";

                                Columns = new Column { thumbnailImageUrl = MessageImg, title = title, text = content, actions = actions };
                                carouselTemplate.columns.Add(Columns);
                            }
                            isRock.LineBot.Utility.ReplyTemplateMessage(ReceivedMessage.events[0].replyToken, carouselTemplate, ChannelAccessToken);
                        }
                    }
                }
                else
                {
                    string erroeMsg = "";
                    if(checkResult["Msg"].ToString() == "搜尋格式錯誤！")
                    {
                        erroeMsg = checkResult["Msg"].ToString() + 
                            "\n若要查詢氣象資訊請輸入" +
                            "\n『天氣 縣市名稱』，例如：天氣 臺中市";
                    }
                    else
                    {
                        erroeMsg = checkResult["Msg"].ToString();
                    }
                    isRock.LineBot.Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, erroeMsg, ChannelAccessToken);

                }
                return Ok();

            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message.ToString());
                return Ok();
            }
        }

        public class Field
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Location
        {
            public string locationName { get; set; }
            public List<WeatherElement> weatherElement { get; set; }
        }

        public class Parameter
        {
            public string parameterName { get; set; }
            public string parameterUnit { get; set; }
        }

        public class Records
        {
            public string datasetDescription { get; set; }
            public List<Location> location { get; set; }
        }

        public class Result
        {
            public string resource_id { get; set; }
            public List<Field> fields { get; set; }
        }

        public class Root
        {
            public string success { get; set; }
            public Result result { get; set; }
            public Records records { get; set; }
        }

        public class Time
        {
            public string startTime { get; set; }
            public string endTime { get; set; }
            public Parameter parameter { get; set; }
        }

        public class WeatherElement
        {
            public string elementName { get; set; }
            public List<Time> time { get; set; }
        }


    }
}

