using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;
using System.Security.Policy;

namespace Crawler
{
    internal class Crawler
    {
        static void Main(string[] args)
        {
            string baseUrl = "http://dnd5e.wikidot.com/spells";

            Console.WriteLine("Are you sure you want to continue? \npress enter to continue or anything else, to stop the program");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                try
                {
                    // Create a WebClient to download the HTML content
                    WebClient client = new WebClient();

                    // Download the HTML content for the starting page
                    string sourceCode = client.DownloadString(baseUrl);

                    string fileName = "mainWebsite";

                    // Save the source code to a file
                    using (StreamWriter sw = new StreamWriter("../../../../" + fileName + ".html"))
                    {
                        sw.Write(sourceCode);
                    }

                    // create list of links to innividual spells
                    List<string> hrefs = new List<string>();
                    using (var reader = new StreamReader("../../../../" + fileName + ".html"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // find all links
                            var matches = Regex.Matches(line, 
                                @"<a\s+[^>]*href=(?:'(?<url>[^']*)'|""(?<url>[^""]*)""|(?<url>[^\s>]*))" );

                            foreach (Match match in matches)
                            {
                                var hrefValue = match.Groups["url"].Value;
                                if (hrefValue.StartsWith("/spell:"))
                                    hrefs.Add("http://dnd5e.wikidot.com" + hrefValue);
                            }
                        }
                    }

                    int i = 0;          // for limiter to work, testing purposes
                    int counterOfSuccessfulSaves = 0;    // counter of successfully created spells

                    // download sourcecode of all links and create XML(Spell) files from them
                    foreach (string href in hrefs)
                    {
                        if (i == 5) break; else i++;    // limit to 5, for testing
                        client = new WebClient();
                        string url = client.DownloadString(href);

                        if (SerializeToXML(ExtractSpell(url)))
                            counterOfSuccessfulSaves++;

                        Console.WriteLine("Files created (" + href + ").");
                    }

                    Console.WriteLine("\nTask finished successfully!\nCreated " + counterOfSuccessfulSaves + " spells as XML files.");
                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nERROR: " + e.Message);
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("No action was performed.");
                Console.ReadLine();
            }
        }

        // method for saving spell to XML file
        static bool SerializeToXML(Spell spell)
        {
			try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Spell));

                // Remove invalid filename characters from the URL
                string fileName = new string(spell.name.ToCharArray()
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                    .ToArray());


                string subPath = "../../../../XMLs/";   // path to XMLs folder

                // create XMLs folder if it doesn't already exist 
                if(!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);

                // Save the serialized spell to a file
                using (StreamWriter sw = new StreamWriter(subPath + fileName + ".xml"))
                {
                    serializer.Serialize(sw, spell);
                }

                return true;
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił błąd:\n" + ex.Message);
                return false;
            }

            // TODO: fixfile: 1.load, 2.Replace "â€™" with "&apos;", 3.save 
        }

        // method for extracting spell data from HTML code
        static Spell ExtractSpell(string code)
        {
            string name; 
            string source;
            string levelTemp;
            int level; 
            string school;
            string castingTime;
            string range;
			string components; 
            string duration; 
            List<string> descriptionTemp = new List<string>();
            string[] description;
            string atHigherLevels;
            string spellLists;

            // delete website starters
            code = Between(code, "main-content-wrap col-md-9", "");

            // extract spell's name
            name = Between(code, "<span>", "</span>");
            code = Between(code, "Source: ", "");

            // extract spell's source
            source = Between(code, "", "</p>");
			code = Between(code, "</p>", "");

            // extract spell's level & school
            levelTemp = Between(code, "<p><em>", "</em></p>");
            code = Between(code, "</em></p>", "");
            if (levelTemp.ToLower().Contains("cantrip"))
            {
                level = 0;
                school = Between(levelTemp, "", " cantrip");
            } else
            {
                level = Convert.ToInt32(levelTemp.Substring(1));
                school = Between(levelTemp, "level ", "");
            }

            // extract spell's casting time
            castingTime = Between(code, "</strong> ", "<br />");
            code = Between(code, "<br />", "");

            // extract spell's range
            range = Between(code, "</strong> ", "<br />");
			code = Between(code, "<br />", "");

            // extract spell's components
            components = Between(code, "</strong> ", "<br />");
			code = Between(code, "<br />", "");

            // extract spell's duration
            duration = Between(code, "</strong> ", "</p>");
			code = Between(code, "</p>", "");

            // extract spell's description
            while (!code.Substring(0, "<p><strong><em>".Length + 1).Contains("<p><strong><em>"))
            {
                descriptionTemp.Add(Between(code, "<p>", "</p>"));
                code = Between(code, "</p>", "");
            }
            description = descriptionTemp.ToArray();

            // extract spell's "At Higher Level" value (if any)
            if (code.ToLower().Contains("at higher levels"))
            {
                atHigherLevels = Between(code, "</em></strong> ", "</p>");
                code = Between(code, "</p>", "");
            }
            else atHigherLevels = "";

			// extract spell's spel lists
			spellLists = Between(code, "</em></strong> ", "</p>");

            return new Spell(name, source, level, school, castingTime, range, components, duration, description, spellLists, atHigherLevels);
		}

        public static string Between(string s, string start, string end)
        {
			int startIndex, endIndex;

            if (start == "")
                startIndex = 0;
            else
                startIndex = s.IndexOf(start) + start.Length;

			if (end == "")
                endIndex = s.Length;
            else
			    endIndex = s.IndexOf(end);

			string output = s.Substring(startIndex, endIndex - startIndex);
            return output;
		}
    }
}
