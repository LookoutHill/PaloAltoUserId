using System;
using System.Diagnostics;

namespace org.aha_net.Logging {
    public class LogEventLog : AcLogSink {
        private EventLog log;
        private readonly int maxEntryLength = 31389;

        public LogEventLog(int verbosity = -1) : this(null, verbosity) {}

        public LogEventLog(string name, int verbosity = -1) : base(name) {
            log = new EventLog("Application");
            log.Source = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            if (! EventLog.SourceExists(log.Source))
                EventLog.CreateEventSource(log.Source, "Application");

			Verbosity = verbosity;
        }

        override public void Inform(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

            WriteLog(value, EventLogEntryType.Information);
        }

        override public void Warn(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

            WriteLog(value, EventLogEntryType.Warning);
        }

        override public void Error(string value, int verbosity = Int32.MinValue)
        {
            if (verbosity > Verbosity) return;

            WriteLog(value, EventLogEntryType.Error);
        }

        override public void Flush() {}

        override public void Dispose() {
            if(log == null) return;
            log.Dispose();
            log = null;
        }

        private void WriteLog(string value, EventLogEntryType type) {
			int start = 0;
			int end = (maxEntryLength > value.Length)? value.Length : maxEntryLength;
			while(start < value.Length) {
            	log.WriteEntry(value.Substring(start, end - start), type);
				start += end;
				end = (end + maxEntryLength > value.Length)? value.Length : end + maxEntryLength;
			}
        }
    }
}
