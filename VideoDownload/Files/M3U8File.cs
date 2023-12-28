using System.Diagnostics;
using System.Text;
using VideoDownload.Exceptions;
using VideoDownload.Results;

namespace VideoDownload.Files;

public class M3U8File
{
    public readonly string Version = string.Empty;
    public readonly string MediaSequence = string.Empty;
    public readonly string AllowCache = string.Empty;
    public readonly string TargetDuration = string.Empty;
    public readonly int PlayerListCount = 0;
    public readonly string Content;
    public int ErrorCode;
    public readonly IList<M3U8FilePlayerInfo> PlayerInfos = new List<M3U8FilePlayerInfo>();
    public FileCheckResult CheckResult { get;}

    public M3U8File(string content)
    {
        Content = content;
        int versionIdx = -1, 
            mediaSequenceIdx = -1,
            allowCacheIdx = -1, 
            targetDurationIdx = -1, 
            startIdx=-1,
            endListIdx = -1, 
            firstPlayerInfoTdx = -1;
        if ((   -1 == (startIdx=Content.IndexOf("#EXTM3U", StringComparison.Ordinal))
             | -1 == (versionIdx = Content.IndexOf("#EXT-X-VERSION:", StringComparison.Ordinal))
             | -1 == (mediaSequenceIdx = Content.IndexOf("#EXT-X-MEDIA-SEQUENCE:", StringComparison.Ordinal))
            // | -1 == (allowCacheIdx = Content.IndexOf("#EXT-X-ALLOW-CACHE:", StringComparison.Ordinal))
             | -1 == (targetDurationIdx = Content.IndexOf("#EXT-X-TARGETDURATION:", StringComparison.Ordinal))
             | -1 == (firstPlayerInfoTdx = Content.IndexOf("#EXTINF:", StringComparison.Ordinal))
             | -1 == (endListIdx = Content.IndexOf("#EXT-X-ENDLIST", StringComparison.Ordinal))))
        {
            CheckResult = new FileCheckResult(
                "M3U8文件格式错误！索引信息->" +
                             $"\n\t#EXTM3U {startIdx}" +
                             $"\n\t#EXT-X-VERSION: {versionIdx}" +
                             $"\n\t#EXT-X-MEDIA-SEQUENCE: {mediaSequenceIdx}" +
                             $"\n\t#EXT-X-ALLOW-CACHE: {allowCacheIdx}" +
                             $"\n\t#EXT-X-TARGETDURATION: {targetDurationIdx}" +
                             $"\n\t#EXTINF: {firstPlayerInfoTdx}" +
                             $"\n\t#EXT-X-ENDLIST {endListIdx}\n\n\t以下是完整文件内容----------------->\n{this.Content}");
            return;
        }
        Version = Content[versionIdx..Content.IndexOf('\n', versionIdx)];
        MediaSequence = Content[mediaSequenceIdx..Content.IndexOf('\n', mediaSequenceIdx)];
        //AllowCache = Content[allowCacheIdx..Content.IndexOf('\n', allowCacheIdx)];
        TargetDuration = Content[targetDurationIdx..Content.IndexOf('\n', targetDurationIdx)];
        var m3U8PlayerList = Content[(firstPlayerInfoTdx + "#EXTINF:".Length)..endListIdx].Split("#EXTINF:");
        
        foreach (var item in m3U8PlayerList)
        {
            var info = item.Split('\n');
            if (info.Length != 3)
            {
                var sb = new StringBuilder();
                foreach (var s in info)
                {
                    sb.Append(s);
                }
                CheckResult = new FileCheckResult($"M3U8播放列表解析错误! ->\n\t{sb}");
                return;
            }
            PlayerInfos.Add(new M3U8FilePlayerInfo(info[0], info[1]));
        }
        PlayerListCount = PlayerInfos.Count;
        CheckResult = new FileCheckResult();
    }
}

public partial struct M3U8FilePlayerInfo
{
    public readonly string Info;
    public readonly string Link;

    public M3U8FilePlayerInfo(string info, string link)
    {
        Info = info;
        Link = link;
    }
}