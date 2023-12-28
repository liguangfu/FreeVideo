using N_m3u8DL_RE.Parser.Config;
using N_m3u8DL_RE.Common.Entity;
using N_m3u8DL_RE.Common.Enum;
using N_m3u8DL_RE.Parser;
using N_m3u8DL_RE.Common.DLResource;
using N_m3u8DL_RE.Common.Log;
using N_m3u8DL_RE.Common.Util;
using N_m3u8DL_RE.Processor;
using N_m3u8DL_RE.Config;
using N_m3u8DL_RE.Util;
using N_m3u8DL_RE.DownloadManager;
using N_m3u8DL_RE.CommandLine;
using System.Text;

namespace N_m3u8DL_RE.MAUI;


public class M3U8DownloadTask
{
    public async Task<bool> DoWorkAsync(M3U8DownloadArgs args)
    {
        var option = new MyOption
        {
            Input = args.Url,
            SaveDir = args.SavePath,
            TmpDir = args.TmpDir,
            SaveName = args.SaveName,
            ThreadCount = args.TaskCount,
            DelAfterDone = args.IsDelSubTs,
            BinaryMerge=true,
        };
        Logger.IsWriteFile = !option.NoLog;
        Logger.InitLogFile();
        Logger.LogLevel = option.LogLevel;

        if (option.UseSystemProxy == false)
        {
            HTTPUtil.HttpClientHandler.UseProxy = false;
        }

        if (option.CustomProxy != null)
        {
            HTTPUtil.HttpClientHandler.Proxy = option.CustomProxy;
            HTTPUtil.HttpClientHandler.UseProxy = true;
        }

        //检查互斥的选项

        if (!option.MuxAfterDone && option.MuxImports != null && option.MuxImports.Count > 0)
        {
            throw new ArgumentException("MuxAfterDone disabled, MuxImports not allowed!");
        }

        //LivePipeMux开启时 LiveRealTimeMerge必须开启
        if (option.LivePipeMux && !option.LiveRealTimeMerge)
        {
            Logger.WarnMarkUp("LivePipeMux detected, forced enable LiveRealTimeMerge");
            option.LiveRealTimeMerge = true;
        }

        //预先检查ffmpeg
        if (option.FFmpegBinaryPath == null)
            option.FFmpegBinaryPath = GlobalUtil.FindExecutable("ffmpeg");

        if (string.IsNullOrEmpty(option.FFmpegBinaryPath) || !File.Exists(option.FFmpegBinaryPath))
        {
            throw new FileNotFoundException(ResString.ffmpegNotFound);
        }

        Logger.Extra($"ffmpeg => {option.FFmpegBinaryPath}");

        //预先检查mkvmerge
        if (option.MuxOptions != null && option.MuxOptions.UseMkvmerge && option.MuxAfterDone)
        {
            if (option.MkvmergeBinaryPath == null)
                option.MkvmergeBinaryPath = GlobalUtil.FindExecutable("mkvmerge");
            if (string.IsNullOrEmpty(option.MkvmergeBinaryPath) || !File.Exists(option.MkvmergeBinaryPath))
            {
                throw new FileNotFoundException("mkvmerge not found");
            }
            Logger.Extra($"mkvmerge => {option.MkvmergeBinaryPath}");
        }

        //预先检查
        if ((option.Keys != null && option.Keys.Length > 0) || option.KeyTextFile != null)
        {
            if (string.IsNullOrEmpty(option.DecryptionBinaryPath))
            {
                if (option.UseShakaPackager)
                {
                    var file = GlobalUtil.FindExecutable("shaka-packager");
                    var file2 = GlobalUtil.FindExecutable("packager-linux-x64");
                    var file3 = GlobalUtil.FindExecutable("packager-osx-x64");
                    var file4 = GlobalUtil.FindExecutable("packager-win-x64");
                    if (file == null && file2 == null && file3 == null && file4 == null) throw new FileNotFoundException("shaka-packager not found!");
                    option.DecryptionBinaryPath = file ?? file2 ?? file3 ?? file4;
                    Logger.Extra($"shaka-packager => {option.DecryptionBinaryPath}");
                }
                else
                {
                    var file = GlobalUtil.FindExecutable("mp4decrypt");
                    if (file == null) throw new FileNotFoundException("mp4decrypt not found!");
                    option.DecryptionBinaryPath = file;
                    Logger.Extra($"mp4decrypt => {option.DecryptionBinaryPath}");
                }
            }
            else if (!File.Exists(option.DecryptionBinaryPath))
            {
                throw new FileNotFoundException(option.DecryptionBinaryPath);
            }
        }

        //默认的headers
        var headers = new Dictionary<string, string>()
        {
            ["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36"
        };
        //添加或替换用户输入的headers
        foreach (var item in option.Headers)
        {
            headers[item.Key] = item.Value;
            Logger.Extra($"User-Defined Header => {item.Key}: {item.Value}");
        }

        var parserConfig = new ParserConfig()
        {
            AppendUrlParams = option.AppendUrlParams,
            UrlProcessorArgs = option.UrlProcessorArgs,
            BaseUrl = option.BaseUrl!,
            Headers = headers,
            CustomMethod = option.CustomHLSMethod,
            CustomeKey = option.CustomHLSKey,
            CustomeIV = option.CustomHLSIv,
        };

        //demo1
        parserConfig.ContentProcessors.Insert(0, new DemoProcessor());
        //demo2
        parserConfig.KeyProcessors.Insert(0, new DemoProcessor2());
        //for www.nowehoryzonty.pl
        parserConfig.UrlProcessors.Insert(0, new NowehoryzontyUrlProcessor());

        //等待任务开始时间
        if (option.TaskStartAt != null && option.TaskStartAt > DateTime.Now)
        {
            Logger.InfoMarkUp(ResString.taskStartAt + option.TaskStartAt);
            while (option.TaskStartAt > DateTime.Now)
            {
                await Task.Delay(1000);
            }
        }

        var url = option.Input;

        //流提取器配置
        var extractor = new StreamExtractor(parserConfig);
        await extractor.LoadSourceFromUrlAsync(url);

        //解析流信息
        var streams = await extractor.ExtractStreamsAsync();


        //全部媒体
        var lists = streams.OrderBy(p => p.MediaType).ThenByDescending(p => p.Bandwidth).ThenByDescending(GetOrder);
        //基本流
        var basicStreams = lists.Where(x => x.MediaType == null || x.MediaType == MediaType.VIDEO);
        //可选音频轨道
        var audios = lists.Where(x => x.MediaType == MediaType.AUDIO);
        //可选字幕轨道
        var subs = lists.Where(x => x.MediaType == MediaType.SUBTITLES);

        //尝试从URL或文件读取文件名
        if (string.IsNullOrEmpty(option.SaveName))
        {
            option.SaveName = OtherUtil.GetFileNameFromInput(option.Input);
        }

        //生成文件夹
        var tmpDir = Path.Combine(option.TmpDir ?? FileSystem.AppDataDirectory, $"{option.SaveName ?? DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}");
        //记录文件
        extractor.RawFiles["meta.json"] = GlobalUtil.ConvertToJson(lists);
        //写出文件
        await WriteRawFilesAsync(option, extractor, tmpDir);

        Logger.Info(ResString.streamsInfo, lists.Count(), basicStreams.Count(), audios.Count(), subs.Count());

        foreach (var item in lists)
        {
            Logger.InfoMarkUp(item.ToString());
        }

        var selectedStreams = new List<StreamSpec>();
        if (option.DropVideoFilter != null || option.DropAudioFilter != null || option.DropSubtitleFilter != null)
        {
            basicStreams = FilterUtil.DoFilterDrop(basicStreams, option.DropVideoFilter);
            audios = FilterUtil.DoFilterDrop(audios, option.DropAudioFilter);
            subs = FilterUtil.DoFilterDrop(subs, option.DropSubtitleFilter);
            lists = basicStreams.Concat(audios).Concat(subs).OrderBy(x => true);
        }

        if (option.DropVideoFilter != null) Logger.Extra($"DropVideoFilter => {option.DropVideoFilter}");
        if (option.DropAudioFilter != null) Logger.Extra($"DropAudioFilter => {option.DropAudioFilter}");
        if (option.DropSubtitleFilter != null) Logger.Extra($"DropSubtitleFilter => {option.DropSubtitleFilter}");
        if (option.VideoFilter != null) Logger.Extra($"VideoFilter => {option.VideoFilter}");
        if (option.AudioFilter != null) Logger.Extra($"AudioFilter => {option.AudioFilter}");
        if (option.SubtitleFilter != null) Logger.Extra($"SubtitleFilter => {option.SubtitleFilter}");

        if (option.AutoSelect)
        {
            if (basicStreams.Any())
                selectedStreams.Add(basicStreams.First());
            var langs = audios.DistinctBy(a => a.Language).Select(a => a.Language);
            foreach (var lang in langs)
            {
                selectedStreams.Add(audios.Where(a => a.Language == lang).OrderByDescending(a => a.Bandwidth).ThenByDescending(GetOrder).First());
            }
            selectedStreams.AddRange(subs);
        }
        else if (option.SubOnly)
        {
            selectedStreams.AddRange(subs);
        }
        else if (option.VideoFilter != null || option.AudioFilter != null || option.SubtitleFilter != null)
        {
            basicStreams = FilterUtil.DoFilterKeep(basicStreams, option.VideoFilter);
            audios = FilterUtil.DoFilterKeep(audios, option.AudioFilter);
            subs = FilterUtil.DoFilterKeep(subs, option.SubtitleFilter);
            selectedStreams = basicStreams.Concat(audios).Concat(subs).ToList();
        }
        else
        {
            //展示交互式选择框
            selectedStreams = FilterUtil.SelectStreams(lists);
        }

        if (!selectedStreams.Any())
            throw new Exception(ResString.noStreamsToDownload);

        //HLS: 选中流中若有没加载出playlist的，加载playlist
        //DASH/MSS: 加载playlist (调用url预处理器)
        if (selectedStreams.Any(s => s.Playlist == null) || extractor.ExtractorType == ExtractorType.MPEG_DASH || extractor.ExtractorType == ExtractorType.MSS)
            await extractor.FetchPlayListAsync(selectedStreams);

        //直播检测
        var livingFlag = selectedStreams.Any(s => s.Playlist?.IsLive == true) && !option.LivePerformAsVod;
        if (livingFlag)
        {
            Logger.WarnMarkUp($"[white on darkorange3_1]{ResString.liveFound}[/]");
        }

        //无法识别的加密方式，自动开启二进制合并
        if (selectedStreams.Any(s => s.Playlist.MediaParts.Any(p => p.MediaSegments.Any(m => m.EncryptInfo.Method == EncryptMethod.UNKNOWN))))
        {
            Logger.WarnMarkUp($"[darkorange3_1]{ResString.autoBinaryMerge3}[/]");
            option.BinaryMerge = true;
        }

        //应用用户自定义的分片范围
        if (!livingFlag)
            FilterUtil.ApplyCustomRange(selectedStreams, option.CustomRange);

        //应用用户自定义的广告分片关键字
        FilterUtil.CleanAd(selectedStreams, option.AdKeywords);

        //记录文件
        extractor.RawFiles["meta_selected.json"] = GlobalUtil.ConvertToJson(selectedStreams);

        Logger.Info(ResString.selectedStream);
        foreach (var item in selectedStreams)
        {
            Logger.InfoMarkUp(item.ToString());
        }

        //写出文件
        await WriteRawFilesAsync(option, extractor, tmpDir);

        if (option.SkipDownload)
        {
            return false;
        }


        Logger.InfoMarkUp(ResString.saveName + $"[deepskyblue1]{option.SaveName}[/]");

        //开始MuxAfterDone后自动使用二进制版
        if (!option.BinaryMerge && option.MuxAfterDone)
        {
            option.BinaryMerge = true;
            Logger.WarnMarkUp($"[darkorange3_1]{ResString.autoBinaryMerge6}[/]");
        }

        //下载配置
        var downloadConfig = new DownloaderConfig()
        {
            MyOptions = option,
            DirPrefix = tmpDir,
            Headers = parserConfig.Headers, //使用命令行解析得到的Headers
        };

        var result = false;

        if (extractor.ExtractorType == ExtractorType.HTTP_LIVE)
        {
            var sldm = new HTTPLiveRecordManager(downloadConfig, selectedStreams, extractor);
            result = await sldm.StartRecordAsync();
        }
        else if (!livingFlag)
        {
            //开始下载
            var sdm = new SimpleDownloadManager(downloadConfig, selectedStreams, extractor);
            result = await sdm.StartDownloadAsync();
        }
        else
        {
            var sldm = new SimpleLiveRecordManager2(downloadConfig, selectedStreams, extractor);
            result = await sldm.StartRecordAsync();
        }

        if (result)
        {
            Logger.InfoMarkUp("[white on green]Done[/]");
        }
        else
        {
            Logger.ErrorMarkUp("[white on red]Failed[/]");
        }
        return result;
    }

    private async Task WriteRawFilesAsync(MyOption option, StreamExtractor extractor, string tmpDir)
    {
        //写出json文件
        if (option.WriteMetaJson)
        {
            if (!Directory.Exists(tmpDir)) Directory.CreateDirectory(tmpDir);
            Logger.Warn(ResString.writeJson);
            foreach (var item in extractor.RawFiles)
            {
                var file = Path.Combine(tmpDir, item.Key);
                if (!File.Exists(file)) await File.WriteAllTextAsync(file, item.Value, Encoding.UTF8);
            }
        }
    }

    int GetOrder(StreamSpec streamSpec)
    {
        if (streamSpec.Channels == null) return 0;
        else
        {
            var str = streamSpec.Channels.Split('/')[0];
            return int.TryParse(str, out var order) ? order : 0;
        }
    }
}
