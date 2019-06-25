using System;

namespace org.aha_net.Logging {
    public interface ILogSink {
		string Id { get; }
		string Name { get; }
		int Verbosity { get; set; }

        void Inform(bool value, int verbosity = 0);

        void Inform(char value, int verbosity = 0);

        void Inform(char[] buffer, int verbosity = 0);

        void Inform(decimal value, int verbosity = 0);

        void Inform(double value, int verbosity = 0);

        void Inform(int value, int verbosity = 0);

        void Inform(long value, int verbosity = 0);

        void Inform(object value, int verbosity = 0);

        void Inform(float value, int verbosity = 0);

        void Inform(string value, int verbosity = 0);

        void Inform(uint value, int verbosity = 0);

        void Inform(ulong value, int verbosity = 0);

        void Inform(string format, object arg0, int verbosity = 0);

        void Inform(string format, object[] arg, int verbosity = 0);

        void Inform(char[] buffer, int index, int count, int verbosity = 0);

        void Inform(string format, object arg0, object arg1, int verbosity = 0);

        void Inform(string format, object arg0, object arg1, object arg2, int verbosity = 0);

        void Warn(bool value, int verbosity = 0);

        void Warn(char value, int verbosity = 0);

        void Warn(char[] buffer, int verbosity = 0);

        void Warn(decimal value, int verbosity = 0);

        void Warn(double value, int verbosity = 0);

        void Warn(int value, int verbosity = 0);

        void Warn(long value, int verbosity = 0);

        void Warn(object value, int verbosity = 0);

        void Warn(float value, int verbosity = 0);

        void Warn(string value, int verbosity = 0);

        void Warn(uint value, int verbosity = 0);

        void Warn(ulong value, int verbosity = 0);

        void Warn(string format, object arg0, int verbosity = 0);

        void Warn(string format, object[] arg, int verbosity = 0);

        void Warn(char[] buffer, int index, int count, int verbosity = 0);

        void Warn(string format, object arg0, object arg1, int verbosity = 0);

        void Warn(string format, object arg0, object arg1, object arg2, int verbosity = 0);

        void Error(bool value, int verbosity = Int32.MinValue);

        void Error(char value, int verbosity = Int32.MinValue);

        void Error(char[] buffer, int verbosity = Int32.MinValue);

        void Error(decimal value, int verbosity = Int32.MinValue);

        void Error(double value, int verbosity = Int32.MinValue);

        void Error(int value, int verbosity = Int32.MinValue);

        void Error(long value, int verbosity = Int32.MinValue);

        void Error(object value, int verbosity = Int32.MinValue);

        void Error(float value, int verbosity = Int32.MinValue);

        void Error(string value, int verbosity = Int32.MinValue);

        void Error(uint value, int verbosity = Int32.MinValue);

        void Error(ulong value, int verbosity = Int32.MinValue);

        void Error(string format, object arg0, int verbosity = Int32.MinValue);

        void Error(string format, object[] arg, int verbosity = Int32.MinValue);

        void Error(char[] buffer, int index, int count, int verbosity = Int32.MinValue);

        void Error(string format, object arg0, object arg1, int verbosity = Int32.MinValue);

        void Error(string format, object arg0, object arg1, object arg2, int verbosity = Int32.MinValue);

        void Fatal(Exception exp, int verbosity = Int32.MinValue);

        void Fatal(bool value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(char value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(char[] buffer, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(decimal value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(double value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(int value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(long value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(object value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(float value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(string value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(uint value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(ulong value, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(string format, object arg0, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(string format, object[] arg, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(char[] buffer, int index, int count, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(string format, object arg0, object arg1, Exception exp, int verbosity = Int32.MinValue);

        void Fatal(string format, object arg0, object arg1, object arg2, Exception exp, int verbosity = Int32.MinValue);

        void Abort(bool value, int verbosity = Int32.MinValue);

        void Abort(char value, int verbosity = Int32.MinValue);

        void Abort(char[] buffer, int verbosity = Int32.MinValue);

        void Abort(decimal value, int verbosity = Int32.MinValue);

        void Abort(double value, int verbosity = Int32.MinValue);

        void Abort(int value, int verbosity = Int32.MinValue);

        void Abort(long value, int verbosity = Int32.MinValue);

        void Abort(object value, int verbosity = Int32.MinValue);

        void Abort(float value, int verbosity = Int32.MinValue);

        void Abort(string value, int verbosity = Int32.MinValue);

        void Abort(uint value, int verbosity = Int32.MinValue);

        void Abort(ulong value, int verbosity = Int32.MinValue);

        void Abort(string format, object arg0, int verbosity = Int32.MinValue);

        void Abort(string format, object[] arg, int verbosity = Int32.MinValue);

        void Abort(char[] buffer, int index, int count, int verbosity = Int32.MinValue);

        void Abort(string format, object arg0, object arg1, int verbosity = Int32.MinValue);

        void Abort(string format, object arg0, object arg1, object arg2, int verbosity = Int32.MinValue);

        void Flush();

        void Dispose();
    }
}
