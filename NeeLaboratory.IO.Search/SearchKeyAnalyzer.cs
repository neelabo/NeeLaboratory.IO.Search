﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NeeLaboratory.IO.Search
{
    public class SearchKeyAnalyzer
    {
        delegate void StateFunc(Context context);

        private enum State
        {
            S00 = 0,
            S01,
            S02,
            S03,
            S04,
            S05,
            S06,
            END,
            ERR = -1,
        }

        private enum Trigger
        {
            End = 0,
            Space,
            DoubleQuote,
            Any,
        }

        private readonly State[,] _table = new State[,]
        {
            {State.END, State.S01, State.S04, State.S02, }, // S00
            {State.S00, State.S00, State.S00, State.S00, }, // S01
            {State.S03, State.S03, State.S02, State.S02, }, // S02
            {State.END, State.END, State.END, State.END, }, // S03
            {State.S06, State.S05, State.S06, State.S05, }, // S04
            {State.S06, State.S05, State.S06, State.S05, }, // S05
            {State.END, State.END, State.END, State.END, }, // S06
        };

        private readonly SearchOptionAliasMap _alias;
        private readonly SearchOptionMap _options;
        private readonly List<StateFunc> _stateMap;


        public SearchKeyAnalyzer() : this(new SearchOptionMap(), new SearchOptionAliasMap())
        {
        }

        public SearchKeyAnalyzer(SearchOptionMap options, SearchOptionAliasMap alias)
        {
            _options = options;
            _alias = alias;
            _stateMap = new List<StateFunc>
            {
                State00,
                State01,
                State02,
                State03,
                State04,
                State05,
                State06,
            };

        }


        public List<SearchKey> Analyze(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new List<SearchKey>();
            }

            var context = new Context(_options, _alias, source);
            while (!context.IsEnd)
            {
                var state = State.S00;
                while (state != State.END)
                {
                    ////Debug.WriteLine($"{state}: {context.StateString()}");
                    var func = _stateMap[(int)state];
                    func.Invoke(context);
                    var trigger = context.ReadTrigger();
                    state = _table[(int)state, (int)trigger];
                    if (state == State.ERR)
                    {
                        throw new ApplicationException("Keyword StateMachine Exception.");
                    }
                }
            }
            return context.Result;
        }


        private void State00(Context context)
        {
        }

        private void State01(Context context)
        {
            context.Next();
        }

        private void State02(Context context)
        {
            context.Push();
            context.Next();
        }

        private void State03(Context context)
        {
            context.Answer(false);
        }

        private void State04(Context context)
        {
            context.Next();
        }

        private void State05(Context context)
        {
            context.Push();
            context.Next();
        }

        private void State06(Context context)
        {
            context.Answer(true);
            context.Next();
        }


        private class Context
        {
            private readonly SearchOptionMap _options;
            private readonly SearchOptionAliasMap _alias;
            private readonly string _source;
            private int _header;
            private SearchKey _work;


            public Context(SearchOptionMap options, SearchOptionAliasMap alias, string source)
            {
                _options = options;
                _alias = alias;
                _source = source;
                _header = 0;
                ResetWork();
            }

            public bool IsEnd => _header >= _source.Length;

            public List<SearchKey> Result { get; private set; } = new List<SearchKey>();


            [MemberNotNull(nameof(_work))]
            private void ResetWork()
            {
                _work = new SearchKey("", SearchConjunction.And, SearchOperatorProfiles.TrueSearchOperationProfile, SearchPropertyProfiles.TextPropertyProfile);
            }

            public void Next()
            {
                _header = (_header < _source.Length) ? _header + 1 : _header;
            }

            public void Back()
            {
                _header = (_header > 0) ? _header - 1 : _header;
            }

            public char Read()
            {
                return (_header < _source.Length) ? _source[_header] : '\0';
            }

            public Trigger ReadTrigger()
            {
                var c = Read();
                if (char.IsWhiteSpace(c))
                {
                    return Trigger.Space;
                }
                return c switch
                {
                    '\0' => Trigger.End,
                    '"' => Trigger.DoubleQuote,
                    _ => Trigger.Any,
                };
            }

            public void Push()
            {
                _work.Word += Read();
            }


            public void Answer(bool isExact)
            {
                if (string.IsNullOrEmpty(_work.Word))
                {
                    ResetWork();
                    return;
                }

                if (_work.Pattern == SearchOperatorProfiles.TrueSearchOperationProfile)
                {
                    _work.Pattern = isExact ? SearchOperatorProfiles.ExactSearchOperationProfile : SearchOperatorProfiles.FuzzySearchOperationProfile;
                }

                if (_work.Pattern != SearchOperatorProfiles.ExactSearchOperationProfile && _work.Word[0] == '/')
                {
                    var options = _alias.Decode(_work.Word);
                    foreach (var option in options)
                    {
                        if (_options.TryGetValue(option, out var value))
                        {
                            switch (value)
                            {
                                case ConjunctionSearchOption conjunctionSearchOption:
                                    _work.Conjunction = conjunctionSearchOption.SearchConjunction;
                                    break;
                                case PropertySearchOption propertySearchOption:
                                    _work.Property = propertySearchOption.Profile;
                                    break;
                                case OperationSearchOption operationSearchOption:
                                    _work.Pattern = operationSearchOption.Profile;
                                    break;
                                default:
                                    throw new InvalidOperationException($"Not supported search option type: {value.GetType()}");
                            }
                        }
                        else
                        {
                            throw new SearchKeywordOptionException($"Not supported option: {_work.Word}") { Option = _work.Word };
                        }
                    }
                    _work.Word = "";
                }
                else
                {
                    // 実際にフィルターを生成することでフォーマットをチェックする
                    var _ = _work.Pattern.CreateFunc(_work.Property, _work.Word);

                    ////Debug.WriteLine($"SearchKey: {_work}");
                    Result.Add(_work);
                    ResetWork();
                }
            }

            public string StateString()
            {
                return $"Header={_header}, Char={Read()}";
            }
        }
    }
}
