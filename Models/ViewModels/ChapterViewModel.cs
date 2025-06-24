namespace DrakimaWebsite.Models.ViewModels
{
    public class ChapterViewModel
    {
        public int ChapterNumber { get; set; }
        public string? Title { get; set; }
        public string? HtmlContent { get; set; }
        public List<string>? Sections { get; set; }
        public List<string>? ImagePaths { get; set; }
    }
}