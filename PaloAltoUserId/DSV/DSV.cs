using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using org.aha_net.Logging;
using org.aha_net.Records;

namespace org.aha_net.DSV {
    public class DsvLineConfig {
        private readonly string separator;
        private readonly string[] separatorList;

        public DsvLineConfig(string _separator = ",") {
            separator = _separator;
            separatorList = new string[] { separator };
        }

        public int IndexOfSeparator(string line) {
            return line.IndexOf(separator);
        }

        public string Separator {
            get {
                return separator;
            }
        }

        public string[] Split(string line) {
            return line.Split(separatorList, StringSplitOptions.None);
        }
    }

    public abstract class DsvLineHandler {
        protected readonly DsvLineHandler next;

        public DsvLineHandler(DsvLineHandler _next = null) {
            next = _next;
        }

        public abstract void Process(string line);

        protected virtual void ProcessNext(string line) {
            if(next != null) next.Process(line);
        }
    }

    public class FilterDsvLineInvalidDhcpLogEntries : DsvLineHandler {
        private readonly DsvLineConfig config;

        public FilterDsvLineInvalidDhcpLogEntries(DsvLineConfig _config, DsvLineHandler next) : base(next) {
            config = _config;
        }

        public override void Process(string line) {
            if(Regex.IsMatch(line, @"^\d+" + Regex.Escape(config.Separator)))
                ProcessNext(line);
        }
    }

    public class CountDsvLine : DsvLineHandler {
        private int count;

        public CountDsvLine(DsvLineHandler next = null) : base(next) {
            count = 0;
        }

        public override void Process(string line) {
            if(count % 1000 == 0) Log.Inform("count: " + count);
            count++;
            ProcessNext(line);
        }
    }

    public class PrintDsvLine : DsvLineHandler {
        public PrintDsvLine(DsvLineHandler next = null) : base(next) {}

        public override void Process(string line) {
            Log.Inform(line);
            ProcessNext(line);
        }
    }

    public class ConvertDsvLineToDsvRecord : DsvLineHandler {
        private readonly DsvLineConfig config;
        private readonly DsvRecordHandler next_;

        public ConvertDsvLineToDsvRecord(DsvLineConfig _config, DsvRecordHandler _next) : base() {
            config = _config;
            next_ = _next;
        }

        public override void Process(string line) {
            ProcessNext(config.Split(line));
        }

        protected void ProcessNext(string[] record) {
            next_.Process(record);
        }
    }

    public abstract class DsvRecordHandler {
        protected readonly DsvRecordHandler next;

        public DsvRecordHandler(DsvRecordHandler _next = null) {
            next = _next;
        }

        public abstract void Process(string[] record);

        protected virtual void ProcessNext(string[] record) {
            if(next != null) next.Process(record);
        }
    }

    public class PrintDsvRecord : DsvRecordHandler {
        public PrintDsvRecord(DsvRecordHandler next = null) : base(next) {}

        public override void Process(string[] record) {
            Log.Inform("|" + string.Join("|", record) + "|");
            ProcessNext(record);
        }
    }

    public class TagDsvRecord : DsvRecordHandler {
        private readonly string tag;

        public TagDsvRecord(string _tag, DsvRecordHandler next = null) : base(next) {
            tag = _tag;
        }

        public override void Process(string[] record) {
            List<string> taggedRecord = new List<string>();
            foreach(var field in record) {
                taggedRecord.Add(field);
            }
            taggedRecord.Add(tag);
            ProcessNext(taggedRecord.ToArray());
        }
    }

    public class CountDsvRecord : DsvRecordHandler {
        private int count;

        public CountDsvRecord(DsvRecordHandler next = null) : base(next) {
            count = 0;
        }

        public override void Process(string[] record) {
            if(count % 1000 == 0) Log.Inform("count: " + count);
            count++;
            ProcessNext(record);
        }
    }

    public class ConvertDsvRecordToDhcpRecord : DsvRecordHandler {
        private readonly DhcpRecordHandler next_;

        public ConvertDsvRecordToDhcpRecord(DhcpRecordHandler _next) : base() {
            next_ = _next;
        }

