using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public abstract class SearchOperation
    {
        public SearchOperation(SearchValueContext context, string property, string format)
        {
            Context = context;
            Property = property;
            Format = format;
        }

        protected SearchValueContext Context { get; }
        public string Property { get; }
        protected string Format { get; }

        public abstract bool IsMatch(ISearchItem e);


#if false
        // NOTE: リフレクションを使っているので微妙
        public static SearchOperation Create<T>(SearchValueContext context, string value)
            where T : SearchOperation, new()
        {
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(SearchValueContext), typeof(string) });
            if (constructor == null) throw new InvalidOperationException();
            return (T)constructor.Invoke(new object[] {context, value });
        }
#endif
    }

    /// <summary>
    /// 文字列：曖昧検索
    /// </summary>
    public class FuzzySearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public FuzzySearchOperation(SearchValueContext context, string property, string format)
            : base(context, property, format)
        {
            var s = format;
            s = StringUtils.ToNormalizedWord(s, true);
            s = Regex.Escape(s);
            s = SearchCore.ToFuzzyNumberRegex(s);
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            s = this.Context.FuzzyStringCache.GetString(s);
            return _regex.Match(s).Success;
        }

        // TODO: for static virtual method (C#11)
        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new FuzzySearchOperation(context, property, pattern);
        }
    }

    /// <summary>
    /// 文字列：単語検索
    /// </summary>
    public class WordSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public WordSearchOperation(SearchValueContext context, string property, string format) : base(context, property, format)
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

        public override bool IsMatch(ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            s = this.Context.WordStringCache.GetString(s);
            return _regex.Match(s).Success;
        }

        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new WordSearchOperation(context, property, pattern);
        }
    }

    /// <summary>
    /// 文字列：完全検索
    /// </summary>
    public class ExactSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public ExactSearchOperation(SearchValueContext context, string property, string format) : base(context, property, format)
        {
            var s = Regex.Escape(format);
            _regex = new Regex(s, RegexOptions.Compiled);
        }

        public override bool IsMatch(ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }

        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new ExactSearchOperation(context, property, pattern);
        }
    }

    /// <summary>
    /// 文字列：正規表現検索
    /// </summary>
    public class RegularExpressionSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public RegularExpressionSearchOperation(SearchValueContext context, string property, string format) : base(context, property, format)
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

        public override bool IsMatch(ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }

        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new RegularExpressionSearchOperation(context, property, pattern);
        }
    }

    /// <summary>
    /// 文字列：正規表現検索 (IgnoreCase)
    /// </summary>
    public class RegularExpressionIgnoreSearchOperation : SearchOperation
    {
        private readonly Regex _regex;

        public RegularExpressionIgnoreSearchOperation(SearchValueContext context, string property, string format) : base(context, property, format)
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

        public override bool IsMatch(ISearchItem e)
        {
            var s = e.GetValue(Property).ToString();
            return _regex.Match(s).Success;
        }

        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new RegularExpressionIgnoreSearchOperation(context, property, pattern);
        }
    }

    /// <summary>
    /// 比較：より大きい
    /// </summary>
    public class GraterThanSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public GraterThanSearchOperation(SearchValueContext context, string property, string format) : base(context, property, format)
        {
            _referenceValue = context.CreateSearchValue(property, format);
        }

        public override bool IsMatch(ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) > 0;
        }

        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new GraterThanSearchOperation(context, property, pattern);
        }
    }

    /// <summary>
    /// 比較：より小さい
    /// </summary>
    public class LessThanSearchOperation : SearchOperation
    {
        private readonly SearchValue _referenceValue;

        public LessThanSearchOperation(SearchValueContext context, string property, string format) : base(context, property, format)
        {
            _referenceValue = context.CreateSearchValue(property, format);
        }

        public override bool IsMatch(ISearchItem e)
        {
            var value = e.GetValue(Property);

            return value.CompareTo(_referenceValue) < 0;
        }

        public static SearchOperation Create(SearchValueContext context, string property, string pattern)
        {
            return new LessThanSearchOperation(context, property, pattern);
        }
    }


}
