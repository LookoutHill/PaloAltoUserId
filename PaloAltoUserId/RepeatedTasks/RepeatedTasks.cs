using System;
using System.Threading;
using System.Threading.Tasks;

namespace org.aha_net.RepeatedTasks {
    public class RepeatedTask {
        protected Func<bool> act;
        protected Func<bool> wait;

        public RepeatedTask(Func<bool> _act, Func<bool> _wait) {
            act = _act;
            wait = _wait;
        }

        protected RepeatedTask() {}

        public async Task ProcessAsync() {
            Task taskProcess = null;
            try     { taskProcess = Task.Run(() => { Process(); }); await taskProcess; }
            catch   {}
            finally { if(taskProcess != null) taskProcess.Dispose(); }
        }

        public virtual void Process() {
            do { if(! act()) break; } while(wait());
        }
    }
}
