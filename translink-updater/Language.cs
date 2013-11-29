using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace translinkupdater
{
	public class Language
	{
		private static IDictionary<string, string>Misspellings = new Dictionary<string, string> () {
			{"afrikaan", "afrikaans"},
			{"armenisk", "armeniska"},
			{"atjehnesiska", "acehnesiska"},
			{"azerbadjanska", "azerbajdzjanska"},
			{"azerbajdjanska", "azerbajdzjanska"},
			{"bashkiriska", "basjkiriska"},
			{"bulgarisk", "bulgariska"},
			{"danske", "danska"},
			{"dhivehi", "divehi"},
			{"engelsk", "engelska"},
			{"engeska", "engelska"},
			{"english", "engelska"},
			{"estländska", "estniska"},
			{"fince", "finska"},
			{"finka", "finska"},
			{"finskaa", "finska"},
			{"finsk kale romani", "finsk romani"},
			{"franka", "franska"},
			{"franskan", "franska"},
			{"french", "franska"},
			{"västfrisiska", "frisiska"},
			{"friulian", "friuliska"},
			{"friulska", "friuliska"},
			{"galisiska", "galiciska"},
			{"gallego", "galiciska"},
			{"grekiska, modern", "grekiska"},
			{"modern grekiska", "grekiska"},
			{"modern Grekiska", "grekiska"},
			{"hornjserbsce", "högsorbiska"},
			{"indomesiska", "indonesiska"},
			{"italianska", "italienska"},
			{"jiddish", "jiddisch"},
			{"khakas", "khakasiska"},
			{"kirgisiska", "kirgiziska"},
			{"komi-syrjänska", "komi"},
			{"chakassiska", "khakasiska"},
			{"kazakhiska", "kazakiska"},
			{"kumykiska", "kumyk"},
			{"plattyska", "lågtyska"},
			{"makedoniska", "makedonska"},
			{"malaysiska", "malajiska"},
			{"manniska", "manx"},
			{"maoriska", "maori"},
			{"moksha", "moksja"},
			{"nederlandska", "nederländska"},
			{"nederlädska", "nederländska"},
			{"nederländiska", "nederländska"},
			{"nederlänska", "nederländska"},
			{"nynorsk", "nynorska"},
			{"nynorskal", "nynorska"},
			{"ojibwa", "ojibwe"}, // should maybe be the other way around!
			{"panjabi", "punjabi"},
			{"pitjantjara", "pitjantjatjara"},
			{"portugisisiska", "portugisiska"},
			{"portugiska", "portugisiska"},
			{"português", "portugisiska"},
			{"portugusiska", "portugisiska"},
			{"rätotomanska", "rätoromanska"},
			{"sardinska", "sardiska"},
			{"skotsk-gäliska", "skotsk gäliska"},
			{"sotho", "sesotho"},
			{"sloveniska", "slovenska"},
			{"spanksa", "spanska"},
			{"tajik", "tadzjikiska"},
			{"tigrinya", "tigrinska"},
			{"tjeckien", "tjeckiska"},
			{"turk", "turkiska"},
			{"turkmenska", "turkmeniska"},
			{"tysla", "tyska"},
			{"uighur", "uiguriska"},
			{"ukrainiska", "ukrainska"},
			{"vespsiska", "vepsiska"},
			{"volapük/volapyk", "volapük"},
			{"võro", "võru"},
			{"österikiska", "österikiska"},
		};
		private static IDictionary<string, string>UnofficialByName = new Dictionary<string, string> () {
			// Being moved...
			{"norska", "no"},

			// Discussed, but have template
			{"kantonesiska", "yue"},

			// Not discussed, don't template, but have ISO 639-3 code
			{"alabama", "<!--akz-->"},
			{"akkadiska", "<!--akk-->"},
			{"elsassiska", "<!--als-->"},
			{"fijiansk hindi", "<!--hif-->"},
			{"flamländska", "vls"},
			{"herero", "<!--hz-->"},
			{"hettitiska", "<!--hit-->"},
			{"kampidanesiska", "<!--sro-->"},
			{"luhya", "<!--luy-->"},
			{"kinesiska (mandarin)", "<!--zh-->"},
			{"kiribatiska", "<!--gil-->"},
			{"kvänska", "<!--fkv-->"},
			{"logudoresiska", "<!--src-->"},
			{"nentsiska", "<!--yrk-->"},
			{"nordkurdiska", "<!--kmr-->"},
			{"sassaresiska", "<!--sdc-->"},
			{"serbokroatiska", "<!--sh-->"},

			// Ignored or no ISO 639-3 code
			{"kalabriska", ""},
			{"kalmuckiska", ""},
			{"lombardiska", ""},
			{"lågsaxiska", ""},
			{"lågsachsiska", ""},
			{"meru", ""},
			{"nedersaxiska", ""},
			{"samiska", ""},
			{"saterfrisiska", ""},
			{"slovio", ""},
			{"sorbiska", ""},
			{"toki pona", ""},
			{"valencianska", ""},
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
			else if (UnofficialByName.TryGetValue (langName, out val))
				return val;
			else if (langName.StartsWith ("{{") && langName.EndsWith ("}}"))
				return langName.Substring (2, langName.Length - 4);
			else
				throw new LanguageException ("Unrecognized language name '" + langName + "'");
		}

		public class LanguageException : Exception
		{
			public LanguageException (string message) : base(message)
			{
			}
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
			if (newText.IndexOf ("\n*{{") != -1) {
				newText = Regex.Replace (
					newText,
					@"\n\*\{\{([a-z-]+)\}\}:",
					new MatchEvaluator (ExpandTemplateCallback)
				);
			}
			if (newText == section.Text) {
				return false;
			} else {
				section.Text = newText;
				return true;
			}
		}

		private static string ExpandTemplateCallback (Match m)
		{
			return "\n*" + GetName (m.Groups [1].Captures [0].Value) + ":";
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

