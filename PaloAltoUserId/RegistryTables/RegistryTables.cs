using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace org.aha_net.RegistryTables {
	class RegistryPath {
        public static Tuple<RegistryKey, string> Split(string regPath) {
            RegistryKey hive = null;
            if(regPath.IndexOf(@"HKEY_LOCAL_MACHINE\") == 0) {
                hive = Registry.LocalMachine;
                regPath = regPath.Substring(19);
            } else if(regPath.IndexOf(@"HKEY_CURRENT_USER\") == 0) {
                hive = Registry.CurrentUser;
                regPath = regPath.Substring(18);
            } else if(regPath.IndexOf(@"HKEY_CLASSES_ROOT\") == 0) {
                hive = Registry.ClassesRoot;
                regPath = regPath.Substring(18);
            } else if(regPath.IndexOf(@"HKEY_USERS\") == 0) {
                hive = Registry.Users;
                regPath = regPath.Substring(11);
            } else if(regPath.IndexOf(@"HKEY_CURRENT_CONFIG\") == 0) {
                hive = Registry.CurrentConfig;
                regPath = regPath.Substring(20);
            } else {
                hive = Registry.LocalMachine;
            }

            return new Tuple<RegistryKey, string> (hive, regPath);
        }

        public static RegistryKey Open(string regPath, bool create = false) {
            var retVal = Split(regPath);
            RegistryKey hive = retVal.Item1;
            regPath = retVal.Item2;

            if(create) return hive.CreateSubKey(regPath);
            else       return hive.OpenSubKey(regPath, true);
        }
    }

	public interface RegistryTable<T> {
        T this[string key] { get; set; }
		void Add(string key, T value);
		void AddOrReplace(string key, T value);
		void Clear();
		bool ContainsKey(string key);
        int Count { get; }
        List<string> Keys { get; }
		void Remove(string key);
		bool TryAdd(string key, T value);
		bool TryGetValue(string key, out T value);
        List<T> Values { get; }
    }

	public class RegSzDictionary : RegistryTable<string> {
        private readonly Dictionary<string, string> dict;
        private readonly RegistryKey regKey;

        public RegSzDictionary(string regPath, bool create = false) {
            dict = new Dictionary<string, string>();
            regKey = RegistryPath.Open(regPath, create);
            Load();
        }

		public string this[string key] {
            get {
                return dict[key];
            }
            set {
                lock(this) {
                    dict[key] = value;
                    regKey.SetValue(key, value);
                }
            }
        }

		public void Add(string key, string value) {
            lock(this) {
                dict.Add(key, value);
                regKey.SetValue(key, value);
            }
        }

		public void AddOrReplace(string key, string value) {
            lock(this) {
                this[key] = value;
                regKey.SetValue(key, value);
            }
        }

		public void Clear() {
            lock(this) {
                foreach(var key in Keys) {
                    regKey.DeleteValue(key, false);
                }
                dict.Clear();
            }
        }

		public bool ContainsKey(string key) {
            lock(this) return dict.ContainsKey(key);
        }

		public int Count {
            get {
                lock(this) return dict.Count;
            }
        }

		public List<string> Keys {
            get {
                lock(this) {
                    List<string> keys = new List<string>();
                    foreach(var key in dict.Keys) {
                        keys.Add(key);
                    }
                    return keys;
                }
            }
        }

		public void Load() {
            lock(this) {
                dict.Clear();
                foreach(var key in regKey.GetValueNames()) {
                    string value = (string) regKey.GetValue(key);
                    if(value != null) dict.Add(key, value);
                }
            }
        }

		public void Remove(string key) {
            lock(this) {
                dict.Remove(key);
                regKey.DeleteValue(key, false);
            }
        }

		public bool TryAdd(string key, string value) {
            lock(this) {
                if(! ContainsKey(key)) {
                    dict.Add(key, value);
                    regKey.SetValue(key, value);
                    return true;
                }
            }
            return false;
        }

		public bool TryGetValue(string key, out string value) {
            lock(this) {
                if(ContainsKey(key)) {
                    value = this[key];
                    return true;
                }
            }
            value = null;
            return false;
        }

		public List<string> Values {
            get {
                lock(this) {
                    List<string> values = new List<string>();
                    foreach(var value in dict.Values) {
                        values.Add(value);
                    }
                    return values;
                }
            }
        }
    }

	public class RegSzRawDictionary : RegistryTable<string> {
        private readonly RegistryKey _hive;
        private readonly string _regPath;
        private RegistryKey regKey;

        public RegSzRawDictionary(string regPath, bool create = false) {
            var retVal = RegistryPath.Split(regPath);
            _hive = retVal.Item1;
            _regPath = retVal.Item2;
            Open(create);
        }

        private void Open(bool create = false) {
            regKey = _hive.OpenSubKey(_regPath, create);
        }

        public string this[string key] {
            get {
                Open();
                return (string) regKey.GetValue(key);
            }
            set {
                regKey.SetValue(key, value);
            }
        }

		public void Add(string key, string value) {
            regKey.SetValue(key, value);
        }

		public void AddOrReplace(string key, string value) {
            regKey.SetValue(key, value);
        }

		public void Clear() {
            foreach(var key in Keys) {
                regKey.DeleteValue(key, false);
            }
        }

		public bool ContainsKey(string key) {
            Open();
            return regKey.GetValue(key) != null;
        }

        public int Count {
            get {
                Open();
                return regKey.ValueCount;
            }
        }

        public List<string> Keys {
            get {
                List<string> keys = new List<string>();
                Open();
                foreach(var key in regKey.GetValueNames()) {
                    keys.Add(key);
                }
                return keys;
            }
        }

		public void Remove(string key) {
            regKey.DeleteValue(key, false);
        }

		public bool TryAdd(string key, string value) {
            if(! ContainsKey(key)) {
                regKey.SetValue(key, value);
                return true;
            }
            return false;
        }

		public bool TryGetValue(string key, out string value) {
            value = this[key];
            if(value == null) {
                return false;
            } else {
                return true;
            }
        }

		public List<string> Values {
            get {
                List<string> values = new List<string>();
                Open();
                foreach(var key in regKey.GetValueNames()) {
                    values.Add(this[key]);
                }
                return values;
            }
        }
    }

	public abstract class RegDsvDictionary<T> : RegistryTable<T> {
        private readonly RegistryTable<string> dict;

        public RegDsvDictionary(string regPath, bool create = false) {
            dict = new RegSzDictionary(regPath, create);
        }

        public RegDsvDictionary(RegistryTable<string> _dict, bool create = false) {
            dict = _dict;
        }

        protected abstract string ToDsv(T value);
        protected abstract T FromDsv(string value);
        protected abstract T Replace(T newer, T older);

        public T this[string key] {
            get {
                return FromDsv(dict[key]);
            }
            set {
                AddOrReplace(key, value);
            }
        }

		public void Add(string key, T value) {
            lock(dict) dict.Add(key, ToDsv(value));
        }

		public void AddOrReplace(string key, T value) {
            lock(dict) {
                if(dict.ContainsKey(key)) dict[key] = ToDsv(Replace(value, this[key]));
                else                      Add(key, value);
            }
        }

		public void Clear() {
            lock(dict) dict.Clear();
        }

		public bool ContainsKey(string key) {
            return dict.ContainsKey(key);
        }

		public int Count {
            get {
                lock(dict) return dict.Count;
            }
        }

        public List<string> Keys {
            get {
                lock(dict) {
                    List<string> keys = new List<string>();
                    foreach(var key in dict.Keys) {
                        keys.Add(key);
                    }
                    return keys;
                }
            }
        }

		public void Remove(string key) {
            lock(dict) dict.Remove(key);
        }

        public bool TryAdd(string key, T value) {
            lock(dict) return dict.TryAdd(key, ToDsv(value));
        }

        public bool TryGetValue(string key, out T value) {
            lock(dict) {
                string str;
                var retVal = dict.TryGetValue(key, out str);
                if(str != null) value = FromDsv(str);
                else            value = default(T);
                return retVal;
            }
        }

        public List<T> Values {
            get {
                lock(dict) {
                    List<T> values = new List<T>();
                    foreach(var value in dict.Values) {
                        values.Add(FromDsv(value));
                    }
                    return values;
                }
            }
        }
    }
}

