using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace translinkupdater
{
	public class Language
	{
		private static IDictionary<string, string>Misspellings = new Dictionary<string, string> () {
			{"atjehnesiska", "acehnesiska"},
			{"azerbadjanska", "azerbajdzjanska"},
			{"bashkiriska", "basjkiriska"},
			{"dhivehi", "divehi"},
			{"friulian", "friuliska"},
			{"friulska", "friuliska"},
			{"galisiska", "galiciska"},
			{"indomesiska", "indonesiska"},
			{"jiddish", "jiddisch"},
			{"khakas", "khakasiska"},
			{"kirgisiska", "kirgiziska"},
			{"komi-syrjänska", "komi"},
			{"chakassiska", "khakasiska"},
			{"kazakhiska", "kazakiska"},
			{"kumykiska", "kumyk"},
			{"makedoniska", "makedonska"},
			{"manniska", "manx"},
			{"moksha", "moksja"},
			{"nynorsk", "nynorska"},
			{"panjabi", "punjabi"},
			{"pitjantjara", "pitjantjatjara"},
			{"português", "portugisiska"},
			{"sotho", "sesotho"},
			{"spanksa", "spanska"},
			{"tajik", "tadzjikiska"},
			{"uighur", "uiguriska"},
			{"ukrainiska", "ukrainska"},
			{"österikiska", "österikiska"},
		};
		private static IDictionary<string, string>UnofficialByName = new Dictionary<string, string> () {
			{"serbokroatiska", "<!--sh-->"},

			// Ignored or no ISO 639-3 code
			{"kalmuckiska", ""},
			{"toki pona", ""},
		};
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
			string val = null;
			if (ByName.TryGetValue (langName, out val))
				return val;
			else if (Misspellings.TryGetValue (langName, out val))
				return ByName [val];
			else if (UnofficialByName.TryGetValue(langName, out val))
				return val;
			else
				throw new Exception("Unrecognized language name '" + langName + "'");
		}

		public static string GetName (string langCode)
		{
			if (ByCode == null)
				Init ();
			return ByCode [langCode];
		}

		public static bool CorrectMisspellings (Section section)
		{
			var newText = section.Text;
			foreach (var x in Misspellings) {
				if (newText.IndexOf (x.Key) != -1) {
					newText = newText.Replace (
						"\n*" + x.Key + ": ",
						"\n*" + x.Value + ": ");
				}
			}
			if (newText == section.Text) {
				return false;
			} else {
				section.Text = newText;
				return true;
			}
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

