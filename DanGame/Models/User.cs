using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class User 
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    //public string Status { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdateAt { get; set; }

    public virtual ICollection<ArticalCommentLike> ArticalCommentLikes { get; set; } = new List<ArticalCommentLike>();

    public virtual ICollection<ArticalCommentReply> ArticalCommentReplies { get; set; } = new List<ArticalCommentReply>();

    public virtual ICollection<ArticalComment> ArticalComments { get; set; } = new List<ArticalComment>();

    public virtual ICollection<ArticalLike> ArticalLikes { get; set; } = new List<ArticalLike>();

    public virtual ICollection<ArticalView> ArticalViews { get; set; } = new List<ArticalView>();

    public virtual ICollection<ArticleList> ArticleLists { get; set; } = new List<ArticleList>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatRoomMember> ChatRoomMembers { get; set; } = new List<ChatRoomMember>();

    public virtual ICollection<CreditCardInfo> CreditCardInfos { get; set; } = new List<CreditCardInfo>();

    public virtual ICollection<Friendship> FriendshipFriendUsers { get; set; } = new List<Friendship>();

    public virtual ICollection<Friendship> FriendshipUsers { get; set; } = new List<Friendship>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual UserProfile? UserProfile { get; set; }
}
