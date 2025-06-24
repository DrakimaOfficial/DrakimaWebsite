using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DrakimaWebsite.Models.ViewModels;
using Markdig;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DrakimaWebsite.Controllers
{
    [Authorize]
    public class NovelController : Controller
    {
        private readonly string _novelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/novels");

        [HttpGet]
        public IActionResult Read(string novelId = "AGynoidsDream", int? chapter = 1)
        {
            int chapterNumber = chapter ?? 1;
            string novelPath = Path.Combine(_novelPath, novelId);
            string markdownFile = Path.Combine(novelPath, $"chapter{chapterNumber:D2}.md");

            if (!System.IO.File.Exists(markdownFile))
            {
                return NotFound("Chapter not found.");
            }

            string markdownContent = System.IO.File.ReadAllText(markdownFile, System.Text.Encoding.UTF8);
            string title = "Chapter " + chapterNumber;
            var firstLine = markdownContent.Split('\n').FirstOrDefault(l => l.StartsWith("# "));
            if (firstLine != null) title = firstLine.TrimStart('#').Trim();

            var sections = Regex.Split(markdownContent, @"(?=^## )", RegexOptions.Multiline)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            var sectionContents = new List<string>();
            var imagePaths = new List<string>();

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            for (int i = 0; i < sections.Count; i++)
            {
                // Render markdown to HTML, preserving <img> tags
                string htmlContent = Markdig.Markdown.ToHtml(sections[i], pipeline);
                sectionContents.Add(htmlContent);

                // Check for hardcoded <img> tags in the markdown
                var imageMatch = Regex.Match(sections[i], @"<img\s+src=""([^""]+)""\s*/>");
                if (imageMatch.Success)
                {
                    string imagePath = imageMatch.Groups[1].Value.Replace("~/", "/");
                    string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        imagePaths.Add(imagePath);
                    }
                    else
                    {
                        imagePaths.Add(string.Empty);
                        System.Diagnostics.Debug.WriteLine($"Hardcoded image not found: {physicalPath}");
                    }
                }
                else if (sections[i].StartsWith("## "))
                {
                    // Try section{i} (old working convention) and fallback to section{i + 1}
                    string imagePath = $"/images/chapters/chapter{chapterNumber}-section{i}.jpg";
                    string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chapters", $"chapter{chapterNumber}-section{i}.jpg");
                    if (!System.IO.File.Exists(physicalPath))
                    {
                        imagePath = $"/images/chapters/chapter{chapterNumber}-section{i + 1}.jpg";
                        physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chapters", $"chapter{chapterNumber}-section{i + 1}.jpg");
                    }
                    if (System.IO.File.Exists(physicalPath))
                    {
                        imagePaths.Add(imagePath);
                    }
                    else
                    {
                        imagePaths.Add(string.Empty);
                        System.Diagnostics.Debug.WriteLine($"Image not found: {physicalPath}");
                    }
                }
                else
                {
                    imagePaths.Add(string.Empty);
                }
            }

            var model = new ChapterViewModel
            {
                ChapterNumber = chapterNumber,
                Title = title,
                HtmlContent = string.Join("", sectionContents),
                Sections = sectionContents,
                ImagePaths = imagePaths
            };

            bool hasNextChapter = System.IO.File.Exists(Path.Combine(novelPath, $"chapter{(chapterNumber + 1):D2}.md"));
            ViewBag.HasNextChapter = hasNextChapter;
            ViewBag.NovelId = novelId;

            return View(model);
        }

        [HttpGet]
        public IActionResult TableOfContents(string novelId = "AGynoidsDream")
        {
            var novelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/novels", novelId);
            var chapters = Directory.GetFiles(novelPath, "chapter*.md")
                .Select(f => int.TryParse(Path.GetFileNameWithoutExtension(f).Replace("chapter", ""), out int num) ? num : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();
            ViewBag.NovelId = novelId;
            return View(chapters);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var novels = new List<NovelViewModel>
            {
                new NovelViewModel { Title = "A Gynoid's Dream", SummaryPath = "AGynoidsDream", ImagePath = "/images/agynoidsdream-cover.jpg" },
                new NovelViewModel { Title = "Spooky Shenanigans", SummaryPath = "SpookyShenanigans", ImagePath = "/images/spookyshenanigans-cover.jpg" }
            };
            return View(novels);
        }

        [HttpGet]
        public IActionResult Summary(string id)
        {
            var novelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/novels", id);
            var chapters = Directory.GetFiles(novelPath, "chapter*.md")
                .Select(f => int.TryParse(Path.GetFileNameWithoutExtension(f).Replace("chapter", "").TrimStart('0'), out int num) ? num : 0)
                .Where(n => n > 0)
                .OrderBy(n => n)
                .ToList();

            ViewBag.ChapterTitles = new Dictionary<int, string>();
            if (chapters.Any())
            {
                foreach (var chapter in chapters)
                {
                    string markdownFile = Path.Combine(novelPath, $"chapter{chapter:D2}.md");
                    if (System.IO.File.Exists(markdownFile))
                    {
                        try
                        {
                            string markdownContent = System.IO.File.ReadAllText(markdownFile, System.Text.Encoding.UTF8);
                            var firstLine = markdownContent.Split('\n').FirstOrDefault(l => l.StartsWith("# "));
                            ViewBag.ChapterTitles[chapter] = firstLine?.TrimStart('#').Trim() ?? $"Chapter {chapter}";
                        }
                        catch (IOException)
                        {
                            ViewBag.ChapterTitles[chapter] = $"Chapter {chapter} (Error loading)";
                        }
                    }
                }
            }
            else
            {
                ViewBag.ChapterTitles[1] = "No chapters available";
            }

            ViewBag.NovelTitle = id.Replace("AGynoidsDream", "A Gynoid's Dream").Replace("SpookyShenanigans", "Spooky Shenanigans");
            ViewBag.ImagePath = $"/images/{id}-cover.jpg";
            ViewBag.NovelId = id;
            ViewBag.Description = $"Description for {ViewBag.NovelTitle} goes here. This will be replaced with the actual story synopsis.";
            return View(chapters);
        }
    }
}