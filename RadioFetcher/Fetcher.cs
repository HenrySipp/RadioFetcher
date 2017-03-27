using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using AngleSharp;
using AngleSharp.Dom.Html;

using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace RadioFetcher
{
	
	public static class Fetcher
	{

		static IConfiguration config = Configuration.Default.WithDefaultLoader();

		static IBrowsingContext context = BrowsingContext.New(config);


		public async static Task<Dictionary<string, List<Dictionary<string, string>>>> FetchStationUrlsPerState()
		{
			List<string> states = await getStateUrls();

			Dictionary<string, List<string>> stationurlsByState = new Dictionary<string, List<string>>();

			Console.SetCursorPosition(0, 0);


		

			int totalStationUrlCount = 0;

			Console.SetCursorPosition(0, 0);
			Console.Out.WriteLine($"Fetch States");
			for (int i = 0; i < states.Count; i++)
			{
				var stateUrl = states[i];
				string[] splithack = Regex.Split(stateUrl, "stations_in_");

				var stateName = splithack[1];

				//var substrIndex = stateUrl.IndexOf("stations_in_", 0);
				//Console.Clear();
				Console.SetCursorPosition(0, 0);
				Console.Out.WriteLine($"Fetch State {stateName} {i + 1} / {states.Count}");




				List<string> stationUrls = await getStationUrls(stateUrl);

				stationurlsByState[stateName] = stationUrls;
				totalStationUrlCount += stationUrls.Count;
			}

			//Console.Clear();
			Console.SetCursorPosition(0, 0);
			Console.Out.Write("                                        ");
			var timer = new Stopwatch();
			timer.Start();


			TimeSpan etaTime = new TimeSpan(0, 0, 0, 0);

			int urlindex = 0;
			int stateindex = 0;


			Dictionary<string, List<Dictionary<string, string>>> stationInfosPerState = new Dictionary<string, List<Dictionary<string, string>>>();



			foreach (KeyValuePair<string, List<string>> hey in stationurlsByState)
			{

				List<Dictionary<string, string>> stationInfos = new List<Dictionary<string, string>>();

				var stationurls = hey.Value;
				var statename = hey.Key;

				stateindex++;


				int localIndex = 0;

				foreach (var url in stationurls)
				{
					localIndex++;

					var info = await getStationInfoRaw(url);
					stationInfos.Add(info);
					urlindex++;

					if (urlindex % 10 == 0)
					{
						var millisecondPerItem = (float)timer.ElapsedMilliseconds / (float)stateindex;
						var eta = (int)((totalStationUrlCount - stateindex) * millisecondPerItem);

						etaTime = new TimeSpan(0, 0, 0, 0, eta);
					}
					//Console.Clear();
					Console.SetCursorPosition(0, 0);
					Console.Out.WriteLine($"Fetch State {statename} ({stateindex} / {states.Count}) ... Station ( {localIndex} / {stationurls.Count}) ({urlindex} / {totalStationUrlCount}) ... { (((float)urlindex / (float)totalStationUrlCount)).ToString("P") }  ETA: { etaTime.ToString(@"hh\:mm\:ss") }");
					//	Console.Out.WriteLine($"Fetch Station {i + 1} / {stationUrls.Count} ... { (((float)i / (float)stationUrls.Count)).ToString("P") }  ETA: { etaTime.ToString(@"hh\:mm\:ss") }");

				}

				stationInfosPerState[statename] = stationInfos;
			}
			return stationInfosPerState;
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
					if (link != null)
					{
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


			var lastSlashPos = url.LastIndexOf('/');

			var callsign = url.Substring(lastSlashPos + 1);
			info["callsign"] = callsign; //title.TextContent.ToString();
			return info;
		}


	}
}
