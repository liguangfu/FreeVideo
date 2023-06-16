namespace FreeVideo.Models;

public class SearchVideoListModel
{
    public SearchVideoListModel() { }

    public SearchVideoListModel(string type_id, string type_name, string vod_id, string vod_name, string vod_en, string vod_play_from, string vod_remarks, string vod_time)
    {
        this.type_id = type_id;
        this.type_name = type_name;
        this.vod_id = vod_id;
        this.vod_name = vod_name;
        this.vod_en = vod_en;
        this.vod_play_from = vod_play_from;
        this.vod_remarks = vod_remarks;
        this.vod_time = vod_time;
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

    public  string vod_image { get; set; }

}
