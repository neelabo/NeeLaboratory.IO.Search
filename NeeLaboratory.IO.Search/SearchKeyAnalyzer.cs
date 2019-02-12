// Copyright (c) 2015-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            S07,
            S08,
            S09,
            S10,
            S11,
            S12,
            S13,
            S14,
            END,
            ERR = -1,
        }

        private enum Trigger
        {
            End = 0,
            Space,
            AtMark,
            Minus,
            DoubleQuote,
            Any,
        }

        private State[,] _table = new State[,]
        {
            {State.S03, State.S13, State.S01, State.S01, State.S01, State.S01, }, // S00
            {State.ERR, State.ERR, State.S04, State.S07, State.S10, State.S02, }, // S01
            {State.S03, State.S03, State.S02, State.S02, State.S02, State.S02, }, // S02
            {State.END, State.END, State.END, State.END, State.END, State.END, }, // S03
            {State.S03, State.S03, State.S05, State.S01, State.S01, State.S01, }, // S04
            {State.S03, State.S03, State.S02, State.S02, State.S02, State.S02, }, // S05
            {State.END, State.END, State.END, State.END, State.END, State.END, }, // S06 (no used)
            {State.S03, State.S03, State.S01, State.S08, State.S01, State.S01, }, // S07
            {State.S03, State.S03, State.S02, State.S02, State.S02, State.S02, }, // S08
            {State.END, State.END, State.END, State.END, State.END, State.END, }, // S09 (no used)
            {State.S03, State.S11, State.S11, State.S11, State.S12, State.S11, }, // S10
            {State.S03, State.S11, State.S11, State.S11, State.S12, State.S11, }, // S11
            {State.S03, State.S03, State.S03, State.S03, State.S11, State.S03, }, // S12
            {State.S00, State.S00, State.S00, State.S00, State.S00, State.S00, }, // S13
            {State.END, State.END, State.END, State.END, State.END, State.END, }, // S14 (no used)
            {State.ERR, State.ERR, State.ERR, State.ERR, State.ERR, State.ERR, }, // SEND
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
                State07,
                State08,
                State09,
                State10,
                State11,
                State12,
                State13,
                State14,
            };
        }


        public List<SearchKey> Analyze(string source)
        {
            var context = new Context(source);
            while (!context.IsEnd)
            {
                var state = State.S00;
                context.ResetWork();
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
        }

        private void State02(Context context)
        {
            context.Push();
            context.Next();
        }

        private void State03(Context context)
        {
            context.Answer();
        }

        private void State04(Context context)
        {
            context.SetIsWord(true);
            context.Next();
        }

        private void State05(Context context)
        {
            context.SetIsWord(false);
            context.Push();
            context.Next();
        }

        private void State06(Context context)
        {
            throw new NotImplementedException();
        }

        private void State07(Context context)
        {
            context.SetIsExclude(true);
            context.Next();
        }

        private void State08(Context context)
        {
            context.SetIsExclude(false);
            context.Push();
            context.Next();
        }

        private void State09(Context context)
        {
            throw new NotImplementedException();
        }

        private void State10(Context context)
        {
            context.SetIsPerfect(true);
            context.Next();
        }

        private void State11(Context context)
        {
            context.Push();
            context.Next();
        }

        private void State12(Context context)
        {
            context.Next();
        }

        private void State13(Context context)
        {
            context.Next();
        }

        private void State14(Context context)
        {
            throw new NotImplementedException();
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
            }

            public bool IsEnd => _header >= _source.Length;

            public List<SearchKey> Result { get; private set; } = new List<SearchKey>();

            public void ResetWork()
            {
                _work = new SearchKey();
            }

            public void SetIsExclude(bool flag)
            {
                _work.IsExclude = flag;
            }

            public void SetIsPerfect(bool flag)
            {
                _work.IsPerfect = flag;
            }

            public void SetIsWord(bool flag)
            {
                _work.IsWord = flag;
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

                switch (Read())
                {
                    case '\0':
                        return Trigger.End;
                    case '@':
                        return Trigger.AtMark;
                    case '-':
                        return Trigger.Minus;
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

            public void Answer()
            {
                if (string.IsNullOrEmpty(_work.Word)) return;
                Result.Add(_work.Clone());
            }

            public void Answer(string word)
            {
                _work.Word = word;
                Answer();
            }

            public string StateString()
            {
                return $"Header={_header}, Char={Read()}";
            }
        }

    }
}
