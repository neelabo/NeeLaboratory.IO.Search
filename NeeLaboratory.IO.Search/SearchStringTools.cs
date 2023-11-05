using CSharp.Japanese.Kanaxs;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public static class SearchStringTools
    {
        private static readonly Regex _regexSpace = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex _regexNumber = new(@"0*(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// 正規化された文字列に変換する
        /// </summary>
        /// <param name="src"></param>
        /// <param name="isFuzzy"></param>
        /// <returns></returns>
        public static string ToNormalizedWord(string src, bool isFuzzy)
        {
            string? s = src;

            s = KanaEx.ToPadding(s); // 濁点を１文字にまとめる
            if (s is null) return "";

            try
            {
                s = s.Normalize(NormalizationForm.FormKC); // 正規化
            }
            catch (ArgumentException)
            {
                // 無効なコードポイントがある場合は正規化はスキップする
            }

            s = s.ToUpper(); // アルファベットを大文字にする

            if (isFuzzy)
            {
                s = KanaEx.ToKatakanaWithNormalize(s); // ひらがなをカタカナにする ＋ 特定文字の正規化
                if (s is null) return "";

                s = _regexSpace.Replace(s, ""); // 空白の削除
            }

            return s;
        }

        /// <summary>
        /// 単語区切り用の正規表現生成
        /// </summary>
        public static string? GetNotCodeBlockRegexString(char c)
        {
            if ('0' <= c && c <= '9')
                return @"\D";
            //else if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
            //    return @"\P{L}";
            else if (0x3040 <= c && c <= 0x309F)
                return @"\P{IsHiragana}";
            else if (0x30A0 <= c && c <= 0x30FF)
                return @"\P{IsKatakana}";
            else if ((0x3400 <= c && c <= 0x4DBF) || (0x4E00 <= c && c <= 0x9FFF) || (0xF900 <= c && c <= 0xFAFF))
                return @"[^\p{IsCJKUnifiedIdeographsExtensionA}\p{IsCJKUnifiedIdeographs}\p{IsCJKCompatibilityIdeographs}]";
            else if (new Regex(@"^\p{L}").IsMatch(c.ToString()))
                return @"\P{L}";
            else
                return null;
        }

        /// <summary>
        /// (数値)部分を0*(数値)という正規表現に変換
        /// </summary>
        public static string ToFuzzyNumberRegex(string source)
        {
            return _regexNumber.Replace(source, match => "0*" + match.Groups[1]);
        }
    }
}
