/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/OpenTypeScript.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: trunk
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Globalization;
using System.Reflection;
using System.Text;

namespace PdfBox.Net.FontBox.TTF;

public static class OpenTypeScript
{
    public const string INHERITED = "Inherited";
    public const string UNKNOWN = "Unknown";
    public const string TAG_DEFAULT = "DFLT";

    private static readonly IReadOnlyDictionary<string, string[]> UNICODE_SCRIPT_TO_OPENTYPE_TAG_MAP =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Adlam"] = ["adlm"],
            ["Ahom"] = ["ahom"],
            ["Anatolian_Hieroglyphs"] = ["hluw"],
            ["Arabic"] = ["arab"],
            ["Armenian"] = ["armn"],
            ["Avestan"] = ["avst"],
            ["Balinese"] = ["bali"],
            ["Bamum"] = ["bamu"],
            ["Bassa_Vah"] = ["bass"],
            ["Batak"] = ["batk"],
            ["Bengali"] = ["bng2", "beng"],
            ["Bhaiksuki"] = ["bhks"],
            ["Bopomofo"] = ["bopo"],
            ["Brahmi"] = ["brah"],
            ["Braille"] = ["brai"],
            ["Buginese"] = ["bugi"],
            ["Buhid"] = ["buhd"],
            ["Canadian_Aboriginal"] = ["cans"],
            ["Carian"] = ["cari"],
            ["Caucasian_Albanian"] = ["aghb"],
            ["Chakma"] = ["cakm"],
            ["Cham"] = ["cham"],
            ["Cherokee"] = ["cher"],
            ["Common"] = [TAG_DEFAULT],
            ["Coptic"] = ["copt"],
            ["Cuneiform"] = ["xsux"],
            ["Cypriot"] = ["cprt"],
            ["Cyrillic"] = ["cyrl"],
            ["Deseret"] = ["dsrt"],
            ["Devanagari"] = ["dev2", "deva"],
            ["Duployan"] = ["dupl"],
            ["Egyptian_Hieroglyphs"] = ["egyp"],
            ["Elbasan"] = ["elba"],
            ["Ethiopic"] = ["ethi"],
            ["Georgian"] = ["geor"],
            ["Glagolitic"] = ["glag"],
            ["Gothic"] = ["goth"],
            ["Grantha"] = ["gran"],
            ["Greek"] = ["grek"],
            ["Gujarati"] = ["gjr2", "gujr"],
            ["Gurmukhi"] = ["gur2", "guru"],
            ["Han"] = ["hani"],
            ["Hangul"] = ["hang"],
            ["Hanunoo"] = ["hano"],
            ["Hatran"] = ["hatr"],
            ["Hebrew"] = ["hebr"],
            ["Hiragana"] = ["kana"],
            ["Imperial_Aramaic"] = ["armi"],
            [INHERITED] = [INHERITED],
            ["Inscriptional_Pahlavi"] = ["phli"],
            ["Inscriptional_Parthian"] = ["prti"],
            ["Javanese"] = ["java"],
            ["Kaithi"] = ["kthi"],
            ["Kannada"] = ["knd2", "knda"],
            ["Katakana"] = ["kana"],
            ["Kayah_Li"] = ["kali"],
            ["Kharoshthi"] = ["khar"],
            ["Khmer"] = ["khmr"],
            ["Khojki"] = ["khoj"],
            ["Khudawadi"] = ["sind"],
            ["Lao"] = ["lao "],
            ["Latin"] = ["latn"],
            ["Lepcha"] = ["lepc"],
            ["Limbu"] = ["limb"],
            ["Linear_A"] = ["lina"],
            ["Linear_B"] = ["linb"],
            ["Lisu"] = ["lisu"],
            ["Lycian"] = ["lyci"],
            ["Lydian"] = ["lydi"],
            ["Mahajani"] = ["mahj"],
            ["Malayalam"] = ["mlm2", "mlym"],
            ["Mandaic"] = ["mand"],
            ["Manichaean"] = ["mani"],
            ["Marchen"] = ["marc"],
            ["Meetei_Mayek"] = ["mtei"],
            ["Mende_Kikakui"] = ["mend"],
            ["Meroitic_Cursive"] = ["merc"],
            ["Meroitic_Hieroglyphs"] = ["mero"],
            ["Miao"] = ["plrd"],
            ["Modi"] = ["modi"],
            ["Mongolian"] = ["mong"],
            ["Mro"] = ["mroo"],
            ["Multani"] = ["mult"],
            ["Myanmar"] = ["mym2", "mymr"],
            ["Nabataean"] = ["nbat"],
            ["Newa"] = ["newa"],
            ["New_Tai_Lue"] = ["talu"],
            ["Nko"] = ["nko "],
            ["Ogham"] = ["ogam"],
            ["Ol_Chiki"] = ["olck"],
            ["Old_Italic"] = ["ital"],
            ["Old_Hungarian"] = ["hung"],
            ["Old_North_Arabian"] = ["narb"],
            ["Old_Permic"] = ["perm"],
            ["Old_Persian"] = ["xpeo"],
            ["Old_South_Arabian"] = ["sarb"],
            ["Old_Turkic"] = ["orkh"],
            ["Oriya"] = ["ory2", "orya"],
            ["Osage"] = ["osge"],
            ["Osmanya"] = ["osma"],
            ["Pahawh_Hmong"] = ["hmng"],
            ["Palmyrene"] = ["palm"],
            ["Pau_Cin_Hau"] = ["pauc"],
            ["Phags_Pa"] = ["phag"],
            ["Phoenician"] = ["phnx"],
            ["Psalter_Pahlavi"] = ["phlp"],
            ["Rejang"] = ["rjng"],
            ["Runic"] = ["runr"],
            ["Samaritan"] = ["samr"],
            ["Saurashtra"] = ["saur"],
            ["Sharada"] = ["shrd"],
            ["Shavian"] = ["shaw"],
            ["Siddham"] = ["sidd"],
            ["SignWriting"] = ["sgnw"],
            ["Sinhala"] = ["sinh"],
            ["Sora_Sompeng"] = ["sora"],
            ["Sundanese"] = ["sund"],
            ["Syloti_Nagri"] = ["sylo"],
            ["Syriac"] = ["syrc"],
            ["Tagalog"] = ["tglg"],
            ["Tagbanwa"] = ["tagb"],
            ["Tai_Le"] = ["tale"],
            ["Tai_Tham"] = ["lana"],
            ["Tai_Viet"] = ["tavt"],
            ["Takri"] = ["takr"],
            ["Tamil"] = ["tml2", "taml"],
            ["Tangut"] = ["tang"],
            ["Telugu"] = ["tel2", "telu"],
            ["Thaana"] = ["thaa"],
            ["Thai"] = ["thai"],
            ["Tibetan"] = ["tibt"],
            ["Tifinagh"] = ["tfng"],
            ["Tirhuta"] = ["tirh"],
            ["Ugaritic"] = ["ugar"],
            [UNKNOWN] = [TAG_DEFAULT],
            ["Vai"] = ["vai "],
            ["Warang_Citi"] = ["wara"],
            ["Yi"] = ["yi  "]
        };

    private static int[] unicodeRangeStarts = [0];
    private static string[] unicodeRangeScripts = [UNKNOWN];

    static OpenTypeScript()
    {
        const string resourceName = "PdfBox.Net.FontBox.FontBox.TTF.Scripts.txt";
        using Stream? resourceAsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (resourceAsStream == null)
        {
            return;
        }

        using BufferedStream input = new(resourceAsStream);
        ParseScriptsFile(input);
    }

    private static void ParseScriptsFile(Stream inputStream)
    {
        List<(int Start, int End, string Script)> unicodeRanges = [];
        using StreamReader rd = new(inputStream, System.Text.Encoding.ASCII, leaveOpen: true);
        int lastStart = int.MinValue;
        int lastEnd = int.MinValue;
        string? lastScript = null;
        while (true)
        {
            string? s = rd.ReadLine();
            if (s == null)
            {
                break;
            }

            int comment = s.IndexOf('#');
            if (comment != -1)
            {
                s = s[..comment];
            }

            if (s.Length < 2)
            {
                continue;
            }

            string[] parts = s.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            string characters = parts[0];
            string script = parts[1];
            int start;
            int end;
            int rangeDelim = characters.IndexOf("..", StringComparison.Ordinal);
            if (rangeDelim == -1)
            {
                start = end = Convert.ToInt32(characters, 16);
            }
            else
            {
                start = Convert.ToInt32(characters[..rangeDelim], 16);
                end = Convert.ToInt32(characters[(rangeDelim + 2)..], 16);
            }

            if (start == lastEnd + 1 && script.Equals(lastScript, StringComparison.Ordinal))
            {
                unicodeRanges[^1] = (lastStart, end, script);
                lastEnd = end;
            }
            else
            {
                unicodeRanges.Add((start, end, script));
                lastStart = start;
                lastEnd = end;
                lastScript = script;
            }
        }

        unicodeRangeStarts = new int[unicodeRanges.Count];
        unicodeRangeScripts = new string[unicodeRanges.Count];
        for (int i = 0; i < unicodeRanges.Count; i++)
        {
            unicodeRangeStarts[i] = unicodeRanges[i].Start;
            unicodeRangeScripts[i] = unicodeRanges[i].Script;
        }
    }

    private static string GetUnicodeScript(int codePoint)
    {
        EnsureValidCodePoint(codePoint);
        UnicodeCategory type = char.GetUnicodeCategory(char.ConvertFromUtf32(codePoint), 0);
        if (type == UnicodeCategory.OtherNotAssigned)
        {
            return UNKNOWN;
        }

        int scriptIndex = Array.BinarySearch(unicodeRangeStarts, codePoint);
        if (scriptIndex < 0)
        {
            scriptIndex = -scriptIndex - 2;
        }

        return scriptIndex < 0 || scriptIndex >= unicodeRangeScripts.Length ? UNKNOWN : unicodeRangeScripts[scriptIndex];
    }

    public static string[] GetScriptTags(int codePoint)
    {
        EnsureValidCodePoint(codePoint);
        string unicode = GetUnicodeScript(codePoint);
        return UNICODE_SCRIPT_TO_OPENTYPE_TAG_MAP.TryGetValue(unicode, out string[]? tags) ? tags : [TAG_DEFAULT];
    }

    private static void EnsureValidCodePoint(int codePoint)
    {
        if (codePoint < char.MinValue || codePoint > 0x10FFFF)
        {
            throw new ArgumentException($"Invalid codepoint: {codePoint}");
        }
    }
}
