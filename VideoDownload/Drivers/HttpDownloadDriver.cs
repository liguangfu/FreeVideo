#define LOG1 //日志代码开关
#define HWR //RequestUseHttpWebRequest=> HWR / RequestUseHttpClient => HC

using CocoaAniCore.Downloaders.Enums;
using CocoaAniCore.Downloaders.Exceptions;
using VideoDownload.Args;
using VideoDownload.Drivers.Base;
using VideoDownload.Enums;
using VideoDownload.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

#if LOG
using log4net;
using log4net.Core;
#endif

namespace VideoDownload.Drivers;

public class HttpDownloadDriver : IHttpFileDownloadDriver
{
#if LOG
    private static readonly ILog Log = LogManager.GetLogger("VideoDownload");
#endif
    public static HttpDownloadDriver Instance = new HttpDownloadDriver();
    public static string DriverName => DownloadFileType.Default.ToString();

    public async Task<HttpDownloadTask> ExecuteAsync(HttpDownloadTask task)
    {
        //多任务下载跳转
        if (task.Args.TaskCount > 1) return await ExecuteMultipleTaskAsync(task);
#if HC
        HttpResponseMessage? response = null;
#endif
#if HWR
        HttpWebResponse? response = null;
#endif
        try
        {
            //执行下载初始化 可能出现连接异常 => 抛给try catch 直接本次结束下载
            response = await ExecuteDownloadInitAsync(task);
            //初始化任务保存流 可能会出现异常 => 封装到task.Result.Error 中 并返回是否初始化成功
            if (!task.Result.InitStream()) return task;
#if HC //获取流
            var stream = await response!.Content.ReadAsStreamAsync();
#endif
#if HWR //获取流
            var stream = response.GetResponseStream();
#endif
#if LOG
            Log.Debug($"http下载 开始 [{task.Args.TaskName}] -> " +
                      $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte   " +
                      $"范围：{task.Args.RangeStart} ~ {task.Args.RangeEnd}");
#endif
            //执行下载 可能会出现异常 => 抛给try catch 直接本次结束下载
            await ExecuteDownloadAsync(task, stream);
            //执行下载 结果检查 可能会出现异常 => 封装到task.Result.Error 中 并返回是否初检查成功
            if (ExecuteDownloadResultCheck(task) == false) return task;
            //刷新下载任务保存流
            await task.Result.Stream!.FlushAsync();
            //重置流坐标 以便后续读
            task.Result.Stream.Seek(0, SeekOrigin.Begin);
            //任务全部完成 设置进度为100%
            task.Result.Progress = 1;
            return task;
        }
        catch (Exception e)
        {
            task.Result.Error = new FileDownloadException($"http下载 失败 [{task.Args.TaskName}] -> {e.Message}", e);
            return task;
        }
        finally
        {
            //重试错误
            if (task.Result.IsError)
            {
#if LOG
                //唯一的错误打印通道 
                Log.Error($"Http下载 错误 [{task.Args.TaskName}] -> 第{task.Result.ErrorRetriesCounter+1}次失败! {task.Result.Error!.Message}");
#endif
                if (task.Result.ErrorRetriesCounter < task.Args.ErrorRetries)
                {
                    task.Result.ErrorRetriesCounter++;
                    task.Result.Error = null;
                    task.Result.State = DownloadState.Start;
                    //递归调用 直到重试次数达到设定值
                    await ExecuteAsync(task);
                }
            }
#if HWR
            response?.Close();
#endif
        }
    }

