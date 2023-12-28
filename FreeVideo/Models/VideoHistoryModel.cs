using SQLite;

namespace FreeVideo.Models
{
    public class VideoHistoryModel
    {

        /// <summary>
        /// 视频标识
        /// </summary>
        [PrimaryKey]
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
        /// 
        /// </summary>
        public string vod_actor { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string vod_content { get; set; }
        /// <summary>
        /// 图集
        /// </summary>
        public string vod_pic { get; set; }
        /// <summary>
        /// 播放列表
        /// </summary>
        public string vod_play_url { get; set; }//"HD国语版$https://cdn2.yzzy-tv-cdn.com/20220324/1234_a7dc4ee1/index.m3u8"

        /// <summary>
        /// 当前播放，如：HD国语版
        /// </summary>
        public string current_play { get; set; }

        /// <summary>
        /// 当前播放位置
        /// </summary>
        public TimeSpan current_duration { get; set; }

        /// <summary>
        /// 最新观看时间
        /// </summary>
        public DateTime show_time { get; set; }
    }

}
