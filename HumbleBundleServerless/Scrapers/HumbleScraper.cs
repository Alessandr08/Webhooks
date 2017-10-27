﻿using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleBot
{
    public class HumbleScraper
    {

        private List<string> visitedUrls = new List<string>();

        private List<HumbleGame> foundGames = new List<HumbleGame>();

        public virtual string GetBaseUrl()
        {
            return "https://www.humblebundle.com";
        }

        public List<HumbleGame> Scrape()
        {
            ScrapePage(GetBaseUrl());

            return foundGames;
        }

        private void ScrapePage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);
            var response = document.DocumentNode;

            var finalUrl = web.ResponseUri.ToString();

            visitedUrls.Add(finalUrl);

            if (finalUrl != GetBaseUrl())
                ScrapeSections(response, finalUrl);

            VisitOtherPages(response);
        }

        private static string GetBundleName(HtmlNode response)
        {
            return response.CssSelect("#active-subtab").First().InnerText.CleanInnerText();
        }

        private void ScrapeSections(HtmlNode response, string finalUrl)
        {
            string bundleName = GetBundleName(response);

            foreach (var section in response.CssSelect(".dd-game-row"))
            {
                var sectionTitle = "";

                try
                {
                    sectionTitle = section.CssSelect(".dd-header-headline").First().InnerText.CleanInnerText();
                }
                catch
                {
                    sectionTitle = section.CssSelect(".fi-content-header").First().InnerText.CleanInnerText();
                }

                if (sectionTitle.Contains("average"))
                {
                    sectionTitle = "Beat the Average!";
                }

                if (string.IsNullOrEmpty(sectionTitle))
                {
                    continue;
                }

                FindGamesInSection(finalUrl, bundleName, section, sectionTitle);
            }
        }

        private void FindGamesInSection(string finalUrl, string bundleName, HtmlNode section, string sectionTitle)
        {
            foreach (var gameTitle in section.CssSelect(".dd-image-box-caption"))
            {
                var title = gameTitle.InnerText.CleanInnerText();
                if (!foundGames.Any(x => x.Title == title))
                {
                    foundGames.Add(new HumbleGame
                    {
                        Bundle = bundleName,
                        URL = finalUrl,
                        Title = title,
                        Section = sectionTitle
                    });
                }
            }

            if (section.CssSelect(".fi-content-body").Any())
            {
                var title = section.CssSelect(".fi-content-body").First().InnerText.CleanInnerText();
                if (!foundGames.Any(x => x.Title == title))
                {
                    foundGames.Add(new HumbleGame
                    {
                        Bundle = bundleName,
                        URL = finalUrl,
                        Title = title,
                        Section = sectionTitle
                    });
                }
            }
        }

        private void VisitOtherPages(HtmlNode response)
        {
            foreach (var tab in response.CssSelect(".subtab-button").Where(x => x.GetAttributeValue("href").StartsWith("/")))
            {
                var nextPage = "https://www.humblebundle.com" + tab.Attributes["href"].Value;

                if (!visitedUrls.Contains(nextPage))
                {
                    ScrapePage(nextPage);
                }
            }
        }  
    }

    public class HumbleGame
    {
        public string Bundle { get; set; }
        public string URL { get; set; }
        public string Section { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return "[" + Bundle + "]  [" + Section + "]  " + Title;
        }
    }
}