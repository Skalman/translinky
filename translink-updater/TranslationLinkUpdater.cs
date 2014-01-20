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
		public delegate bool PerformSave (
			string title,
			string summary,
			out string changedSummary,
			string wikitextBefore,
			string wikitextAfter,
			out string changedWikitext);

		public delegate void PageDoneCallback (string title);

		public delegate void Log (string message);

		private static void LogWrite (
			Log log, string format, params object[] args)
		{
			log (string.Format (format, args));
		}

		private static bool saveReturnTrue (string title,
		                                    string summary, out string changedSummary,
		                                    string before, string after, out string changedWikitext)
		{
			changedSummary = summary;
			changedWikitext = after;
			return true;
		}

		private static void Noop (string title)
		{
		}

		public static void Update (
			/*string startAt = "",*/
			IEnumerable<Api.Page> pages,
			int maxPages = 10,
			PerformSave saveCallback = null,
			PageDoneCallback pageDoneCallback = null,
			Log logCallback = null)
		{
			logCallback ("UPDATE PAGES");
			if (saveCallback == null)
				saveCallback = saveReturnTrue;
			if (pageDoneCallback == null)
				pageDoneCallback = Noop;
			if (logCallback == null)
				logCallback = Console.WriteLine;

			var step = 2000;

			var pagesInBatch = new List<Api.Page> ();
			foreach (var page in pages) {
				pagesInBatch.Add (page);
				if (pagesInBatch.Count == step) {
					UpdateBatch (pagesInBatch, saveCallback, pageDoneCallback, logCallback);
					pagesInBatch.Clear ();
				}
			}
			if (pagesInBatch.Count != 0)
				UpdateBatch (pagesInBatch, saveCallback, pageDoneCallback, logCallback);
		}

		protected static void UpdateBatch (
			List<Api.Page> pages,
			PerformSave saveCallback,
			PageDoneCallback pageDoneCallback,
			Log log)
		{
			Console.WriteLine ("UPDATE BATCH");
			// Find links in pages
			var links = new Dictionary<string, List<string>> ();
			foreach (var page in pages) {
				Console.WriteLine ("Got page {0}", page.Title);
				var translations = GetTranslations (page.Title, page.Text);
				foreach (var translation in translations) {
					if (!links.ContainsKey (translation.LangCode)) {
						links.Add (translation.LangCode, new List<string> ());
					}
					links [translation.LangCode].Add (translation.Title);
				}
			}

			LogWrite (log, "See whether pages exist on {0} wiki(s)...", links.Count);

			// Check whether the pages exist
			var linksExist = new Dictionary<string, IDictionary<string, bool>> ();
			foreach (var l in links) {
				IDictionary<string, bool> pagesExist = null;
				if (Language.HasWiki (l.Key)) {
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
						pagesExist [title] = false;
				}
				linksExist.Add (l.Key, pagesExist);
			}

			// Update the pages and save them
			foreach (var page in pages) {
				string proposedSummary;
				string proposedWikitext;
				var oldWikitext = page.Text;
				if (UpdateTranslations (
						page.Title,
						oldWikitext,
						linksExist,
						out proposedWikitext,
						out proposedSummary)) {
					string summary;
					string wikitext;

					if (saveCallback (
							page.Title,
							proposedSummary, out summary,
							oldWikitext, proposedWikitext, out wikitext)) {
						page.Text = wikitext;

						try {
							Api.SavePage (
								page,
								summary,
								nocreate: true,
								bot: proposedWikitext == wikitext);
							LogWrite (log, "{0}: Saved ({1})",
							         page.Title,
							         summary);
						} catch (Api.EditConflictException) {
							LogWrite (log, "{0}: Edit conflict, skipped ({1})",
							          page.Title,
							          summary);
						}
					} else {
						LogWrite (log, "{0}: Chose not to save ({1})",
						         page.Title,
						         summary);
					}
				} else {
					Console.WriteLine ("{0}: No update",
					                  page.Title);
				}
				pageDoneCallback (page.Title);
			}
		}

		static bool UpdateTranslations (
			string title,
			string wikitext,
			Dictionary<string, IDictionary<string, bool>> linksExist,
			out string newWikitext,
			out string summary)
		{
			var sections = new List<Section> ();
			var lastEnd = 0;
			var summaryParts = new SortedSet<string> ();

			// Not just translations, but important formatting
			wikitext = FormatPage (wikitext, summaryParts);

			foreach (var s in GetTranslationSections(title, wikitext)) {
				sections.Add (new Section (title, wikitext, lastEnd, s.Start - lastEnd));
				FormatTranslationSection (s, summaryParts);
				UpdateTranslations (s, linksExist, summaryParts);
				sections.Add (s);
				lastEnd = s.End;
			}
			sections.Add (new Section (title, wikitext, lastEnd, wikitext.Length - lastEnd));
			if (summaryParts.Count > 0) {
				newWikitext = string.Join ("", sections);
				summary = string.Join ("; ", summaryParts);
				return true;
			} else {
				newWikitext = wikitext;
				summary = null;
				return false;
			}
		}

		static string FormatPage (string wikitext, ISet<string> summaryParts)
		{
			// Added on many pages by User:Pametzma
			if (wikitext.IndexOf ("----") != -1) {
				var newWikitext = Regex.Replace (wikitext, @"\n+(\{\{nollpos\}\}\n|\-{4,}\n)+\n*", "\n\n");
				if (wikitext != newWikitext) {
					summaryParts.Add ("ta bort onödig {{nollpos}} och ----");
					wikitext = newWikitext;
				}
			}

			if (wikitext.IndexOf ("\n=====Översättningar=====\n") != -1) {
				summaryParts.Add ("översättningsrubrik är H4");
				wikitext = wikitext.Replace ("\n=====Översättningar=====\n", "\n====Översättningar====\n");
			}

			if (wikitext.IndexOf ("\n====Motsvarande namn på andra språk====\n") != -1) {
				summaryParts.Add ("'Motsvarande namn på andra språk' > 'Översättningar'");
				wikitext = wikitext.Replace ("\n====Motsvarande namn på andra språk====\n", "\n====Översättningar====\n");
			}

			return wikitext;
		}

		protected static void FormatTranslationSection (Section section, ISet<string> summary)
		{
			if (section.Text.IndexOf (" \n") != -1 || section.Text.IndexOf ("\t\n") != -1) {
				section.Text = Regex.Replace (section.Text, @"[ \t]+\n", "\n");
				summary.Add ("-whitespace");
			}
			if (section.Text.IndexOf ("{{topp") != -1) {
				if (section.Text.IndexOf ("{{topp-göm") != -1)
					section.Text = section.Text.Replace ("{{topp-göm", "{{topp");
				section.Text = Regex.Replace (section.Text, @"\{\{(topp[^\}]*|mitt|botten)\}\}", "{{ö-$1}}");
				summary.Add ("{{topp}} > {{ö-topp}}");
			}
			if (section.Text.IndexOf ("\n*:") != -1) {
				section.Text = section.Text.Replace ("\n*:", "\n**");
				summary.Add ("underspråk med **");
			}
			if (section.Text.IndexOf ("\n:*") != -1) {
				section.Text = section.Text.Replace ("\n:*", "\n**");
				summary.Add ("underspråk med **");
			}
			if (section.Text.IndexOf ("\n**") != -1
				&& Regex.IsMatch (section.Text, @"\*[^:\n[\]{}]+\n\*\*")) {
				section.Text = Regex.Replace (
					section.Text,
					@"(\*[^:\n[\]{}]+)(\n\*\*)",
					"$1:$2");
				summary.Add ("huvudspråk följt av kolon");

			}
			if (section.Text.IndexOf ("\n**bokmål:") != -1 || section.Text.IndexOf ("\n**nynorska:") != -1) {
				var newText = section.Text;
				// Two sub-languages
				newText = Regex.Replace (
					newText,
					@"\n" +
					@"\*(bokmål|norska):\n" +
					@"\*\*(bokmål|nynorska?): ([^\n]+)\n" +
					@"\*\*(bokmål|nynorska?): ([^\n]+)\n",
					"\n" +
					"*$2: $3\n" +
					"*$4: $5\n"
				);
				// Two sub-languages
				newText = Regex.Replace (
					newText,
					@"\n" +
					@"\*(bokmål|norska):\n" +
					@"\*\*(bokmål|nynorska?): ([^\n]+)\n",
					"\n" +
					"*$2: $3\n"
				);
				if (newText != section.Text) {
					section.Text = newText;
					summary.Add ("fixa norska");
				}
			}
			if (section.Text.IndexOf ("\n'''") != -1) {
				section.Text = Regex.Replace (
					section.Text,
					@"\n'''([^\n]+?)'''\n\{\{ö\-topp\|?\}\}",
					"\n{{ö-topp|$1}}");
				section.Text = Regex.Replace (
					section.Text,
					@"\n'''([^\n]+?)'''\n",
					// pass on to the next step
					"\n;$1\n");
				summary.Add ("använd {{ö-topp}}");
			}
			if (section.Text.IndexOf ("\n;") != -1 && section.Text.IndexOf ("{{ö-topp") != -1) {
				section.Text = Regex.Replace (
					section.Text,
					@"\n;([^\n]+)\n\{\{ö\-topp\|?\}\}",
					"\n{{ö-topp|$1}}");
			}
			if (section.Text.IndexOf ("\n;") != -1) {
				var lines = section.Text.Split ('\n');
				var startedAt = -1;
				for (var i = 0; i < lines.Length; i++) {
					// end previous section
					if (startedAt != -1 && !lines [i].StartsWith ("*")) {
						InsertMidBottom (lines, startedAt, i);
						startedAt = -1;
					}

					// start new section
					if (lines [i].StartsWith (";")) {
						lines [i] = "{{ö-topp|" + lines [i].Substring (1) + "}}";
						startedAt = i;
					}
				}
				section.Text = string.Join ("\n", lines);
				summary.Add ("använd {{ö-topp}}");
			}
			if (section.Text.IndexOf ("{{ö-topp") == -1) {
				var lines = section.Text.Split ('\n');
				var startedAt = -1;
				var i = 0;
				for (; i < lines.Length; i++) {
					if (startedAt == -1 && lines [i].StartsWith ("*")) {
						lines [i] = "{{ö-topp}}\n" + lines [i];
						startedAt = i;
					} else if (startedAt != -1 && !lines [i].StartsWith ("*")) {
						InsertMidBottom (lines, startedAt - 1, i);
						section.Text = string.Join ("\n", lines);
						summary.Add ("använd {{ö-topp}}");

						// only insert one - won't know what to do with broken lists anyway
						break;
					}
				}
			}
			if (Language.CorrectMisspellings (section)) {
				summary.Add ("korrigera språknamn");
			}
			if (section.Text.IndexOf ("\n* ") != -1 || section.Text.IndexOf ("\n** ") != -1) {
				section.Text = section.Text.Replace ("\n* ", "\n*");
				section.Text = section.Text.Replace ("\n** ", "\n**");
				summary.Add ("ta bort mellanslag före språknamn");
			}
			if (Regex.IsMatch (
					section.Text,
					@":(\{\{ö|\[\[)|" +
				@"[ \t]:[ \t]*(\{\{ö|\[\[)"
			)) {
				var newText = Regex.Replace (
					section.Text,
					@"(\n\*[^:\n\{\[]+?)[ \t]*:[ \t]*(\{\{ö|\[\[)",
					"$1: $2");
				if (section.Text != newText) {
					summary.Add ("språknamn+kolon+mellanslag");
					section.Text = newText;
				}
			}
			EnsureSorted (section, summary);
		}

		static void EnsureSorted (Section section, ISet<string> summary)
		{
			var lines = section.Text.Split ('\n');
			int startedAt = 0;
			bool sortError = false;
			bool hasHadSortError = false;
			string lastLine = null;
			for (var i = 0; i < lines.Length; i++) {
				if (lines [i].StartsWith ("{{ö-topp")) {
					startedAt = i;
				} else if (lines [i] == "{{ö-botten}}") {
					if (sortError) {
						Sort (lines, startedAt, i);
						hasHadSortError = true;
					}
					sortError = false;
					lastLine = null;
				} else if (!sortError && lines [i].StartsWith ("*") && !lines [i].StartsWith ("**")) {
					if (StringComparer.Cmp (lines [i], lastLine) < 0) {
						sortError = true;
					}
					lastLine = lines [i];
				}
			}
			if (hasHadSortError) {
				summary.Add ("sortera översättningar");
				lines = Array.ConvertAll (lines, delegate(string str) {
					return str == null ? "" : str + "\n";
				}
				);
				lines [lines.Length - 1] = lines [lines.Length - 1].TrimEnd ('\n');
				var wikitext = string.Join ("", lines);
				if (wikitext.IndexOf ("\n:") != -1) {
					throw new SortException ("Refuse to sort because translation section contains '\\n:'");
				}
				section.Text = wikitext;
			}
		}

		public class SortException : Exception
		{
			public SortException (string message) : base(message)
			{
			}
		}
		static void Sort (string[] lines, int before, int after)
		{
			var nullsInFront = 0;
			// prepare by removing {{ö-mitt}} and combining sublangs
			for (var i = before + 1; i < after; i++) {
				if (lines [i] == "{{ö-mitt}}" || lines [i] == "") {
					lines [i] = null;
					nullsInFront++;
				} else if (lines [i].StartsWith ("**")) {
					lines [i] = lines [i - 1] + "\n" + lines [i];
					lines [i - 1] = null;
					nullsInFront++;
				}
			}

			Array.Sort (lines, before + 1, after - before - 1, StringComparer.Singleton ());

			before = before + nullsInFront;
			lines [before + (after - before) / 2] += "\n{{ö-mitt}}";
		}
		private class StringComparer : IComparer<string>
		{
			private static StringComparer instance = null;

			public static StringComparer Singleton ()
			{
				if (instance == null)
					instance = new StringComparer ();
				return instance;
			}

			public static int Cmp (string str1, string str2)
			{
				return Singleton ().Compare (str1, str2);
			}

			public int Compare (string str1, string str2)
			{
				return String.CompareOrdinal (str1, str2);
			}
		}

		private static void UpdateTranslations (
			Section section,
			Dictionary<string, IDictionary<string, bool>> linksExist,
			ISet<string> summary)
		{
			var title = section.DocumentTitle;
			var sections = new List<Section> ();
			var lastEnd = 0;
			var wikitext = section.Text;
			foreach (var t in GetTranslations(new Section(title, wikitext))) {
				// Add the previous, non-translation section
				sections.Add (new Section (title, wikitext, lastEnd, t.Start - lastEnd));
				var langLinksExist = linksExist [t.LangCode];
				t.Exists = langLinksExist [t.Title];
				sections.Add (t);
				lastEnd = t.End;
			}
			sections.Add (new Section (title, wikitext, lastEnd, wikitext.Length - lastEnd));

			if (lastEnd != 0) {
				section.Text = string.Join ("", sections);
				if (wikitext != section.Text)
					summary.Add ("uppdatera {{ö}}");

			}
		}

		private static void InsertMidBottom (string[] lines, int beforeList, int afterList)
		{
			if (beforeList < 0)
				return;
			for (var i = beforeList + (afterList - beforeList) / 2; i <= afterList; i++) {
				if (i + 1 == afterList || !lines [i + 1].StartsWith ("**")) {
					lines [i] += "\n{{ö-mitt}}";
					break;
				}
			}
			lines [afterList - 1] += "\n{{ö-botten}}";
		}

		public static IEnumerable<Translation> GetTranslations (string title, string wikitext)
		{
			foreach (var section in GetTranslationSections(title, wikitext)) {
				foreach (var translation in GetTranslations(section)) {
					yield return translation;
				}
			}
		}

		private static IEnumerable<Translation> GetTranslations (Section section)
		{
			string lang = null;
			int nextStart = section.Start;
			var wikitext = section.Text;
			if (wikitext.IndexOf ("\n*:") != -1)
				wikitext = wikitext.Replace ("\n*:", "\n**");
			if (wikitext.IndexOf ("\n:*") != -1)
				wikitext = wikitext.Replace ("\n:*", "\n**");

			var lines = wikitext.Split ('\n');

			foreach (var line in lines) {
				int position = nextStart;
				nextStart += line.Length + 1; // +1 for \n

				if (!line.StartsWith ("*"))
					continue;
				if (line.IndexOf (':') == -1) {
					if (Regex.IsMatch(line, @"^\*[^[\]{}]+$")) {
						// assume missed colon
						var langName = line.Substring (1);
						lang = Language.GetCode (langName.Trim ());
					}
					continue;
				}
				if (line.StartsWith ("**")) {
					if (line.StartsWith ("**nynorska:")) {
						// special case which will be handled later either way
						lang = "nn";
					} else if (line.StartsWith ("**bokmål:")) {
						lang = "no";
					} else {
						// do nothing, use the language from the previous line
					}
				} else {
					var langName = line.Substring (1, line.IndexOf (':') - 1);
					lang = Language.GetCode (langName.Trim ());
				}
				if (lang == null) {
					Console.WriteLine ("No language found for " + line);
				}

				// {{ö|..|XXX}} or {{ö+|..|XXX}} or [[XXX]]
				var reAdditionalTemplate = @"( *[\{']{2}(m|f|mf|c|u|n|p|d|s)[\}']{2})?";
				var matches = Regex.Matches (
					line,
					// template matches: {{ö|en|translation}}
					@"\{\{ö[\-\+]?\|[^\|]*\|" +
					@"([^\|\}]*)" + // 1 (word)
					@"([^\}]*)\}\}" + // 2 (additional params)
					reAdditionalTemplate + // 3, 4 (additional template)
					@"|" +
				// link matches: [[translation]]
					@"\[\[" +
					@"([^\|\]]+)" + // 5 (word)
					@"(\|[^\|\]]+)?" + // 6 (link text)
					@"\]\]" +
					reAdditionalTemplate
				); // 7, 8 (additional template)

				foreach (Match m in matches) {
					string word;
					var additionalParams = "";
					if (m.Groups [1].Captures.Count > 0) {
						// template
						word = m.Groups [1].Captures [0].Value;
						additionalParams = m.Groups [2].Captures [0].Value;
						if (m.Groups [4].Captures.Count > 0)
							additionalParams += "|" + m.Groups [4].Captures [0].Value;
					} else {
						// raw link
						word = m.Groups [5].Captures [0].Value;
						if (m.Groups [8].Captures.Count > 0)
							additionalParams += "|" + m.Groups [8].Captures [0].Value;
						// link text
						if (m.Groups [6].Captures.Count > 0) {
							// handle [[fusion|nuclear fusion]],
							// [[#Engelska|fusion]], [[#Engelska|nuclear fusion]]
							var skip = false;
							if (word [0] == '#'
								&& word.ToLower () == "#" + Language.GetName (lang)) {
								word = section.DocumentTitle;
								if ("|" + word == m.Groups [6].Captures [0].Value) {
									skip = true;
								}
							}
							if (!skip) {
								additionalParams += "|text="
									+ m.Groups [6].Captures [0].Value.Substring (1);
							}
						}
					}
					additionalParams = additionalParams.TrimStart ('|');
					word = word.Trim (' ', '\t', '\u200e');

					var translation = new Translation (
						documentTitle: section.DocumentTitle,
						langCode: lang,
						title: word,
						additionalParams: additionalParams == "" ? null
							: additionalParams.Split (new char[]{'|'}),
						allText: section.AllText,
						start: position + m.Index,
						length: m.Length,
						match: m);

					yield return translation;
				}
			}
		}

		public static List<Section> GetTranslationSections (string title, string wikitext)
		{
			if (wikitext.IndexOf ("\n=====Översättningar=====\n") != -1)
				wikitext = wikitext.Replace ("\n=====Översättningar=====\n", "\n====Översättningar====\n");

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
				res.Add (new Section (title, wikitext, start, end - start));
			}
			return res;
		}

	}
}

