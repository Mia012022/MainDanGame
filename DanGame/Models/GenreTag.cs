using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class GenreTag
{
    public int TagId { get; set; }

    public string TagName { get; set; } = null!;

    public virtual ICollection<App> Apps { get; set; } = new List<App>();
}
