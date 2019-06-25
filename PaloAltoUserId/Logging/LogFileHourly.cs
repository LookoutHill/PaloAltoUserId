﻿using System;
using System.IO;
using System.Text;
using org.aha_net.AccurateTiming;
using org.aha_net.Logging.File;

namespace org.aha_net.Logging {
    public class LogFileHourly : AcLogSink {
        private static readonly StreamCache logFiles = new StreamCache();
        private FileStream file;
        private string pathFormat;
        private readonly AccurateTimer timer;

        public LogFileHourly(string pathFormat, bool autoFlush = false, int verbosity = -1) : this(null, pathFormat, autoFlush, verbosity) {}

        public LogFileHourly(string name, string _pathFormat, bool autoFlush = false, int verbosity = -1) {
            pathFormat = _pathFormat;
            AutoFlush = autoFlush;

            ChangeLogFile();

            Verbosity = verbosity;

            var dueTime = DateTime.Parse(DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:00:00"));
            var interval = TimeSpan.FromHours(1);
            timer = new AccurateTimer(ChangeLogFile, dueTime, interval);
        }

        public bool AutoFlush { get; set; }

        public void ChangeLogFile() {
            lock(this) {
                if(file != null) logFiles.Close(file);
                file = logFiles.Open(LogPath(pathFormat), true);
            }
        }

        public string LogPath(string pathFormat) {
            return string.Format(pathFormat, DateTime.Now.ToString("yyyyMMddHH"));
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
