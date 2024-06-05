using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ArticalLike
{
    public int ArticalLikeId { get; set; }

    public int ArticalId { get; set; }

    public int UserId { get; set; }

    public virtual ArticleList Artical { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
