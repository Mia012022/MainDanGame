using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class AppMedium
{
    public int MediaId { get; set; }

    public int AppId { get; set; }

    public string MediaType { get; set; } = null!;

    public string? Title { get; set; }

    public string ThumbnailPath { get; set; } = null!;

    public string OriginalPath { get; set; } = null!;

    public virtual App App { get; set; } = null!;
}
