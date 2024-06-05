using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ArticalCommentLike
{
    public int CommentLikeId { get; set; }

    public int CommentId { get; set; }

    public int UserId { get; set; }

    public virtual ArticalComment Comment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
