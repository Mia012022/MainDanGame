using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ArticleList
{
    public int ArticalId { get; set; }

    public int UserId { get; set; }

    public string ArticalTitle { get; set; } = null!;

    public string ArticalContent { get; set; } = null!;

    public DateTime ArticalCreateDate { get; set; }

    public byte[] ArticalCoverPhoto { get; set; } = null!;

    public string? ArticleCategory { get; set; }

    public virtual ICollection<ArticalComment> ArticalComments { get; set; } = new List<ArticalComment>();

    public virtual ICollection<ArticalLike> ArticalLikes { get; set; } = new List<ArticalLike>();

    public virtual User User { get; set; } = null!;
}