    public async Task<HttpDownloadTask> ExecuteMultipleTaskAsync(HttpDownloadTask task)
    {
#if HC
        HttpResponseMessage? response = null;
#endif
#if HWR
        HttpWebResponse? response = null;
#endif
        try
        {
            //建立连接
#if LOG
            Log.Info($"Http多任务下载 初始化 [{task.Args.TaskName}] -> 建立连接中... 子任务数量：{task.Args.TaskCount}");
#endif
            //执行下载初始化 可能出现连接异常 => 抛给try catch 直接本次结束下载
            response = await ExecuteDownloadInitAsync(task);
            //初始化任务保存流 可能会出现异常 => 封装到task.Result.Error 中 并返回是否初始化成功
            if (!task.Result.InitStream()) return task;
            //#if HC
            //            var stream = await response!.Content.ReadAsStreamAsync();
            //#endif
            //#if HWR
            //            var stream = response.GetResponseStream();
            //#endif
#if LOG
            Log.Info($"Http多任务下载 初始化 [{task.Args.TaskName}] -> 连接成功了!   子任务数量：{task.Args.TaskCount} " +
                     $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte ");
#endif
            //判断是否符合多线程下载 如果响应没有ContentLength，转换为单线程下载
            if (task.Result.FileSize <= 0)
            {
                task.Args.TaskCount = 1;
#if LOG
                Log.Warn($"Http多任务下载 警告 [{task.Args.TaskName}] -> 当前文件未返回文件长度,无法任务下载，已转换为单任务程下载!  URL: {task.Args.Url}");
#endif
                await ExecuteAsync(task);
                return task;
            }

            //初始化任务保存流 可能会出现异常 => 封装到task.Result.Error 中 并返回是否初始化成功
            if (!task.Result.InitStream()) return task;

            //获取临时目录路径
            var tempFileSaveDirPath = task.Args.SavePath != DownloadConfig.SaveOnMemory
                ? $"{task.Args.SavePath}.download" //任务返回流 是保存在本地的 临时目录
                : $"{DownloadConfig.TempDir}/{task.Args.TaskName}"; //任务返回流 是保存在内存的 临时目录
            //创建临时目录
            Directory.CreateDirectory(tempFileSaveDirPath);
            //分发子任务
            if (MultipleTaskDistributeAndStartSubTask(task, tempFileSaveDirPath) == false)
            {
                return task;
            }

            //合并文件
            if (await MultipleTaskMergeFileAsync(task) == false)
            {
                return task;
            }

            //刷新文件流
            await task.Result.Stream!.FlushAsync();
            //设置文件流到 0
            task.Result.Stream.Seek(0, SeekOrigin.Begin);
            //删除临时目录
            if (Directory.Exists(tempFileSaveDirPath))
            {
                Directory.Delete(tempFileSaveDirPath, true);
            }
#if LOG
            Log.Info($"Http多任务下载 成功[{task.Args.TaskName}] -> " +
                     $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte ");
#endif
            return task;
        }
        catch (Exception e)
        {
            task.Result.Error = new FileDownloadException(e.Message, e.InnerException!);
            return task;
        }
        finally
        {
            // 错误重试
            if (task.Result.IsError)
            {
#if LOG
                //唯一的错误打印通道 
                Log.Error($"Http多任务下载 错误 [{task.Args.TaskName}] -> 第{task.Result.ErrorRetriesCounter + 1}次失败! {task.Result.Error!.Message}");
#endif
                if (task.Result.ErrorRetriesCounter < task.Args.ErrorRetries)
                {
                    task.Result.ErrorRetriesCounter++;
                    task.Result.Error = null;
                    //递归调用 直到重试次数达到设定值
                    await ExecuteAsync(task);
                }
            }
#if HWR
            response?.Close();
#endif
        }
    }

    protected bool ExecuteDownloadResultCheck(HttpDownloadTask task)
    {
        //检验下载大小
        if (task.Result.FileSize == -1 || task.Result.FileSize == task.Result.Stream!.Length)
            return true;


        task.Result.Error = new FileDownloadException(
            $"http下载 错误 [{task.Args.TaskName}] -> 下载大小与描述不一致！" +
            $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte   " +
            $"应下：{task.Result.FileSize} 已下：{task.Result.Stream.Length}" +
            $"范围：{task.Args.RangeStart} ~ {task.Args.RangeEnd} ");
        return false;
#if LOG
        Log.Info($"http下载 成功 [{task.Args.TaskName}] -> " +
                 $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte   " +
                 $"范围：{task.Args.RangeStart * 1.0 / DownloadConfig.MB:0.000} ~ {task.Args.RangeEnd * 1.0 / DownloadConfig.MB:0.000}");
#endif
    }

    protected async Task<HttpDownloadTask> ExecuteDownloadAsync(HttpDownloadTask task, Stream stream)
    {
        //进度更新最小分辨率
        var maxInternalProgressUpdateCounter =
            Convert.ToInt64(task.Result.FileSize * DownloadConfig.ProgressUpdateAcquisitionResolution);
        //内部进度更新阈值计数器 Byte为单位
        long internalProgressCounter = 0;
        //实际更新阈值计数器 百分比为单位 默认扩大100倍 100% = 100f
        float taskProgressCounter = 0;
        //流中单次读取大小
        var readSize = 0;
        //缓冲区
        var buffer = new byte[DownloadConfig.DefaultDownloadBufferSize];
        while ((readSize = await stream.ReadAsync(buffer, 0, DownloadConfig.DefaultDownloadBufferSize)) != 0)
        {
            //写入主任务流
            await task.Result.Stream!.WriteAsync(buffer, 0, readSize);
            //下载进度更新控制 内部进度更新阈值计数器+读取大小
            if ((internalProgressCounter += readSize) < maxInternalProgressUpdateCounter) continue;
            //计数器溢出次数
            var internalProgressCounterOutOfCount =
                (float)(internalProgressCounter * 1.0 / maxInternalProgressUpdateCounter);
            //重置内部计数器
            internalProgressCounter = 0;
            //更新外部进度更新阈值 += ?% * 溢出次数
            taskProgressCounter +=
                DownloadConfig.ProgressUpdateAcquisitionResolution * internalProgressCounterOutOfCount;
            //检测是否达到外部更新阈值
            if (!(taskProgressCounter >= task.Args.ProgressNotifyThreshold)) continue;
            //溢出次数
            var taskProgressCounterOutOfCount = taskProgressCounter / task.Args.ProgressNotifyThreshold;
            //计数器溢出重置
            taskProgressCounter = 0;
            //更新进度
            task.Result.Progress += task.Args.ProgressNotifyThreshold * taskProgressCounterOutOfCount;
#if LOG
            Log.Debug($"http下载 进行 [{task.Args.TaskName}] -> [{task.Result.Progress * 100:00.00}%] " +
                      $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB ");// {task.Result.FileSize} Byte 
            //$"范围：{task.Args.RangeStart * 1.0 / DownloadConfig.MB:0.00}~{task.Args.RangeEnd * 1.0 / DownloadConfig.MB:0.00} MB / " +);
#endif
        }

        return task;
    }



    protected bool MultipleTaskDistributeAndStartSubTask(HttpDownloadTask task, string tempFileSaveDirPath)
    {
        //计算子任务下载大小和范围
        var subTaskWorkSize = task.Result.FileSize / task.Args.TaskCount;
        //创建子下载任务 收集子任务的返回值
        //下载范围指针 [0 -> ContentLength ] 多任务下载断点下载恢复不可用此方法
        long downloadRangePointer = -1;
#if LOG
        Log.Info($"Http多任务下载 初始化 [{task.Args.TaskName}] -> 分配子任务中...  " +
                 $"子任务数量：{task.Args.TaskCount} " +
                 $"文件总大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte");
#endif
        //分配子任务
        for (var i = 0; i < task.Args.TaskCount; i++)
        {
            var subTask = new HttpDownloadTask(new HttpDownloadArgs()
            {
                TaskName = $"{task.Args.TaskName}::{DownloadConfig.SubTaskPrefixName}-{i:00}",
                SavePath =
                    $"{tempFileSaveDirPath}/{DownloadConfig.SubTaskPrefixName}-{i:00}.{DownloadConfig.TempFileSuffixName}",
                Url = task.Args.Url,
                //设置子任务通知主任务的进度更新的进度阈值
                ProgressNotifyThreshold = DownloadConfig.DefaultSubTaskProgressNotifyParentThreshold,
                ErrorRetries = task.Args.SubTaskErrorRetries,
                RangeStart = ++downloadRangePointer,
                RangeEnd = i >= (task.Args.TaskCount - 1)
                    ? task.Result.FileSize - 1 //文件大小-1
                    : (downloadRangePointer += subTaskWorkSize) //指针+子任务工作大小
            });
            //添加子任务到主任务的子任务字典
            task.Result.SubTaskDict[i] = subTask;
            subTask.Result.FileSize = (subTask.Args.RangeEnd - subTask.Args.RangeStart) + 1;
            if (!subTask.Result.InitStream()) //子任务返回流初始化
            {
                task.Result.Error = subTask.Result.Error;
                return false;
            }

            //设置子任务的进度更新事件关联主任务
            subTask.Args.ProgressUpdate += (o, e) =>
            {
                task.Result.Progress += // +=(子任务的进度差 / 子任务数)
                    (e.DownloadResult.Progress - e.DownloadResult.LastProgress) / task.Args.TaskCount;
            };
#if LOG
            Log.Info($"Http多任务下载 初始化 [{subTask.Args.TaskName}] -> 分配子任务成功！ " +
                     $"大小：{subTask.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {subTask.Result.FileSize} Byte   " +
                     $"范围：{subTask.Args.RangeStart} ~ {subTask.Args.RangeEnd}");
#endif
            //启动子任务
            var notUse = subTask.ExecuteAsync();
        }
#if LOG
        Log.Info($"Http多任务下载 开始 [{task.Args.TaskName}] -> 所有子任分配成功并启动！ 子任务数量：{task.Args.TaskCount} " +
                 $"大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB / {task.Result.FileSize} Byte ");
#endif
        return true;

    }

    protected async Task<bool> MultipleTaskMergeFileAsync(HttpDownloadTask task)
    {
        //合并文件
        var buffer = new byte[DownloadConfig.DefaultDownloadBufferSize];
        for (var i = 0; i < task.Args.TaskCount; i++)
        {
            //从主任务的子任务字典中取出子任务
            var subTask = task.Result.SubTaskDict[i];
            //按顺序等子任务完成
            await subTask.WorkTask!;
            //子任务结果检查
            if (subTask.Result.State == DownloadState.Error)
            {
                task.Result.Error = new FileDownloadException(
                    $"Http多任务下载 合并 [{task.Args.TaskName}]+=[{subTask.Args.TaskName}] 失败 -> 下载发生错误",
                    subTask.Result.Error!);
                return false;
            }

            var readSize = 0;

            while ((readSize = await subTask.Result.Stream!.ReadAsync
                       (buffer, 0, DownloadConfig.DefaultDownloadBufferSize)) != 0)
            {
                await task.Result.Stream!.WriteAsync(buffer, 0, readSize);
            }
#if LOG
            Log.Info(
                $"Http多任务下载 合并 [{task.Args.TaskName}] += [{subTask.Args.TaskName}] 成功 -> 下载进度:[{task.Result.Progress:00.00}%] " +
                $"子文件大小：{subTask.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB  " +
                $"主文件大小：{task.Result.FileSize * 1.0 / DownloadConfig.MB:0.00} MB ");
#endif
            subTask.Result.Stream.Close();
            await task.Result.Stream!.FlushAsync();
        }

        return true;
    }

#if HWR //HttpWebRequest 处理下载请求
#pragma warning disable SYSLIB0014
    protected async Task<HttpWebResponse> ExecuteDownloadInitAsync(HttpDownloadTask task)
    {
        // ReSharper disable once AccessToStaticMemberViaDerivedType
        var request = (HttpWebRequest)HttpWebRequest.Create(task.Args.Url);
        if (task.Args.RangeEnd != 0)
            request.AddRange(task.Args.RangeStart, task.Args.RangeEnd);
        var response = (HttpWebResponse)await request.GetResponseAsync();
        //var stream = response.GetResponseStream();
        //初始化 非范围下载的下载大小
        if (task.Args.RangeEnd == 0)
            task.Args.RangeEnd = task.Result.FileSize = response.ContentLength;
        return response;
    }
#pragma warning restore SYSLIB0014
#endif
#if HC //HttpClient 处理下载请求 -不好用有Content缓存，太慢了，进度半天不更新
    protected async Task<HttpResponseMessage?> ExecuteDownloadInitAsync(HttpDownloadTask task)
    {
        var client = DownloadConfig.DefaultHttpClient;
        if (task.Args.RangeEnd != 0)
            client.DefaultRequestHeaders.Range = new RangeHeaderValue(task.Args.RangeStart, task.Args.RangeEnd);
        var response = await client.GetAsync(task.Args.Url);
        //初始化 非范围下载的下载大小
        if (task.Args.RangeEnd == 0)
            task.Args.RangeEnd = task.Result.FileSize = response.Content.Headers.ContentLength ?? 0;
        return response;
    }
#endif
}
