using System;
using System.Threading;
using System.Threading.Tasks;
using org.aha_net.Logging;

namespace org.aha_net.AccurateTiming {
    public class AccurateTimer : IDisposable {
        private readonly Action                  _callback;
        private          DateTime                _dueTime;
        private readonly TimeSpan                _period;
        private readonly CancellationTokenSource _cts;

        public AccurateTimer(Action callback, DateTime dueTime, TimeSpan period) {
            _callback = callback;
            _dueTime  = dueTime;
            _period   = period;
            _cts      = new CancellationTokenSource();
            (new Thread(new ThreadStart(Run))).Start();
        }

        public void Run() {
			try { // MATT
            while(! _cts.IsCancellationRequested) {
                while(_untilDueTime > TimeSpan.Zero) {
            		Log.Inform("AccurateTimer waiting for: " + _untilDueTime); // MATT
                    Task taskDelay = null;
                    try                 { taskDelay = Task.Delay(_untilDueTime, _cts.Token); taskDelay.Wait(); }
                    catch(Exception ex) { Log.Error("Handled Exception in AccurateTimer: " + ex.ToString()); if(_cts.IsCancellationRequested) return; } // MATT
                    finally             { if(taskDelay != null) taskDelay.Dispose(); }
                }
            	Log.Inform("AccurateTimer is running its callback at: " + _dueTime); // MATT
                (new Thread(new ThreadStart(_callback))).Start();
                _dueTime += _period;
            }
            	Log.Warn("AccurateTimer has been disposed!"); // MATT
			} // MATT
			catch(Exception ex) { // MATT
                Log.Error("Unhandled Exception in AccurateTimer: " + ex.ToString()); // MATT
			} // MATT
        }

        private TimeSpan _untilDueTime {
            get {
                return _dueTime - DateTime.Now;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if(disposing) _cts.Cancel();
        }

        ~AccurateTimer() {
            Dispose(false);
        }
    }
}
