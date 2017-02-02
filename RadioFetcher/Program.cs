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




		public static void Main(string[] args)
		{


			var fetchOrProcess = "Process";

			Task.Run(async () =>
			{
				if (fetchOrProcess == "Process")
				{

					await Processor.Process();

				}
				else {

					Dictionary<string, List<Dictionary<string, string>>> stationInfosByState = await Fetcher.FetchStationUrlsPerState();


					string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
					System.IO.Directory.CreateDirectory(path + "/RadioData");


					foreach (KeyValuePair<string, List<Dictionary<string, string>>> pair in stationInfosByState)
					{
						var stationinfos = pair.Value;
						//write it incase we fuck up!
						var woahtherebuddy = JsonConvert.SerializeObject(stationinfos, Formatting.Indented);
						System.IO.File.WriteAllText(path + $"/RadioData/{pair.Key}.json", woahtherebuddy);
					}
				}
				//transfer to sql db
			}).Wait();
		}


	}
}
