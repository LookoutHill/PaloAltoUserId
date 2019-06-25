using System;
using System.Collections.Generic;
using Microsoft.Win32;
using org.aha_net.DSV;
using org.aha_net.Logging;
using org.aha_net.PaloAlto;
using org.aha_net.Records;
using org.aha_net.RegistryTables;

namespace org.aha_net.PaloAltoUserId {
    public class PaloAltoUserIdUpdater {
        private static PaloAltoUserIdUpdater singleton = null;
        private static readonly Object mutex = new Object();
        public static PaloAltoUserIdUpdater Instance {
            get {
                lock(mutex) {
                    if(singleton == null) singleton = new PaloAltoUserIdUpdater();
                    return singleton;
                }
            }
        }

        private readonly DhcpRecordsByMac publicCache;
        private readonly DhcpRecordsByMac dhcpCache;
        private readonly IasRecordsByMac iasCache;
        private readonly UidRecordsByIp uidCache;
        private readonly RegistryTable<string> registeredMacs;
        private readonly RegistryTable<string> ahanetMacs;
        private readonly PublicRecordsByMac publicMacs;
        private readonly RateLimitedMacs rateLimitedMacs;
        private readonly RegistryTable<string> debugTable;
        private readonly MapUserIp mapper;

        private readonly int      wifi_reauth_plus_padding;
        private readonly int      dhcp_lease_duration_plus_padding;
        private readonly TimeSpan dhcp_lease_timespan_plus_padding;
        private readonly DateTime summertime_start;
        private readonly DateTime summertime_end;
        private readonly TimeSpan public_abuse_session_length_timespan;
        private readonly int      public_abuse_session_count_threshold;
        private readonly TimeSpan public_abuse_retain_session_count_timespan;

