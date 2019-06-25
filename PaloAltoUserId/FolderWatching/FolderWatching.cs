using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using org.aha_net.PaloAltoUserId;
using org.aha_net.FileStreaming;
using org.aha_net.Logging;
using org.aha_net.DSV;

namespace org.aha_net.FolderWatching {
    public abstract class IFolderWatcher {
        public abstract string[] Files { get; }
        public abstract string[] FilteredFiles { get; }
    }

    public class FolderWatcher : IFolderWatcher {
        private readonly string path = null;
        private readonly string pattern = null;
        private readonly FileFilter filter;

        public FolderWatcher(string _path, string _pattern, FileFilter _filter = null) {
            path = _path;
            pattern = _pattern;
            if(_filter != null) filter = _filter;
            else                filter = new FileFilterAll();
        }

        public override string[] Files {
            get {
                return Directory.GetFiles(path, pattern);
            }
        }

        public override string[] FilteredFiles {
            get {
                return filter.Select(Files);
            }
        }
    }

    public class FolderWatchers : IFolderWatcher {
        private readonly List<IFolderWatcher> watchers;
        private readonly FileFilter filter;

        public FolderWatchers(string path, string pattern, FileFilter filter = null) : this(new FolderWatcher(path, pattern), filter) {}

        public FolderWatchers(FolderWatcher watcher, FileFilter filter = null) : this(new FolderWatcher[] {watcher}, filter) {}

        public FolderWatchers(FolderWatcher[] _watchers, FileFilter filter = null) : this(filter) {
            foreach(var watcher in _watchers) {
                watchers.Add(watcher);
            }
        }

        public FolderWatchers(FileFilter _filter = null) {
            watchers = new List<IFolderWatcher>();

            if(_filter != null) filter = _filter;
            else                filter = new FileFilterAll();
        }

        public void AddWatcher(string path, string pattern) {
            watchers.Add(new FolderWatcher(path, pattern));
        }

        public void AddWatcher(IFolderWatcher watcher) {
            watchers.Add(watcher);
        }

        public override string[] Files {
            get {
                List<string> files = new List<string>();
                foreach(var watcher in watchers) {
                    foreach(var path in watcher.Files) {
                        files.Add(path);
                    }
                }
                return files.ToArray();
            }
        }

        public override string[] FilteredFiles {
            get {
                return filter.Select(Files);
            }
        }
    }

    public abstract class FileFilter {
        protected readonly FileFilter next;

        public FileFilter(FileFilter _next = null) {
            next = _next;
        }

        public virtual string[] Select(string[] inList) {
            List<string> outList = new List<string>();
            foreach(var path in inList) {
                if(Test(path)) outList.Add(path);
            }
            if(next != null) return next.Select(outList.ToArray());
            else             return outList.ToArray();
        }

        protected abstract bool Test(string path);
    }

    public class FileFilterAll : FileFilter {
        protected override bool Test(string path) {
            return true;
        }
    }

    public class FileFilterWriteLocked : FileFilter {
        protected override bool Test(string path) {
            return IsFileWriteLocked(path);
        }

        private static bool IsFileWriteLocked(string path) {
            try   { (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)).Dispose(); return false; }
            catch { return true; }
        }
    }

    public class FileFilterRecent : FileFilter {
        private readonly TimeSpan interval;

        private static readonly TimeSpan _1_day_and_10_seconds = new TimeSpan(1, 0, 0, 10);
        public FileFilterRecent(FileFilter next = null) : this(_1_day_and_10_seconds, next) {}

        public FileFilterRecent(TimeSpan _interval, FileFilter next = null) : base(next) {
            interval = _interval;
        }

        protected override bool Test(string path) {
            FileInfo info = new FileInfo(path);
            if(info.LastWriteTime >= (DateTime.Today - interval)) {
                return true;
            } else {
                return false;
            }
        }
    }

    public class FileFilterChanged : FileFilter {
        private class FileTrackingInfo {
            public FileTrackingInfo(DateTime lastWriteTime, long length) {
                LastWriteTime = lastWriteTime;
                Length = length;
            }

            public DateTime LastWriteTime { get; private set; }
            public long Length { get; private set; }
        }

        private Dictionary<string, FileTrackingInfo> tracking;
        private Dictionary<string, FileTrackingInfo> _tracking;

        public FileFilterChanged(FileFilter next = null) : base(next) {
            tracking = new Dictionary<string, FileTrackingInfo>();
            _tracking = new Dictionary<string, FileTrackingInfo>();
        }

        private void Reset() {
            tracking = _tracking;
            _tracking = new Dictionary<string, FileTrackingInfo>();
        }

        public override string[] Select(string[] inList) {
            string[] outList = base.Select(inList);
            Reset();
            return outList;
        }

        protected override bool Test(string path) {
            using(var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                var info = new FileInfo(path);

                _tracking.Add(path, new FileTrackingInfo(info.LastWriteTime, file.Length));

                if(tracking.ContainsKey(path)) return info.LastWriteTime > tracking[path].LastWriteTime || file.Length != tracking[path].Length;
                else                           return true;
            }
        }
    }

    public class FileFollower {
        private readonly FileStreamCancellable stream;
        private readonly DsvLineHandler handler;

        public FileFollower(FileStreamCancellable _stream, DsvLineHandler _handler) {
            stream = _stream;
            handler = _handler;
        }

        public bool IsFollowing {
            get {
                return stream.IsFollowing;
            }
        }

        public void Process() {
            while(! stream.EndOfStream) {
                var line = stream.ReadLine();
                if(line != null) {
                    handler.Process(line);
                } else {
                    Log.Inform(">>>>> NULL [" + stream.Name + "] <<<<<");
                }
            }
        }
    }

    public class FileFollowerManager {
        private readonly Dictionary<string, FileFollower> followers;
        private readonly DsvLineHandler handler;
        private readonly CancellationToken token;

        public FileFollowerManager(DsvLineHandler _handler, CancellationToken _token) {
            followers = new Dictionary<string, FileFollower>();
            handler = _handler;
            token = _token;
        }

        public bool AreAllFollowing {
            get {
                if(followers.Count == 0) return false;
            	lock(followers) {
                	foreach(var follower in followers.Values) {
                    	if(! follower.IsFollowing) return false;
                	}
                }
                return true;
            }
        }

        public bool IsWatching(string path) {
            return followers.ContainsKey(path);
        }

        public void Watch(string path) {
            FileFollower follower = null;
            lock(followers) {
                if(! IsWatching(path)) {
                    Log.Inform(string.Format(">>>>>  START   |  |  |     |  |                      |  |          |  |" + path + "|"));
                    follower = new FileFollower(new FileStreamLiveFollower(path, token), handler);
                    followers.Add(path, follower);
                }
            }
            if(follower != null) {
                (new Thread(() => Process(follower, path))).Start();
            }
        }

        private void Process(FileFollower follower, string path) {
            follower.Process();
            lock(followers) followers.Remove(path);
            Log.Inform(string.Format("<<<<<  DONE    |XX|  |     |  |                      |  |          |  |" + path + "|"));
        }
    }
}
