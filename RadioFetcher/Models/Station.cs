using System;
using Newtonsoft.Json;
using SQLite;
using SQLitePCL;

namespace RadioFetcher
{
	public class Station
	{
		public string callsign { get; set; }

		public string frequency { get; set; }

		[Indexed]
		public float latitude { get; set; }

		[Indexed]
		public float longitude { get; set; }


		public string city { get; set; }

		public string state { get; set; }

		public string broadcastArea { get; set; }

		public string format { get; set; }

		[MaxLength(255)]
		public string affiliations { get; set; }

		public string website { get; set; }

		public bool isAdultAlternative { get; set; }

		public bool isAdultContemporary { get; set; } //Top 40

		public bool isOldies { get; set; }

		public bool isChildren { get; set; }

		public bool isReligious { get; set; }

		public bool isCountry { get; set; }

		public bool isHipHop { get; set; }

		public bool isClassicHits { get; set; }

		public bool isClassical { get; set; }

		public bool isSchool { get; set; }

		public bool isComedy { get; set; }

		public bool isFullService { get; set; }

		public bool isJazz { get; set; }


		public bool isPublicRadio { get; set; }

		public bool isRhythmicContemporary { get; set; }

		public bool isRock { get; set; }

		public bool isNewsTalk { get; set; }

		public bool isSports { get; set; }

		public bool isSpanish { get; set; }

		public bool isVariety { get; set; }

	}
}
