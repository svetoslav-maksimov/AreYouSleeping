using System;
using System.Collections.Generic;

namespace AreYouSleeping.Updater
{
    public record ReleaseDto
    {
        public string? Html_url { get; set; }
        public long? Id { get; set; }
        public string? Tag_name { get; set; }
        public DateTime? Published_at { get; set; }
        public List<ReleaseAssetDto>? Assets { get; set; }
    }

    public record ReleaseAssetDto
    {
        public string? Name { get; set; }
        public string? Content_type { get; set; }
        public string? Browser_download_url { get; set; }
    }
}
