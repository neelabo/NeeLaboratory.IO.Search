using NeeLaboratory.IO.Search.Diagnostics;
using NeeLaboratory.IO.Search.FileSearch;
using NeeLaboratory.IO.Search.FileNode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NeeLaboratory.IO.Search.Test
{
    public class SearchEngineTest
    {
        private static readonly string _folderRoot = @"TestFolders";
        private static readonly string _folderSub1 = @"TestFolders\SubFolder1";
        private static readonly string _folderSub2 = @"TestFolders\SubFolder2";

        private static readonly string _fileAppend1 = @"TestFolders\SubFolder1\append1.txt";
        private static readonly string _fileAppend2 = @"TestFolders\SubFolder1\append2.bin";
        private static readonly string _fileAppend2Ex = @"TestFolders\SubFolder1\append2.txt";

        private readonly ITestOutputHelper _output;


        public SearchEngineTest(ITestOutputHelper output)
        {
            _output = output;
        }


        /// <summary>
        /// テスト環境初期化
        /// </summary>
        public static SearchEngine CreateTestEnvironment()
        {
            // 不要ファイル削除
            if (File.Exists(_fileAppend1)) File.Delete(_fileAppend1);
            if (File.Exists(_fileAppend2)) File.Delete(_fileAppend2);
            if (File.Exists(_fileAppend2Ex)) File.Delete(_fileAppend2Ex);

            // エンジン初期化
            var engine = new SearchEngine();
            engine.AllowFolder = false;
            SearchEngine.Logger.SetLevel(SourceLevels.All);
            engine.AddSearchAreas(new NodeArea(_folderRoot, true), new NodeArea(_folderSub1, true), new NodeArea(_folderSub2, true));
            engine.CommandEngineLogger.SetLevel(SourceLevels.All);

            return engine;
        }



        /// <summary>
        /// 非同期標準テスト
        /// </summary>
        [Fact]
        public async Task SearchEngineTestAsync()
        {
            // 初期化
            var engine = new SearchEngine();

            // 検索パス設定
            engine.AddSearchAreas(new NodeArea(_folderSub1, true));
            engine.AddSearchAreas(new NodeArea(_folderRoot, true));


            // 検索１：通常検索
            SearchResult result = await engine.SearchAsync("File1");

            // 結果表示
            foreach (var item in result.Items)
            {
                _output.WriteLine($"{item.Name}");
            }
        }



        /// <summary>
        /// 検索範囲テスト
        /// </summary>
        [Fact]
        public async Task SearchEngineAreaTest()
        {
            Development.Logger.SetLevel(SourceLevels.Verbose);

            var engine = new SearchEngine();

            // パスの追加
            engine.AddSearchAreas(new NodeArea(_folderRoot, false));
            await engine.WaitAsync(CancellationToken.None);
            //engine.DumpTree(true);
            Assert.Equal(7, engine.NodeCount);

            engine.SetSearchAreas(new ObservableCollection<NodeArea>() { new NodeArea(_folderRoot, true) });
            await engine.WaitAsync(CancellationToken.None);
            //engine.DumpTree(true);
            Assert.Equal(13, engine.NodeCount);

            engine.SetSearchAreas(new ObservableCollection<NodeArea>() { new NodeArea(_folderRoot, true), new NodeArea(_folderSub1, true) });
            await engine.WaitAsync(CancellationToken.None);
            Assert.Equal(13, engine.NodeCount);

            // 変則エリア。NodeTreeの結合が発生
            engine.SetSearchAreas(new ObservableCollection<NodeArea>() { new NodeArea(_folderRoot, false), new NodeArea(_folderSub1, true) });
            await engine.WaitAsync(CancellationToken.None);
            engine.DumpTree(true);
            Assert.Equal(10, engine.NodeCount);

            engine.AllowFolder = true;
            var result = await engine.SearchAsync("SubFolder1");
            Assert.Single(result.Items);
        }


        /// <summary>
        /// 基本検索テスト
        /// </summary>
        [Theory]
        [InlineData(9, "file")]
        [InlineData(0, "/word あいう")]
        [InlineData(1, "/word あいうえお")]
        [InlineData(0, "/word ウエオ")]
        [InlineData(1, "/word アイウエオ")]
        [InlineData(0, "/re file")]
        [InlineData(9, "/ire file")]
        [InlineData(3, "File3")]
        [InlineData(6, "File3 /or File2")]
        [InlineData(3, "file3")]
        [InlineData(1, "file3 /not sub")]
        [InlineData(10, "/since 2018-01-01")]
        [InlineData(0, "/until 2018-01-01")]
        public async Task SearchEngineSearchTest(int expected, string keyword)
        {
            var engine = CreateTestEnvironment();

            SearchResult result = await engine.SearchAsync(keyword);

            foreach(var item in result.Items)
            {
                _output.WriteLine($"{(item.FileInfo.IsDirectory ? 'D' : 'F')}: {item.FileInfo.Path}");
            }

            Assert.Equal(expected, result.Items.Count);
        }


        /// <summary>
        /// マルチ検索テスト
        /// </summary>
        [Fact]
        public async Task SearchEngineMultiSearchTest()
        {
            var engine = CreateTestEnvironment();

            List<SearchResult> result;

            var keywords = new string[] { "file", "/word あいう", "/word あいうえお", "/word ウエオ" };
            var answers = new int[] { 9, 0, 1, 0 };

            result = await engine.MultiSearchAsync(keywords);
            Assert.Equal(answers[0], result[0].Items.Count);
            Assert.Equal(answers[1], result[1].Items.Count);
            Assert.Equal(answers[2], result[2].Items.Count);
            Assert.Equal(answers[3], result[3].Items.Count);
        }


        /// <summary>
        /// ファイルシステム監視テスト
        /// </summary>
        [Fact]
        public async Task SearchEngineWatchResultTest()
        {
            var engine = CreateTestEnvironment();

            var result = await engine.SearchAsync(".txt");
            var resultCount = result.Items.Count;

            var watcher = new SearchResultWatcher(engine, result);

            // ファイル追加 ...
            using (FileStream stream = File.Create(_fileAppend1)) { }
            using (FileStream stream = File.Create(_fileAppend2)) { }

            await Task.Delay(100);
            Assert.True(result.Items.Count == resultCount + 1);


            // 名前変更
            var fileAppend2Ex = Path.ChangeExtension(_fileAppend2, ".txt");
            File.Move(_fileAppend2, fileAppend2Ex);
            await Task.Delay(100);
            Assert.True(result.Items.Count == resultCount + 2);

            // 内容変更
            using (FileStream stream = File.Open(fileAppend2Ex, FileMode.Append))
            {
                stream.WriteByte(0x00);
            }
            await Task.Delay(100);
            var item = result.Items.First(e => e.Path == Path.GetFullPath(fileAppend2Ex));
            Assert.Equal(1, item.FileInfo.Size);

            // ファイル削除...
            File.Delete(_fileAppend1);
            File.Delete(fileAppend2Ex);
            await Task.Delay(100);
            await engine.WaitAsync(CancellationToken.None);

            // 戻ったカウント確認
            Assert.True(result.Items.Count == resultCount);
        }

        /// <summary>
        /// あいまい変換テスト
        /// </summary>
        [Fact]
        public void SearchEngineNormalizeTest()
        {
            string normalized;

            normalized = SearchStringTools.ToNormalizedWord("ひらがなゔう゛か゛", true);
            Assert.Equal("ヒラガナヴヴガ", normalized);

            normalized = SearchStringTools.ToNormalizedWord("ﾊﾝｶｸｶﾞﾅｳﾞ", true);
            Assert.Equal("ハンカクガナヴ", normalized);

            normalized = SearchStringTools.ToNormalizedWord("混合された日本語ﾃﾞス。", true);
            Assert.Equal("混合サレタ日本語デス。", normalized);

            normalized = SearchStringTools.ToNormalizedWord("㌫", true);
            Assert.Equal("パ-セント", normalized);

            normalized = SearchStringTools.ToNormalizedWord("♡♥❤?", true);
            Assert.Equal("♡♡♡?", normalized);
        }




        /// <summary>
        /// 検索キーワード解析テスト
        /// </summary>
        [Fact]
        public void SearchEngineKeywordAnalyzeTest()
        {
            var context = new SearchContext();
            context.AddProfile(new DateSearchProfile());
            var analyzer = new SearchKeyAnalyzer(context.KeyOptions, context.KeyAlias);
            List<SearchKey> keys;

            keys = analyzer.Analyze("");
            Assert.Empty(keys);

            keys = analyzer.Analyze("    ");
            Assert.Empty(keys);

            keys = analyzer.Analyze("word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);


            keys = analyzer.Analyze("    word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);

            keys = analyzer.Analyze("\"word1\"");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Exact, "word1"), keys[0]);

            keys = analyzer.Analyze("\"word1 word2 ");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Exact, "word1 word2 "), keys[0]);

            keys = analyzer.Analyze("/and word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);

            keys = analyzer.Analyze("/or word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.Or, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);

            keys = analyzer.Analyze("/not word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.Not, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);


            keys = analyzer.Analyze("/m0 word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Exact, "word1"), keys[0]);

            keys = analyzer.Analyze("/m1 word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Word, "word1"), keys[0]);

            keys = analyzer.Analyze("/m2 word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);

            keys = analyzer.Analyze("/re word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.RegularExpression, "word1"), keys[0]);

            keys = analyzer.Analyze("/ire word1");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.RegularExpressionIgnoreCase, "word1"), keys[0]);

            // multi
            keys = analyzer.Analyze("\"word1 word2\" word3");
            Assert.Equal(2, keys.Count);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Exact, "word1 word2"), keys[0]);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word3"), keys[1]);

            keys = analyzer.Analyze("word1 /or word2");
            Assert.Equal(2, keys.Count);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);
            Assert.Equal(new SearchKey(SearchConjunction.Or, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word2"), keys[1]);

            keys = analyzer.Analyze("word1 /or /m1 word2");
            Assert.Equal(2, keys.Count);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);
            Assert.Equal(new SearchKey(SearchConjunction.Or, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Word, "word2"), keys[1]);

            keys = analyzer.Analyze("word1 /not /re \"word2 word3\"");
            Assert.Equal(2, keys.Count);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);
            Assert.Equal(new SearchKey(SearchConjunction.Not, SearchPropertyProfiles.Text, null, SearchFilterProfiles.RegularExpression, "word2 word3"), keys[1]);

            keys = analyzer.Analyze("word1 /not /or /re /m1 word2  ");
            Assert.Equal(2, keys.Count);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);
            Assert.Equal(new SearchKey(SearchConjunction.Or, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Word, "word2"), keys[1]);

            keys = analyzer.Analyze("word1 /not /or /re /m1 \"word2 word3\" ");
            Assert.Equal(2, keys.Count);
            Assert.Equal(new SearchKey(SearchConjunction.And, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Fuzzy, "word1"), keys[0]);
            Assert.Equal(new SearchKey(SearchConjunction.Or, SearchPropertyProfiles.Text, null, SearchFilterProfiles.Word, "word2 word3"), keys[1]);


            keys = analyzer.Analyze("/until -5day");
            Assert.Single(keys);

            keys = analyzer.Analyze("/until +10month");
            Assert.Single(keys);

            keys = analyzer.Analyze("/until 123year");
            Assert.Single(keys);

            Assert.Throws<SearchKeywordDateTimeException>(() => analyzer.Analyze("/since -day"));
            Assert.Throws<SearchKeywordDateTimeException>(() => analyzer.Analyze("/since 1"));
            Assert.Throws<SearchKeywordDateTimeException>(() => analyzer.Analyze("/since day"));
            Assert.Throws<SearchKeywordDateTimeException>(() => analyzer.Analyze("/since +-1day"));
            Assert.Throws<SearchKeywordDateTimeException>(() => analyzer.Analyze("/since -1days"));

            keys = analyzer.Analyze("/since 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.GreaterThanEqual, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/until 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.LessThanEqual, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date /m.lt 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.LessThan, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date /m.le 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.LessThanEqual, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date /m.eq 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.Equal, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date /m.ne 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.NotEqual, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date /m.ge 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.GreaterThanEqual, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date /m.gt 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.GreaterThan, "2018-01-01"), keys[0]);
        }

        /// <summary>
        /// 検索キーワード解析詳細テスト
        /// </summary>
        [Fact]
        public void SearchEngineKeywordAnalyzeParameterTest()
        {
            var context = new SearchContext();
            context.AddProfile(new DateSearchProfile());
            var analyzer = new SearchKeyAnalyzer(context.KeyOptions, context.KeyAlias);
            List<SearchKey> keys;

            keys = analyzer.Analyze("/p.date /m.gt 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, null, SearchFilterProfiles.GreaterThan, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date.Any /m.gt 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, "any", SearchFilterProfiles.GreaterThan, "2018-01-01"), keys[0]);

            keys = analyzer.Analyze("/p.date.Any.Test /m.gt 2018-01-01");
            Assert.Single(keys);
            Assert.Equal(new SearchKey(SearchConjunction.And, DateSearchPropertyProfiles.Date, "any.test", SearchFilterProfiles.GreaterThan, "2018-01-01"), keys[0]);
        }

        [Fact]
        public void SearchEngineKeywordAnalyzeOptionExceptionTest()
        {
            var context = new SearchContext();
            var analyzer = new SearchKeyAnalyzer(context.KeyOptions, context.KeyAlias);
            List<SearchKey> keys;

            Assert.Throws<SearchKeywordOptionException>(() =>
            {
                keys = analyzer.Analyze("word1 /unknown word2");
                Assert.Equal(2, keys.Count);
            });
        }

        [Fact]
        public void SearchEngineKeywordAnalyzeRegularExpressionExceptionTest()
        {
            var context = new SearchContext();
            var analyzer = new SearchKeyAnalyzer(context.KeyOptions, context.KeyAlias);
            List<SearchKey> keys;

            Assert.Throws<SearchKeywordRegularExpressionException>(() =>
            {
                keys = analyzer.Analyze("word1 /re ^(hoge");
                Assert.Equal(2, keys.Count);
            });

            Assert.Throws<SearchKeywordRegularExpressionException>(() =>
            {
                keys = analyzer.Analyze("word1 /ire ^(hoge");
                Assert.Equal(2, keys.Count);
            });
        }


        [Theory]
        [InlineData(1, "/p.date /m.eq 2018-02-01")]
        [InlineData(2, "/p.date /m.ne 2018-02-01")]
        [InlineData(1, "/p.date /m.lt 2018-02-01")]
        [InlineData(2, "/p.date /m.le 2018-02-01")]
        [InlineData(1, "/p.date /m.gt 2018-02-01")]
        [InlineData(2, "/p.date /m.ge 2018-02-01")]
        public void SearchCoreCompareTest(int expected, string keyword)
        {
            var context = new SearchContext();
            context.AddProfile(new DateSearchProfile());
            var search = new Searcher(context);
            var items = new List<SampleSearchItem>
            {
                new SampleSearchItem("2018-01-01"),
                new SampleSearchItem("2018-02-01"),
                new SampleSearchItem("2018-03-01"),
            };

            var result = search.Search(keyword, items, CancellationToken.None);
            Assert.Equal(expected, result.Count());
        }
    }

    public class SampleSearchItem : ISearchItem
    {
        private string _value;


        public SampleSearchItem(string value)
        {
            _value = value;
        }

        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(_value);
                case "date":
                    return new DateTimeSearchValue(DateTime.Parse(_value));
                default:
                    throw new NotSupportedException();
            }
        }
    }

}