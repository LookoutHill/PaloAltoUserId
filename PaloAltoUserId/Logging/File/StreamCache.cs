using System;
using System.Collections.Generic;
using System.IO;

namespace org.aha_net.Logging.File {
    public class StreamEntry {
        public StreamEntry(string path, bool append) {
            if(append) {
                stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            } else {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            }
        }

        public FileStream stream;
        public int count = 1;
    }

    public class StreamCache : Dictionary<string, StreamEntry> {
        new public StreamEntry this[string path] {
            get {
                return base[path.ToLower()];
            }
            set {
                base[path.ToLower()] = value;
            }
        }

        new public void Add(string path, StreamEntry entry) {
            base.Add(path.ToLower(), entry);
        }

        public void Close(FileStream stream) {
            lock(this) {
                stream.Flush(true);
                string path = stream.Name;
                StreamEntry entry = this[path];
                entry.count--;
                if(entry.count == 0) {
                    Remove(path);
                    entry.stream.Dispose();
                    entry.stream = null;
                }
            }
        }

        new public bool ContainsKey(string path) {
            return base.ContainsKey(path.ToLower());
        }

        public FileStream Open(string path, bool append = true) {
            StreamEntry entry = null;
            lock(this) {
                path = Path.GetFullPath(path);
                if(ContainsKey(path)) {
                    entry = this[path];
                    entry.count++;
                } else {
                    entry = new StreamEntry(path, append);
                    this[path] = entry;
                }
            }
            return entry.stream;
        }

        new public void Remove(string path) {
            base.Remove(path.ToLower());
        }
    }
}
