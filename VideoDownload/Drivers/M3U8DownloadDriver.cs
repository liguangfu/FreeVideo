#define LOG1
using CocoaAniCore.Downloaders.Enums;
using CocoaAniCore.Downloaders.Exceptions;
using System.Collections.Concurrent;
using System.Diagnostics;
using VideoDownload.Args;
using VideoDownload.Constants;
using VideoDownload.Drivers.Base;
using VideoDownload.Enum;
using VideoDownload.Enums;
using VideoDownload.Files;
using VideoDownload.Tasks;
using VideoDownload.Util;

#if LOG
using log4net;
#endif


namespace VideoDownload.Drivers;

public class M3U8DownloadDriver : HttpDownloadDriver,IHttpFileDownloadDriver
{
#if LOG
    private static readonly ILog Log = LogManager.GetLogger("VideoDownload");
#endif
    private static readonly HttpClient HttpClient = new HttpClient();
    public new string DriverName => DownloadFileType.M3U8.ToString();
    public new static M3U8DownloadDriver Instance = new M3U8DownloadDriver();
    protected M3U8DownloadDriver()
    {
    }
    public new async Task<HttpDownloadTask> ExecuteAsync(HttpDownloadTask task)
    {
        //获取是否为M3U8Task 
        var m3Task = (task.GetType() == typeof(M3U8DownloadTask)) ? (M3U8DownloadTask)task : null;
        //InitDo -> GetM3U8File -> Check Format
        if(!await ExecuteInitAsync(task))return task;
        //临时保存的文件夹路径
        var tempFileSaveDirPath = m3Task!.Args.IsSaveSubTs ?
            $"{task.Args.SavePath}-TS-LIST"
            : $"{DownloadConfig.TempDir}/{task.Args.TaskName}-TS";
        //创建临时目录
        Directory.CreateDirectory(tempFileSaveDirPath);
        var m3File = (M3U8File)task.Result.CustomResult;
        //创建子任务流字典
        var streamDict = new ConcurrentDictionary<int, Stream>();
        //分发子任务 启动子任务 
        if (false==DistributeTsFileAndStartSubTask(task, tempFileSaveDirPath, m3File,streamDict)) return task;
        //等待子任务结束 合并子任务结果
        if (false==await WaitSubTaskAndMergeTsFileAsync(task, streamDict)) return task;
        //合并结束任务成功
        task.Result.Progress = 1;
        //重置流位置到0
        task.Result.Stream!.Seek(0, SeekOrigin.Begin);
        //删除临时目录 使用主任务的路径的不删除
        if (tempFileSaveDirPath.StartsWith(DownloadConfig.TempDir))
        {
            Directory.Delete(tempFileSaveDirPath, true);
        }
        //输出日志
#if LOG
        Log.Info($"M3U8下载 完成 [{task.Args.TaskName}] -> " +
                 $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte   " +
                 $"保存路径：{task.Args.SavePath}");
#endif
        return task;
    }

