using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Security.Cryptography;

namespace CommunityNewsApp
{
    // Interfaces
    public interface INewsItem
    {
        string Title { get; set; }
        string Content { get; set; }
        DateTime PublishDate { get; set; }
        string Author { get; set; }
        NewsCategory Category { get; set; }
        bool IsApproved { get; set; }

        void ValidateContent();
        void ModerateContent(ContentModerator moderator);
    }

    public interface IModeratable
    {
        bool IsApproved { get; set; }
        void ModerateContent(ContentModerator moderator);
    }

    // Enums
    public enum NewsCategory
    {
        Politics,
        Weather,
        Sports,
        Community,
        Technology
        
    }

    // Exceptions
    public class InvalidContentException : Exception
    {
        public InvalidContentException(string message) : base(message) { }
    }

    public class HateSpeechDetectedException : InvalidContentException
    {
        public HateSpeechDetectedException() : base("Content contains prohibited hate speech") { }
    }

   

    // ErrorHandling
    public static class ErrorHandling
    {
        public class EnumHandler
        {
            public class InvalidCategoryException : Exception
            {
                public InvalidCategoryException(int value)
                    : base($"Category {value} does not exist. Please select a category from 0-{Enum.GetValues(typeof(NewsCategory)).Length - 1}")
                {
                }
            }
            public virtual void ProcesEnumValue(int inputValue)
            {
                if (!Enum.IsDefined(typeof(NewsCategory), inputValue))
                {
                    throw new InvalidCategoryException(inputValue);
                }

                NewsCategory category = (NewsCategory)inputValue;
                Console.WriteLine($"Processing documents in category: {category}"); 
            }
        }
    }
    // Base News Item
    public abstract class NewsItemBase : INewsItem, IModeratable
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public string Author { get; set; }
        public NewsCategory Category { get; set; }
        public bool IsApproved { get; set; }
        public int ViewCount { get; private set; }

        protected NewsItemBase(string title, string content, string author, NewsCategory category)
        {
            Title = SecurityHelper.SanitizeInput(title);
            Content = SecurityHelper.SanitizeInput(content);
            Author = author;
            Category = category;
            PublishDate = DateTime.Now;
            IsApproved = false;
        }

        public virtual void ValidateContent()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
                throw new InvalidContentException("Title and content cannot be empty");

