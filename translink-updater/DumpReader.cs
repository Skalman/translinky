using System;
using System.Collections.Generic;
using System.Xml;

namespace translinkupdater
{
	public class DumpReader
	{
		protected string fileName;

		public DumpReader (string fileName)
		{
			this.fileName = fileName;
		}

		public IEnumerable<Api.Page> Pages (
			ISet<int> namespaces = null,
			string startAt = null,
			int maxPages = -1)
		{
			var xml = XmlReader.Create (this.fileName);

			if (!xml.IsStartElement ("mediawiki"))
				yield break;

			xml.MoveToContent ();
			xml.ReadToDescendant ("page");

			if (startAt == "")
				startAt = null;
			bool hasStarted = startAt == null;

			do {
				xml.ReadToDescendant ("title");
				var title = xml.ReadElementString ();
				if (!hasStarted) {
					if (title == startAt) {
						hasStarted = true;
					} else if (title + "!" == startAt) {
						hasStarted = true;
						continue;
					} else {
						continue;
					}
				}

				xml.ReadToNextSibling ("ns");
				var ns = xml.ReadElementContentAsInt ();
				if (namespaces != null && !namespaces.Contains (ns))
					continue;

				xml.ReadToNextSibling ("revision");
				xml.ReadToDescendant ("timestamp");
				var timestamp = xml.ReadElementString ();
				xml.ReadToNextSibling ("text");
				var text = xml.ReadElementString ();
				yield return new Api.Page (title, text, timestamp);
				maxPages--;
			} while (maxPages != 0 && xml.ReadToFollowing("page"));
			xml.Close ();
		}
	}
}

