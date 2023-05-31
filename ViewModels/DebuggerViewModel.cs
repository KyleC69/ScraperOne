using System.Collections.ObjectModel;
using ScraperOne.DataModels;
using ScraperOne.Services;

namespace ScraperOne.ViewModels;

public class DebuggerViewModel
{
    public DebuggerViewModel()
    {
        BlogData = new ObservableCollection<IBlog>();

        GetBlogs();

        Console.WriteLine($"BlogDFata {BlogData.Count}");
    }

    public static ObservableCollection<IBlog> BlogData { get; set; }


    private static async void GetBlogs()
    {
        var blogs = await ManagerService.GetIBlogsAsync("/storage/ScraperOne/3Blogs/Index");


        foreach (var blog in blogs) BlogData.Add(blog);
    }
}