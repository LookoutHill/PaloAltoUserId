using System;
using System.IO;
using System.Text;
using org.aha_net.Logging.File;

namespace org.aha_net.Logging {
    public class LogFile : AcLogSink {
        public LogFile(string path, bool append = true, bool autoFlush = false, int verbosity = -1) : this(null, path, append, autoFlush, verbosity) {}

        public LogFile(string name, string path, bool append = true, bool autoFlush = false, int verbosity = -1) {
            AutoFlush = autoFlush;

            file = logs.Open(path, append);

            Verbosity = verbosity;
        }

/*
        public LogFile(string name, string path, bool append = true, int verbosity = -1) : this(name, logs.Open(path, append), verbosity) {}

        public LogFile(FileStream stream, int verbosity = -1) : this(null, stream, verbosity) { }

        public LogFile(string name, FileStream stream, int verbosity = -1) : base(name) {
            log = StreamWriter.Synchronized(new StreamWriter(stream));

            Verbosity = verbosity;
        }
*/

        private static StreamCache logs = new StreamCache();
        private FileStream file;

        public bool AutoFlush { get; set; }

        override public void Inform(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

            WriteLog(value);
        }

        override public void Warn(string value, int verbosity = 0) {
            if(verbosity > Verbosity) return;

            WriteLog(value);
        }

        override public void Error(string value, int verbosity = Int32.MinValue) {
            if(verbosity > Verbosity) return;

            WriteLog(value);
        }

        override public void Flush() {
            lock(this) {
                file.Flush(true);
            }
        }

        override public void Dispose() {
            if(file != null) logs.Close(file);
            file = null;
        }

        private void WriteLog(string value) {
            lock(this) {
                byte[] bytes = Encoding.ASCII.GetBytes(value + Environment.NewLine);
                file.Write(bytes, 0, bytes.Length);
                if(AutoFlush) Flush();
            }
        }
    }
}
