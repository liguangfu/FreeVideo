using SQLite;

namespace FreeVideo.Models
{
    public class VideoPlayListModel
    {
        [PrimaryKey]
        public string play_id { get; set; }//"40910-1"

        public string vod_id { get; set; }//"40910"

        public string name { get; set; }

        public string url { get; set; }

        /// <summary>
        /// 本地地址
        /// </summary>
        public string local_path { get; set; }

        /// <summary>
        /// 当前播放位置
        /// </summary>
        public TimeSpan current_duration { get; set; }

        public string getPlayUrl()
        {
            if (!string.IsNullOrEmpty(local_path))
            {
                return local_path;
            }
            return url;
        }
    }
}
