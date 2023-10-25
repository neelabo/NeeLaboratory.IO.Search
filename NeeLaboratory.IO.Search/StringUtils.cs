using CSharp.Japanese.Kanaxs;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public static class StringUtils
    {
        private static readonly Regex _regexSpace = new(@"\s+", RegexOptions.Compiled);

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
    }
}
