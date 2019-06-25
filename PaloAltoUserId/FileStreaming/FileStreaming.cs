using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace org.aha_net.FileStreaming {
    public class FileStreamCancellable : IDisposable {
        protected FileStream                 file = null;
        protected StreamReader               stream = null;
        protected readonly CancellationToken token;
        private Task                         taskDelayUntilCancelled = null;

        public FileStreamCancellable(string path, CancellationToken _token, int startLine = 0) {
            taskDelayUntilCancelled = Task.Delay(-1, token);

            token = _token;
            isFollowing = false;

            Open(path, startLine);
        }

        public virtual bool EndOfStream {
            get {
                if(stream == null) return true;
                else               return stream.EndOfStream || token.IsCancellationRequested;
            }
        }

        public long Length {
            get {
                if(file == null) return -1;
                else             return file.Length;
            }
        }

        public string Name {
            get {
                if(file == null) return null;
                else             return file.Name;
            }
        }

        public long Position {
            get {
                if(file == null) return -1;
                else             return file.Position;
            }
            set {
                file.Seek(value, SeekOrigin.Begin);
                stream.DiscardBufferedData();
            }
        }

        protected bool isFollowing;
        public bool IsFollowing {
            get {
                if(stream == null) return false;
                else               return isFollowing;
            }
        }

        private void Open(string path, long startLine) {
            file = TryOpenFileStream(path);
            if(file == null) return;

            stream = new StreamReader(file, true);
            InitStreamReaderPosition(stream, startLine);
        }

        protected virtual FileStream TryOpenFileStream(string path) {
            var _file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if(_file != null && Length == 0) {
                _file.Dispose();
                _file = null;
            }
            return _file;
        }

        private void InitStreamReaderPosition(StreamReader stream, long startLine) {
            if(startLine > 0) {
                stream.DiscardBufferedData();
                SkipLines(startLine);
            } else if(startLine < 0) {
                SkipLinesFromEnd(-startLine);
            } else {
                stream.DiscardBufferedData();
            }
        }

        private void SkipLines(long count) {
            while(! EndOfStream && count > 0) {
                count--;
                stream.ReadLine();
            }
        }

        private void SkipLinesFromEnd(long count) {
            if(count <= 0) throw new ArgumentException("The value of count is " + count + ". This value must be greater than zero.");

            long _Length;
            long offset = 0;
            string[] lines = null;
            var lineTerminators = new[] { "\r\n", "\n", "\r" };
            long blockSize = 60 * count;
            do {
                _Length = Length;
                offset = Math.Min(_Length, offset + blockSize);
                Position = _Length - offset;
                if(Position == 0) return;

                lines = stream.ReadToEnd().Split(lineTerminators, StringSplitOptions.None);
            } while(lines.Length-2 < count);
            Position = _Length - offset;

            long numLinesToDiscard = lines.Length - count - 1;
            do {
                ReadLine();
                numLinesToDiscard--;
            } while(numLinesToDiscard > 0);
        }

        public virtual string ReadLine() {
            if(stream == null) {
                return null;
            } else {
                using(var taskReadLine = ReadLineAsync()) {
                    taskReadLine.Wait();
                    return taskReadLine.Result;
                }
            }
        }

        public async Task<string> ReadLineAsync() {
            Task<string> taskReadLine = null;
            Task taskReadLineUnlessCancelled = null;
            try {
                taskReadLine = stream.ReadLineAsync();
                taskReadLineUnlessCancelled = Task.WhenAny(taskReadLine, taskDelayUntilCancelled);
                await taskReadLineUnlessCancelled;
                return taskReadLine.Result;
            } catch {
                return null;
            } finally {
                if(taskReadLine != null) taskReadLine.Dispose();
                if(taskReadLineUnlessCancelled != null) taskReadLineUnlessCancelled.Dispose();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                if(stream != null) {
                    stream.Dispose();
                    stream = null;
                }

                if(file != null) {
                    file.Dispose();
                    file = null;
                }

                if(taskDelayUntilCancelled != null) {
                    taskDelayUntilCancelled.Dispose();
                    taskDelayUntilCancelled = null;
                }
            }
        }

        ~FileStreamCancellable() {
            Dispose(false);
        }
    }

    public abstract class AFileStreamFollower : FileStreamCancellable, IDisposable {
        public AFileStreamFollower(string path, CancellationToken token, int startLine = 0) : base(path, token, startLine) {}

        protected override FileStream TryOpenFileStream(string path) {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        protected static void DelayUnlessCancelled(int msecs, CancellationToken token) {
            Task taskDelay = null;
            try     { taskDelay = Task.Delay(msecs, token); taskDelay.Wait(); }
            catch   {}
            finally { if(taskDelay != null) taskDelay.Dispose(); }
        }
    }

    public class FileStreamFollower : AFileStreamFollower, IDisposable {
        public FileStreamFollower(string path, CancellationToken token, int startLine = 0) : base(path, token, startLine) {}

        public override bool EndOfStream {
            get {
                if(stream == null) return true;
                else               return token.IsCancellationRequested;
            }
        }

        public override string ReadLine() {
            if(stream == null) return null;

            while(! token.IsCancellationRequested) {
                string line = base.ReadLine();
                if(line != null) return line;
                const int _100_msecs = 100;
                DelayUnlessCancelled(_100_msecs, token);
                isFollowing = true;
            }
            return null;
        }
    }

    public class FileStreamLiveFollower : AFileStreamFollower, IDisposable {
        private readonly string path;

        public FileStreamLiveFollower(string _path, CancellationToken token, int startLine = 0) : base(_path, token, startLine) {
            path = _path;
        }

        public override bool EndOfStream {
            get {
                if(stream == null) return true;
                else               return (stream.EndOfStream && ! IsWriteLocked(path)) || token.IsCancellationRequested;
            }
        }

        private static bool IsWriteLocked(string path) {
            try   { (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)).Dispose(); return false; }
            catch { return true; }
        }

        public override string ReadLine() {
            if(stream == null) return null;

            int counter = 0;
            while(! token.IsCancellationRequested) {
                string line = base.ReadLine();
                if(line != null) return line;
                const int _100_msecs = 100;
                DelayUnlessCancelled(_100_msecs, token);
                if(counter == 0 && ! IsWriteLocked(path)) return null;
                counter = (counter+1) % 600;
                isFollowing = true;
            }
            return null;
        }
    }
}