        private PaloAltoUserIdUpdater() {
            var configKey = RegistryPath.Open(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId");

            wifi_reauth_plus_padding = Int32.Parse((string) configKey.GetValue("wifi_reauth_plus_padding"));
            dhcp_lease_duration_plus_padding = Int32.Parse((string) configKey.GetValue("dhcp_lease_duration_plus_padding"));
            dhcp_lease_timespan_plus_padding = TimeSpan.FromMinutes(dhcp_lease_duration_plus_padding);
            summertime_start = DateTime.Parse((string) configKey.GetValue("summertime_start"));
            summertime_end = DateTime.Parse((string) configKey.GetValue("summertime_end"));
            public_abuse_session_length_timespan = TimeSpan.FromDays(Double.Parse((string) configKey.GetValue("public_abuse_session_length_timespan")));
            public_abuse_session_count_threshold = Int32.Parse((string) configKey.GetValue("public_abuse_session_count_threshold"));
            public_abuse_retain_session_count_timespan = TimeSpan.FromDays(Double.Parse((string) configKey.GetValue("public_abuse_retain_session_count_timespan")));

            rateLimitedMacs = new RateLimitedMacs(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId\RateLimitedMacs", true);
            publicCache = new DhcpRecordsByMac(rateLimitedMacs, dhcp_lease_duration_plus_padding);
            dhcpCache = new DhcpRecordsByMac(rateLimitedMacs, dhcp_lease_duration_plus_padding);
            iasCache = new IasRecordsByMac(wifi_reauth_plus_padding);
            uidCache = new UidRecordsByIp();
            registeredMacs = new RegSzRawDictionary(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId\RegisteredMacs", true);
            ahanetMacs = new RegSzDictionary(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId\AhanetMacs", true);
            publicMacs = new PublicRecordsByMac(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId\PublicMacs", dhcp_lease_timespan_plus_padding, public_abuse_retain_session_count_timespan, true);
            debugTable = new RegSzRawDictionary(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId\Debug", true);
            mapper = MapUserIp.Instance;
        }

        public UidRecord CreateUid(DhcpRecord record) {
            var mac = record.mac;
            UidRecord uid = null;
            IasRecord iasRecord;
            string logon;
            if(iasCache.TryGetValue(mac, out iasRecord)) {
                uid = new UidRecord(record, iasRecord, wifi_reauth_plus_padding);
                if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("CreateUid") || debugTable.ContainsKey("iasCache")) Log.Inform("DEBUG: DHCP: CreateUid: iasCache: " + uid);
            } else if(registeredMacs.TryGetValue(mac, out logon)) {
                uid = new UidRecord(record, logon, dhcp_lease_duration_plus_padding);
                if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("CreateUid") || debugTable.ContainsKey("registeredMacs")) Log.Inform("DEBUG: DHCP: CreateUid: registeredMacs: " + uid);
            } else {
                if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("CreateUid")) Log.Inform("DEBUG: DHCP: CreateUid: NULL");
            }
            return uid;
        }

        public UidRecord CreateUid(IasRecord record) {
            var mac = record.mac;
            UidRecord uid = null;
            DhcpRecord dhcpRecord;
            if(dhcpCache.TryGetValue(mac, out dhcpRecord)) {
                uid = new UidRecord(record, dhcpRecord, wifi_reauth_plus_padding);
                if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("CreateUid") || debugTable.ContainsKey("dhcpCache")) Log.Inform("DEBUG: IAS: CreateUid: dhcpCache: " + uid);
            } else {
                if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("CreateUid")) Log.Inform("DEBUG: IAS: CreateUid: NULL");
            }
            return uid;
        }

        public bool IsBetween(DateTime subject, DateTime startRange, DateTime endRange) {
            return subject >= startRange && subject <= endRange;
        }

        public bool IsOlderThan5Minutes(DateTime date) {
            return date < DateTime.Now.AddMinutes(-5);
        }

        public bool IsSummertime(DateTime date) {
            return IsBetween(date, summertime_start, summertime_end);
        }

        public void LoginAhanet(UidRecord record) {
            mapper.AddLogin(record.logon, record.ip, record.duration);
        }

        public void LoginCached() {
            lock(uidCache) {
                foreach(var uid in uidCache.Values) {
                    LoginAhanet(uid);
                }
            }
            lock(publicCache) {
                foreach(var record in publicCache.Values) {
                    LoginPublic(record);
                }
            }
        }

        public void LoginPublic(DhcpRecord record) {
            mapper.AddLogin(@"aha-net\aha-public", record.ip, dhcp_lease_duration_plus_padding);
        }

        public void LogoutPublic(DhcpRecord record) {
            mapper.AddLogout(@"aha-net\aha-public", record.ip);
        }

        public void ProcessDhcp(DhcpRecord record) {
            var mac = record.mac;
            if(rateLimitedMacs.IsBlocked(mac)) return;
            {
                var header = "DEBUG: ProcessDhcp: BEFORE: ";
                try {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + record);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("ahanetMacs")) && ahanetMacs.ContainsKey(mac)) Log.Inform(header + mac + ": ahanetMacs: " + ahanetMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("registeredMacs")) && registeredMacs.ContainsKey(mac)) Log.Inform(header + mac + ": registeredMacs: " + registeredMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) && publicMacs.ContainsKey(mac)) Log.Inform(header + mac + ": publicMacs: " + publicMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("rateLimitedMacs")) && rateLimitedMacs.ContainsKey(mac)) Log.Inform(header + mac + ": rateLimitedMacs: " + rateLimitedMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicCache")) && publicCache.ContainsKey(mac)) Log.Inform(header + mac + ": publicCache: " + publicCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("dhcpCache")) && dhcpCache.ContainsKey(mac)) Log.Inform(header + mac + ": dhcpCache: " + dhcpCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("iasCache")) && iasCache.ContainsKey(mac)) Log.Inform(header + mac + ": iasCache: " + iasCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("uidCache")) && uidCache.ContainsKey(record.ip)) Log.Inform(header + mac + ": uidCache: " + uidCache[record.ip]);
                } catch {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + "Interrupted during debug info retreival.");
                }
            }
            if(record.IsPublic) {
                if(ahanetMacs.ContainsKey(mac) || registeredMacs.ContainsKey(mac)) {
                    ProcessDhcpPublicLogout(record, mac);
                } else {
                    ProcessDhcpPublicLogin(record, mac);
                }
            } else {
                ProcessDhcpAhanet(record, mac);
            }
            {
                var header = "DEBUG: ProcessDhcp: AFTER: ";
                try {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + record);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("ahanetMacs")) && ahanetMacs.ContainsKey(mac)) Log.Inform(header + mac + ": ahanetMacs: " + ahanetMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("registeredMacs")) && registeredMacs.ContainsKey(mac)) Log.Inform(header + mac + ": registeredMacs: " + registeredMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) && publicMacs.ContainsKey(mac)) Log.Inform(header + mac + ": publicMacs: " + publicMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("rateLimitedMacs")) && rateLimitedMacs.ContainsKey(mac)) Log.Inform(header + mac + ": rateLimitedMacs: " + rateLimitedMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicCache")) && publicCache.ContainsKey(mac)) Log.Inform(header + mac + ": publicCache: " + publicCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("dhcpCache")) && dhcpCache.ContainsKey(mac)) Log.Inform(header + mac + ": dhcpCache: " + dhcpCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("iasCache")) && iasCache.ContainsKey(mac)) Log.Inform(header + mac + ": iasCache: " + iasCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("uidCache")) && uidCache.ContainsKey(record.ip)) Log.Inform(header + mac + ": uidCache: " + uidCache[record.ip]);
                } catch {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + "Interrupted during debug info retreival.");
                }
            }
        }

        public void ProcessDhcpAhanet(DhcpRecord record, string mac) {
            ahanetMacs.AddOrReplace(mac, record.name);
            publicCache.RemoveIfObsolete(mac, record);
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessDhcpAhanet: NEXT: dhcpCache.AddOrReplace: " + record);
            if(! dhcpCache.AddOrReplace(mac, record)) return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessDhcpAhanet: NEXT:              CreateUid: " + record);
            var uid = CreateUid(record);
            if(uid == null)                           return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessDhcpAhanet: NEXT:  uidCache.AddOrReplace: " + record);
            if(! uidCache.AddOrReplace(uid.ip, uid))  return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessDhcpAhanet: NEXT:  ! IsOlderThan5Minutes: " + record);
            if(IsOlderThan5Minutes(record.date))      return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessDhcpAhanet: NEXT:            LoginAhanet: " + record);
            Log.Inform(string.Format("{0,-4}|{1,-19}  {2,-12}  {3,-15}  {4,-29}  {5,-5}  {6,-29}|", uid.source, uid.date.ToString("s"), mac, uid.ip, uid.logon, uid.duration, uid.note));
            LoginAhanet(uid);
        }

        public void ProcessDhcpPublicLogin(DhcpRecord record, string mac) {
            var publicRecord = new PublicRecord(record);
            publicMacs[mac] = publicRecord;
            if(! IsSummertime(DateTime.Now)) {
                if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) Log.Inform("DEBUG: ProcessDhcpPublicLogin: NEXT:   public_abuse_session: " + record);
                if(publicMacs[mac].SessionAge(dhcp_lease_timespan_plus_padding) >= public_abuse_session_length_timespan || publicMacs[mac].sessionCount >= public_abuse_session_count_threshold) {
                    ahanetMacs.AddOrReplace(record.mac, "PUBLIC:" + record.name);
                    ProcessDhcpPublicLogout(record, mac);
                    return;
                }
            }
            if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) Log.Inform("DEBUG: ProcessDhcpPublicLogin: NEXT: publicCache.AddOrReplace: " + record);
            if(! publicCache.AddOrReplace(mac, record)) return;
            if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) Log.Inform("DEBUG: ProcessDhcpPublicLogin: NEXT:    ! IsOlderThan5Minutes: " + record);
            if(IsOlderThan5Minutes(record.date))        return;
            if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) Log.Inform("DEBUG: ProcessDhcpPublicLogin: NEXT:              LoginPublic: " + record);
            Log.Inform(string.Format("{0,-4}|{1,-19}  {2,-12}  {3,-15}  {4,-29}|", "_pub", record.date.ToString("s"), mac, record.ip, record.name));
            LoginPublic(record);
        }

        public void ProcessDhcpPublicLogout(DhcpRecord record, string mac) {
            dhcpCache.RemoveIfObsolete(mac, record);
            publicCache.RemoveIfObsolete(mac, record);
            if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) Log.Inform("DEBUG: ProcessDhcpPublicLogout: NEXT:    ! IsOlderThan5Minutes: " + record);
            if(IsOlderThan5Minutes(record.date)) return;
            if(debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) Log.Inform("DEBUG: ProcessDhcpPublicLogout: NEXT:             LogoutPublic: " + record);
            Log.Inform(string.Format("{0,-4}|{1,-19}  {2,-12}  {3,-15}  {4,-29}|", "ban", record.date.ToString("s"), mac, record.ip, record.name));
            LogoutPublic(record);
        }

        public void ProcessIas(IasRecord record) {
            var mac = record.mac;
            DhcpRecord dhcpRecord;
            if(dhcpCache.TryGetValue(mac, out dhcpRecord)) {
                ahanetMacs.AddOrReplace(mac, dhcpRecord.name);
                publicCache.RemoveIfObsolete(mac, dhcpRecord);
            }
            {
                var header = "DEBUG: ProcessIas: BEFORE: ";
                try {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + record);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("ahanetMacs")) && ahanetMacs.ContainsKey(mac)) Log.Inform(header + mac + ": ahanetMacs: " + ahanetMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("registeredMacs")) && registeredMacs.ContainsKey(mac)) Log.Inform(header + mac + ": registeredMacs: " + registeredMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) && publicMacs.ContainsKey(mac)) Log.Inform(header + mac + ": publicMacs: " + publicMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("rateLimitedMacs")) && rateLimitedMacs.ContainsKey(mac)) Log.Inform(header + mac + ": rateLimitedMacs: " + rateLimitedMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicCache")) && publicCache.ContainsKey(mac)) Log.Inform(header + mac + ": publicCache: " + publicCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("dhcpCache")) && dhcpCache.ContainsKey(mac)) Log.Inform(header + mac + ": dhcpCache: " + dhcpCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("iasCache")) && iasCache.ContainsKey(mac)) Log.Inform(header + mac + ": iasCache: " + iasCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("uidCache")) && dhcpRecord != null && uidCache.ContainsKey(dhcpRecord.ip)) Log.Inform(header + mac + ": uidCache: " + uidCache[dhcpRecord.ip]);
                } catch {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + "Interrupted during debug info retreival.");
                }
            }
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessIas: NEXT: iasCache.AddOrReplace: " + record);
            if(! iasCache.AddOrReplace(mac, record)) return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessIas: NEXT:             CreateUid: " + record);
            var uid = CreateUid(record);
            if(uid == null)                          return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessIas: NEXT: uidCache.AddOrReplace: " + record);
            if(! uidCache.AddOrReplace(uid.ip, uid)) return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessIas: NEXT: ! IsOlderThan5Minutes: " + record);
            if(IsOlderThan5Minutes(record.date))     return;
            if(debugTable.ContainsKey(mac)) Log.Inform("DEBUG: ProcessIas: NEXT:           LoginAhanet: " + record);
            Log.Inform(string.Format("{0,-4}|{1,-19}  {2,-12}  {3,-15}  {4,-29}  {5,-5}  {6,-29}|", uid.source, uid.date.ToString("s"), mac, uid.ip, uid.logon, uid.duration, uid.note));
            LoginAhanet(uid);
            {
                var header = "DEBUG: ProcessIas: AFTER: ";
                try {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + record);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("ahanetMacs")) && ahanetMacs.ContainsKey(mac)) Log.Inform(header + mac + ": ahanetMacs: " + ahanetMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("registeredMacs")) && registeredMacs.ContainsKey(mac)) Log.Inform(header + mac + ": registeredMacs: " + registeredMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicMacs")) && publicMacs.ContainsKey(mac)) Log.Inform(header + mac + ": publicMacs: " + publicMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("rateLimitedMacs")) && rateLimitedMacs.ContainsKey(mac)) Log.Inform(header + mac + ": rateLimitedMacs: " + rateLimitedMacs[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("publicCache")) && publicCache.ContainsKey(mac)) Log.Inform(header + mac + ": publicCache: " + publicCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("dhcpCache")) && dhcpCache.ContainsKey(mac)) Log.Inform(header + mac + ": dhcpCache: " + dhcpCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("iasCache")) && iasCache.ContainsKey(mac)) Log.Inform(header + mac + ": iasCache: " + iasCache[mac]);
                    if((debugTable.ContainsKey(mac) || debugTable.ContainsKey("uidCache")) && dhcpRecord != null && uidCache.ContainsKey(dhcpRecord.ip)) Log.Inform(header + mac + ": uidCache: " + uidCache[dhcpRecord.ip]);
                } catch {
                    if(debugTable.ContainsKey(mac)) Log.Inform(header + "Interrupted during debug info retreival.");
                }
            }
        }

        public void RemoveStaleEntries() {
            dhcpCache.RemoveStaleEntries();
            iasCache.RemoveStaleEntries();
            uidCache.RemoveStaleEntries();
            publicCache.RemoveStaleEntries();
        }
    }

    public class UpdateDhcpRecord : DhcpRecordHandler {
        public UpdateDhcpRecord() : base() {}

        public override void Process(DhcpRecord record) {
            PaloAltoUserIdUpdater.Instance.ProcessDhcp(record);
        }
    }

    public class UpdateIasRecord : IasRecordHandler {
        public UpdateIasRecord() : base() {}

        public override void Process(IasRecord record) {
            PaloAltoUserIdUpdater.Instance.ProcessIas(record);
        }
    }
}
