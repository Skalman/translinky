using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace translinkupdater
{
	public class Language
	{

		private static IDictionary<string, string>ByCode = null;
		private static IDictionary<string, string>ByName = null;
		private static ISet<string>WithWiki = null;

		public static bool HasWiki (string langCode)
		{
			if (WithWiki == null) {
				var map = Api.Get ("action=query&meta=siteinfo&siprop=interwikimap&sifilteriw=local");
				WithWiki = new SortedSet<string> ();

				foreach (var iw in (IEnumerable<JToken>)map["query"]["interwikimap"]) {
					if ("https://" + iw ["prefix"] + ".wiktionary.org/wiki/$1" ==
						"" + iw ["url"]) {
						WithWiki.Add ((string)iw ["prefix"]);
					}
				}
			}
			return WithWiki.Contains (langCode);

		}

		public static string GetCode (string langName)
		{
			if (ByName == null)
				Init ();
			return ByName [langName];
		}

		public static string GetName (string langCode)
		{
			if (ByCode == null)
				Init ();
			return ByCode [langCode];
		}

		private static void Init ()
		{
			var wikitext = Api.GetPage ("Wiktionary:Stilguide/Språknamn").Text;
			ByCode = new Dictionary<string, string> ();
			ByName = new Dictionary<string, string> ();
			foreach (Match match in Regex.Matches (wikitext, @"\n\{\{språk\|([^\|]+)\|([^\|]+)\|")) {
				ByCode.Add (
					match.Groups [2].Captures [0].Value,
					match.Groups [1].Captures [0].Value
				);
				ByName.Add (
					match.Groups [1].Captures [0].Value,
					match.Groups [2].Captures [0].Value
				);
			}
		}

	}
}

