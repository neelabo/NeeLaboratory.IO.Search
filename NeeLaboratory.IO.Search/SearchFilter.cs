using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public abstract class SearchFilter
    {
        public SearchFilter(SearchPropertyProfile property, string format)
        {
            Property = property;
            Format = format;
        }

        public SearchPropertyProfile Property { get; }
        protected string Format { get; }

        public abstract bool IsMatch(SearchContext context, ISearchItem e);
    }


    /// <summary>
    /// 全一致 (既定のフィルター)
    /// </summary>
    public class TrueSearchFilter : SearchFilter
    {
        public TrueSearchFilter(SearchPropertyProfile property, string format)
            : base(property, format)
        {
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            return true;
        }
    }

    /// <summary>
    /// 文字列：曖昧検索
    /// </summary>
    public class FuzzySearchFilter : SearchFilter
    {
        private readonly Regex _regex;

        public FuzzySearchFilter(SearchPropertyProfile property, string format)
            : base(property, format)
        {
            var s = format;
            s = SearchStringTools.ToNormalizedWord(s, true);
            s = Regex.Escape(s);
            s = SearchStringTools.ToFuzzyNumberRegex(s);
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            s = context.FuzzyStringCache.GetString(s);
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：単語検索
    /// </summary>
    public class WordSearchFilter : SearchFilter
    {
        private readonly Regex _regex;

        public WordSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            var s = format;
            var first = SearchStringTools.GetNotCodeBlockRegexString(s.First());
            var last = SearchStringTools.GetNotCodeBlockRegexString(s.Last());
            s = SearchStringTools.ToNormalizedWord(s, false);
            s = Regex.Escape(s);
            s = SearchStringTools.ToFuzzyNumberRegex(s);
            if (first != null) s = $"(^|{first}){s}";
            if (last != null) s = $"{s}({last}|$)";
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            s = context.WordStringCache.GetString(s);
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：完全検索
    /// </summary>
    public class ExactSearchFilter : SearchFilter
    {
        private readonly Regex _regex;

        public ExactSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            var s = Regex.Escape(format);
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：正規表現検索
    /// </summary>
    public class RegularExpressionSearchFilter : SearchFilter
    {
        private readonly Regex _regex;

        public RegularExpressionSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            try
            {
                _regex = new Regex(format, RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                throw new SearchKeywordRegularExpressionException($"RegularExpression error: {format}", ex);
            }
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
          var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：正規表現検索 (IgnoreCase)
    /// </summary>
    public class RegularExpressionIgnoreCaseSearchFilter : SearchFilter
    {
        private readonly Regex _regex;

        public RegularExpressionIgnoreCaseSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            try
            {
                _regex = new Regex(format, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                throw new SearchKeywordRegularExpressionException($"RegularExpression error: {format}", ex);
            }
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }
    }


    /// <summary>
    /// 比較：等しい
    /// </summary>
    public class EqualSearchFilter : SearchFilter
    {
        private readonly SearchValue _referenceValue;

        public EqualSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);
            return value.CompareTo(_referenceValue) == 0;
        }
    }

    /// <summary>
    /// 比較：等しくない
    /// </summary>
    public class NotEqualSearchFilter : SearchFilter
    {
        private readonly SearchValue _referenceValue;

        public NotEqualSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);
            return value.CompareTo(_referenceValue) != 0;
        }
    }

    /// <summary>
    /// 比較：より大きい
    /// </summary>
    public class GreaterThanSearchFilter : SearchFilter
    {
        private readonly SearchValue _referenceValue;

        public GreaterThanSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) > 0;
        }
    }

    /// <summary>
    /// 比較：より小さい
    /// </summary>
    public class LessThanSearchFilter : SearchFilter
    {
        private readonly SearchValue _referenceValue;

        public LessThanSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) < 0;
        }
    }


    /// <summary>
    /// 比較：以上
    /// </summary>
    public class GreaterThanEqualSearchFilter : SearchFilter
    {
        private readonly SearchValue _referenceValue;

        public GreaterThanEqualSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) >= 0;
        }
    }

    /// <summary>
    /// 比較：以下
    /// </summary>
    public class LessThanEqualSearchFilter : SearchFilter
    {
        private readonly SearchValue _referenceValue;

        public LessThanEqualSearchFilter(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) <= 0;
        }
    }

}
