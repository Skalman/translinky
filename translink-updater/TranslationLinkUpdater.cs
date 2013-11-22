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

		public delegate void PageDoneCallback(string title);

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
			string startAt = "", int maxPages = 10,
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
					UpdateBatch (pages, saveCallback, pageDoneCallback, logCallback);
					pages.Clear ();
				}
			}
			if (pages.Count != 0)
				UpdateBatch (pages, saveCallback, pageDoneCallback, logCallback);
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
						pagesExist.Add (title, false);
				}
				linksExist.Add (l.Key, pagesExist);
			}

			// Update the pages and save them
			foreach (var page in pages) {
				string proposedSummary;
				string proposedWikitext;
				var oldWikitext = page.Text;
				if (UpdateTranslations (
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

						Api.SavePage (
							page,
							summary,
							nocreate: true,
							bot: proposedWikitext == wikitext);
						LogWrite (log, "{0}: Saved ({1})",
						         page.Title,
						         summary);
					} else {
						LogWrite (log, "{0}: Chose not to save ({1})",
						         page.Title,
						         summary);
					}
				} else {
					LogWrite (log, "{0}: Doesn't need update",
					          page.Title);
				}
				pageDoneCallback(page.Title);
			}
		}

		static bool UpdateTranslations (
			string wikitext,
			Dictionary<string, IDictionary<string, bool>> linksExist,
			out string newWikitext,
			out string summary)
		{
			var sections = new List<Section> ();
			var lastEnd = 0;
			var summaryParts = new SortedSet<string> ();

			// Not just translations, but important formatting
			wikitext = FormatPage(wikitext, summaryParts);

			foreach (var s in GetTranslationSections(wikitext)) {
				sections.Add (new Section (wikitext, lastEnd, s.Start - lastEnd));
				FormatTranslationSection (s, summaryParts);
				UpdateTranslations (s, linksExist, summaryParts);
				sections.Add (s);
				lastEnd = s.End;
			}
			sections.Add (new Section (wikitext, lastEnd, wikitext.Length - lastEnd));
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
			if (wikitext.IndexOf("----") != -1) {
				summaryParts.Add("tar bort onödig {{nollpos}} och ----");
				wikitext = Regex.Replace(wikitext, @"\n+(\{\{nollpos\}\}\n|\-{4,}\n)+\n*", "\n\n");
			}
			return wikitext;
		}

		protected static void FormatTranslationSection (Section section, ISet<string> summary)
		{
			if (section.Text.IndexOf ("{{topp") != -1) {
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
						// only insert one - won't know what to do with broken lists anyway
						break;
					}
				}
				InsertMidBottom (lines, startedAt - 1, i);
				section.Text = string.Join ("\n", lines);
				summary.Add ("använd {{ö-topp}}");
			}
		}

		private static void UpdateTranslations (
			Section section,
			Dictionary<string, IDictionary<string, bool>> linksExist,
			ISet<string> summary)
		{
			var sections = new List<Section> ();
			var lastEnd = 0;
			var wikitext = section.Text;
			foreach (var t in GetTranslations(new Section(wikitext))) {
				// Add the previous, non-translation section
				sections.Add (new Section (wikitext, lastEnd, t.Start - lastEnd));
				t.Exists = linksExist [t.LangCode] [t.Title];
				sections.Add (t);
				lastEnd = t.End;
			}
			sections.Add (new Section (wikitext, lastEnd, wikitext.Length - lastEnd));

			if (lastEnd != 0) {
				section.Text = string.Join ("", sections);
				if (wikitext != section.Text)
					summary.Add ("uppdaterar {{ö}}");

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
			string lang = null;
			int nextStart = section.Start;
			var wikitext = section.Text;

			foreach (var line in wikitext.Split('\n')) {
				int position = nextStart;
				nextStart += line.Length + 1; // +1 for \n

				if (!line.StartsWith ("*") || line.IndexOf (':') == -1)
					continue;
				if (!line.StartsWith ("**") && !line.StartsWith ("*:") && !line.StartsWith (":*")) {
					var langName = line.Substring (1, line.IndexOf (':') - 1);
					lang = Language.GetCode (langName);
				}
				if (lang == null) {
					Console.WriteLine ("No language found for " + line);
				}

				// {{ö|..|XXX}} or {{ö+|..|XXX}} or [[XXX]]
				var reAdditionalTemplate = @"( ?\{\{(m|f|mf|c|u|n|p|d|s)\}\})?";
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
							additionalParams += m.Groups [7].Captures [0].Value;
					}
					additionalParams = additionalParams.TrimStart('|');

					var translation = new Translation (
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

	}
}

