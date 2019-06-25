using System;
using System.IO;

namespace org.aha_net.Logging {
    public class LogConsole : AcLogSink {
		public static object Lock = new object();

        private TextWriter _logData;
        private TextWriter _logErrors;
        private ConsoleColor _fgColor;

        public LogConsole(int verbosity = -1) : this(null, verbosity) { }

        public LogConsole(string name, int verbosity = -1) : this(name, Console.Out, Console.Error, verbosity) { }

        public LogConsole(TextWriter logData, TextWriter logErrors, int verbosity = -1) : this(null, logData, logErrors, verbosity) { }

        public LogConsole(string name, TextWriter logData, TextWriter logErrors, int verbosity = -1) : base(name)
        {
            _logData = logData;
            _logErrors = logErrors;

            _fgColor = Console.ForegroundColor;

            Verbosity = verbosity;
        }

        override public void Inform(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

			lock(Lock) {
                Console.ForegroundColor = _fgColor;
                WriteLog(value, _logData);
            }
        }

        override public void Warn(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

			lock(Lock) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteLog(value, _logErrors);
                Console.ForegroundColor = _fgColor;
            }
        }

        override public void Error(string value, int verbosity = Int32.MinValue) {
            if(verbosity > Verbosity) return;

			lock(Lock) {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLog(value, _logErrors);
                Console.ForegroundColor = _fgColor;
            }
        }

        override public void Flush() {
            _logErrors.Flush();
            _logData.Flush();
            lock(Lock) { Console.ForegroundColor = _fgColor; }
        }

        override public void Dispose() {
            Flush();
        }

        private void WriteLog(string value, TextWriter log) {
            log.WriteLine(value);
        }
    }
}
