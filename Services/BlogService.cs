using Microsoft.Extensions.Logging;
using ScraperOne.DataModels;
using ScraperOne.DataModels.Files;

namespace ScraperOne.Services
{
    public interface IBlogService
    {
        bool CheckIfBlogShouldCheckDirectory(string url);
        bool CheckIfFileExistsInDirectory(string url);
        bool CreateDataFolder();
        bool IsMediaIndexOnFile(string mediaIndex);
        void SaveFiles();
        void UpdateBlogPostCount(string propertyName);
        void UpdateBlogProgress();
    }

    public class BlogService : IBlogService
    {
        private readonly IBlog _blog;

        private readonly IFiles _files;

        //private readonly object _lockObjectDb = new object();
        private readonly object _lockObjectDirectory = new();
        private readonly object _lockObjectPostCount = new();
        private readonly object _lockObjectProgress = new();
        private readonly ILogger _logger;

        public BlogService(ILogger logger, IBlog blog, IFiles files)
        {
            _logger = logger;
            _blog = blog;
            _files = files;
        }

        public void UpdateBlogProgress()
        {
            lock (_lockObjectProgress)
            {
                _blog.Progress = (int)(_blog.DownloadedItems / (double)_blog.TotalCount * 100);
            }
        }

        public void UpdateBlogPostCount(string propertyName)
        {
            lock (_lockObjectPostCount)
            {
                System.Reflection.PropertyInfo property = typeof(IBlog).GetProperty(propertyName);
                int postCounter = (int)property.GetValue(_blog);
                postCounter++;
                property.SetValue(_blog, postCounter, null);
            }
        }

        //public void UpdateBlogDB(string fileName)
        //{
        //    lock (_lockObjectDb)
        //    {
        //        _files.Links.Add(fileName);
        //    }
        //}

        public bool CreateDataFolder()
        {
            if (string.IsNullOrEmpty(_blog.Name))
            {
                return false;
            }

            string blogPath = _blog.DownloadLocation;

            if (!Directory.Exists(blogPath))
            {
                _ = Directory.CreateDirectory(blogPath);
            }

            return true;
        }

        //public bool CheckIfFileExistsInDB(string url)
        //{
        //    var fileName = url.Split('/').Last();
        //    Monitor.Enter(_lockObjectDb);
        //    try
        //    {
        //        return _files.Links.Contains(fileName);
        //    }
        //    finally
        //    {
        //        Monitor.Exit(_lockObjectDb);
        //    }
        //}

        public bool CheckIfBlogShouldCheckDirectory(string url)
        {
            return _blog.CheckDirectoryForFiles && CheckIfFileExistsInDirectory(url);
        }

        public bool CheckIfFileExistsInDirectory(string url)
        {
            string fileName = url.Split('/').Last();
            Monitor.Enter(_lockObjectDirectory);
            try
            {
                string blogPath = _blog.DownloadLocation;
                return File.Exists(Path.Combine(blogPath, fileName));
            }
            finally
            {
                Monitor.Exit(_lockObjectDirectory);
            }
        }

        public void SaveFiles()
        {
            _ = _files.Save();
        }

        public bool IsMediaIndexOnFile(string v)
        {
            throw new NotImplementedException();
        }
    }
}