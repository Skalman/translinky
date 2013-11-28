using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace translinkupdater
{
	public class Api
	{
		protected static string UserAgent =
			"C# script by https://sv.wiktionary.org/wiki/User:Skalman";

		public static string EncodeParameters (IDictionary<string, string> parameters)
		{
			if (parameters == null && parameters.Count == 0)
				return "";

			IEnumerable<string> list = parameters.Select (
				p => Uri.EscapeDataString (p.Key) +
				"=" + Uri.EscapeDataString (p.Value)
			);
			return string.Join ("&", list);
		}

		protected static HttpWebRequest PrepareRequest (string uri)
		{
			// Ignore invalid SSL certificates
			ServicePointManager.ServerCertificateValidationCallback = (a,b,c,d) => true;
			var req = (HttpWebRequest)WebRequest.Create (uri);
			req.UserAgent = UserAgent;
			req.MaximumAutomaticRedirections = 1;
			return req;
		}

		protected static JObject GetResponseJson (HttpWebRequest req)
		{
			var res = (HttpWebResponse)req.GetResponse ();
			Stream stream = res.GetResponseStream ();
			var reader = new StreamReader (stream, Encoding.UTF8);
			var jsonText = reader.ReadToEnd ();
			res.Close ();
			reader.Close ();
			return JObject.Parse (jsonText);
		}

		public static JObject Get (string parameters,
		                           string domain = "https://sv.wiktionary.org",
		                           bool cookies = false)
		{
			var uri = domain + "/w/api.php?format=json&maxlag=2&" + parameters;
			Console.WriteLine ("GET {0}", uri);

			var req = PrepareRequest (uri);
			if (cookies) {
				if (Cookies == null)
					Cookies = new CookieContainer ();
				req.CookieContainer = Cookies;
			}

			var json = GetResponseJson (req);
			if (json ["error"] != null && (string)json ["error"] ["code"] == "maxlag") {
				Console.WriteLine ("Maxlag: Sleeping for 5 sec...");
				Thread.Sleep (5000);
				return Get (
					parameters: parameters,
		            domain: domain,
					cookies: cookies);
			}

			return json;
		}

		public static JObject Get (IDictionary<string, string> parameters,
		                           string domain = "https://sv.wiktionary.org",
		                           bool cookies = false)
		{
			return Get (
				EncodeParameters (parameters),
				domain: domain,
				cookies: cookies);
		}

		protected static CookieContainer Cookies = null;

		public static JObject Post (IDictionary<string, string> parameters,
		                            string domain = "https://sv.wiktionary.org",
		                            string action = null,
		                            bool showAllData = false,
		                            bool cookies = true)
		{
			return Post (
				EncodeParameters (parameters),
				domain: domain,
				action: action,
				showAllData: showAllData,
				cookies: cookies);
		}

		public static JObject Post (string data,
		                            string domain = "https://sv.wiktionary.org",
		                            string action = null,
		                            bool showAllData = false,
		                            bool cookies = true,
		                            string uriAppend = null)
		{
			var uri = domain + "/w/api.php?format=json&maxlag=2";
			if (action != null)
				uri += "&action=" + action;
			if (uriAppend != null)
				uri += "&" + uriAppend;
			var req = PrepareRequest (uri);
			if (cookies) {
				if (Cookies == null)
					Cookies = new CookieContainer ();
				req.CookieContainer = Cookies;
			}
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";

			Console.WriteLine (
				"POST {0} (data: {1})",
				uri,
				showAllData ? data : Regex.Replace (
					data,
					@"((^|&)[^=]*)=[^&]+",
					"$1=..."
			)
			);
			byte[] postData = Encoding.ASCII.GetBytes (data);
			req.ContentLength = postData.Length;
			Stream stream = req.GetRequestStream ();
			stream.Write (postData, 0, postData.Length);
			stream.Close ();

			var json = GetResponseJson (req);
			if (json ["error"] != null && (string)json ["error"] ["code"] == "maxlag") {
				Console.WriteLine ("Maxlag: Sleeping for 5 sec...");
				Thread.Sleep (5000);
				return Post (
					data: data,
					domain: domain,
					action: action,
					showAllData: showAllData,
					cookies: cookies,
					uriAppend : uriAppend);
			}

			return json;
		}

		protected static string _signedInUser = null;

		public static string SignedInUser {
			get { return _signedInUser; }
		}

		public static bool SignIn (string username, string password)
		{
			var tokenResponse = Post (new Dictionary<string,string> {
				{"lgname", username},
			}, action: "login");
			// result should be: "NeedToken"
			var token = (string)tokenResponse ["login"] ["token"];
			var response = Post (new Dictionary<string,string> {
				{"lgname", username},
				{"lgpassword", password},
				{"lgtoken", token}
			}, action: "login");

			var isSuccess = (string)response ["login"] ["result"] == "Success";
			if (isSuccess)
				_signedInUser = username;
			return true;
		}

		protected static string EditToken = null;

		public static void SavePage (
			Page page,
			string summary,
			bool nocreate=false,
			bool bot=true)
		{
			SavePage (
				title: page.Title,
				wikitext: page.Text,
				summary: summary,
				nocreate: nocreate,
				bot: bot,
				timestamp: page.Timestamp);
		}

		public static bool SavePage (
			string title,
			string wikitext,
			string summary,
			bool nocreate=false,
			bool bot=true,
			string timestamp=null)
		{
			if (EditToken == null) {
				var tokens = Get ("action=tokens", cookies: true);
				EditToken = (string)tokens ["tokens"] ["edittoken"];
			}
			var editResponse = Post (
				new Dictionary<string, string>{
					{"action", "edit"},
					{"title", title},
					{"text", wikitext},
					{"summary", summary},
					{nocreate ? "nocreate" : "", ""},
					{"assert", "user"},
					{"basetimestamp", timestamp},
					{bot ? "bot" : "", ""},
					{"token", EditToken},
				},
				cookies: true
			);

			if (editResponse ["error"] != null && (string)editResponse ["error"] ["code"] == "badtoken") {
				var loggedInInfo = Get ("action=query&meta=userinfo&uiprop=groups", cookies: true);
				if (loggedInInfo ["query"] ["userinfo"] ["anon"] != null) {
					_signedInUser = null;
					EditToken = null;
					throw new NotLoggedInException ();
				} else {
					throw new Exception (
						"Logged in as " +
						loggedInInfo ["query"] ["userinfo"] ["name"] +
						" but has invalid token."
					);
				}
			}

			if ((string)editResponse ["edit"] ["result"] == "Success") {
				return true;
			} else {
				return false;
			}
		}

		public class NotLoggedInException : Exception
		{
			public NotLoggedInException () : base()
			{
			}
		}

		public static Page GetPage (string title)
		{
			var response = Api.Get (
				new Dictionary<string, string> {
					{"action", "query"},
					{"titles", title},
					{"prop", "revisions"},
					{"rvprop", "timestamp|content"},
				}
			);
			var pages = (IDictionary<string, JToken>)response ["query"] ["pages"];
			foreach (var page in pages) {
				return new Page (page.Value);
			}
			throw new Exception ("Could not retrieve page '" + title + "'");
		}

		public class Page
		{
			protected JToken Json;
			protected string _text, _title, _timestamp;

			public Page (JToken json)
			{
				Json = json;
				_text = null;
				_title = null;
				_timestamp = null;
			}

			public Page (string title, string text, string timestamp = "")
			{
				_text = text;
				_title = title;
				_timestamp = timestamp;
			}

			public string Text {
				get {
					if (_text == null)
						_text = (string)Json ["revisions"] [0] ["*"] + "\n";
					return _text;
				}
				set {
					_text = value;
				}
			}

			public string Title {
				get {
					if (_title == null)
						_title = (string)Json ["title"];
					return _title;
				}
			}

			public string Timestamp {
				get {
					if (_timestamp == null)
						_timestamp = (string)Json ["revisions"] [0] ["timestamp"];
					return _timestamp;
				}
			}

			public override string ToString ()
			{
				return Title;
			}
		}

		public static IDictionary<string, bool> PagesExist (
			string langCode, IList<string> pages,
			IDictionary<string, bool> addTo = null)
		{
			var res = new Dictionary<string, bool> ();
			if (pages.Count <= 50) {
				PagesExistMax50 (langCode, pages, res);
			} else {
				for (var i = 0; i < pages.Count; i += 50) {
					PagesExistMax50 (
						langCode,
						new List<string> (pages.Skip (i).Take (50)),
						res);
				}
			}

			return res;
		}

		private static void PagesExistMax50 (
			string langCode, IList<string> pages,
			IDictionary<string, bool> addTo)
		{
			/*
			if (langCode != "ml") {
				foreach (var p in pages) {
					addTo [p] = false;
				}
				return;
			}
			/**/
			if (pages.Count == 1) {
				// avoid normalizing and rerequesting
				addTo [pages [0]] = PageExists (langCode, pages [0]);
				return;
			}

			var response = Api.Get (
				new Dictionary<string, string> {
					{"action", "query"},
					{"titles", string.Join("|", pages)}
				},
				"https://" + langCode + ".wiktionary.org",
				cookies: true
			);
			var pageDict = (IDictionary<string, JToken>)response ["query"] ["pages"];

			foreach (var kv in pageDict) {
				addTo [(string)kv.Value ["title"]] = (string)kv.Value ["missing"] != "";
			}
			var normalized = (IList<JToken>)response ["query"] ["normalized"];
			if (normalized != null) {
				foreach (var item in normalized) {
					var from = (string)item ["from"];
					var to = (string)item ["to"];
					addTo [from] = addTo [to];
				}
			}
			// Unicode normalization - try again
			foreach (var p in pages) {
				if (!addTo.ContainsKey (p)) {
					addTo.Add (p, PageExists (langCode, p));
				}
			}
		}

		public static bool PageExists (string langCode, string title)
		{
			var response = Api.Get (
				new Dictionary<string, string> {
					{"action", "query"},
					{"titles", title}
				},
				"https://" + langCode + ".wiktionary.org"
			);
			var pageDict = (IDictionary<string, JToken>)response ["query"] ["pages"];

			foreach (var kv in pageDict) {
				return (string)kv.Value ["missing"] != "";
			}
			throw new Exception ("Shouldn't be able to reach this");
		}

		public static IEnumerable<Page>PagesInCategory (
			string category,
			int ns = -1,
			int step = 50,
			int maxPages = -1,
			string startAt = "")
		{
			string gcmcontinue = "";
			if (maxPages < 0) {
				// make it "infinitely" many
				maxPages = int.MaxValue;
			}
			int pagesLeft = maxPages;
			while (gcmcontinue != null && pagesLeft > 0) {
				step = Math.Min (pagesLeft, step);
				var response = Api.Get (
					new Dictionary<string, string> {
						{"action", "query"},
						{"generator", "categorymembers"},
						{"prop", "revisions"},
						{"rvprop", "timestamp|content"},
						{"gcmtitle", "Category:" + category},
						{"gcmnamespace", ns == -1 ? "" : ns.ToString()},
						{"gcmlimit", step.ToString()},
						{"gcmstartsortkeyprefix", startAt},
						{"gcmcontinue", gcmcontinue},
					}
				);
				try {
					gcmcontinue = (string)response ["query-continue"] ["categorymembers"] ["gcmcontinue"];
				} catch (NullReferenceException) {
					gcmcontinue = null;
				}
				var members = (IDictionary<string, JToken>)response ["query"] ["pages"];
				foreach (var m in members) {
					yield return new Page (m.Value);
				}
				pagesLeft -= step;
			}
		}
	}
}

