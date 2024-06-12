
using X.PagedList;
namespace DanGame.Models
{
	public class PageListViewModel
	{
		public X.PagedList.IPagedList<ArticleList>? ArticleList { get; set; }
		public List<ArticalView>? ArticalViews { get; set; }

		public List<ArticalComment>? ArticalComments { get; set; }
		public List<TopUserViewModel>? TopUsers { get; set; }

		public List<PopularArticleViewModel> PopularArticle { get; set; }
		

	}
	public class PopularArticleViewModel
	{
		public int ArticalID { get; set; }
		public int Amount { get; set; }
		public string ArticalTitle { get; set; }
		public string ArticalContent { get; set; }
	}
	
}