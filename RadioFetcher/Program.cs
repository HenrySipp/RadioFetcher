using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom.Html;

using Newtonsoft.Json;

namespace RadioFetcher
{
	class MainClass
	{



        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			// Normally this would be an arg but honestly I just run this from 
            // visual studio so... manually change this
			var fetchOrProcess = "Process";

			Task.Run(async () =>
			{
                // Here we process the prefetched data

				if (fetchOrProcess == "Process")
				{
					await Processor.Process();
				}
				else {
                    // This loop fetches the data from wikipedia
					Dictionary<string, List<Dictionary<string, string>>> stationInfosByState = await Fetcher.FetchStationUrlsPerState();


					string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
					System.IO.Directory.CreateDirectory(path + "/RadioData");

                    // Save the data to a folder in our desktop.
                    // Just in case we have an error later when transferring to our SQL db.
					foreach (KeyValuePair<string, List<Dictionary<string, string>>> pair in stationInfosByState)
					{
						var stationinfos = pair.Value;
						var woahtherebuddy = JsonConvert.SerializeObject(stationinfos, Formatting.Indented);
						System.IO.File.WriteAllText(path + $"/RadioData/{pair.Key}.json", woahtherebuddy);
					}
				}
			}).Wait();
		}


	}
}
