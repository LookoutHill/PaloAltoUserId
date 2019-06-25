using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using org.aha_net.Logging;
using org.aha_net.RegistryTables;

namespace org.aha_net.Records {
    public class DhcpRecord {
        public readonly DateTime date;
        public readonly string mac;
        public readonly string ip;
        public readonly string key;
        public readonly string name;
        private readonly int type;

        private static readonly int fieldType = 0;
        private static readonly int fieldDate = 1;
        private static readonly int fieldTime = 2;
        private static readonly int fieldIp = 4;
        private static readonly int fieldFqdn = 5;
        private static readonly int fieldMac = 6;
        public DhcpRecord(string[] record) {
            type = Int32.Parse(record[fieldType]);
            if(! (IsTypeNewLease || IsTypeRenewedLease)) throw new ArgumentException("Invalid type (" + type + ") in type field(" + fieldType + ").");
            date = DateTime.Parse(string.Format("{0}   {1}", record[fieldDate], record[fieldTime]));
            mac = record[fieldMac].Trim().ToLower();
            if(! Regex.IsMatch(mac, @"^[0-9a-f]+$")) throw new ArgumentException("Invalid MAC address (" + mac + ") in mac field(" + fieldMac + ").");
            ip = record[fieldIp].Trim().ToLower();
            if(! Regex.IsMatch(ip, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$")) throw new ArgumentException("Invalid IP address (" + ip + ") in ip field(" + fieldIp + ").");
            key = string.Format("{0}-{1}", mac, ip);
            name = record[fieldFqdn].Trim().ToLower().Replace(".aha-net.org", "");
        }

        private static readonly int typeNewDhcpLease = 10;
        private static readonly int typeNewBootpLease = 20;
        public bool IsTypeNewLease {
            get {
                return type == typeNewDhcpLease || type == typeNewBootpLease;
            }
        }

        private static readonly int typeRenewedDhcpLease = 11;
        private static readonly int typeRenewedBootpLease = 21;
        public bool IsTypeRenewedLease {
            get {
                return type == typeRenewedDhcpLease || type == typeRenewedBootpLease;
            }
        }

        public bool IsPublic {
            get {
                return ip.Substring(0, 5).Equals("10.9.");
            }
        }

        public override string ToString() {
            return string.Format("dhcp|{0,-19}  {1,-12}  {2,-15}  {3,-29}  {4,-2}  {5}|", date.ToString("s"), mac, ip, name, type, key);
        }
    }

    public class IasMap : Dictionary<string, string> {
        public IasMap(string[] record) {
            Add("apIpAddr", record[0]);
            Add("username", record[1]);
            Add("datetime", record[2] + "   " + record[3]);
            Add("service",  record[4]);
            Add("computer", record[5]);
            for(int index = 6; index < record.Length-1; index += 2) {
                if(! ContainsKey(record[index])) Add(record[index], record[index+1]);
            }
        }

        private static readonly string fieldMac = "31";
        private static readonly string fieldType = "4136";

        public bool HasMacAddr {
            get {
                return ContainsKey(fieldMac);
            }
        }

        public bool FromComputerAccount {
            get {
                return this["username"].IndexOf("host/") == 0;
            }
        }

        private static readonly string fieldRetVal = "4142";
        private static readonly string retValSuccess = "0";
        public bool IsUnsuccessful {
            get {
                return ! this[fieldRetVal].Trim().Equals(retValSuccess);
            }
        }

        private static readonly string typeAcceptRequest = "1";
        public bool IsTypeAcceptRequest {
            get {
                return this[fieldType].Trim().Equals(typeAcceptRequest);
            }
        }

        private static readonly string typeAccessAccept = "2";
        public bool IsTypeAccessAccept {
            get {
                return this[fieldType].Trim().Equals(typeAccessAccept);
            }
        }

        private static readonly string fieldStamp = "25";
        public string Key {
            get {
                return string.Format("{0}-{1}", this["computer"], this[fieldStamp]);
            }
        }

        public void CopyMac(IasMap that) {
            this[fieldMac] = that[fieldMac];
        }

        public override string ToString() {
            string str = "IasMap|";
            foreach(var key in Keys) {
                str += key + ":" + this[key] + "|";
            }
            return str;
        }
    }

    public class IasRecord {
        public readonly DateTime date;
        public readonly string mac;
        public readonly string logon;
        public readonly string key;
        public readonly string ap;

        private static readonly string fieldAp = "4128";
        private static readonly string fieldLogon = "4129";
        private static readonly string fieldMac = "31";

        public IasRecord(IasMap map) {
            date = DateTime.Parse(map["datetime"]);
            mac = FormatMacAddr(map[fieldMac]);
            if(! Regex.IsMatch(mac, @"^[0-9a-f]+$")) throw new ArgumentException("Invalid MAC address (" + mac + ") in field " + fieldMac + ".");
            logon = FormatLogon(map[fieldLogon]);
//            if(! Regex.IsMatch(logon, @"^[a-z][-a-z0-9]*$")) throw new ArgumentException("Invalid logon (" + logon + ") in field " + fieldLogon + ".");
            key = string.Format("{0}-{1}", mac, logon);
            ap = map[fieldAp];
        }

        private static string FormatLogon(string _logon) {
            return _logon.Trim().ToLower();
        }

        private static string FormatMacAddr(string mac) {
            return mac.Replace("-", "").Replace(".", "").Replace(":", "").ToLower();
        }

        public override string ToString() {
            return string.Format("IAS |{0,-19}  {1,-12}  {2,-29}  {3,-29}  {4}|", date.ToString("s"), mac, logon, ap, key);
        }
    }

    public class PublicRecord {
        public readonly string   mac;
        public readonly string   name;
        public readonly DateTime firstContact;
        public readonly DateTime sessionStart;
        public readonly DateTime lastContact;
        public readonly int      sessionCount;

        private static readonly string[] separatorList = new string[] { "," };
        public PublicRecord(string dsv) {
            var record = dsv.Split(separatorList, StringSplitOptions.None);
            mac          = record[0];
            name         = record[1];
            firstContact = DateTime.Parse(record[2]);
            sessionStart = DateTime.Parse(record[3]);
            lastContact  = DateTime.Parse(record[4]);
            sessionCount = Int32.Parse(record[5]);
        }

        public PublicRecord(DhcpRecord dhcp) : this(dhcp.mac, dhcp.name, dhcp.date, dhcp.date, dhcp.date) {}

        public PublicRecord(string _mac, string _name, DateTime _firstContact, DateTime _sessionStart, DateTime _lastContact, int _sessionCount = 0) {
            mac          = _mac;
            name         = _name;
            firstContact = _firstContact;
            sessionStart = _sessionStart;
            lastContact  = _lastContact;
            sessionCount = _sessionCount;
        }

        public PublicRecord Merge(PublicRecord that, TimeSpan maxSessionTimeSpan, TimeSpan retainCountTimeSpan) {
            if(maxSessionTimeSpan <= TimeSpan.Zero) throw new ArgumentException("Unable to merge PublicRecords. The maxSessionTimeSpan argument must be greater than zero.");
            if(retainCountTimeSpan <= maxSessionTimeSpan)   throw new ArgumentException("Unable to merge PublicRecords. The retainCountTimeSpan argument must be greater than the maxSessionTimeSpan argument.");

            if(this.lastContact >= that.lastContact) return this;

            int      _sessionCount;
            DateTime _sessionStart;
            var      sessionAge = that.lastContact - this.lastContact;
            if(sessionAge >= maxSessionTimeSpan) {
                if(sessionAge >= retainCountTimeSpan) _sessionCount = 0;
                else                                  _sessionCount = this.sessionCount + 1;
                _sessionStart = that.lastContact;
            } else {
                _sessionCount = this.sessionCount;
                _sessionStart = this.sessionStart;
            }

            return new PublicRecord(that.mac, that.name, this.firstContact, _sessionStart, that.lastContact, _sessionCount);
        }

        public TimeSpan SessionAge(TimeSpan maxSessionTimeSpan) {
            var now = DateTime.Now;
            var sessionEnd = lastContact + maxSessionTimeSpan;
            if(sessionEnd > now) sessionEnd = now;
            return sessionEnd - sessionStart;
        }

        public string ToDsv() {
            return string.Format("{0},{1},{2},{3},{4},{5}", mac, name, firstContact.ToString("s"), sessionStart.ToString("s"), lastContact.ToString("s"), sessionCount);
        }

        public override string ToString() {
            return string.Format("_pub|{0,-19}  {1,-19}  {2,-19}  {3}  {4,-12}  {5}|", firstContact.ToString("s"), sessionStart.ToString("s"), lastContact.ToString("s"), sessionCount, mac, name);
        }
    }

    public class UidRecord {
        public readonly DateTime date;
        public readonly string ip;
        public readonly string logon;
        public readonly int duration;
        public readonly string key;
        public readonly string source;
        public readonly string note;

        public UidRecord(IasRecord ias, DhcpRecord dhcp, int _duration) {
            duration = _duration;
            date = ias.date;
            ip = dhcp.ip;
            logon = ias.logon;
            key = string.Format("{0}-{1}", ip, logon);
            source = "IAS";
            note = ias.ap;
        }

        public UidRecord(DhcpRecord dhcp, IasRecord ias, int _duration) {
            duration = _duration;
            date = dhcp.date;
            ip = dhcp.ip;
            logon = ias.logon;
            key = string.Format("{0}-{1}", ip, logon);
            source = "dhcp";
            note = dhcp.name;
        }

        public UidRecord(DhcpRecord dhcp, string _logon, int _duration) {
            duration = _duration;
            date = dhcp.date;
            ip = dhcp.ip;
            logon = @"aha-net\" + _logon;
            key = string.Format("{0}-{1}", ip, logon);
            source = "DHCP";
            note = dhcp.name;
        }

        public override string ToString() {
            return string.Format("uid |{0,-19}  {1,-15}  {2,-29}  {3,-5}  {4,4}  {5,-29}  {6}|", date.ToString("s"), ip, logon, duration, source, note, key);
        }
    }

    public class RateLimitedMacs : RegSzRawDictionary {
        public RateLimitedMacs(string regPath, bool create = false) : base(regPath, create) {}

        public bool IsBlocked(string key) {
            if(GetValue(key) < 0) return true;
            else                  return false;
        }

        public bool IsRateLimited(string key) {
            if(GetValue(key) > 0) return true;
            else                  return false;
        }

        public int GetValue(string key) {
            string value;
            if(TryGetValue(key, out value)) {
                int seconds;
                Int32.TryParse(value.Trim(), out seconds);
                return seconds;
            }
            return 0;
        }
    }

    public class DhcpRecordsByMac : RecordDictionary<DhcpRecord> {
        protected readonly RateLimitedMacs rateLimits;
        private readonly int               staleThreshold;

        public DhcpRecordsByMac(RateLimitedMacs _rateLimits, int _staleThreshold) {
            rateLimits = _rateLimits;
            staleThreshold = _staleThreshold;
        }

        protected override bool IsObsolete(string key, DhcpRecord record) {
            return RateLimitChattyDevices(key, record);
        }

        private bool RateLimitChattyDevices(string key, DhcpRecord record) {
            var seconds = rateLimits.GetValue(key);
            if(seconds >= 0 && seconds < 60) seconds = 60;
            return this[key].date + TimeSpan.FromSeconds(seconds) < record.date;
        }

        protected override bool IsStale(string key) {
            return this[key].date.AddSeconds(staleThreshold) < DateTime.Now;
        }
    }

    public class IasMaps : Dictionary<string, IasMap>, IDisposable {
        private readonly CancellationTokenSource cts;

        public IasMaps() {
            cts = new CancellationTokenSource();
            (new Thread(new ThreadStart(Maintain))).Start();
        }

        public void AddOrReplace(string key, IasMap map) {
            lock(this) {
                if(ContainsKey(key)) this[key] = map;
                else                 Add(key, map);
            }
        }

        public void LockAndRemove(string key) {
            lock(this) { Remove(key); }
        }

        public IasMap RemoveAndReturn(string key) {
            lock(this) {
                if(ContainsKey(key)) {
                    var map = this[key];
                    Remove(key);
                    return map;
                } else {
                    return null;
                }
            }
        }

        private static readonly TimeSpan _10_minutes = TimeSpan.FromMinutes(10);

        private void Maintain() {
            while(cts.Token.IsCancellationRequested) {
                using(var taskDelay = Task.Delay(_10_minutes)) { taskDelay.Wait(); }
                RemoveStaleEntries();
            }
        }

        private void RemoveStaleEntries() {
            lock(this) {
                var countBefore = Count;
                List<string> keyList = new List<string>();
                foreach(var key in Keys) {
                    if(DateTime.Now - DateTime.Parse(this[key]["datetime"]) >= _10_minutes)
                        keyList.Add(key);
                    if(cts.Token.IsCancellationRequested) return;
                }
                foreach(var key in keyList) {
                    Remove(key);
                    if(cts.Token.IsCancellationRequested) return;
                }
                Log.Inform(string.Format("IAS map count: {0} -> {1}", countBefore, Count));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if(disposing) cts.Cancel();
        }

        ~IasMaps() {
            Dispose(false);
        }
    }

    public class IasRecordsByMac : RecordDictionary<IasRecord> {
        private readonly int staleThreshold;
        public IasRecordsByMac(int _staleThreshold) {
            staleThreshold = _staleThreshold;
        }

        protected override bool IsObsolete(string key, IasRecord record) {
            return this[key].date < record.date;
        }

        protected override bool IsStale(string key) {
            return this[key].date.AddSeconds(staleThreshold) < DateTime.Now;
        }
    }

    public class PublicRecordsByMac : RegDsvDictionary<PublicRecord> {
        private readonly TimeSpan maxSessionTimeSpan;
        private readonly TimeSpan retainCountTimeSpan;
        public PublicRecordsByMac(string regPath, TimeSpan _maxSessionTimeSpan, TimeSpan _retainCountTimeSpan, bool create = false) : base(regPath, create) {
            maxSessionTimeSpan = _maxSessionTimeSpan;
            retainCountTimeSpan = _retainCountTimeSpan;
        }

        protected override string ToDsv(PublicRecord value) {
            return value.ToDsv();
        }

        protected override PublicRecord FromDsv(string value) {
            return new PublicRecord(value);
        }

        protected override PublicRecord Replace(PublicRecord newer, PublicRecord older) {
            return older.Merge(newer, maxSessionTimeSpan, retainCountTimeSpan);
        }
    }

    public class UidRecordsByIp : RecordDictionary<UidRecord> {
        protected override bool IsObsolete(string key, UidRecord record) {
            return CollapseIasDhcpPairsButAllowAllChangedLogons(key, record);
        }

        private bool CollapseIasDhcpPairsButAllowAllChangedLogons(string key, UidRecord record) {
            return this[key].date.AddSeconds(5) < record.date || (this[key].date < record.date && ! this[key].logon.Equals(record.logon));
        }

        protected override bool IsStale(string key) {
            return this[key].date.AddSeconds(this[key].duration) < DateTime.Now;
        }
    }

    public abstract class RecordDictionary<T> : Dictionary<string, T> {
        public new void Add(string key, T record) {
            lock(this) {
                RemoveIfStale(key);
                base.Add(key, record);
            }
        }

        public bool AddOrReplace(string key, T record) {
            lock(this) {
                if(ContainsKey(key)) {
                    if(IsObsolete(key, record)) {
                        this[key] = record;
                        return true;
                    }
                } else {
                    base.Add(key, record);
                    return true;
                }
            }
            return false;
        }

        public new bool ContainsKey(string key) {
            lock(this) {
                if(base.ContainsKey(key)) {
                    if(IsStale(key)) {
                        Remove(key);
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    return false;
                }
            }
        }

        protected abstract bool IsObsolete(string key, T record);
        protected abstract bool IsStale(string key);

        public string[] KeyList {
            get {
                lock(this) {
                    List<string> keyList = new List<string>();
                    foreach(var key in Keys) {
                        if(! IsStale(key)) keyList.Add(key);
                    }
                    return keyList.ToArray();
                }
            }
        }

        public new void Remove(string key) {
            lock(this) base.Remove(key);
        }

        public bool RemoveIfObsolete(string key, T record) {
            lock(this) {
                if(ContainsKey(key) && IsObsolete(key, record)) {
                    base.Remove(key);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveIfStale(string key) {
            lock(this) {
                if(base.ContainsKey(key) && IsStale(key)) {
                    base.Remove(key);
                    return true;
                }
            }
            return false;
        }

        public void RemoveStaleEntries() {
            lock(this) foreach(var key in KeyList) {}
        }

        public bool TryAdd(string key, T record) {
            lock(this) {
                if(! ContainsKey(key)) {
                    base.Add(key, record);
                    return true;
                }
            }
            return false;
        }

        public new bool TryGetValue(string key, out T record) {
            lock(this) {
                if(ContainsKey(key)) {
                    record = this[key];
                    return true;
                }
            }
            record = default(T);
            return false;
        }
    }
}
