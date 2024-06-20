using DanGame.Models;

namespace DanGame.Models
{
	public class UserArticlesViewModel
	{
		public User User { get; set; }
		public UserProfile UserProfile { get; set; }
		public List<ArticleList> Articles { get; set; }
		public List<ArticleList> LikedArticles { get; set; }

		public List<ArticalComment> Comments { get; set; }
		public List<ArticalLike> Likes { get; set; }
		public List<ArticalView> Views { get; set; }
		public List<ArticalCommentLike> CommentLikes { get; set; }

		public int TotalLikesCounts { get; set; }
		public Dictionary<int, int>? CommentCounts { get; set; }
		public Dictionary<int, int>? ReplyCounts { get; set; }
		public Dictionary<int, int>? TotalCounts { get; set; }
		public Dictionary<int, int>? LikeCounts { get; set; }
		public List<CommentWithArticleViewModel>? commentWithArticleViewModels { get; set; }	

	}
	public class CommentWithArticleViewModel
	{
		public int CommentId { get; set; }
		public string CommentContent { get; set; }
		public int ArticalId { get; set; }
		public string ArticalTitle { get; set; }
		public string ArticalContent { get; set; }
		public DateTime CommentDate { get; set; }
	}
}