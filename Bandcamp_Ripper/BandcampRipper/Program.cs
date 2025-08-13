using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Security.Cryptography.X509Certificates;

namespace BandcampRipper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                HtmlWeb web = new HtmlWeb();
                //TODO: Test args to ensure that it's a valid web address.
                HtmlDocument source = web.Load(args[0]);
                string urlAddress = args[0];
                var script_nodes = source.DocumentNode.SelectNodes("//script");
                HtmlNode music_data_node = new HtmlNode(HtmlNodeType.Text, source, 0);
                HtmlNode music_meta_data_node = new HtmlNode(HtmlNodeType.Text, source, 0);
                //string track_attributes;
                bool found_data_node = false;
                bool found_meta_data_node = false;
                foreach (var node in script_nodes.ToList())
                {
                    if (found_meta_data_node == false && node.Attributes["type"].Value == "application/ld+json")
                    {
                        music_meta_data_node = node;
                        found_meta_data_node = true;
                        continue;
                    }
                    if (found_data_node == false && node.Attributes.Contains("data-cart"))
                    {
                        music_data_node = node;
                        found_data_node = true;
                        continue;
                    }
                    if (found_data_node && found_meta_data_node)
                    {
                        break;
                    }
                }

                var meta_data_string = music_meta_data_node.InnerHtml;
                var meta_data_json = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(meta_data_string);
                var artist = meta_data_json["byArtist"]["name"].ToString();
                var album = meta_data_json["name"].ToString();

                var track_blob = music_data_node.GetAttributeValue("data-tralbum", "");
                var track_blob_edited_1 = Regex.Replace(track_blob, "&quot;", "\"");
                var data_parsed_down_1 = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(track_blob_edited_1);
                var track_info = data_parsed_down_1["trackinfo"];
                List<string> titles = new List<string>();
                List<string> files = new List<string>();
                for (int i = 0; i < track_info.AsArray().Count; i++)
                {
                    //Console.WriteLine(track_info[i]);
                    var node = track_info[i];
                    string title = node["title"].ToString().Replace('/', '-');
                    titles.Add(title);
                    var node2 = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(node["file"]);
                    files.Add(node2["mp3-128"].ToString());
                }
                Console.WriteLine("Titles and Files get!");

                var pwd = Directory.GetCurrentDirectory();
                var dest_dir = pwd + "/" + artist + "/"  + album;
                Directory.CreateDirectory(dest_dir);
                Directory.SetCurrentDirectory(dest_dir);

                for (int i = 0; i < titles.Count; i++)
                {
                    using (var client = new WebClient())
                    {
                        string filename = (i + 1).ToString() + ". " + titles[i] + ".mp3";
                        Console.WriteLine("Now downloading " + filename);
                        client.DownloadFile(files[i], (i + 1).ToString() + ". " + titles[i] + ".mp3");
                    }
                }
                Console.WriteLine("All done!");
            }
            else
            {
            }
        }
    }
}

 