using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class AppDetail
{
    public int AppId { get; set; }

    public string AppName { get; set; } = null!;

    public string AppType { get; set; } = null!;

    public string DevloperName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ShortDescription { get; set; } = null!;

    public DateOnly ReleaseDate { get; set; }

    public string SupportedLanguage { get; set; } = null!;

    public string HeaderImage { get; set; } = null!;

    public string BackgroundImage { get; set; } = null!;

    public string CapsuleImage { get; set; } = null!;

    public string Website { get; set; } = null!;

    public int Price { get; set; }

    public string Platform { get; set; } = null!;

    public int Downloaded { get; set; }

    public virtual App? App { get; set; } = null!;
}
