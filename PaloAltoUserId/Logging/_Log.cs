using System;
using System.Collections.Generic;

namespace org.aha_net.Logging {
    public class _Log : Dictionary<string, ILogSink> {
        static _Log() {
		    sink = new LogConsole();
		    defaultId = sink.Id;
        }

        public _Log() {
		    base[defaultId] = sink;
        }

		private static ILogSink sink;
		public static readonly string defaultId;

        new public ILogSink this[string id] {
            set {
                lock(this) {
                    ILogSink _sink; TryGetValue(id, out _sink);
			        if(_sink != null && _sink == value) return;
			        base[id] = value;
			        if(_sink != null) {
                        if(_sink == sink) sink = value;
			            _sink.Dispose();
                    }
                }
		    }
		}

        new public void Add(string id, ILogSink sink) {
		    lock(this) base.Add(id, sink);
        }

        public string FindIdFromName(string name) {
            if(name == null) return defaultId;

            string id = null;
            lock(this) {
                foreach (var key in Keys) {
                    if(name.Equals(base[key].Name)) {
                        id = key;
                        break;
                    }
                }
            }
            return id;
		}

        public void FlushAll() {
            lock(this) {
                foreach (var id in Keys) {
                    base[id].Flush();
                }
            }
		}

        public ILogSink Get() {
		    return sink;
        }

        public ILogSink Get(string id) {
            if(id == null) return base[defaultId];
		    return base[id];
        }

        public ILogSink GetByName(string name) {
            if(name == null) return base[defaultId];
            lock(this) {
		        return base[FindIdFromName(name)];
            }
        }

        new public void Remove(string id) {
            if(id.Equals(defaultId)) return;

            lock(this) {
                var _sink = base[id];
                if(_sink == sink) Select();
			    base.Remove(id);
			    if(_sink != null) _sink.Dispose();
            }
        }

        public void Remove(ILogSink value) {
            Remove(value.Id);
        }

        public void RemoveAll() {
            Select();
            lock(this) {
                foreach (var id in new List<string>(Keys)) {
                    if(ContainsKey(id)) Remove(id);
                }
            }
		}

        public void RemoveByName(string name) {
            lock(this) {
                Remove(FindIdFromName(name));
            }
        }

        public void Replace(string id) {
            lock(this) {
                if(id.Equals(sink.Id)) return;
                var _sink = sink;
                Select(id);
                Remove(_sink.Id);
            }
        }

        public void Replace(ILogSink value) {
            lock(this) {
                var id = sink.Id;
                this[value.Id] = value;
		        sink = value;
                if(! id.Equals(value.Id)) Remove(id);
            }
        }

        public void ReplaceByName(string name) {
            lock(this) {
                Replace(FindIdFromName(name));
            }
        }

        public void Select() {
		    sink = base[defaultId];
        }

        public void Select(string id) {
            if(id == null) id = defaultId;
		    sink = base[id];
		}

        public void SelectByName(string name) {
            string id = FindIdFromName(name);
            var discard = base[id]; // Throw error if id is null.
            Select(id);
        }

        public void SetAndSelect(ILogSink value) {
            lock(this) {
			    this[value.Id] = value;
                sink = value;
            }
        }

        /*
        public void ToString(string tag) {
            lock(this) {
                string str = "Log (" + tag + ") {";
                foreach (var id in Keys) {
                    str += "\tsink: " + id
                }
                str += "\tdefault: " + defaultId
                str += "\tselected: " + sink.Id
                str += "} Log (" + tag + ")"
            }
        }
        */
    }
}
