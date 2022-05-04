using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AfficheParser
{
    static class Parser
    {
        public static List<Film> GetFilms(string html)
        {
            var films = new List<Film>();

            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);
            foreach (IElement element in document.QuerySelectorAll("div.filmdesc"))
            {
                var table = element.QuerySelector(".film-table").QuerySelectorAll("tr");

                string title = element.QuerySelector(".film-title").TextContent.Replace("\n", "");
                string poster = "https://langal.ru/" + element.QuerySelector("img").GetAttribute("src");
                string startDate = "";
                string endDate = "";
                string duration = "";
                string director = "";
                string actors = "";
                string description = element.QuerySelector(".story").TextContent.Replace("\n", "");

                foreach (var row in table)
                {
                    string content = row.QuerySelectorAll("td")[1].TextContent.Replace("\n", "");
                    switch (row.QuerySelector(".label").TextContent)
                    {
                        case "В прокате":
                            var dates = Regex.Replace(content, "\n\n\n\n.*", "")
                                         .Replace("c ", "")
                                         .Replace(" по ", ";")
                                         .Split(';');
                            startDate = dates[0].Replace("\n", "");
                            endDate = Regex.Replace(dates[1].Replace("\n", ""), "Указан.*", "");
                            break;
                        case "Продолжительность":
                            duration = content;
                            break;
                        case "Режиссер":
                            director = content;
                            break;
                        case "Актеры":
                            actors = content;
                            break;
                    }
                }

                var film = new Film
                {
                    title = title,
                    poster = poster,
                    actors = actors,
                    director = director,
                    description = description,
                    startDate = startDate,
                    endDate = endDate,
                    duration = duration
                };
                films.Add(film);
            }

            return films;
        }

        public static string GetFilmsJson(string html)
        {
            return JsonConvert.SerializeObject(GetFilms(html));
        }

        public static void ParseFilmsToJson()
        {
            using (WebClient client = new WebClient())
            {
                string raw = client.DownloadString("https://langal.ru/affiche/");
                string films = GetFilmsJson(raw);
                using (StreamWriter writer = new StreamWriter("films.json"))
                {
                    writer.WriteLine(films);
                }
            }
        }

        public static void SaveFilmsToFile(List<Film> films)
        {
            string json = JsonConvert.SerializeObject(films);
            using (StreamWriter writer = new StreamWriter("films.json"))
            {
                writer.WriteLine(json);
            }
        }

        public static List<Film> GetFilmsFromJson()
        {
            using (StreamReader reader = new StreamReader("films.json"))
            {
                return JsonConvert.DeserializeObject<List<Film>>(reader.ReadToEnd());
            }
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