        public override void Process(string[] _record) {
            DhcpRecord record;
            try   { record = new DhcpRecord(_record); }
            catch { return; }
            ProcessNext(record);
        }

        protected void ProcessNext(DhcpRecord record) {
            next_.Process(record);
        }
    }

    public abstract class DhcpRecordHandler {
        protected readonly DhcpRecordHandler next;

        public DhcpRecordHandler(DhcpRecordHandler _next = null) {
            next = _next;
        }

        public abstract void Process(DhcpRecord record);

        protected virtual void ProcessNext(DhcpRecord record) {
            if(next != null) next.Process(record);
        }
    }

    public class FilterDhcpRecordUnwanted : DhcpRecordHandler {
        public FilterDhcpRecordUnwanted(DhcpRecordHandler next = null) : base(next) {}

        public override void Process(DhcpRecord record) {
            const string pattonAnalogTelephoneAdapter = "00a0ba0a";
            if(record.mac.Length < 12)                                          { Log.Warn(string.Format("Dropping record with short MAC address {0} from {1}.", record.mac, record.date.ToString("s"))); return; }
			if(record.mac.Substring(0, 8).Equals(pattonAnalogTelephoneAdapter)) { return; }
            ProcessNext(record);
        }
    }

    public class ConvertDsvRecordToIasMap : DsvRecordHandler {
        private readonly IasMapHandler next_;

        public ConvertDsvRecordToIasMap(IasMapHandler _next) : base() {
            next_ = _next;
        }

        public override void Process(string[] record) {
            IasMap map = new IasMap(record);
            ProcessNext(map);
        }

        protected void ProcessNext(IasMap map) {
            next_.Process(map);
        }
    }

    public abstract class IasMapHandler {
        protected readonly IasMapHandler next;

        public IasMapHandler(IasMapHandler _next = null) {
            next = _next;
        }

        public abstract void Process(IasMap record);

        protected virtual void ProcessNext(IasMap record) {
            if(next != null) next.Process(record);
        }
    }

    public class FilterIasMapFromComputerAccount : IasMapHandler {
        public FilterIasMapFromComputerAccount(IasMapHandler next = null) : base(next) {}

        public override void Process(IasMap map) {
            if(map.FromComputerAccount) return;
            ProcessNext(map);
        }
    }

    public class CountIasMap : IasMapHandler {
        private int count;

        public CountIasMap(IasMapHandler next = null) : base(next) {
            count = 0;
        }

        public override void Process(IasMap map) {
            if(count % 1000 == 0) Log.Inform("count: " + count);
            count++;
            ProcessNext(map);
        }
    }

    public class ConvertIasMapToIasRecord : IasMapHandler {
        private readonly IasRecordHandler next_;
        private readonly IasMaps requests;

        public ConvertIasMapToIasRecord(IasRecordHandler _next) : base() {
            next_ = _next;
            requests = new IasMaps();
        }

        public override void Process(IasMap map) {
            if(map.IsUnsuccessful) {
                ProcessOther(map);
                return;
            }

            if(map.IsTypeAcceptRequest)     ProcessRequest(map);
            else if(map.IsTypeAccessAccept) ProcessAccept(map);
            else                            ProcessOther(map);
        }

        private void ProcessAccept(IasMap map) {
            var request = requests.RemoveAndReturn(map.Key);
            if(request == null) return;
            map.CopyMac(request);

            IasRecord record;
            try   { record = new IasRecord(map); }
            catch { return; }
            ProcessNext(record);
        }

        private void ProcessOther(IasMap map) {
            requests.LockAndRemove(map.Key);
        }

        private void ProcessRequest(IasMap map) {
            if(map.HasMacAddr) requests.AddOrReplace(map.Key, map);
        }

        protected void ProcessNext(IasRecord record) {
            next_.Process(record);
        }
    }

    public abstract class IasRecordHandler {
        protected readonly IasRecordHandler next;

        public IasRecordHandler(IasRecordHandler _next = null) {
            next = _next;
        }

        public abstract void Process(IasRecord record);

        protected virtual void ProcessNext(IasRecord record) {
            if(next != null) next.Process(record);
        }
    }
}
