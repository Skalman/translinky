Uppdatera översättningslänkar på [Wiktionary]
=============================================

Det här är ett program för att uppdatera översättningslänkar i svenskspråkiga
Wiktionary. Det är avsett att normalisera länkarna och visa blå länk till andra
Wiktionary-upplagor endast där de har motsvarande sida.

I kort: Programmet korrigerar användning av mallen {{ö}}.

VARNING:
* Kontrollera alltid att du har communityns stöd innan du låter
  programmet uppdatera alla uppslag!
* Kör programmet som bot, men gör regelbundna stickprov så att du vet
  att det fungerar som väntat.


Teknik
------

Det här projektet är ett sätt för mig att lära mig mer om och använda mer av C#.

* Projektet är utvecklat i C# och använder Json.NET och GTK#
* MonoDevelop har använts som IDE och projekt och har bara testats på Linux


Licens
------

Alla filer skrivna särskilt för det här projektet utges under [MPL 2.0].

Projektet kräver också Json.NET, vilket utges under MIT-licensen.



  [Wiktionary]: https://sv.wiktionary.org "Svenskspråkiga Wiktionary"
  [MPL 2.0]: https://www.mozilla.org/MPL/2.0/ "Mozilla Public License, version 2.0"
