using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom.Html;

using Newtonsoft.Json;

namespace RadioFetcher
{
	class MainClass
	{


		static IConfiguration config = Configuration.Default.WithDefaultLoader();

		static IBrowsingContext context = BrowsingContext.New(config);

		public static void Main(string[] args)
		{

			List<Dictionary<string, string>> stationInfos = new List<Dictionary<string, string>>();

			List<string> stationUrls = new List<string>();

			Task.Run(async () =>
			{
				List<string> states = await getStateUrls();


				Console.SetCursorPosition(0, 0);
	
				for (int i = 0; i < states.Count; i++)
				{
					var stateUrl = states[i];
					Console.SetCursorPosition(0, 0);
					Console.Out.WriteLine($"Fetch State {i + 1} / {states.Count}");

					stationUrls.AddRange(await getStationUrls(stateUrl));
					////Console.Out.WriteLine("Fetching State " + stateUrl);
					//List<string> stationUrls = await getStationUrls(stateUrl);
				}


				var timer = new Stopwatch();
				timer.Start();


				TimeSpan etaTime = new TimeSpan(0, 0, 0, 0);
             	for (int i = 0; i < stationUrls.Count; i++)
				{
					var stationUrl = stationUrls[i];
					//Console.Out.WriteLine("Station" + stationUrl);
					Console.SetCursorPosition(0, 1);

					if (i % 10 == 0)
					{
						var millisecondPerItem = (float)timer.ElapsedMilliseconds / (float)i;
						var eta = (int)((stationUrls.Count - i) * millisecondPerItem);

						etaTime = new TimeSpan(0, 0, 0, 0, eta);
					}

					Console.Out.WriteLine($"Fetch Station {i + 1} / {stationUrls.Count} ... { (((float)i / (float)stationUrls.Count)).ToString("P") }  ETA: { etaTime.ToString(@"hh\:mm\:ss") }");

					var info = await getStationInfoRaw(stationUrl);
					stationInfos.Add(info);

					//if (i >= 100)
					//	break;
				}
				timer.Stop();
				var woahtherebuddy = JsonConvert.SerializeObject(stationInfos, Formatting.Indented);


				string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				System.IO.File.WriteAllText(path + "/oof.json", woahtherebuddy);



				//var props = await stationRawInfo("https://en.wikipedia.org/wiki/WAAO-FM");

				//var a = await stationUrlsForState("");
			}).Wait();
		}

		/// <summary>
		/// gets the nav urls for each state lists
		/// </summary>
		/// <returns>The state urls.</returns>
		async static Task<List<string>> getStateUrls()
		{
			string toplevel = "https://en.wikipedia.org/wiki/Category:Lists_of_radio_stations_in_the_United_States";
			var document = await context.OpenAsync(toplevel);


			var urls = new List<string>();
			//Wikipedia does like alphabetical ordering by group so this gets those lists
			var alphaLists = document.QuerySelectorAll(".mw-category-group ul");
			foreach (var al in alphaLists)
			{

				//Now, each of the alphabetical lists needs to be done
				foreach (var li in al.Children)
				{
					var link = li.QuerySelector("a");
					//var link = li.FirstChild;
					var a = link as AngleSharp.Dom.Html.IHtmlAnchorElement;
					if (a == null) continue;

					if (a.Href.Contains("radio_stations_in") && !a.Href.Contains("United") && !a.Href.Contains("America"))
					{
						var url = a.Href;
						urls.Add(url);
					}
				}

			}


			return urls;
		}

		/// <summary>
		/// given a url for a list of radio stations in a state
		/// return a list of all the radio stations in that list
		/// </summary>
		/// <returns>The radio station urls for the state.</returns>
		/// <param name="stateurl">Stateurl.</param>
		async static Task<List<string>> getStationUrls(string stateurl)
		{
			var document = await context.OpenAsync(stateurl);

			var urls = new List<string>();

			//TODO: THIS PART
			var tableBody = document.QuerySelector(".wikitable tbody");
			foreach (var row in tableBody.Children)
			{
				var linkbox = row.FirstChild;

				if (linkbox != null)
				{
					var link = linkbox.FirstChild as AngleSharp.Dom.Html.IHtmlAnchorElement;
					if (link != null) {
						var url = link.Href;
						urls.Add(url);
						//Console.Out.WriteLine(url);
					}
				}
			}


			return urls;
		}



		/// <summary>
		/// Gets the information for an individual radio station page
		/// </summary>
		/// <returns>The raw info as a dictionary</returns>
		async static Task<Dictionary<string, string>> getStationInfoRaw(string url)
		{

			var address = url;
			var document = await context.OpenAsync(address);

			var infobox = document.QuerySelector(".infobox.vcard");
			if (infobox == null)
				return null;


			var table = infobox.QuerySelector("tbody");
			Dictionary<string, string> info = new Dictionary<string, string>();
			foreach (var row in table.Children)
			{
				if (row.Children.Length == 2)
				{
					var key = row.Children[0].TextContent.ToString();
					var val = row.Children[1].TextContent.ToString();

					info[key] = val;
				}
			}
			return info;
		}


		static void log(string a)
		{
			Console.Out.WriteLine(a);
		}

	}
}
