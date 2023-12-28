#define LOG
using System.Net;
using VideoDownload.Drivers;
using VideoDownload.Drivers.Base;

namespace VideoDownload;
public class DownloadConfig
{
    public static string UseLoggerName = "VideoDownload";
    /// <summary>
    /// 全局默认使用的HttpClient 
    /// </summary>
    public static HttpClient DefaultHttpClient = GetDefaultHttpClient();
    /// <summary>
    /// 子任务前缀名
    /// </summary>
    public static string SubTaskPrefixName = "SUB";
    /// <summary>
    /// 临时文件后缀名
    /// </summary>
    public static string TempFileSuffixName = "cocoa";
    /// <summary>
    /// 子任务通知主任务进度更新 子任务需要达到的进度
    /// </summary>
    public static float DefaultSubTaskProgressNotifyParentThreshold = 0.05f;
    /// <summary>
    /// 进度更新最小分辨率
    /// </summary>
    public static float ProgressUpdateAcquisitionResolution = 0.005f;
    /// <summary>
    /// 默认进度更新事件触发阈值
    /// </summary>
    public static float DefaultProgressNotifyThreshold = 0.01f;
    /// <summary>
    /// 默认错误重试次数
    /// </summary>
    public static int DefaultErrorRetries = 3;
    /// <summary>
    /// 子任务默认错误重试次数
    /// </summary>
    public static int DefaultSubTaskErrorRetries = 3;


    /// <summary>
    /// 最大下载速度
    /// </summary>
    public static long MaxDownloadSpeed = long.MaxValue;

    /// <summary>
    /// 默认内存流大小
    /// </summary>
    public static int DefaultMemoryStreamSize = 102400;
    /// <summary>
    /// 默认下载任务数量
    /// </summary>
    public static int DefaultTaskCount = 1;
    /// <summary>
    /// 默认下载缓冲区大小
    /// </summary>
    public static int DefaultDownloadBufferSize = 104000;
    /// <summary>
    /// 默认的下载任务名称
    /// </summary>
    public static string DefaultTaskName = "DefaultTask";
    /// <summary>
    /// 默认的下载任务定制参数
    /// </summary>
    public static string DefaultCustomArg = "DefaultCustomArg";
    /// <summary>
    /// 默认下载使用的驱动
    /// </summary>
    public static IHttpFileDownloadDriver DefaultDriver = HttpDownloadDriver.Instance;
    /// <summary>
    /// 默认添加的Http请求头
    /// </summary>
    public static Dictionary<string, string> DefaultHttpRequestHeaders = GetDefaultHttpRequestHeaders();
    /// <summary>
    /// 默认的定制返回值
    /// </summary>
    public static object DefaultCustomResult = "DefaultCustomResult";
    /// <summary>
    /// 默认M3U8文件下载是否合成TS
    /// </summary>
    public static bool DefaultM3U8IsMergeTs = true;
    /// <summary>
    /// 默认M3U8是否保存TS片段
    /// </summary>
    public static bool DefaultM3U8IsSaveSubTs = false;
    /// <summary>
    /// 默认的临时下载文件存放路径
    /// </summary>
    public static string TempDir = FileSystem.Current.CacheDirectory + "/TmpDownload";

    /// <summary>
    /// 默认下载请求头创建器
    /// </summary>
    /// <returns>请求头字典</returns>
    private static Dictionary<string, string> GetDefaultHttpRequestHeaders()
    {
        var dict = new Dictionary<string, string>();
        dict.Add("Accept", "*/*");
        dict.Add("client-version", "1DS-Web-JS-3.2.4");
        return dict;
    }
    /// <summary>
    /// 配置默认的HttpClient的参数
    /// </summary>
    /// <returns>全局HttpClient</returns>
    private static HttpClient GetDefaultHttpClient()
    {
        var socketsHttpHandler = new SocketsHttpHandler()
        {
            MaxResponseHeadersLength = 64, //单位: KB
        };
        var httpClient = new HttpClient(socketsHttpHandler);
        //httpClient.MaxResponseContentBufferSize = 1024000000;
        //httpClient.DefaultRequestVersion = HttpVersion.Version20; ;
        return httpClient;
    }


    public static readonly int GB = 1024 * 1024 * 1024;
    public static readonly int MB = 1024 * 1024;
    public static readonly int KB = 1024;

    /// <summary>
    /// 下载文件保存路径为内存的路径标识
    /// </summary>
    public static readonly string SaveOnMemory = "SaveOnMemory";
    /// <summary>
    /// 未定义URL的值
    /// </summary>
    public static readonly string UndefinedUrl = "UndefinedUrl";

}