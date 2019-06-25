using System;
using System.Threading;
using System.Threading.Tasks;
using org.aha_net.FolderWatching;
using org.aha_net.RepeatedTasks;

namespace org.aha_net.PaloAltoUserId
{
    public class FolderWatcherManager : RepeatedTask
    {
        public FolderWatcherManager(string path, string pattern, FileFollowerManager manager, CancellationToken token) : this(new FolderWatcher(path, pattern), manager, token) {}

        public FolderWatcherManager(IFolderWatcher watcher, FileFollowerManager manager, CancellationToken token)
        {
            wait = () => {
                const int _5_seconds = 5000;
                Task taskDelay = null;
                try     { taskDelay = Task.Delay(_5_seconds, token); taskDelay.Wait(); return true; }
                catch   { return false; }
                finally { if(taskDelay != null) taskDelay.Dispose(); }
            };

            act = () => {
                foreach(var path in watcher.FilteredFiles) {
                    manager.Watch(path);
                    if(token.IsCancellationRequested) return false;
                }
                return true;
            };
        }
    }
}
