using System;
using System.Threading.Tasks;

namespace Crawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var crawler = new Crawler();
            var target = new Uri("https://monzo.com/");

            crawler
            .AddFilters(
                Filters.Unique(),
                Filters.SchemaContains("https"),
                Filters.isSameDomain(target)
            )
            .Crawl(target);

            Console.ReadLine();
        }
    }
}
