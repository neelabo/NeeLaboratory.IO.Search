using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeeLaboratory.IO.Search
{
    public abstract class SearchValue
    {
        public abstract int CompareTo(SearchValue other);
        public abstract SearchValue Parse(string value);
        public override string ToString() => base.ToString() ?? "";
    }


    public class StringSearchValue : SearchValue
    {
        public static StringSearchValue Default { get; } = new("");


        private readonly string _value;

        public StringSearchValue(string value)
        {
            _value = value;
        }

        public override int CompareTo(SearchValue other)
        {
            return _value.CompareTo(((StringSearchValue)other)._value);
        }

        public override SearchValue Parse(string value)
        {
            return new StringSearchValue(value);
        }

        public override string ToString()
        {
            return _value;
        }
    }

    public class DateTimeSearchValue : SearchValue
    {
        public static DateTimeSearchValue Default { get; } = new(default);

        private static readonly Regex _regexDateTimeCustom = new(@"^([+-]?\d+)(day|month|year)$");
        private static readonly string _stringFormat = "yyyy/MM/dd HH:mm";

        private readonly DateTime _value;

        public DateTimeSearchValue(DateTime date)
        {
            _value = date;
        }

        public override int CompareTo(SearchValue other)
        {
            return _value.CompareTo(((DateTimeSearchValue)other)._value);
        }

        public override SearchValue Parse(string value)
        {
            try
            {
                var match = _regexDateTimeCustom.Match(value);
                if (match.Success)
                {
                    var num = int.Parse(match.Groups[1].Value);
                    var dateTime = match.Groups[2].Value switch
                    {
                        "day" => DateTime.Now.AddDays(num),
                        "month" => DateTime.Now.AddMonths(num),
                        "year" => DateTime.Now.AddYears(num),
                        _ => throw new NotSupportedException(),
                    };
                    return new DateTimeSearchValue(dateTime);
                }
                else
                {
                    var dateTime = DateTime.Parse(value);
                    return new DateTimeSearchValue(dateTime);
                }
            }
            catch (Exception ex)
            {
                throw new SearchKeywordDateTimeException($"DateTime parse error: Cannot parse {value}", ex);
            }
        }

        public override string ToString()
        {
            return _value.ToString(_stringFormat);
        }
    }

}
