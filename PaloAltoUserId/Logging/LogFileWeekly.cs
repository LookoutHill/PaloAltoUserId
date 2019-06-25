using System;
using System.IO;
using System.Text;
using org.aha_net.AccurateTiming;
using org.aha_net.Logging.File;

namespace org.aha_net.Logging {
    public class LogFileWeekly : AcLogSink {
        private static readonly StreamCache logFiles = new StreamCache();
        private FileStream file;
        private string pathFormat;
        private readonly AccurateTimer timer;

        public LogFileWeekly(string pathFormat, bool autoFlush = false, int verbosity = -1) : this(null, pathFormat, autoFlush, verbosity) {}

        public LogFileWeekly(string name, string _pathFormat, bool autoFlush = false, int verbosity = -1) {
            pathFormat = _pathFormat;
            AutoFlush = autoFlush;

            ChangeLogFile();

            Verbosity = verbosity;

            var dueTime = DateTime.Now.AddDays(1).Date;
            var interval = TimeSpan.FromDays(1);
            timer = new AccurateTimer(ChangeLogFile, dueTime, interval);
        }

        public bool AutoFlush { get; set; }

        public void ChangeLogFile() {
            lock(this) {
                if(file != null) logFiles.Close(file);
                var path = LogPath(pathFormat);
                file = logFiles.Open(path, WillAppend(path));
            }
        }

        private string LogPath(string pathFormat) {
            return string.Format(pathFormat, DateTime.Now.DayOfWeek.ToString().Substring(0, 3));
        }

        private bool WillAppend(string path) {
            var info = new FileInfo(path);
            return info.LastWriteTime.Date == DateTime.Today;
        }

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
            if(file != null) logFiles.Close(file);
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
