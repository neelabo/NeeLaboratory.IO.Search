using System;
using System.Collections.Generic;

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
        public static string DefaultPropertyName => "text";


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

        public static SearchValue CreateSearchValue(string format)
        {
            return new StringSearchValue(format);
        }
    }

    public class DateTimeSearchValue : SearchValue
    {
        public static DateTimeSearchValue Default { get; } = new(default);
        public static string DefaultPropertyName => "date";


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
            return new DateTimeSearchValue(DateTime.Parse(value));
        }

        public static SearchValue CreateSearchValue(string format)
        {
            return new DateTimeSearchValue(DateTime.Parse(format));
        }
    }


    //public delegate SearchValue CreateSearchValueFunc(string format);


    public class SearchPropertyMap
    {
        private Dictionary<string, SearchPropertyProfile> _map = new();


        public SearchPropertyProfile this[string s]
        {
            get => _map[s];
        }


        public void Add(SearchPropertyProfile profile)
        {
            _map.Add(profile.Name, profile);
        }

        //public SearchValue Parse(string property, string format)
        //{
        //    return _map[property].Parse(format);
        //}
    }


    public class SearchPropertyProfile
    {
        public SearchPropertyProfile(string name, SearchValue defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public SearchValue DefaultValue { get; }

        public SearchValue Parse(string format)
        {
            return DefaultValue.Parse(format);
        }
    }

}
