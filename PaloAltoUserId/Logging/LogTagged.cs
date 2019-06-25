using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace org.aha_net.Logging {
    public class LogTagged : AcLogSink {
        public readonly ILogSink Sink;

        public bool TagWithDateTime { get; set; }
        public bool TagWithMarker { get; set; }
        public bool TagWithMethod { get; set; }
        public bool TagWithThread { get; set; }

        public LogTagged(ILogSink sink, int verbosity = -1) : this(null, sink, verbosity) {}

        public LogTagged(string name, ILogSink sink, int verbosity = -1) : base(name) {
            Sink = sink;

            TagWithDateTime = false;
            TagWithMarker = false;
            TagWithMethod = false;
            TagWithThread = false;

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

			Sink.Inform(ApplyTags(value));
        }

        override public void Warn(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

			Sink.Warn(ApplyTags(value));
        }

        override public void Error(string value, int verbosity = Int32.MinValue) {
            if(verbosity > Verbosity) return;

			Sink.Error(ApplyTags(value));
        }

        override public void Fatal(Exception exp, int verbosity = Int32.MinValue) {
			Flush();
            Sink.Fatal(ApplyTags(""), exp, verbosity);
        }

        override public void Fatal(string value, Exception exp, int verbosity = Int32.MinValue) {
			Flush();
            Sink.Fatal(ApplyTags(value), exp, verbosity);
        }

        override public void Flush() {
			Sink.Flush();
		}

        override public void Dispose() {
            Flush();
            Sink.Dispose();
        }

        private string ApplyTags(string msg) {
            if (TagWithDateTime) msg = ApplyDateTimeTag(msg);
            if (TagWithThread) msg = ApplyThreadTag(msg);
            if (TagWithMethod) msg = ApplyMethodTag(msg);
            if (TagWithMarker) msg = ApplyMarker(msg);
            return msg;
        }

        private string ApplyDateTimeTag(string msg) {
            return DateTime.Now.ToString("s") + " " + msg;
        }

        private string ApplyMarker(string msg) {
            return ">>>>> " + msg;
        }

        private string ApplyMethodTag(string msg) {
            StackTrace trace = new StackTrace();
            for (int index = 0; index < trace.FrameCount; index++)
            {
                MethodBase method = trace.GetFrame(index).GetMethod();
                Type type = method.ReflectedType;
                if (type.Equals(typeof(LogTagged))) continue;

                return String.Format("{0} <m:{1}.{2}>", msg, type.Name, method.Name);
            }

            return String.Format("{0} <m:UNKNOWN>", msg);
        }

        private string ApplyThreadTag(string msg) {
            string threadName = Thread.CurrentThread.Name;
            if (threadName == null) threadName = Thread.CurrentThread.ManagedThreadId.ToString();
            return String.Format("{0} <t:{1}>", msg, threadName);
        }
    }
}
