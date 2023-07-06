using FreeVideo.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FreeVideo.Services;


public class zyk1080SearchVideoService : ISearchVideoService, IDisposable
{
    HttpClient _client;
    JsonSerializerOptions _serializerOptions;
    protected string GetListUrl
    {
        get { return "https://api.1080zyku.com/inc/api_mac10.php?ac=list&wd={0}"; }
    }
    protected string GetDetailUrl
    {
        get { return "https://api.1080zyku.com/inc/api_mac10.php?ac=detail&ids={0}"; }
    }


    public zyk1080SearchVideoService()
    {
        _client = new HttpClient();
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<List<SearchVideoListModel>> GetSearchListAsync(string query)
    {
        var Items = new List<SearchVideoListModel>();

        Uri uri = new Uri(string.Format(this.GetListUrl, query));
        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<zyk1080ListResult>(content, _serializerOptions);
                if (result.code == 1 && result.list != null && result.list.Count > 0)
                {
                    foreach (var item in result.list)
                    {
                        Items.Add(new SearchVideoListModel(item.type_id, item.type_name, item.vod_id, item.vod_name, item.vod_en, item.vod_play_from, item.vod_remarks, item.vod_time));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return Items;

    }

    public async Task<SearchVideoDetailtModel> GetSearchDetailAsync(string vod_id)
    {
        Uri uri = new Uri(string.Format(this.GetDetailUrl, vod_id));
        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<zyk1080DetailResult>(content, _serializerOptions);
                if (result.code == 1 && result.list != null && result.list.Count > 0)
                {
                    var model = new SearchVideoDetailtModel(result.list[0].type_id, result.list[0].type_name, result.list[0].vod_id, result.list[0].vod_name, result.list[0].vod_enname, result.list[0].vod_play_from, result.list[0].vod_remarks, result.list[0].vod_time, result.list[0].vod_actor, result.list[0].vod_content, result.list[0].vod_pic);
                    if (!string.IsNullOrEmpty(result.list[0].vod_play_url))
                    {
                        model.vod_play_url = result.list[0].vod_play_url;
                        var list = result.list[0].vod_play_url.Split('#');
                        foreach (var item in list)
                        {
                            var play_url = item.Split('$');
                            if (play_url.Length == 2)
                            {
                                model.AddVodPlay(play_url[0], play_url[1]);
                            }
                        }
                    }
                    return model;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return null;
    }

    public void Dispose()
    {
        _client.Dispose();
    }


    #region model
    public class zyk1080BaseResult
    {
        public int code { get; set; }
        public int limit { get; set; }
        public string msg { get; set; }
        public int page { get; set; }
        public int pagecount { get; set; }
        public int total { get; set; }
    }

    public class zyk1080ListResult : zyk1080BaseResult
    {
        // [JsonProperty("class")]
        [JsonPropertyName("class")]
        public List<VodTypeResult> vodtype { get; set; }

        public List<VodListResult> list { get; set; }

    }
    public class VodTypeResult
    {
        public string type_id { get; set; }
        public string type_name { get; set; }
    }
    public class VodListResult
    {
        /// <summary>
        /// 视频分类标识
        /// </summary>
        public string type_id { get; set; }//"12"
        /// <summary>
        /// 视频分类名称
        /// </summary>
        public string type_name { get; set; } //"国产剧"

        /// <summary>
        /// 视频标识
        /// </summary>
        public string vod_id { get; set; }//"40910"
        /// <summary>
        /// 视频名称
        /// </summary>
        public string vod_name { get; set; }//"云襄传"
        /// <summary>
        /// 视频英文名称
        /// </summary>
        public string vod_en { get; set; } //"yunxiangchuan"

        /// <summary>
        /// 视频来源
        /// </summary>
        public string vod_play_from { get; set; }//"1080zyk"

        /// <summary>
        /// 视频备注
        /// </summary>
        public string vod_remarks { get; set; } //"全36集"

        /// <summary>
        /// 更新时间
        /// </summary>
        public string vod_time { get; set; } //"2023-05-21 19:55:43"
    }


    public class zyk1080DetailResult : zyk1080BaseResult
    {
        public List<VodDetailResult> list { get; set; }
    }

    public class VodDetailResult
    {
        public string type_id { get; set; }//"6"
        public string type_name { get; set; }//"喜剧片"
        public string vod_actor { get; set; }//"许君聪,卜钰,包贝尔,于洋,德柏,张经纬,张泰维,杨山,贺军,陈秋伶,张美伦,马翼"
        public string vod_area { get; set; }//"大陆"
        public string vod_class { get; set; }//"悬疑"
        public string vod_color { get; set; }//""
        public string vod_content { get; set; }//"　　影片讲述了一起“女鬼”深夜伸冤索命，小镇百姓人心惶惶，小捕快郭幻（许君聪 饰）为查清案件真相，竟误抓了假扮女鬼的夏虫（卜钰 饰），还意外发现一起蹊跷血案。寡妇离奇失踪，尸体现身荒郊野外，幕后真凶势力猖狂，捕快郭幻能否查清幕后真相，将恶霸绳之以法。"
        public string vod_director { get; set; }//"浦宇"
        public string vod_down { get; set; }//"0"
        public string vod_down_from { get; set; }//"0"
        public string vod_down_note { get; set; }//"0"
        public string vod_down_server { get; set; }//"0"
        public string vod_down_url { get; set; }//"0"
        public string vod_duration { get; set; }//"82"
        public string vod_enname { get; set; }//"dahuashenbu"
        public string vod_hits { get; set; }//"51"
        public string vod_hits_day { get; set; }//"1"
        public string vod_hits_month { get; set; }//"1"
        public string vod_hits_week { get; set; }//"1"
        public string vod_id { get; set; }//"123"
        public int vod_isend { get; set; }//1
        public string vod_lang { get; set; }//"国语"
        public string vod_letter { get; set; }//"D"
        public string vod_level { get; set; }//"0"
        public string vod_lock { get; set; }//"0"
        public string vod_name { get; set; }//"大话神捕"
        public string vod_pic { get; set; }//"https://pic1.zykpic.com/upload/vod/2022-03-21/202203211647855806.jpg"
        public string vod_play_from { get; set; }//"1080zyk"
        public string vod_play_note { get; set; }//""
        public string vod_play_server { get; set; }//"no"
        public string vod_play_url { get; set; }//"HD国语版$https://cdn2.yzzy-tv-cdn.com/20220324/1234_a7dc4ee1/index.m3u8"
        public string vod_points_down { get; set; }//"0"
        public string vod_points_play { get; set; }//"0"
        public string vod_remarks { get; set; }//"HD国语版"
        public string vod_score { get; set; }//"3.6"
        public string vod_score_all { get; set; }//"0"
        public string vod_score_num { get; set; }//"0"
        public string vod_serial { get; set; }//"0"
        public string vod_sub { get; set; }//"暴走神捕"
        public string vod_tag { get; set; }//""
        public string vod_time { get; set; }//"2022-03-25 10:35:43"
        public string vod_up { get; set; }//"0"
        public string vod_year { get; set; }//"2021"
    }
    #endregion
}
