﻿using N_m3u8DL_RE.Common.Entity;
using N_m3u8DL_RE.Common.Enum;
using N_m3u8DL_RE.Parser.Config;

namespace N_m3u8DL_RE.Parser.Extractor;

public interface IExtractor
{
    ExtractorType ExtractorType { get; }

    ParserConfig ParserConfig { get; set; }

    Task<List<StreamSpec>> ExtractStreamsAsync(string rawText);

    Task FetchPlayListAsync(List<StreamSpec> streamSpecs);
    Task RefreshPlayListAsync(List<StreamSpec> streamSpecs);

    string PreProcessUrl(string url);

    void PreProcessContent();
}