    private async Task<bool> WaitSubTaskAndMergeTsFileAsync(HttpDownloadTask task, ConcurrentDictionary<int,Stream> streamDict)
    {
        //获取是否为M3U8Task 
        var m3Task = (task.GetType() == typeof(M3U8DownloadTask)) ? (M3U8DownloadTask)task : null;
        //遍历子任务收集结果
        for (var i = 0; i < task.Args.TaskCount; i++)
        {
            var subTask = task.Result.SubTaskDict[i];
            //等待子任务完成
            await subTask.WorkTask!;
            if (subTask.Result.IsError)
            {
                task.Result.Error = subTask.Result.Error;
                return false;
            }
            //是否需要合成TS
            if (m3Task?.Args.IsMergeTs == false) continue;
            //获取子任务下载TS的范围
            var range = (DownloadTsRange)subTask.Args.CustomArg;
            //遍历流 合并流
            for (var j = range.Start; j < range.End; j++)
            {
                if (!streamDict.ContainsKey(j))
                {
                    if (!subTask.Result.IsError)
                    {
                        throw new Exception();
                    }
                }
                var stream = streamDict[j];
                //子任务完成结果合并
                var readCount = 0;
                var buffer = new byte[DownloadConfig.DefaultDownloadBufferSize];
                while ((readCount = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await task.Result.Stream!.WriteAsync(buffer, 0, readCount);
                }
                //清空子任务的下载任务资源
                //关闭流
                stream.Close();
                //刷新流
                await task.Result.Stream!.FlushAsync();
#if LOG
                Log.Info($"M3U8下载 合并 [{task.Args.TaskName}] -> 任务进度：{task.Result.Progress:0.00}%  合并第 {j} 个TS");
#endif
            }
        }
        return true;
    }
    private bool DistributeTsFileAndStartSubTask(HttpDownloadTask task,string tempDir,M3U8File m3File,
        ConcurrentDictionary<int,Stream> streamDict)
    {
        var subTaskWorkCount = m3File.PlayerListCount / task.Args.TaskCount;
#if LOG
        Log.Info($"M3U8下载 开始 [{task.Args.TaskName}] -> 文件解析成功 Task总量：{task.Args.TaskCount} / TS总量：{m3File.PlayerListCount} / 平均TS量：{subTaskWorkCount} ");
#endif

        //分发任务
        var distributeTsCounter = 0;
        if (streamDict == null) throw new ArgumentNullException(nameof(streamDict));
        var oneTsProgressValue = 0.999f / m3File.PlayerListCount;
        for (var i = 0; i < task.Args.TaskCount; i++)
        {
            //创建子任务描述信息 设置下载范围 剩余的下载任务信息分给最后一个子任务
            var tsRange = new DownloadTsRange
            {
                Start = distributeTsCounter,
                End = i >= task.Args.TaskCount - 1 ?
                    m3File.PlayerListCount
                    : (distributeTsCounter += subTaskWorkCount)
            };
            tsRange.Pointer = tsRange.Start;
            //创建子任务
            var subTask = new HttpDownloadTask(new HttpDownloadArgs() { CustomArg = tsRange });
            //收集子任务到主任务的子任务字典
            task.Result.SubTaskDict[i] = subTask;
            //启动子任务 设置执行在修改器上
            subTask.Adjuster = (subTaskIterate) =>
            {
                //不是第一次进来就是 成功的完成了一个任务
                if (tsRange.Pointer != tsRange.Start) task.Result.Progress += oneTsProgressValue;
                //任务全部完成
                if (tsRange.Pointer >= tsRange.End) return false;
                //不可能发生
                if (task.Result.IsError) return false;
                //需要修改的任务内容
                subTaskIterate.Args.TaskName = $"{task.Args.TaskName}-M3U8-SUB-{tsRange.Pointer}";
                subTaskIterate.Args.SavePath = $"{tempDir}/{tsRange.Pointer}.ts";
                subTaskIterate.Args.Url = m3File.PlayerInfos[tsRange.Pointer].Link;
                subTaskIterate.Args.RangeEnd = 0;
                subTaskIterate.Args.RangeStart = 0;
                subTaskIterate.Args.ErrorRetries = task.Args.SubTaskErrorRetries;
                subTaskIterate.Result.Stream = null;
                subTaskIterate.Result.Progress = 0;
                subTaskIterate.Result.LastProgress = 0;
                subTaskIterate.Result.State = DownloadState.UnInit;
                subTaskIterate.Result.FileSize = 0;
                //初始化返回流
                if (!subTaskIterate.Result.InitStream()) return false;
                //收集流 
                if (subTaskIterate.Result.Stream != null) streamDict[tsRange.Pointer] = subTaskIterate.Result.Stream;
                //不可能为null
                else throw new FileDownloadException("subTaskIterate._result.Stream == null");
                //ts+1
                tsRange.Pointer++;
                return true;
            };
            var notUse = subTask.ExecuteAsync();
#if LOG
            Log.Info($"M3U8下载 分发 [{task.Args.TaskName}] -> 任务{i} TS范围：" +
                     $"{tsRange.Start} ~ {tsRange.End}");
#endif
        }
        return true;
    }

    private async Task<bool> ExecuteInitAsync(HttpDownloadTask task)
    {
        //获取是否为M3U8Task 
        var m3Task = (task.GetType() == typeof(M3U8DownloadTask)) ? (M3U8DownloadTask)task : null;
        //源文件下载
        var m3FileGetTask = new HttpDownloadTask(new HttpDownloadArgs() { Url = task.Args.Url });
        await m3FileGetTask.ExecuteAsync();
        if (m3FileGetTask.Result.State == DownloadState.Error)
        {
            Debug.Assert(m3FileGetTask.Result.Error != null, "m3U8FileTaskInfo.Error != null");
            task.Result.Error = new FileDownloadException(
                $"M3U8下载 错误 [{task.Args.TaskName}] -> 文件获取错误：{m3FileGetTask.Result.Error.Message}",
                m3FileGetTask.Result.Error);
            return false;
        }

        //格式检查
        var m3U8Content = m3FileGetTask.Result.ReadToString().Trim();
        if (m3U8Content.Contains("#EXT-X-STREAM-INF"))
        {
            //Logger.Warn(ResString.masterM3u8Found);
            m3U8Content = await ParseMasterListAsync(task.Args.Url,m3U8Content);
        }
        var m3U8 = new M3U8File(m3U8Content);

        if (m3U8.CheckResult.IsError)
        {
            Debug.Assert(m3U8.CheckResult.ErrorMessage != null, "m3U8.CheckResult.ErrorMessage != null");
            task.Result.Error = new FileDownloadException($"M3U8下载 错误 [{task.Args.TaskName}] " +
                                                          $"-> 文件格式错误：{m3U8.CheckResult.ErrorMessage}");
            return false;
        }
        //返回值初始化
        task.Result.CustomResult = m3U8;
        if (m3Task?.Result != null) m3Task.Result.M3U8File = m3U8;
        //如果不需要合成文件 就不初始化文件流
        if (m3Task != null && m3Task.Args.IsMergeTs == false)
        {
            task.Result.State = DownloadState.Start;
        }
        else if (!task.Result.InitStream())
        {
#if LOG
            Log.Error($"M3U8下载 错误 [{task.Args.TaskName}] -> 返回文件流初始化失败！ " +
                      $"{task.Result.Error?.Message}", task.Result.Error);
#endif
            return false;
        }
        return true;
    }


    private async Task<string> ParseMasterListAsync(string baseUrl,string m3U8Content)
    {
        using StringReader sr = new StringReader(m3U8Content);
        string? line;
        bool expectPlaylist = false;
        string url = string.Empty;

        while ((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            if (line.StartsWith(HLSTags.ext_x_stream_inf))
            {
                var bandwidth = string.IsNullOrEmpty(ParserUtil.GetAttribute(line, "AVERAGE-BANDWIDTH")) ? ParserUtil.GetAttribute(line, "BANDWIDTH") : ParserUtil.GetAttribute(line, "AVERAGE-BANDWIDTH");
                
                expectPlaylist = true;
            }
            else if (line.StartsWith(HLSTags.ext_x_media))
            {
                var type = ParserUtil.GetAttribute(line, "TYPE").Replace("-", "_");
                if (System.Enum.TryParse<MediaType>(type, out var mediaType))
                {
                    //跳过CLOSED_CAPTIONS类型（目前不支持）
                    if (mediaType == MediaType.CLOSED_CAPTIONS)
                    {
                        continue;
                    }
                }
                url = ParserUtil.GetAttribute(line, "URI");

                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                url = ParserUtil.CombineURL(baseUrl, url);
                
            }
            else if (line.StartsWith("#"))
            {
                continue;
            }
            else if (expectPlaylist)
            {
                url = ParserUtil.CombineURL(baseUrl, line);
                expectPlaylist = false;
            }
        }
        if (!string.IsNullOrEmpty(url))
        {

            var m3FileGetTask = new HttpDownloadTask(new HttpDownloadArgs() { Url = url });
            await m3FileGetTask.ExecuteAsync();
            if (m3FileGetTask.Result.State == DownloadState.Error)
            {
                Debug.Assert(m3FileGetTask.Result.Error != null, "m3U8FileTaskInfo.Error != null");
            }
            else
            {
                m3U8Content = m3FileGetTask.Result.ReadToString().Trim();
            }

        }
        return m3U8Content;
    }

}
struct DownloadTsRange
{
    public int Pointer;
    public int Start;
    public int End;
};