            if (ContainsHateSpeech())
                throw new HateSpeechDetectedException();
        }

        protected virtual bool ContainsHateSpeech()
        {
            var prohibitedTerms = new List<string> { "hate", "violence", "discrimination","die", "kill", "euthanise", "jews", "nigger", "hitler", "nazi",  };
            return prohibitedTerms.Any(term => Content.ToLower().Contains(term));
        }

        public virtual void ModerateContent(ContentModerator moderator)
        {
            moderator.Review(this);
        }

        public void IncrementViewCount() => ViewCount++;

        public event ContentFlaggedEventHandler? ContentFlagged;

        protected virtual void OnContentFlagged(string reason)
        {
            ContentFlagged?.Invoke(this, new ContentFlaggedEventArgs(this, reason));
        }
    }

    // Concrete News Item
    public class CommunityNewsItem : NewsItemBase
    {
        public string Location { get; set; }
        public List<string> Tags { get; set; }

        public CommunityNewsItem(string title, string content, string author,
                              NewsCategory category, string location)
            : base(title, content, author, category)
        {
            Location = location;
            Tags = new List<string>();
        }

        public override void ValidateContent()
        {
            base.ValidateContent();
            if (string.IsNullOrWhiteSpace(Location))
                throw new InvalidContentException("Community news must have a location");

            if (ContainsHateSpeech())
            {
                OnContentFlagged("Hate speech detected");
                throw new HateSpeechDetectedException();
            }
        }
    }

    // Moderator
    public class ContentModerator
    {
        public void Review(INewsItem item)
        {
            try
            {
                item.ValidateContent();
                item.IsApproved = true;
                Console.WriteLine($"Approved: {item.Title}");
            }
            catch (InvalidContentException ex)
            {
                Console.WriteLine($"Rejected: {ex.Message}");
                item.IsApproved = false;
            }
        }
    }

    // News Manager
    public class NewsManager
    {
        private readonly List<INewsItem> _newsItems = new List<INewsItem>();
        private readonly ContentModerator _moderator = new ContentModerator();

        public event Action<INewsItem>? NewsPublished;

        public void AddNewsItem(INewsItem newsItem)
        {
            new Thread(() => {
                newsItem.ModerateContent(_moderator);
                if (newsItem.IsApproved)
                {
                    lock (_newsItems)
                    {
                        _newsItems.Add(newsItem);
                    }
                    NewsPublished?.Invoke(newsItem);
                }
            }).Start();
        }

        public List<INewsItem> GetApprovedNews(NewsCategory? category = null)
        {
            lock (_newsItems)
            {
                return _newsItems
                    .Where(item => item.IsApproved &&
                                 (category == null || item.Category == category))
                    .OrderByDescending(item => item.PublishDate)
                    .ToList();
            }
        }
    }

    // Security
    public static class SecurityHelper
    {
        public static string SanitizeInput(string input)
        {
            return System.Web.HttpUtility.HtmlEncode(input);
        }

        public static string HashPassword(string password)
        {
            // Use SHA256 as the hash algorithm for PBKDF2
            using var deriveBytes = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256);
            byte[] hash = deriveBytes.GetBytes(20);
            byte[] salt = deriveBytes.Salt;
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var parts = storedHash.Split(':');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            // Use SHA256 as the hash algorithm for PBKDF2
            using var deriveBytes = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256);
            byte[] testHash = deriveBytes.GetBytes(20);

            return testHash.SequenceEqual(hash);
        }
    }
    // Delegates
    public delegate void NewsPublishedEventHandler(object sender, NewsEventArgs e);
    public delegate void ContentFlaggedEventHandler(object sender, ContentFlaggedEventArgs e);

    // Event Argument Classes

    public delegate void NewsSelectedEventHandler(object sender, NewsSelectedEventArgs e);

    public class NewsSelectedEventArgs : EventArgs
    {
        public INewsItem SelectedNews { get; }
        public NewsSelectedEventArgs(INewsItem newsItem) => SelectedNews = newsItem;
    }

    public class NewsEventArgs : EventArgs
    {
        public INewsItem NewsItem { get; }
        public NewsEventArgs(INewsItem item = null!) => NewsItem = item;
    }

    public class ContentFlaggedEventArgs : EventArgs
    {
        public INewsItem FlaggedItem { get; }
        public string Reason { get; }
        public ContentFlaggedEventArgs(INewsItem item, string reason)
        {
            FlaggedItem = item;
            Reason = reason;
        }
    }

   

    // Main Application
    class Program
    {
        static NewsManager newsManager = new NewsManager();
        static Dictionary<string, string> userDatabase = new Dictionary<string, string>(); // username:hashedPassword
        static event NewsSelectedEventHandler? SelectedNews;
        public delegate void UnregisteredUserAttemptEventHandler(string username);
        static event UnregisteredUserAttemptEventHandler? UnregisteredUserAttempt;

        static void Main(string[] args)
        {
            newsManager.NewsPublished += OnNewsPublished;
            UnregisteredUserAttempt += OnUnregisteredUserAttempt;
            InitializeSampleData();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Community News Network");
                Console.WriteLine("1. Browse News");
                Console.WriteLine("2. Submit News");
                Console.WriteLine("3. Register");
                Console.WriteLine("4. Exit");

                switch (Console.ReadLine())
                {
                    case "1": BrowseNews(); break;
                    case "2": SubmitNews(); break;
                    case "3": RegisterUser(); break;
                    case "4": return;
                }
            }
        }

        static void OnNewsPublished(INewsItem item)
        {
            Console.WriteLine($"\nNew article published: {item.Title} ({item.Category})");
        }

        static void BrowseNews()
        {
            var categories = Enum.GetValues(typeof(NewsCategory));
            Console.WriteLine("\nSelect category:");
            for (int i = 0; i < categories.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {categories.GetValue(i)}");
            }

            try
            {
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    throw new ErrorHandling.EnumHandler.InvalidCategoryException(-1);
                }

                var handler = new ErrorHandling.EnumHandler();
                handler.ProcesEnumValue(choice - 1);

                var selectedCategory = (NewsCategory)(choice - 1);
                var news = newsManager.GetApprovedNews(selectedCategory);

                if (news.Count == 0)
                {
                    Console.WriteLine($"\nNo news available in {selectedCategory} category.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Display articles with numbers
                Console.WriteLine($"\n--- {selectedCategory} News ---");
                for (int i = 0; i < news.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {news[i].Title} (by {news[i].Author})");
                    Console.WriteLine($"   Published: {news[i].PublishDate:g}");
                    Console.WriteLine();
                }

                // Let user select specific article
                Console.WriteLine("Select article to read (0 to go back):");
                if (int.TryParse(Console.ReadLine(), out int articleChoice))
                {
                    if (articleChoice == 0) return;

                    if (articleChoice > 0 && articleChoice <= news.Count)
                    {
                        var selectedNews = news[articleChoice - 1];
                        OnNewsSelected(selectedNews); // Trigger event
                        DisplayNewsContent(selectedNews);
                    }
                    else
                    {
                        Console.WriteLine("Invalid article selection.");
                    }
                }
            }
            catch (ErrorHandling.EnumHandler.InvalidCategoryException ex)
            {
                PrintError($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        // Event trigger method
        static void OnNewsSelected(INewsItem newsItem)
        {
            SelectedNews?.Invoke(null, new NewsSelectedEventArgs(newsItem));
        }

        // Display full article content
        static void DisplayNewsContent(INewsItem newsItem)
        {
            Console.Clear();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"TITLE: {newsItem.Title}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Author: {newsItem.Author}");
            Console.WriteLine($"Published: {newsItem.PublishDate:g}");
            Console.WriteLine($"Category: {newsItem.Category}");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine("\nCONTENT:");
            Console.WriteLine(newsItem.Content);
            Console.WriteLine(new string('-', 60));
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }




        private static void SubmitNews()
        {
            Console.WriteLine("\nEnter your username:");
            string? author = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(author))
            {
                PrintError("Username cannot be empty.");
                Thread.Sleep(1500);
                return;
            }

            // Registration check and event trigger
            if (!userDatabase.ContainsKey(author))
            {
                UnregisteredUserAttempt?.Invoke(author);
                return;
            }

            Console.WriteLine("Enter title:");
            string? title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
            {
                PrintError("Title cannot be empty.");
                Thread.Sleep(1500);
                return;
            }

            Console.WriteLine("Enter content:");
            string? content = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(content))
            {
                PrintError("Content cannot be empty.");
                Thread.Sleep(1500);
                return;
            }

            Console.WriteLine("Select category (1-5):");
            var categories = Enum.GetValues(typeof(NewsCategory));
            for (int i = 0; i < categories.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {categories.GetValue(i)}");
            }

            if (int.TryParse(Console.ReadLine(), out int categoryChoice) &&
                categoryChoice > 0 && categoryChoice <= categories.Length)
            {
                var newsItem = new CommunityNewsItem(
                    title,
                    content,
                    author,
                    (NewsCategory)(categoryChoice - 1),
                    "Default Location"
                );

                newsManager.AddNewsItem(newsItem);
                Console.WriteLine("Your submission is being moderated...");
            }
            else
            {
                PrintError("Invalid category selection");
            }
            Thread.Sleep(2000);
        }

        static void RegisterUser()
        {
            Console.WriteLine("\nEnter new username:");
            string? username = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(username))
            {
                PrintError("Username cannot be empty.");
                Thread.Sleep(1500);
                return;
            }

            Console.WriteLine("Enter password:");
            string? password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password))
            {
                PrintError("Password cannot be empty.");
                Thread.Sleep(1500);
                return;
            }

            if (!userDatabase.ContainsKey(username))
            {
                userDatabase.Add(username, SecurityHelper.HashPassword(password));
                Console.WriteLine("Registration successful!");
            }
            else
            {
                PrintError("Username already exists");
            }
            Thread.Sleep(1500);
        }

        static void InitializeSampleData()
        {
            
            var sampleNews = new List<INewsItem>
    {
        new CommunityNewsItem(
            "Community Womens Month event",
            "To celebrate Women's Month we will be hosting a Ladies brunch on the 23rd of August from 10AM-12PM at the Botanical Garden.There will be guests speakers and an open cocktail bar ",
            "admin",
            NewsCategory.Community,
            "Pretoria"
        ) { IsApproved = true },

        new CommunityNewsItem(
            "Heat Front",
            "There will be a heat front for the rest of the week, average temperature's of 25-28 degrees Celcius",
            "weatherbot",
            NewsCategory.Weather,
            "Gauteng"
        ) { IsApproved = true },

        new CommunityNewsItem(
            "Upset at Old Trafford",
            "Arsenal secure 3 points against Manchester United after a hard fought bout between both teams. United fans upset at the decision making of the referee and VAR..",
            "Jamie Carragher",
            NewsCategory.Sports,
            "International"
        ) { IsApproved = true },

        new CommunityNewsItem(
            "Tech Fair 2025",
            "New gadgets and innovations will be showcased downtown next month.",
            "Mr. Terrific",
            NewsCategory.Technology,
            "Sun Arena Pretoria "
        ) { IsApproved = true }
    };

            
            newsManager.GetType()
                .GetField("_newsItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(newsManager, sampleNews);

            //admin user for testing
            userDatabase.Add("admin", SecurityHelper.HashPassword("password"));
        }

        static void OnUnregisteredUserAttempt(string username)
        {
            Console.WriteLine($"\nUser '{username}' is not registered. Please register before submitting news.");
            Thread.Sleep(2000);
        }

       
        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}