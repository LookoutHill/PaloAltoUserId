using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace org.aha_net.Logging {
    public class LogSplitter : AcLogSink {
        public readonly ILogSink Sink1;
        public readonly ILogSink Sink2;

        public LogSplitter(ILogSink sink1, ILogSink sink2, int verbosity = -1) : this(null, sink1, sink2, verbosity) {}

        public LogSplitter(string name, ILogSink sink1, ILogSink sink2, int verbosity = -1) : base(name) {
            Sink1 = sink1;
            Sink2 = sink2;

			Verbosity = verbosity;
        }

        override public int Verbosity {
            set {
                Sink1.Verbosity = value;
                Sink2.Verbosity = value;
                base.Verbosity = value;
            }
        }

        override public void Inform(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

			Sink1.Inform(value);
			Sink2.Inform(value);
        }

        override public void Warn(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

			Sink1.Warn(value);
			Sink2.Warn(value);
        }

        override public void Error(string value, int verbosity = Int32.MinValue) {
            if(verbosity > Verbosity) return;

			Sink1.Error(value);
			Sink2.Error(value);
        }

        override public void Fatal(Exception exp, int verbosity = Int32.MinValue) {
			Flush();
			Sink1.Fatal("", exp, verbosity);
			Sink2.Fatal("", exp, verbosity);
        }

        override public void Fatal(string value, Exception exp, int verbosity = Int32.MinValue) {
			Flush();
			Sink1.Fatal(value, exp, verbosity);
			Sink2.Fatal(value, exp, verbosity);
        }

        override public void Flush() {
			Sink1.Flush();
			Sink2.Flush();
		}

        override public void Dispose() {
            Flush();
            Sink1.Dispose();
            Sink2.Dispose();
        }
    }
}
