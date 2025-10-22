using System;

namespace SutterAnalyticsApi.Models
{
    public class ImportJob
    {
        public int Id { get; set; }
        public string Token { get; set; } = Guid.NewGuid().ToString("N");
        public string? Source { get; set; }
        public string? DatasetKey { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? FileName { get; set; }
        public string? Mode { get; set; } // upsert | sync
        public string Status { get; set; } = "preview"; // preview | ready | completed | failed

        public int Total { get; set; }
        public int ToCreate { get; set; }
        public int ToUpdate { get; set; }
        public int Unchanged { get; set; }
        public int Conflicts { get; set; }
        public int Errors { get; set; }

        // Store a compact JSON of the preview rows and any conflicts to drive commit decisions
        public string PreviewJson { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}

