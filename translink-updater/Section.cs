using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace translinkupdater
{
	public class Section
	{
		private string _allText;
		private int _start, _length;
		private string _text;

		public string AllText {
			get { return _allText; }
		}

		public virtual string Text {
			get {
				if (_text == null) {
					if (_length == 0)
						_text = "";
					else
						_text = _allText.Substring (_start, _length);
				}
				return _text;
			}
			set {
				_text = value;
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

		public string DocumentTitle {
			get;
			set;
		}
		public Section (string documentTitle, string allText)
		{
			DocumentTitle = documentTitle;
			_allText = allText;
			_start = 0;
			_length = allText.Length;
			_text = allText;
		}

		public Section (string documentTitle, string allText, int start, int length)
		{
			if (start + length > allText.Length) {
				throw new IndexOutOfRangeException ("start + length > allText.length");
			}
			DocumentTitle = documentTitle;
			_allText = allText;
			_start = start;
			_length = length;
			_text = null;
		}

		override public string ToString ()
		{
			return Text;
		}
	}

	public class Translation : Section
	{
		private string _langCode;
		private string _title;
		private IList<string> _additionalParams;

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

		public override string Text {
			get {
				return string.Join ("", new string[] {
					Exists ? "{{ö+|" : "{{ö|",
					_langCode, "|",
					_title,
					AdditionalParams,
					"}}"
				}
				);
			}
		}

		public string AdditionalParams {
			get {
				if (_additionalParams == null || _additionalParams.Count == 0)
					return "";
				else {
					for (var i = _additionalParams.Count - 1; i >= 0; i--) {
						var p = _additionalParams [i];
						if (p == "c") {
							_additionalParams [i] = "u";
						} else if (p.StartsWith ("tr=")) {
							_additionalParams [i] = "trans=" + p.Substring (3);
						} else if (p.StartsWith ("ö=")) {
							_additionalParams = new List<string>(_additionalParams);
							_additionalParams.RemoveAt (i);
						}
					}
					return "|" + string.Join ("|", _additionalParams);
				}
			}
		}

		public Translation (
				string documentTitle,
				string langCode, string title,
				IList<string> additionalParams,
				string allText,
				int start, int length,
				Match match,
				bool exists = true)
				: base(documentTitle, allText, start, length)
		{
			_langCode = langCode;
			_title = title;
			_additionalParams = additionalParams;
			_match = match;
			Exists = exists;
		}
	}
}

