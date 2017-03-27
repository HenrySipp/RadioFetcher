using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace RadioFetcher
{
	public class Processor
	{

		public static async Task Process()
		{

			string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			var db = new SQLiteAsyncConnection(desktop + "/RadioData/_radioStore.db");
			await db.CreateTableAsync<Station>();

			string[] files = Directory.GetFiles(desktop + "/RadioData");

			List<Station> stationsWithoutFormats = new List<Station>();
			foreach (string path in files)
			{
				if (path.Contains(".DS")
					|| path.Contains(".db"))
					continue;

				IList<JObject> items;
				using (StreamReader r = new StreamReader(path))
				{
					string json = await r.ReadToEndAsync();
					items = JsonConvert.DeserializeObject<IList<JObject>>(json);
				}

				Console.Out.WriteLine(path);

				foreach (var fake in items)
				{
					if (fake != null)
					{
						var station = new Station() //JsonConvert.DeserializeObject<Station>(fake.ToString());
						{
						};
						var callsign = (string)fake["callsign"];
						var frequency = (string)fake["Frequency"];
						var city = (string)fake["City"] ?? string.Empty;
						var format = (string)fake["Format"] ?? string.Empty;

						//var state = (string)fake[""];
						var area = (string)fake["Broadcast area"] ?? string.Empty;
						var affil = (string)fake["Affiliations"] ?? string.Empty;
						var site = (string)fake["Website"] ?? string.Empty;



						var coords = (string)fake["Transmitter coordinates"]; //TODO: Don't rely on this. only like 2/3 have it

						if (!String.IsNullOrEmpty(callsign)
							//&& city != null
							&& !String.IsNullOrEmpty(format)
							&& !String.IsNullOrEmpty(frequency)
							&& !String.IsNullOrEmpty(coords)
						   )
						{
							station.callsign = callsign;
							station.format = format;
							station.frequency = frequency;
							station.city = city;
							station.broadcastArea = area;
							station.affiliations = affil;
							station.website = site;

							//39°18′46″N 123°46′57″W﻿ / ﻿39.31278°N 123.78250°W﻿ / 39.31278; -123.78250

							var lastSlashPos = coords.LastIndexOf('/');
							coords = coords.Substring(lastSlashPos + 1); // 39.31278; -123.78250
							coords = coords.Replace(";", "");
							string[] latlon = coords.Split(' ');

							if (latlon.Length == 3)
							{


								float lat = 0f;
								float lon = 0f;
								float.TryParse(latlon[1], out lat);
								float.TryParse(latlon[2], out lon);

								//Console.Out.WriteLine(latlon.Length);
								//Console.Out.WriteLine($"Coords: {coords}        LatLng: {lat} {lon}");
								station.latitude = lat;
								station.longitude = lon;

								if (!AboutEqual(lat, 0) && !AboutEqual(lon, 0))
								{
									if (db == null)
										Console.Out.Write("shit");

								}
							}

							//station.latitude = float.Parse(latlon[1]);
							//station.longitude = float.Parse(latlon[2]);

							var hasformat = setFormat(ref station, format);
							if (!hasformat)
								stationsWithoutFormats.Add(station);

							await db.InsertAsync(station);

						}
					}
				}
			}


			var query = db.Table<Station>();

			var unusedformats = stationsWithoutFormats.Select((Station arg) => arg.format).Distinct().OrderBy((arg) =>  arg).ToList();
			foreach (var form in unusedformats)
			{
				Console.Out.WriteLine(form);
			}
			var yes = await query.ToListAsync();

			var a = yes.Count;

			Console.Out.WriteLine($"Number of stations with formats: {a - stationsWithoutFormats.Count}");
			Console.Out.WriteLine($"Number of stations without formats: {stationsWithoutFormats.Count}");

		}

		//yeah screw my carpal tunnel hands now
		static bool setFormat(ref Station station, string format)
		{
			bool hasFormat = false;
			string f = format.ToLower();
			if (f.Contains("alternative"))
			{
				station.isAdultAlternative = true;
				hasFormat = true;
			}

			if (f.Contains("adult contemporary")
				|| f.Contains(" ac")
				|| f.Contains("contemporary hit")
				|| f.Contains("top-40")
				|| f.Contains("top 40")
				|| f.Contains("adult hits")
				|| f.Contains("variety hits")
				  || f.Contains("mainstream"))
			{
				station.isAdultContemporary = true;
				hasFormat = true;
			}

			if (f.Contains("standards")
				|| f.Contains("nostalgia")
			    || f.Contains("oldies"))
			{
				station.isOldies = true;
				hasFormat = true;
			}

			if (f.Contains("children"))
			{
				station.isChildren = true;
				hasFormat = true;
			}


			if (f.Contains("christian")
				|| f.Contains("gospel")
				|| f.Contains("christmas")
			   	|| f.Contains("religious")
				|| f.Contains("religion")
			    || f.Contains("catholic"))
			{
				station.isReligious = true;
				hasFormat = true;
			}

			if (f.Contains("country"))
			{
				station.isCountry = true;
				hasFormat = true;
			}

			if (f.Contains("hip hop")
				|| f.Contains("hip-hop"))
			{
				station.isHipHop = true;
				hasFormat = true;
			}

			if (f.Contains("classic hits"))
			{
				station.isClassicHits = true;
				hasFormat = true;
			}

			if (f.Contains("classical"))
			{
				station.isClassical = true;
				hasFormat = true;
			}

			if (f.Contains("college")
				|| f.Contains("high school")
				|| f.Contains("school")
			    || f.Contains("school"))
			{
				station.isSchool = true;
				hasFormat = true;
			}


			if (f.Contains("comedy"))
			{
				station.isComedy = true;
				hasFormat = true;
			}

			if (f.Contains("full service"))
			{
				station.isFullService = true;
				hasFormat = true;
			}

			if (f.Contains("jazz"))
			{
				station.isJazz = true;
				hasFormat = true;
			}


			if (f.Contains("public")
				|| f.Contains("npr")
			    || f.Contains("community"))
			{
				station.isPublicRadio = true;
				hasFormat = true;
			}

			if (f.Contains("rhythmic"))
			{
				station.isRhythmicContemporary = true;
				hasFormat = true;
			}

			if (f.Contains("rock"))
			{
				station.isRock = true;
				hasFormat = true;
			}
			if (f.Contains("news")
				|| f.Contains("talk"))
			{
				station.isNewsTalk = true;
				hasFormat = true;
			}

			if (f.Contains("sports"))
			{
				station.isSports = true;
				hasFormat = true;
			}

			if (f.Contains("spanish")
				|| f.Contains("mexican")
				|| f.Contains("tejano")
			    || f.Contains("ranchera")) 
			{
				station.isSpanish = true;
				hasFormat = true;
			}

			if (f.Contains("variety")
				|| f.Contains("80's")
			    || f.Contains("80s"))
			{
				station.isVariety = true;
				hasFormat = true;
			}





			return hasFormat;
		}

		public static bool AboutEqual(double x, double y)
		{
			double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
			return Math.Abs(x - y) <= epsilon;
		}

	}
}
