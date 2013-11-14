using System;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;

namespace translinkupdater
{
	public class TranslationLinkUpdater
	{
		public TranslationLinkUpdater ()
		{
		}

		public delegate bool PerformSave (
			string wikitextBefore,string wikitextAfter);

		public delegate void Log (string message);

		private static void LogWrite (
			Log log, string format, params object[] args)
		{
			log (string.Format (format, args));
		}

		private static bool saveReturnTrue (string before, string after)
		{
			return true;
		}

		public static void Update (
			string startAt = "", int maxPages = 10,
			PerformSave saveCallback = null,
			Log logCallback = null)
		{
			logCallback ("UPDATE");
			if (saveCallback == null)
				saveCallback = saveReturnTrue;
			if (logCallback == null)
				logCallback = Console.WriteLine;

			var step = 20;
			var allPages = Api.PagesInCategory (
				"Svenska/Alla uppslag",
				ns: 0,
				step: step,
				maxPages: maxPages,
				startAt: startAt);
			var pages = new List<Api.Page> ();
			foreach (var page in allPages) {
				pages.Add (page);
				if (pages.Count == step) {
					UpdateBatch (pages, saveCallback, logCallback);
					pages.Clear ();
				}
			}
			if (pages.Count != 0)
				UpdateBatch (pages, saveCallback, logCallback);
		}

		protected static void UpdateBatch (
			List<Api.Page> pages,
			PerformSave saveCallback,
			Log log)
		{
			Console.WriteLine ("UPDATE BATCH");
			// Find links in pages
			var links = new Dictionary<string, List<string>> ();
			foreach (var page in pages) {
				LogWrite (log, "Got page {0}", page.Title);
				var translations = GetTranslations (page.Text);
				foreach (var translation in translations) {
					if (!links.ContainsKey (translation.LangCode)) {
						links.Add (translation.LangCode, new List<string> ());
					}
					links [translation.LangCode].Add (translation.Title);
				}
			}

			// Check whether the pages exist
			var linksExist = new Dictionary<string, IDictionary<string, bool>> ();
			foreach (var l in links) {
				IDictionary<string, bool> pagesExist = null;
				if (LanguageHasWiki (l.Key)) {
					try {
						pagesExist = Api.PagesExist (l.Key, l.Value);
					} catch (WebException e) {
						if (e.Message == "Max. redirections exceeded.") {
							// Ok
						} else {
							throw e;
						}
					} catch (JsonReaderException) {
						// Ok (probably a redirect to Incubator)
					}
				} 
				if (pagesExist == null) {
					pagesExist = new Dictionary<string, bool> ();
					foreach (var title in l.Value)
						pagesExist.Add (title, false);
				}
				linksExist.Add (l.Key, pagesExist);
			}

			// Update the pages and save them
			foreach (var page in pages) {
				string summary;
				string wikitext;
				var oldWikitext = page.Text;
				if (UpdateTranslations (
						oldWikitext,
						linksExist,
						out wikitext,
						out summary)) {
					if (saveCallback (oldWikitext, wikitext)) {
						page.Text = wikitext;

						Api.SavePage (
							page,
							summary,
							nocreate: true);
						LogWrite (log, "Saved {0} ({1})",
						         page.Title,
						         summary);
					} else {
						LogWrite (log, "Doesn't need update: {0}",
						          page.Title);
					}
				}
			}
		}

		static bool UpdateTranslations (
			string wikitext,
			Dictionary<string, IDictionary<string, bool>> linksExist,
			out string newWikitext,
			out string summary)
		{
			var updatedSections = new List<Section> ();
			var lastEnd = 0;
			foreach (var t in GetTranslations(wikitext)) {
				// Add the previous, non-translation section
				updatedSections.Add (new Section (wikitext, lastEnd, t.Start - lastEnd));
				t.Exists = linksExist [t.LangCode] [t.Title];
				updatedSections.Add (t);
				lastEnd = t.End;
			}
			updatedSections.Add (new Section (wikitext, lastEnd, wikitext.Length - lastEnd));
			newWikitext = string.Join ("", updatedSections);
			if (newWikitext != wikitext) {
				summary = "uppdaterar {{ö}}";
				return true;
			} else {
				summary = null;
				return false;
			}
		}

		private static IDictionary<string, string>LanguagesByCode = null;
		private static IDictionary<string, string>LanguagesByName = null;
		private static ISet<string>LanguagesWithWiki = null;

		public static bool LanguageHasWiki (string langCode)
		{
			if (LanguagesWithWiki == null) {
				var map = Api.Get ("action=query&meta=siteinfo&siprop=interwikimap&sifilteriw=local");
				LanguagesWithWiki = new SortedSet<string> ();

				foreach (var iw in (IEnumerable<JToken>)map["query"]["interwikimap"]) {
					if ("https://" + iw ["prefix"] + ".wiktionary.org/wiki/$1" ==
						"" + iw ["url"]) {
						LanguagesWithWiki.Add ((string)iw ["prefix"]);
					}
				}
			}
			return LanguagesWithWiki.Contains (langCode);

		}

		public static string GetLanguageCode (string langName)
		{
			if (LanguagesByName == null)
				InitLanguages ();
			return LanguagesByName [langName];
		}

		public static string GetLanguageName (string langCode)
		{
			if (LanguagesByCode == null)
				InitLanguages ();
			return LanguagesByCode [langCode];
		}

