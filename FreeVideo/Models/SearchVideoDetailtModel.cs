namespace FreeVideo.Models;

public class SearchVideoDetailtModel
{
    public SearchVideoDetailtModel(string type_id, string type_name, string vod_id, string vod_name, string vod_en, string vod_play_from, string vod_remarks, string vod_time, string vod_actor, string vod_content, string vod_pic)
    {
        this.type_id = type_id;
        this.type_name = type_name;
        this.vod_id = vod_id;
        this.vod_name = vod_name;
        this.vod_en = vod_en;
        this.vod_play_from = vod_play_from;
        this.vod_remarks = vod_remarks;
        this.vod_time = vod_time;
        this.vod_actor = vod_actor;
        this.vod_content = vod_content;
        this.vod_pic = vod_pic;
        this.vod_play_list = new List<VideoPlayListModel>();
    }


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

    public string vod_actor { get; set; }
    public string vod_content { get; set; }//"　　影片讲述了一起“女鬼”深夜伸冤索命，小镇百姓人心惶惶，小捕快郭幻（许君聪 饰）为查清案件真相，竟误抓了假扮女鬼的夏虫（卜钰 饰），还意外发现一起蹊跷血案。寡妇离奇失踪，尸体现身荒郊野外，幕后真凶势力猖狂，捕快郭幻能否查清幕后真相，将恶霸绳之以法。"

    public string vod_pic { get; set; }//"https://pic1.zykpic.com/upload/vod/2022-03-21/202203211647855806.jpg"
    /// <summary>
    /// 播放列表
    /// </summary>
    public string vod_play_url { get; set; }//"HD国语版$https://cdn2.yzzy-tv-cdn.com/20220324/1234_a7dc4ee1/index.m3u8"
    public List<VideoPlayListModel> vod_play_list { get; set; }//"HD国语版$https://cdn2.yzzy-tv-cdn.com/20220324/1234_a7dc4ee1/index.m3u8"

    public void AddVodPlay(string key, string value)
    {
        vod_play_list.Add(new VideoPlayListModel() { name = key, url = value });
    }
}
