// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

        private State[,] _table = new State[,]
        {
            {State.END, State.S01, State.S04, State.S02, }, // S00
            {State.S00, State.S00, State.S00, State.S00, }, // S01
            {State.S03, State.S03, State.S02, State.S02, }, // S02
            {State.END, State.END, State.END, State.END, }, // S03
            {State.S06, State.S05, State.S06, State.S05, }, // S04
            {State.S06, State.S05, State.S06, State.S05, }, // S05
            {State.END, State.END, State.END, State.END, }, // S06
        };

        private List<StateFunc> _stateMap;


        public SearchKeyAnalyzer()
        {
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

            var context = new Context(source);
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
            context.Answer(SearchPattern.Standard);
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
            context.Answer(SearchPattern.Exact);
            context.Next();
        }


        private class Context
        {
            private string _source;
            private int _header;
            private SearchKey _work;

            public Context(string source)
            {
                _source = source;
                _header = 0;
                ResetWork();
            }

            public bool IsEnd => _header >= _source.Length;

            public List<SearchKey> Result { get; private set; } = new List<SearchKey>();


            private void ResetWork()
            {
                _work = new SearchKey(null, SearchConjunction.And, SearchPattern.Undefined);
            }

            public void SetPatternIfUndefined(SearchPattern pattern)
            {
                _work.Pattern = pattern;
            }

            public void SetConjunction(SearchConjunction conjunction)
            {
                _work.Conjunction = conjunction;
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
                switch (c)
                {
                    case '\0':
                        return Trigger.End;
                    case '"':
                        return Trigger.DoubleQuote;
                    default:
                        return Trigger.Any;
                }
            }

            public void Push()
            {
                _work.Word += Read();
            }

            public void Answer(SearchPattern pattern)
            {
                if (string.IsNullOrEmpty(_work.Word))
                {
                    ResetWork();
                    return;
                }

                if (_work.Pattern == SearchPattern.Undefined)
                {
                    _work.Pattern = pattern;
                }

                if (_work.Pattern != SearchPattern.Exact && _work.Word[0] == '/')
                {
                    switch (_work.Word)
                    {
                        case "/and":
                            _work.Conjunction = SearchConjunction.And;
                            break;
                        case "/or":
                            _work.Conjunction = SearchConjunction.Or;
                            break;
                        case "/not":
                            _work.Conjunction = SearchConjunction.Not;
                            break;
                        case "/re":
                            _work.Pattern = SearchPattern.RegularExpression;
                            break;
                        case "/ire":
                            _work.Pattern = SearchPattern.RegularExpressionIgnoreCase;
                            break;
                        case "/m0":
                        case "/exact":
                            _work.Pattern = SearchPattern.Exact;
                            break;
                        case "/m1":
                        case "/word":
                            _work.Pattern = SearchPattern.Word;
                            break;
                        case "/m2":
                            _work.Pattern = SearchPattern.Standard;
                            break;
                        case "/since":
                            _work.Pattern = SearchPattern.Since;
                            break;
                        case "/until":
                            _work.Pattern = SearchPattern.Until;
                            break;
                        default:
                            ////Debug.WriteLine($"not support option: {_work.Word}");
                            throw new SearchKeywordOptionException($"Not support option: {_work.Word}") { Option = _work.Word };
                    }

                    _work.Word = null;
                }
                else
                {
                    if (_work.Pattern == SearchPattern.RegularExpression || _work.Pattern == SearchPattern.RegularExpressionIgnoreCase)
                    {
                        try
                        {
                            new Regex(_work.Word);
                        }
                        catch(Exception ex)
                        {
                            throw new SearchKeywordRegularExpressionException($"RegularExpression error: {_work.Word}", ex);
                        }
                    }

                    if (_work.Pattern == SearchPattern.Since || _work.Pattern == SearchPattern.Until)
                    {
                        if (!DateTime.TryParse(_work.Word, out _))
                        {
                            throw new SearchKeywordDateTimeException($"Since error: Cannot parth DateTime: {_work.Word}");
                        }
                    }

                    Debug.WriteLine($"SearchKey: {_work}");
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