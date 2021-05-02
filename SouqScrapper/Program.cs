using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace SouqScrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            // A page to scrape its data.
            string url = "https://egypt.souq.com/eg-en/samsung-a32/s/?as=1&section=2&page=1";

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            // Register the browser to be chrome or firefox.
            httpRequest.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36");

            // Getting the page HTML.
            var html = httpClient.GetStringAsync(url).Result;
            File.WriteAllText("souq.html", html);

            // Load HTML into an object to deal with it.
            var document = new HtmlDocument();
            document.LoadHtml(html);

            // Getting Items.
            var items = document.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "").Contains("single-item"));

            // Initialize a list to hold items data.
            var itemsData = new List<SouqItem>();

            int itemCounter = 1;
            using (WebClient client = new WebClient())
            {
                Directory.CreateDirectory("Products");
                foreach (HtmlNode item in items)
                {
                    // Parsing HTML to SouqItem to know the item data.
                    SouqItem souqItem = SouqItem.ParseHTML(item);

                    Console.WriteLine(souqItem);

                    // Creating Directory with item name to Hold item img and a text file contains item data.
                    Directory.CreateDirectory($"Products/{itemCounter}-{souqItem.Name}");

                    // Downloading item image.
                    client.DownloadFile(new Uri(souqItem.ImgUrl), $"Products/{itemCounter}-{souqItem.Name}/{ souqItem.Name }.{souqItem.ImgExtension}");

                    // Write item data to the text file.
                    File.WriteAllText($"Products/{itemCounter}-{souqItem.Name}/{souqItem.Name}.txt", souqItem.ToString());

                    itemCounter++;
                }
            }

            // Printing total number of items.
            Console.WriteLine("Total Items = {0}", items.Count());
        }
    }

    class SouqItem
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Brand
        /// </summary>
        public string BrandName { get; private set; }

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Price (Magnitude only)
        /// </summary>
        private string PriceNumber { get; set; }

        /// <summary>
        /// Currency of the price
        /// </summary>
        private string PriceCurrency { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public string Price { get => PriceNumber + " " + PriceCurrency; }

        /// <summary>
        /// Item's image's Url
        /// </summary>
        public string ImgUrl { get; private set; }

        /// <summary>
        /// The extension of the item's image
        /// </summary>
        public string ImgExtension
        {
            get
            {
                string[] imgName = ImgUrl.Split('.');
                return imgName[imgName.Length - 1];
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SouqItem"/>.
        /// </summary>
        private SouqItem() { }

        /// <summary>
        /// Initializes a new instance of <see cref="SouqItem"/> using the HTML node of the item.
        /// </summary>
        /// <param name="itemNode">HTML Node</param>
        public static SouqItem ParseHTML(HtmlNode itemNode)
        {
            SouqItem souqItem = new SouqItem();
            souqItem.Name = itemNode.GetAttributeValue("data-name", "");
            souqItem.BrandName = itemNode.GetAttributeValue("data-brand-name", "");
            souqItem.Category = itemNode.GetAttributeValue("data-category-name", "");
            souqItem.ImgUrl = itemNode.Descendants("img").Select(node =>
            {
                string src = node.GetAttributeValue("data-src", "");
                if (src != "") return src;
                else return node.GetAttributeValue("src", "");
            }).ToList()[0];
            souqItem.PriceNumber =
                itemNode.Descendants("span")
                .Where(node => node.GetAttributeValue("class", "") == "itemPrice")
                .Select(node => node.InnerText).ToList()[0];
            souqItem.PriceCurrency =
                itemNode.Descendants("small")
                .Where(node => node.GetAttributeValue("class", "") == "currency-text sk-clr1")
                .Select(node => node.InnerText).ToList()[0];

            return souqItem;
        }

        public override string ToString()
        {
            return
                $@"
Product Name: {Name},
Brand Name: {BrandName}
Category: {Category}
Price: {Price}
Image URL: {ImgUrl}

===========================================================";
        }
    }
}
