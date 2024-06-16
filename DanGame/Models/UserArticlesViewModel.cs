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
		
	}
}