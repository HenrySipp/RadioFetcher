using System;
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
							callsign = string.Empty,
							frequency = string.Empty,
							city = string.Empty,
							state = string.Empty,
							broadcastArea = string.Empty,
							affiliations = string.Empty,
							website = string.Empty
						};
						var callsign = (string)fake["callsign"];
						var frequency = (string)fake["Frequency"];
						var city = (string)fake["city"];
						//var state = (string)fake[""];
						var area = (string)fake["Broadcast area"];
						var affil = (string)fake["Affiliations"];
						var site = (string)fake["Website"];



						var coords = (string)fake["Transmitter coordinates"]; //TODO: Don't rely on this. only like 2/3 have it

						if (callsign != null
							//&& city != null
							&& frequency != null
							&& coords != null)
						{


							station.callsign = callsign;
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

							station.latitude = float.Parse(latlon[1]);
							station.longitude = float.Parse(latlon[2]);

							if (db == null)
								Console.Out.Write("shit");
							
							await db.InsertAsync(station);
						}
					}
				}
			}

			var query = await db.Table<Station>().Where(s => s.callsign != null).ToListAsync();

			var a = query[0];

		}
	}
}
