namespace N_m3u8DL_RE.MAUI;

public class M3U8DownloadArgs
{
    public string Url { get; set; }

    public string SaveName { get; set; }

    public string SavePath { get; set; }

    public string TmpDir { get; set; }

    public int TaskCount { get; set; } = 3;

    public bool IsDelSubTs { get; set; } = true;

}
