using System;

namespace org.aha_net.Logging {
    public abstract class AcLogSink : ILogSink {
        protected AcLogSink() {
            Id = Guid.NewGuid().ToString();
            Name = Id;
        }

        protected AcLogSink(string name) {
            Id = Guid.NewGuid().ToString();
            if(name == null) name = Id;
            Name = name;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public virtual int Verbosity { get; set; }

        public void Inform(bool value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(char value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(char[] buffer, int verbosity = 0) {
			Inform(buffer, 0, buffer.Length, verbosity);
		}

        public void Inform(decimal value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(double value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(int value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(long value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(object value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(float value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public abstract void Inform(string value, int verbosity = 0);

        public void Inform(uint value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(ulong value, int verbosity = 0) {
			Inform("" + value, verbosity);
		}

        public void Inform(string format, object arg0, int verbosity = 0) {
			Inform(string.Format(format, arg0), verbosity);
		}

        public void Inform(string format, object[] arg, int verbosity = 0) {
			Inform(string.Format(format, arg), verbosity);
		}

        public void Inform(char[] buffer, int index, int count, int verbosity = 0) {
			Inform(new string(buffer, index, count), verbosity);
		}

        public void Inform(string format, object arg0, object arg1, int verbosity = 0) {
			Inform(string.Format(format, arg0, arg1), verbosity);
		}

        public void Inform(string format, object arg0, object arg1, object arg2, int verbosity = 0) {
			Inform(string.Format(format, arg0, arg1, arg2), verbosity);
		}

        public void Warn(bool value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(char value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(char[] buffer, int verbosity = 0) {
			Warn(buffer, 0, buffer.Length, verbosity);
		}

        public void Warn(decimal value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(double value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(int value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(long value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(object value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(float value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public abstract void Warn(string value, int verbosity = 0);

        public void Warn(uint value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(ulong value, int verbosity = 0) {
			Warn("" + value, verbosity);
		}

        public void Warn(string format, object arg0, int verbosity = 0) {
			Warn(string.Format(format, arg0), verbosity);
		}

        public void Warn(string format, object[] arg, int verbosity = 0) {
			Warn(string.Format(format, arg), verbosity);
		}

        public void Warn(char[] buffer, int index, int count, int verbosity = 0) {
			Warn(new string(buffer, index, count), verbosity);
		}

        public void Warn(string format, object arg0, object arg1, int verbosity = 0) {
			Warn(string.Format(format, arg0, arg1), verbosity);
		}

        public void Warn(string format, object arg0, object arg1, object arg2, int verbosity = 0) {
			Warn(string.Format(format, arg0, arg1, arg2), verbosity);
		}

        public void Error(bool value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(char value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(char[] buffer, int verbosity = Int32.MinValue) {
			Error(buffer, 0, buffer.Length, verbosity);
		}

        public void Error(decimal value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(double value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(int value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(long value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(object value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(float value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public abstract void Error(string value, int verbosity = Int32.MinValue);

        public void Error(uint value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(ulong value, int verbosity = Int32.MinValue) {
			Error("" + value, verbosity);
		}

        public void Error(string format, object arg0, int verbosity = Int32.MinValue) {
			Error(string.Format(format, arg0), verbosity);
		}

        public void Error(string format, object[] arg, int verbosity = Int32.MinValue) {
			Error(string.Format(format, arg), verbosity);
		}

        public void Error(char[] buffer, int index, int count, int verbosity = Int32.MinValue) {
			Error(new string(buffer, index, count), verbosity);
		}

        public void Error(string format, object arg0, object arg1, int verbosity = Int32.MinValue) {
			Error(string.Format(format, arg0, arg1), verbosity);
		}

        public void Error(string format, object arg0, object arg1, object arg2, int verbosity = Int32.MinValue) {
			Error(string.Format(format, arg0, arg1, arg2), verbosity);
		}

        public virtual void Fatal(Exception exp, int verbosity = Int32.MinValue)
        {
            Error(exp, verbosity);
            Flush();
            throw exp;
        }

        public void Fatal(bool value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(char value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(char[] buffer, Exception exp, int verbosity = Int32.MinValue) {
			Fatal(buffer, 0, buffer.Length, exp, verbosity);
		}

        public void Fatal(decimal value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(double value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(int value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(long value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(object value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(float value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public virtual void Fatal(string value, Exception exp, int verbosity = Int32.MinValue)
        {
            Error(value, verbosity);
            Flush();
            throw exp;
        }

        public void Fatal(uint value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(ulong value, Exception exp, int verbosity = Int32.MinValue) {
			Fatal("" + value, exp, verbosity);
		}

        public void Fatal(string format, object arg0, Exception exp, int verbosity = Int32.MinValue) {
			Fatal(string.Format(format, arg0), exp, verbosity);
		}

        public void Fatal(string format, object[] arg, Exception exp, int verbosity = Int32.MinValue) {
			Fatal(string.Format(format, arg), exp, verbosity);
		}

        public void Fatal(char[] buffer, int index, int count, Exception exp, int verbosity = Int32.MinValue) {
			Fatal(new string(buffer, index, count), exp, verbosity);
		}

        public void Fatal(string format, object arg0, object arg1, Exception exp, int verbosity = Int32.MinValue) {
			Fatal(string.Format(format, arg0, arg1), exp, verbosity);
		}

        public void Fatal(string format, object arg0, object arg1, object arg2, Exception exp, int verbosity = Int32.MinValue) {
			Fatal(string.Format(format, arg0, arg1, arg2), exp, verbosity);
		}

        public void Abort(bool value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(char value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(char[] buffer, int verbosity = Int32.MinValue) {
			Abort(buffer, 0, buffer.Length, verbosity);
		}

        public void Abort(decimal value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(double value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(int value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(long value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(object value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(float value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public virtual void Abort(string value, int verbosity = Int32.MinValue)
        {
            Warn(value, verbosity);
            Flush();
            Environment.Exit(1);
        }

        public void Abort(uint value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(ulong value, int verbosity = Int32.MinValue) {
			Abort("" + value, verbosity);
		}

        public void Abort(string format, object arg0, int verbosity = Int32.MinValue) {
			Abort(string.Format(format, arg0), verbosity);
		}

        public void Abort(string format, object[] arg, int verbosity = Int32.MinValue) {
			Abort(string.Format(format, arg), verbosity);
		}

        public void Abort(char[] buffer, int index, int count, int verbosity = Int32.MinValue) {
			Abort(new string(buffer, index, count), verbosity);
		}

        public void Abort(string format, object arg0, object arg1, int verbosity = Int32.MinValue) {
			Abort(string.Format(format, arg0, arg1), verbosity);
		}

        public void Abort(string format, object arg0, object arg1, object arg2, int verbosity = Int32.MinValue) {
			Abort(string.Format(format, arg0, arg1, arg2), verbosity);
		}

        public abstract void Flush();

        public abstract void Dispose();
    }
}
