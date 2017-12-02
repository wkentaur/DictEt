using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using System.Linq;

namespace DictEtLogic
{
    public static class Core
    {
        public static Dict myDict;
        public static CancellationTokenSource lookupCancelTS;
        public static CancellationTokenSource wordDescCancelTS;

        static Core()
        {
            myDict = new Dict();
            lookupCancelTS = new CancellationTokenSource();
            wordDescCancelTS = new CancellationTokenSource();
        }

        public static async Task<Dict> GetWordsAsync(string lookup, CancellationToken ct)
        {
            const int MAX_LIST_LENGTH = 100;

            if (lookup.Length > 0)
            {
                ct.ThrowIfCancellationRequested();
                myDict.Lookup = lookup;

                List<EnWord> result = await DictDatabase.GetStartsWith(lookup);

                if (result != null && result.Count > 0)
                {
                    myDict.Words.Clear();

                    int count = 0;
                    foreach (EnWord theEnWord in result)
                    {
                        ct.ThrowIfCancellationRequested();
                        myDict.Words.Add(theEnWord.Word);
                        count++;
                        if (count >= MAX_LIST_LENGTH)
                            break;
                    }

                    ct.ThrowIfCancellationRequested();

                    return myDict;
                }
                else
                {
                    return null;
                }

            }

            return null;
        }

        public static async Task<Dict> GetWordsWebAsync(string lookup, CancellationToken ct)
        {
            if (lookup.Length > 0)
            {
                ct.ThrowIfCancellationRequested();
                myDict.Lookup = lookup;

                string lookupUri = "https://tools.wmflabs.org/etwikt/api/api.php?action=opensearch&lang=en&format=json&source=mobile&search=";
                string queryString = lookupUri + myDict.Lookup;

                dynamic results = await DataService.GetDataFromService(queryString, ct).ConfigureAwait(false);

                if (results != null && results.Count > 1)
                {
                    myDict.Words.Clear();

                    foreach (var matchedWord in results[1])
                    {
                        ct.ThrowIfCancellationRequested();
                        myDict.Words.Add((string)matchedWord);
                    }
                    ct.ThrowIfCancellationRequested();

                    return myDict;
                }
                else
                {
                    return null;
                }

            }

            return null;
        }


        public static void EmptyWordLookup()
        {
            lookupCancelTS.Cancel();
            lookupCancelTS = new CancellationTokenSource();
            myDict.Lookup = "";
            myDict.Words.Clear();
        }


        public static async Task<Dict> GetDescAsync(string word, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            string lookupUri = "https://et.wiktionary.org/w/api.php?action=mobileview&sections=all&format=json&page=";

            if (myDict.WordDescs.ContainsKey(word) )
            {
                return myDict;
            }
            else
            {
                string queryString = lookupUri + word;
                dynamic results = await DataService.GetDataFromService(queryString, ct).ConfigureAwait(false);

                if (results != null)
                {
                    myDict.Lookup = word;

                    bool isEnSection = false;
                    string htmlText = "";

                    foreach (var section in results["mobileview"]["sections"])
                    {
                        ct.ThrowIfCancellationRequested();
                        if (isEnSection && section["toclevel"] == 1)
                        {
                            break;
                        }
                        else if (isEnglishStart(section))
                        {
                            isEnSection = true;
                        }
                        else if (isEnSection)
                        {
                            htmlText += section["text"];
                        }

                    }

                    if (htmlText.Length > 0)
                        htmlText = CleanupHtml(htmlText);
                    myDict.WordDescs.Add(word, htmlText);
                    ct.ThrowIfCancellationRequested();

                    return myDict;
                }
                else
                {
                    ct.ThrowIfCancellationRequested();

                    return null;
                } 
            }
        }

        public static string AddHtmlHeaderFooter(string inHtml)
        {
            string outHtml = "";

            if (inHtml.Length > 0)
            {
                outHtml = @"<html><head><style> .hyperdark { font-weight: bold; } </style></head><body>" + 
                    inHtml + 
                    @"</body></html>";
            }

            return outHtml;
        }

        public static string CleanupHtml(string inHtml)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(inHtml);

            // .mw-editsection
            foreach (HtmlNode spanEl in doc.DocumentNode.Descendants("span")
                .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("mw-editsection")).ToList())
            {
                spanEl.ParentNode.RemoveChild(spanEl);
            }
            // .toimeta
            foreach (HtmlNode divEl in doc.DocumentNode.Descendants("div")
                .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("toimeta")).ToList())
            {
                divEl.ParentNode.RemoveChild(divEl);
            }
            // .audiometa
            foreach (HtmlNode tdEl in doc.DocumentNode.Descendants("td")
                .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("audiometa")).ToList())
            {
                tdEl.ParentNode.RemoveChild(tdEl);
            }
            // get blue links to English words
            foreach (HtmlNode ulEl in doc.DocumentNode.Descendants("ul").ToList())
            {
                foreach (HtmlNode linkInUl in ulEl.Descendants("a").ToList())
                {
                    if (linkInUl.Attributes.Contains("class"))   // class="new"
                    {
                        HtmlNode SpanEl = doc.CreateElement("span");
                        SpanEl.AddClass("hyperdark");
                        SpanEl.InnerHtml = linkInUl.InnerHtml;
                        linkInUl.ParentNode.ReplaceChild(SpanEl, linkInUl);
                    }
                    else
                    {
                        linkInUl.Attributes["href"].Value = convertToAppUrl(linkInUl.Attributes["href"].Value);
                    }
                }

            }
            // strip remaining hyperlinks
            foreach (HtmlNode linkEl in doc.DocumentNode.Descendants("a").ToList())
            {
                if (linkEl.Attributes.Contains("href") && linkEl.Attributes["href"].Value.Contains("dict:"))
                {
                    continue;
                }
                else
                {
                    HtmlNode SpanEl = doc.CreateElement("span");
                    SpanEl.AddClass("hyperdark");
                    SpanEl.InnerHtml = linkEl.InnerHtml;
                    linkEl.ParentNode.ReplaceChild(SpanEl, linkEl);
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        private static string convertToAppUrl(string inUrl)
        {
            return inUrl.Replace("/wiki/","dict:");
        }

        private static bool isEnglishStart(dynamic section)
        {
            if (section["id"] > 0 && section["toclevel"] == 1 && 
                Convert.ToString(section["line"]).ToLower() == "inglise")
                return true;

            return false;
        }

    }
}
