using System;
using System.Collections.Generic;

namespace org.aha_net.Logging {
    public class LogBuffered : AcLogSink {
        public readonly ILogSink Sink;

        private List<LogBufferedEntry> buffer = new List<LogBufferedEntry>();

        private readonly object mutex = new object();

        public LogBuffered(ILogSink sink, int verbosity = -1) : this(null, sink, verbosity) {}

        public LogBuffered(string name, ILogSink sink, int verbosity = -1) : base(name) {
            Sink = sink;

            Verbosity = verbosity;
        }

        override public int Verbosity {
            set {
                Sink.Verbosity = value;
                base.Verbosity = value;
            }
        }

        override public void Inform(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

            lock(mutex) buffer.Add(new LogBufferedInform(value, Sink));
        }

        override public void Warn(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

            lock(mutex) buffer.Add(new LogBufferedWarn(value, Sink));
        }

        override public void Error(string value, int verbosity = Int32.MinValue) {
            if(verbosity > Verbosity) return;

            lock(mutex) buffer.Add(new LogBufferedError(value, Sink));
        }

        override public void Fatal(Exception exp, int verbosity = Int32.MinValue) {
            Flush();
            Sink.Fatal(exp, verbosity);
        }

        override public void Fatal(string value, Exception exp, int verbosity = Int32.MinValue) {
            Flush();
            Sink.Fatal(value, exp, verbosity);
        }

        override public void Flush() {
            lock(mutex) {
                foreach(var entry in buffer) {
                    entry.Flush();
                }
                buffer = new List<LogBufferedEntry>();
            }

            Sink.Flush();
        }

        override public void Dispose() {
            Flush();
            Sink.Dispose();
        }
    }

    interface LogBufferedEntry {
        void Flush();
    }

    class LogBufferedInform : LogBufferedEntry {
        private readonly ILogSink _sink;
        private readonly string _entry;

        public LogBufferedInform(string entry, ILogSink sink) {
            _sink = sink;
            _entry = entry;
        }

        public void Flush() {
            _sink.Inform(_entry);
        }
    }

    class LogBufferedWarn : LogBufferedEntry {
        private readonly ILogSink _sink;
        private readonly string _entry;

        public LogBufferedWarn(string entry, ILogSink sink) {
            _sink = sink;
            _entry = entry;
        }

        public void Flush() {
            _sink.Warn(_entry);
        }
    }

    class LogBufferedError : LogBufferedEntry {
        private readonly ILogSink _sink;
        private readonly string _entry;

        public LogBufferedError(string entry, ILogSink sink) {
            _sink = sink;
            _entry = entry;
        }

        public void Flush() {
            _sink.Error(_entry);
        }
    }
}
