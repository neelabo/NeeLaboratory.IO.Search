using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NeeLaboratory.IO.Search.Utility
{
    /// <summary>
    /// ロギング
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// TraceSource
        /// </summary>
        private readonly TraceSource _traceSource;
        public TraceSource TraceSource => _traceSource;

        /// <summary>
        /// constructor
        /// </summary>
        public  Logger(string name)
        {
            _traceSource = new TraceSource(name, SourceLevels.Error);
        }

        /// <summary>
        /// ログ出力レベル設定
        /// </summary>
        /// <param name="level"></param>
        public void SetLevel(SourceLevels level)
        {
            var sourceSwitch = new SourceSwitch($"{_traceSource.Name}.{level}")
            {
                Level = level
            };

            _traceSource.Switch = sourceSwitch;
        }

        /// <summary>
        /// TraceEvent
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void TraceEvent(TraceEventType type, int id, string message)
        {
            _traceSource.TraceEvent(type, id, message);
            _traceSource.Flush();
        }

        /// <summary>
        /// Trace
        /// </summary>
        /// <param name="message"></param>
        public void Trace(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
            _traceSource.Flush();
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message"></param>
        public void Warning(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Warning, 0, message);
            _traceSource.Flush();
        }
    }
}
