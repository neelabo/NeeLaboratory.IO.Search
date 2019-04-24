using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeeLaboratory.IO.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.IO.Search.Test
{
    [TestClass()]
    public class SearchEngineTest
    {
        private TestContext _testContext;
        public TestContext TestContext
        {
            get => _testContext;
            set => _testContext = value;
        }


        private static string _folderRoot = @"TestFolders";
        private static string _folderSub1 = @"TestFolders\SubFolder1";
        private static string _folderSub2 = @"TestFolders\SubFolder2";



        /// <summary>
        /// 非同期標準テスト
        /// </summary>
        [TestMethod()]
        public async Task SearchEngineTestAsync()
        {
            // 初期化
            var engine = new SearchEngine();
            engine.Start();

            // 検索パス設定
            engine.SearchAreas.Add(new SearchArea(_folderSub1, true));
            engine.SearchAreas.Add(new SearchArea(_folderRoot, true));


            // 検索１：通常検索
            SearchOption option = new SearchOption(); // { IsPerfect = false };
            SearchResult result = await engine.SearchAsync("File1", option);

            // 結果表示
            foreach (var item in result.Items)
            {
                _testContext.WriteLine($"{item.Name}");
            }
        }


        private static string _fileAppend1 = @"TestFolders\SubFolder1\append1.txt";
        private static string _fileAppend2 = @"TestFolders\SubFolder1\append2.bin";
        private static string _fileAppend2Ex = @"TestFolders\SubFolder1\append2.txt";


        /// <summary>
        /// テスト環境初期化
        /// </summary>
        public SearchEngine CreateTestEnvironment()
        {
            // 不要ファイル削除
            if (File.Exists(_fileAppend1)) File.Delete(_fileAppend1);
            if (File.Exists(_fileAppend2)) File.Delete(_fileAppend2);
            if (File.Exists(_fileAppend2Ex)) File.Delete(_fileAppend2Ex);

            // エンジン初期化
            var engine = new SearchEngine();
            SearchEngine.Logger.SetLevel(SourceLevels.All);
            engine.SearchAreas.Add(new SearchArea(_folderRoot, true));
            engine.SearchAreas.Add(new SearchArea(_folderSub1, true));
            engine.SearchAreas.Add(new SearchArea(_folderSub2, true));
            engine.Start();
            engine.CommandEngineLogger.SetLevel(SourceLevels.All);

            return engine;
        }


        /// <summary>
        /// 検索範囲テスト
        /// </summary>
        [TestMethod()]
        public async Task SearchEngineAreaTest()
        {
            Development.Logger.SetLevel(SourceLevels.Verbose);

            var engine = new SearchEngine();
            engine.Start();

            // パスの追加
            engine.SearchAreas.Add(new SearchArea(_folderRoot, false));
            await engine.WaitAsync();
            //engine.DumpTree(true);
            Assert.AreEqual(7, engine.NodeCount);

            engine.SearchAreas = new ObservableCollection<SearchArea>() { new SearchArea(_folderRoot, true) };
            await engine.WaitAsync();
            //engine.DumpTree(true);
            Assert.AreEqual(13, engine.NodeCount);

            engine.SearchAreas = new ObservableCollection<SearchArea>() { new SearchArea(_folderRoot, true), new SearchArea(_folderSub1, true) };
            await engine.WaitAsync();
            Assert.AreEqual(13, engine.NodeCount);

            // 変則エリア。NodeTreeの結合が発生
            engine.SearchAreas = new ObservableCollection<SearchArea>() { new SearchArea(_folderRoot, false), new SearchArea(_folderSub1, true) };
            await engine.WaitAsync();
            engine.DumpTree(true);
            Assert.AreEqual(10, engine.NodeCount);

            var result = await engine.SearchAsync("SubFolder1", new SearchOption() { AllowFolder = true });
            Assert.AreEqual(1, result.Items.Count);

            engine.Stop();
        }


        /// <summary>
        /// 基本検索テスト
        /// </summary>
        [TestMethod()]
        public async Task SearchEngineSearchTest()
        {
            var engine = CreateTestEnvironment();

            SearchResult result;

            result = await engine.SearchAsync("file", new SearchOption());
            Assert.AreEqual(9, result.Items.Count);

            result = await engine.SearchAsync("/word あいう", new SearchOption());
            Assert.AreEqual(0, result.Items.Count);

            result = await engine.SearchAsync("/word あいうえお", new SearchOption());
            Assert.AreEqual(1, result.Items.Count);

            result = await engine.SearchAsync("/word ウエオ", new SearchOption());
            Assert.AreEqual(0, result.Items.Count);

            result = await engine.SearchAsync("/word アイウエオ", new SearchOption());
            Assert.AreEqual(1, result.Items.Count);

            result = await engine.SearchAsync("/re file", new SearchOption());
            Assert.AreEqual(0, result.Items.Count);

            result = await engine.SearchAsync("/ire file", new SearchOption());
            Assert.AreEqual(9, result.Items.Count);

            result = await engine.SearchAsync("File3", new SearchOption());
            Assert.AreEqual(3, result.Items.Count);

            result = await engine.SearchAsync("File3 /or File2", new SearchOption());
            Assert.AreEqual(6, result.Items.Count);

            result = await engine.SearchAsync("file3", new SearchOption());
            Assert.AreEqual(3, result.Items.Count);

            result = await engine.SearchAsync("file3 /not sub", new SearchOption());
            Assert.AreEqual(1, result.Items.Count);

            result = await engine.SearchAsync("/since 2018-01-01", new SearchOption());
            Assert.AreEqual(10, result.Items.Count);

            result = await engine.SearchAsync("/until 2018-01-01", new SearchOption());
            Assert.AreEqual(0, result.Items.Count);
        }

        /// <summary>
        /// ファイルシステム監視テスト
        /// </summary>
        [TestMethod()]
        public async Task SearchEngineWatchResultTest()
        {
            var engine = CreateTestEnvironment();

            var result = await engine.SearchAsync(".txt", new SearchOption());
            var resultCount = result.Items.Count;

            var watcher = new SearchResultWatcher(engine, result);
            watcher.Start();

            // ファイル追加 ...
            using (FileStream stream = File.Create(_fileAppend1)) { }
            using (FileStream stream = File.Create(_fileAppend2)) { }

            await Task.Delay(100);
            Assert.IsTrue(result.Items.Count == resultCount + 1);


            // 名前変更
            var fileAppend2Ex = Path.ChangeExtension(_fileAppend2, ".txt");
            File.Move(_fileAppend2, fileAppend2Ex);
            await Task.Delay(100);
            Assert.IsTrue(result.Items.Count == resultCount + 2);

            // 内容変更
            using (FileStream stream = File.Open(fileAppend2Ex, FileMode.Append))
            {
                stream.WriteByte(0x00);
            }
            await Task.Delay(100);
            var item = result.Items.First(e => e.Path == Path.GetFullPath(fileAppend2Ex));
            Assert.AreEqual(1, item.FileInfo.Size);

            // ファイル削除...
            File.Delete(_fileAppend1);
            File.Delete(fileAppend2Ex);
            await Task.Delay(100);
            await engine.WaitAsync();

            // 戻ったカウント確認
            Assert.IsTrue(result.Items.Count == resultCount);

            watcher.Stop();
        }

        /// <summary>
        /// あいまい変換テスト
        /// </summary>
        [TestMethod()]
        public void SearchEngineNormalizeTest()
        {
            string normalized;

            normalized = Node.ToNormalisedWord("ひらがなゔう゛か゛", true);
            Assert.AreEqual("ヒラガナヴヴガ", normalized);

            normalized = Node.ToNormalisedWord("ﾊﾝｶｸｶﾞﾅｳﾞ", true);
            Assert.AreEqual("ハンカクガナヴ", normalized);

            normalized = Node.ToNormalisedWord("混合された日本語ﾃﾞス。", true);
            Assert.AreEqual("混合サレタ日本語デス。", normalized);

            normalized = Node.ToNormalisedWord("㌫", true);
            Assert.AreEqual("パ-セント", normalized);

            normalized = Node.ToNormalisedWord("♡♥❤?", true);
            Assert.AreEqual("♡♡♡?", normalized);
        }


        /// <summary>
        /// 検索キーワード解析テスト
        /// </summary>
        [TestMethod()]
        public void SearchEngineKeywordAnalyzeTest()
        {
            var analyzer = new SearchKeyAnalyzer();
            List<SearchKey> keys;

            keys = analyzer.Analyze("");
            Assert.AreEqual(0, keys.Count);

            keys = analyzer.Analyze("    ");
            Assert.AreEqual(0, keys.Count);

            keys = analyzer.Analyze("word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);


            keys = analyzer.Analyze("    word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);

            keys = analyzer.Analyze("\"word1\"");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Exact), keys[0]);

            keys = analyzer.Analyze("\"word1 word2 ");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1 word2 ", SearchConjunction.And, SearchPattern.Exact), keys[0]);

            keys = analyzer.Analyze("/and word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);

            keys = analyzer.Analyze("/or word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.Or, SearchPattern.Standard), keys[0]);

            keys = analyzer.Analyze("/not word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.Not, SearchPattern.Standard), keys[0]);


            keys = analyzer.Analyze("/m0 word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Exact), keys[0]);

            keys = analyzer.Analyze("/m1 word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Word), keys[0]);

            keys = analyzer.Analyze("/m2 word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);

            keys = analyzer.Analyze("/re word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.RegularExpression), keys[0]);

            keys = analyzer.Analyze("/ire word1");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.RegularExpressionIgnoreCase), keys[0]);

            // multi
            keys = analyzer.Analyze("\"word1 word2\" word3");
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new SearchKey("word1 word2", SearchConjunction.And, SearchPattern.Exact), keys[0]);
            Assert.AreEqual(new SearchKey("word3", SearchConjunction.And, SearchPattern.Standard), keys[1]);

            keys = analyzer.Analyze("word1 /or word2");
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);
            Assert.AreEqual(new SearchKey("word2", SearchConjunction.Or, SearchPattern.Standard), keys[1]);

            keys = analyzer.Analyze("word1 /or /m1 word2");
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);
            Assert.AreEqual(new SearchKey("word2", SearchConjunction.Or, SearchPattern.Word), keys[1]);

            keys = analyzer.Analyze("word1 /not /re \"word2 word3\"");
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);
            Assert.AreEqual(new SearchKey("word2 word3", SearchConjunction.Not, SearchPattern.RegularExpression), keys[1]);

            keys = analyzer.Analyze("word1 /not /or /re /m1 word2  ");
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);
            Assert.AreEqual(new SearchKey("word2", SearchConjunction.Or, SearchPattern.Word), keys[1]);

            keys = analyzer.Analyze("word1 /not /or /re /m1 \"word2 word3\" ");
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual(new SearchKey("word1", SearchConjunction.And, SearchPattern.Standard), keys[0]);
            Assert.AreEqual(new SearchKey("word2 word3", SearchConjunction.Or, SearchPattern.Word), keys[1]);

            keys = analyzer.Analyze("/since 2018-01-01");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("2018-01-01", SearchConjunction.And, SearchPattern.Since), keys[0]);

            Assert.ThrowsException<SearchKeywordDateTimeException>(() => analyzer.Analyze("/since 5"));

            keys = analyzer.Analyze("/until 2018-01-01");
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual(new SearchKey("2018-01-01", SearchConjunction.And, SearchPattern.Until), keys[0]);

            Assert.ThrowsException<SearchKeywordDateTimeException>(() => analyzer.Analyze("/until 5"));
        }


        [TestMethod()]
        [ExpectedException(typeof(SearchKeywordOptionException))]
        public void SearchEngineKeywordAnalyzeOptionExceptionTest()
        {
            var analyzer = new SearchKeyAnalyzer();
            List<SearchKey> keys;

            keys = analyzer.Analyze("word1 /unknown word2");
            Assert.AreEqual(2, keys.Count);
        }

        [TestMethod()]
        [ExpectedException(typeof(SearchKeywordRegularExpressionException))]
        public void SearchEngineKeywordAnalyzeRegularExpressionExceptionTest()
        {
            var analyzer = new SearchKeyAnalyzer();
            List<SearchKey> keys;

            keys = analyzer.Analyze("word1 /re ^(hoge");
            Assert.AreEqual(2, keys.Count);

            keys = analyzer.Analyze("word1 /ire ^(hoge");
            Assert.AreEqual(2, keys.Count);
        }
    }

}