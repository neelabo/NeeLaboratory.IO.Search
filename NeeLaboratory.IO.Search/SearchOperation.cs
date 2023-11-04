using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public abstract class SearchOperation
    {
        public SearchOperation(SearchPropertyProfile property, string format)
        {
            Property = property;
            Format = format;
        }

        public SearchPropertyProfile Property { get; }
        protected string Format { get; }

        public abstract bool IsMatch(SearchValueContext context, ISearchItem e);
    }


    /// <summary>
    /// 全一致 (既定のフィルター)
    /// </summary>
    public class TrueSearchOperation : SearchOperation
    {
        public TrueSearchOperation(SearchPropertyProfile property, string format)
            : base(property, format)
        {
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            return true;
        }
    }

    /// <summary>
    /// 文字列：曖昧検索
    /// </summary>
    public class FuzzySearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public FuzzySearchOperation(SearchPropertyProfile property, string format)
            : base(property, format)
        {
            var s = format;
            s = StringUtils.ToNormalizedWord(s, true);
            s = Regex.Escape(s);
            s = SearchCore.ToFuzzyNumberRegex(s);
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            s = context.FuzzyStringCache.GetString(s);
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：単語検索
    /// </summary>
    public class WordSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public WordSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            var s = format;
            var first = SearchCore.GetNotCodeBlockRegexString(s.First());
            var last = SearchCore.GetNotCodeBlockRegexString(s.Last());
            s = StringUtils.ToNormalizedWord(s, false);
            s = Regex.Escape(s);
            s = SearchCore.ToFuzzyNumberRegex(s);
            if (first != null) s = $"(^|{first}){s}";
            if (last != null) s = $"{s}({last}|$)";
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            s = context.WordStringCache.GetString(s);
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：完全検索
    /// </summary>
    public class ExactSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public ExactSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            var s = Regex.Escape(format);
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：正規表現検索
    /// </summary>
    public class RegularExpressionSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public RegularExpressionSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
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

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
          var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }
    }

    /// <summary>
    /// 文字列：正規表現検索 (IgnoreCase)
    /// </summary>
    public class RegularExpressionIgnoreSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public RegularExpressionIgnoreSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
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

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }
    }


    /// <summary>
    /// 比較：等しい
    /// </summary>
    public class EqualsSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public EqualsSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);
            return value.CompareTo(_referenceValue) == 0;
        }
    }

    /// <summary>
    /// 比較：等しくない
    /// </summary>
    public class NotEqualsSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public NotEqualsSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);
            return value.CompareTo(_referenceValue) != 0;
        }
    }

    /// <summary>
    /// 比較：より大きい
    /// </summary>
    public class GreaterThanSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public GreaterThanSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) > 0;
        }
    }

    /// <summary>
    /// 比較：より小さい
    /// </summary>
    public class LessThanSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public LessThanSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) < 0;
        }
    }


    /// <summary>
    /// 比較：以上
    /// </summary>
    public class GreaterThanEqualSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public GreaterThanEqualSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) >= 0;
        }
    }

    /// <summary>
    /// 比較：以下
    /// </summary>
    public class LessThanEqualSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public LessThanEqualSearchOperation(SearchPropertyProfile property, string format) : base(property, format)
        {
            _referenceValue = property.Parse(format);
        }

        public override bool IsMatch(SearchValueContext context, ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) <= 0;
        }
    }

}