		private static void InitLanguages ()
		{
			var wikitext = Api.GetPage ("Wiktionary:Stilguide/Språknamn").Text;
			LanguagesByCode = new Dictionary<string, string> ();
			LanguagesByName = new Dictionary<string, string> ();
			foreach (Match match in Regex.Matches (wikitext, @"\n\{\{språk\|([^\|]+)\|([^\|]+)\|")) {
				LanguagesByCode.Add (
					match.Groups [2].Captures [0].Value,
					match.Groups [1].Captures [0].Value
				);
				LanguagesByName.Add (
					match.Groups [1].Captures [0].Value,
					match.Groups [2].Captures [0].Value
				);
			}
		}

		public static IEnumerable<Translation> GetTranslations (string wikitext)
		{
			foreach (var section in GetTranslationSections(wikitext)) {
				foreach (var translation in GetTranslations(section)) {
					yield return translation;
				}
			}
		}

		private static IEnumerable<Translation> GetTranslations (Section section)
		{
			/*
			 * TODO find stuff that shouldn't be in the translations section:
			 * \n;
			 * \n:
			 * \n*:
			 * {{topp
			 * 1=
			 * 2=
			 */
			string lang = null;
			int nextStart = section.Start;

			foreach (var line in section.Text.Split('\n')) {
				int position = nextStart;
				nextStart += line.Length + 1; // +1 for \n
				if (!line.StartsWith ("*") || line.IndexOf (':') == -1)
					continue;
				if (!line.StartsWith ("**")) {
					var langName = line.Substring (1, line.IndexOf (':') - 1);
					lang = GetLanguageCode (langName);
				}
				if (lang == null) {
					Console.WriteLine ("No language found for " + line);
				}

				// {{ö|..|XXX}} or {{ö-|..|XXX}} or [[XXX]]
				var reAdditionalTemplate = @"( ?\{\{(m|f|mf|c|u|n|p|d|s)\}\})?";
				var matches = Regex.Matches (
					line,
					// template matches
					@"\{\{ö\-?\|[^\|]*\|" +
					@"([^\|\}]*)" + // 1 (word)
					@"([^\}]*)\}\}" + // 2 (additional params)
					reAdditionalTemplate + // 3, 4 (additional template)
					@"|" +
				// link matches
					@"\[\[" +
					@"([^\|\]]+)" + // 5 (word)
					@"\]\]" +
					reAdditionalTemplate
				); // 6, 7 (additional template)

				foreach (Match m in matches) {
					string word;
					var additionalParams = "";
					if (m.Groups [1].Captures.Count > 0) {
						// template
						word = m.Groups [1].Captures [0].Value;
						additionalParams = m.Groups [2].Captures [0].Value;
						if (m.Groups [4].Captures.Count > 0)
							additionalParams += m.Groups [4].Captures [0].Value;
					} else {
						// raw link
						word = m.Groups [5].Captures [0].Value;
						if (m.Groups [7].Captures.Count > 0)
							additionalParams += m.Groups [4].Captures [0].Value;

					}
					if (additionalParams == "|c")
						additionalParams = "|u";

					var translation = new Translation (
						langCode: lang,
						title: word,
						additionalParams: additionalParams,
						allText: section.AllText,
						start: position + m.Index,
						length: m.Length,
						match: m);

					yield return translation;
				}
			}
		}
		public class Translation : Section
		{
			private string _langCode;
			private string _title;
			private string _additionalParams;

			public bool Exists {
				get;
				set;
			}

			private Match _match;

			public Match Match {
				get { return _match; }
			}

			public string LangCode {
				get { return _langCode; }
			}

			public string Title {
				get { return _title; }
			}

			public string AdditionalParams {
				get { return _additionalParams; }
			}

			public Translation (
				string langCode, string title,
				string additionalParams,
				string allText,
				int start, int length,
				Match match,
				bool exists = true)
				: base(allText, start, length)
			{
				_langCode = langCode;
				_title = title;
				_additionalParams = additionalParams;
				_match = match;
				Exists = exists;
			}

			override public string ToString ()
			{
				return string.Join ("", new string[] {
					Exists ? "{{ö|" : "{{ö-|",
					_langCode, "|",
					_title,
					_additionalParams,
					"}}"
				}
				);
			}
		}

		public static List<Section> GetTranslationSections (string wikitext)
		{
			var res = new List<Section> ();
			for (int end = 0; end < wikitext.Length;) {
				int s1 = wikitext.IndexOf ("\n====Översättningar====\n", end);
				int s2 = wikitext.IndexOf ("\n====Motsvarande namn på andra språk====\n", end);
				int start = s1 == -1 ? s2
					: s2 == -1 ? s1
					: Math.Min (s1, s2);
				if (start == -1)
					break;
				start++;

				end = wikitext.IndexOf ("\n=", start);
				if (end == -1)
					end = wikitext.Length;
				else
					end++;
				res.Add (new Section (wikitext, start, end - start));
			}
			return res;
		}

		public class Section
		{
			private string _allText;
			private int _start, _length;

			public string AllText {
				get { return _allText; }
			}

			public string Text {
				get {
					if (_length == 0)
						return "";
					else
						return _allText.Substring (_start, _length);
				}
			}

			public int Start {
				get { return _start; }
			}

			public int End {
				get { return _start + _length; }
			}

			public int Length {
				get { return _length; }
			}

			public Section (string allText, int start, int length)
			{
				if (start + length > allText.Length) {
					throw new IndexOutOfRangeException ("start + length > allText.length");
				}
				_allText = allText;
				_start = start;
				_length = length;
			}

			override public string ToString ()
			{
				return Text;
			}


		}
	}
}

