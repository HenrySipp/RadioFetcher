using System;
using Newtonsoft.Json;
using SQLite;
using SQLitePCL;

namespace RadioFetcher
{
	public class Station
	{
		//[PrimaryKey]
		public string callsign { get; set; }

		public string frequency { get; set; }


		public float latitude { get; set; }

		public float longitude { get; set; }


		public string city { get; set; }

		public string state { get; set; }

		public string broadcastArea { get; set; }

		public string format { get; set; }

		public string affiliations { get; set; }

		public string website { get; set; }
	}
}
