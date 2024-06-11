using HtmlAgilityPack;
using System.Text;

namespace DanGame.Helpers
{
	public class HtmlParser
	{
		public string ConcatenateParagraphContents(string html)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
			var concatenatedContent = new StringBuilder();

			if (paragraphs != null)
			{
				foreach (var paragraph in paragraphs)
				{

					concatenatedContent.Append(paragraph.InnerText.Trim());
					concatenatedContent.Append(" "); // Add space between paragraphs
				}
			}

			return concatenatedContent.ToString().Trim();
		}
	}
}