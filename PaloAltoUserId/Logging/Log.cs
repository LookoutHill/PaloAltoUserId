using System;
using System.Collections.Generic;

namespace org.aha_net.Logging {
    public static class Log {
		private static _Log logs;

		static Log() {
			logs = new _Log();
        }

        public static string defaultId {
            get {
                return _Log.defaultId;
            }
        }

        public static void Add(ILogSink sink) {
		    logs.Add(sink.Id, sink);
        }

        public static void Set(ILogSink sink) {
		    logs[sink.Id] = sink;
        }

        public static void SetAndSelect(ILogSink sink) {
		    logs.SetAndSelect(sink);
        }

        public static string FindIdFromName(string name) {
            return logs.FindIdFromName(name);
        }

        public static ILogSink Get() {
		    return logs.Get();
        }

        public static ILogSink Get(string id) {
		    return logs.Get(id);
        }

        public static ILogSink GetByName(string name) {
		    return logs.GetByName(name);
        }

        public static void Remove(string id) {
		    logs.Remove(id);
        }

        public static void Remove(ILogSink value) {
		    logs.Remove(value);
        }

        public static void RemoveByName(string name) {
		    logs.Remove(name);
        }

        public static void Replace(string id) {
		    logs.Replace(id);
        }

        public static void Replace(ILogSink value) {
		    logs.Replace(value);
        }

        public static void ReplaceByName(string name) {
		    logs.ReplaceByName(name);
        }

        public static void Select() {
		    logs.Select();
		}

        public static void Select(string id) {
		    logs.Select(id);
		}

        public static void SelectByName(string name) {
		    logs.SelectByName(name);
		}

        public static void FlushAll() {
            logs.FlushAll();
		}

        public static void RemoveAll() {
            logs.RemoveAll();
		}

        public static int Verbosity
        {
            get
            {
                return logs.Get().Verbosity;
            }
            set
            {
                logs.Get().Verbosity = value;
            }
        }

        public static void Inform(bool value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(char value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(char[] buffer, int verbosity = 0) {
			logs.Get().Inform(buffer, verbosity);
		}

        public static void Inform(decimal value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(double value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(int value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(long value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(object value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(float value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(string value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(uint value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(ulong value, int verbosity = 0) {
			logs.Get().Inform(value, verbosity);
		}

        public static void Inform(string format, object arg0, int verbosity = 0) {
			logs.Get().Inform(format, arg0, verbosity);
		}

        public static void Inform(string format, object[] arg, int verbosity = 0) {
			logs.Get().Inform(format, arg, verbosity);
		}

        public static void Inform(char[] buffer, int index, int count, int verbosity = 0) {
			logs.Get().Inform(buffer, index, count, verbosity);
		}

        public static void Inform(string format, object arg0, object arg1, int verbosity = 0) {
			logs.Get().Inform(format, arg0, arg1, verbosity);
		}

        public static void Inform(string format, object arg0, object arg1, object arg2, int verbosity = 0) {
			logs.Get().Inform(format, arg0, arg1, arg2, verbosity);
		}

        public static void Warn(bool value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(char value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(char[] buffer, int verbosity = 0) {
			logs.Get().Warn(buffer, verbosity);
		}

        public static void Warn(decimal value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(double value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(int value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(long value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(object value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(float value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(string value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(uint value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(ulong value, int verbosity = 0) {
			logs.Get().Warn(value, verbosity);
		}

        public static void Warn(string format, object arg0, int verbosity = 0) {
			logs.Get().Warn(format, arg0, verbosity);
		}

        public static void Warn(string format, object[] arg, int verbosity = 0) {
			logs.Get().Warn(format, arg, verbosity);
		}

        public static void Warn(char[] buffer, int index, int count, int verbosity = 0) {
			logs.Get().Warn(buffer, index, count, verbosity);
		}

        public static void Warn(string format, object arg0, object arg1, int verbosity = 0) {
			logs.Get().Warn(format, arg0, arg1, verbosity);
		}

        public static void Warn(string format, object arg0, object arg1, object arg2, int verbosity = 0) {
			logs.Get().Warn(format, arg0, arg1, arg2, verbosity);
		}

        public static void Error(bool value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(char value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(char[] buffer, int verbosity = Int32.MinValue) {
			logs.Get().Error(buffer, verbosity);
		}

        public static void Error(decimal value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(double value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(int value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(long value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(object value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(float value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(string value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(uint value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(ulong value, int verbosity = Int32.MinValue) {
			logs.Get().Error(value, verbosity);
		}

        public static void Error(string format, object arg0, int verbosity = Int32.MinValue) {
			logs.Get().Error(format, arg0, verbosity);
		}

        public static void Error(string format, object[] arg, int verbosity = Int32.MinValue) {
			logs.Get().Error(format, arg, verbosity);
		}

        public static void Error(char[] buffer, int index, int count, int verbosity = Int32.MinValue) {
			logs.Get().Error(buffer, index, count, verbosity);
		}

        public static void Error(string format, object arg0, object arg1, int verbosity = Int32.MinValue) {
			logs.Get().Error(format, arg0, arg1, verbosity);
		}

        public static void Error(string format, object arg0, object arg1, object arg2, int verbosity = Int32.MinValue) {
			logs.Get().Error(format, arg0, arg1, arg2, verbosity);
		}

        public static void Fatal(Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(exp, verbosity);
		}

        public static void Fatal(bool value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(char value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(char[] buffer, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(buffer, exp, verbosity);
		}

        public static void Fatal(decimal value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(double value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(int value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(long value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(object value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(float value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(string value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(uint value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(ulong value, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(value, exp, verbosity);
		}

        public static void Fatal(string format, object arg0, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(format, arg0, exp, verbosity);
		}

        public static void Fatal(string format, object[] arg, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(format, arg, exp, verbosity);
		}

        public static void Fatal(char[] buffer, int index, int count, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(buffer, index, count, exp, verbosity);
		}

        public static void Fatal(string format, object arg0, object arg1, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(format, arg0, arg1, exp, verbosity);
		}

        public static void Fatal(string format, object arg0, object arg1, object arg2, Exception exp, int verbosity = Int32.MinValue) {
			logs.Get().Fatal(format, arg0, arg1, arg2, exp, verbosity);
		}

        public static void Abort(bool value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(char value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(char[] buffer, int verbosity = Int32.MinValue) {
			logs.Get().Abort(buffer, verbosity);
		}

        public static void Abort(decimal value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(double value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(int value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(long value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(object value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(float value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(string value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(uint value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(ulong value, int verbosity = Int32.MinValue) {
			logs.Get().Abort(value, verbosity);
		}

        public static void Abort(string format, object arg0, int verbosity = Int32.MinValue) {
			logs.Get().Abort(format, arg0, verbosity);
		}

        public static void Abort(string format, object[] arg, int verbosity = Int32.MinValue) {
			logs.Get().Abort(format, arg, verbosity);
		}

        public static void Abort(char[] buffer, int index, int count, int verbosity = Int32.MinValue) {
			logs.Get().Abort(buffer, index, count, verbosity);
		}

        public static void Abort(string format, object arg0, object arg1, int verbosity = Int32.MinValue) {
			logs.Get().Abort(format, arg0, arg1, verbosity);
		}

        public static void Abort(string format, object arg0, object arg1, object arg2, int verbosity = Int32.MinValue) {
			logs.Get().Abort(format, arg0, arg1, arg2, verbosity);
		}

        public static void Flush()
        {
            logs.Get().Flush();
        }
    }
}
