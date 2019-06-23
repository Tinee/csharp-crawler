using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler
{
    class Crawler
    {
        private ConcurrentQueue<Uri> workerQueue;
        private List<Func<Uri, bool>> filters;
        private HttpClient client;

        public Crawler()
        {
            client = new HttpClient();
            workerQueue = new ConcurrentQueue<Uri>();
        }

        public void Crawl(Uri target)
        {
            workerQueue.Enqueue(target);

            var threads = new Thread[4];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(Work);
                threads[i].IsBackground = true;
                threads[i].Priority = ThreadPriority.Lowest;
                threads[i].Name = i.ToString();
                threads[i].Start();
            }
        }

        public Crawler AddFilters(params Func<Uri, bool>[] funcs)
        {
            filters = funcs.ToList();
            return this;
        }

        private async void Work()
        {
            while (true)
            {
                Uri uri;
                var exist = workerQueue.TryDequeue(out uri);
                if (exist)
                {

                    var links = await getFilteredPageLinks(uri);
                    links.ForEach(link =>
                    {
                        Console.WriteLine(link.ToString());
                        workerQueue.Enqueue(link);
                    });
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
        }

        private async Task<List<Uri>> getFilteredPageLinks(Uri target)
        {
            var result = await client.GetAsync(target);

            var doc = new HtmlDocument();
            var bodyStream = await result.Content.ReadAsStreamAsync();
            doc.Load(bodyStream);

            return doc.DocumentNode.SelectNodes("//a")
                .Select(node => AnchorNodeToUri(target, node))
                .Where(PassesFilters)
                .ToList();
        }

        private Uri AnchorNodeToUri(Uri baseUri, HtmlNode node) => new Uri(baseUri, node.Attributes["href"]?.Value);

        private bool PassesFilters(Uri uri) => filters.TrueForAll(filter => filter(uri));
    }
}
