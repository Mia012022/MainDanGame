using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ArticalComment
{
    public int CommentId { get; set; }

    public int ArticalId { get; set; }

    public int UserId { get; set; }

    public int ParentOrChild { get; set; }

    public string CommentContent { get; set; } = null!;

    public DateTime CommentCreateDate { get; set; }

    public virtual ArticleList Artical { get; set; } = null!;

    public virtual ICollection<ArticalCommentLike> ArticalCommentLikes { get; set; } = new List<ArticalCommentLike>();

    public virtual ICollection<ArticalCommentReply> ArticalCommentReplies { get; set; } = new List<ArticalCommentReply>();

    public virtual User User { get; set; } = null!;
}
