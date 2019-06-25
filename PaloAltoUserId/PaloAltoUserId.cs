using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using org.aha_net.DSV;
using org.aha_net.FolderWatching;
using org.aha_net.Logging;
using org.aha_net.PaloAlto;
using org.aha_net.Records;
using org.aha_net.RepeatedTasks;
using org.aha_net.RegistryTables;

namespace org.aha_net.PaloAltoUserId
{
    public partial class PaloAltoUserId : ServiceBase
    {
        public static CancellationTokenSource cts;
        public static CancellationToken       token;

        public PaloAltoUserId(string[] args)
        {
            InitializeComponent();

            cts   = new CancellationTokenSource();
            token = cts.Token;
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus); 
            
            (new Thread(new ThreadStart(ThreadInitialize))).Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private void ThreadInitialize()
        {
            var configKey = RegistryPath.Open(@"HKEY_LOCAL_MACHINE\SOFTWARE\AHA-NET\PaloAltoUserId");

            var log_path = (string) configKey.GetValue("log_path");
            var log      = new LogTagged(new LogFileWeekly(log_path, true), 1);
            log.TagWithDateTime = true;
            Log.Replace(log);

            Log.Inform(">>>>> STARTING <<<<<");

            var firewall_address  = (string) configKey.GetValue("firewall_address");
            var firewall_username = (string) configKey.GetValue("firewall_username");
            var firewall_api_key  = (string) configKey.GetValue("firewall_api_key");
            MapUserIp.New(new PanXmlApi(firewall_address, firewall_username, firewall_api_key));

            var dhcpFileFilter        = new FileFilterRecent(new FileFilterChanged());
            var dhcpWatcher           = new FolderWatchers(dhcpFileFilter);
            var dhcp_logfile_wildcard = (string) configKey.GetValue("dhcp_logfile_wildcard");
            foreach(var dhcp_log_paths in ((string) configKey.GetValue("dhcp_log_paths")).Split(';')) {
                dhcpWatcher.AddWatcher(dhcp_log_paths, dhcp_logfile_wildcard);
            }
            var dhcpHandler     = AssembleDhcpRecordHandler(new DsvLineConfig());
            var dhcpFileManager = new FileFollowerManager(dhcpHandler, token);
            var dhcpLogs        = new FolderWatcherManager(dhcpWatcher, dhcpFileManager, token);

            var iasFileFilter        = new FileFilterRecent(new FileFilterChanged());
            var iasWatcher           = new FolderWatchers(iasFileFilter);
            var ias_logfile_wildcard = (string) configKey.GetValue("ias_logfile_wildcard");
            foreach(var ias_log_paths in ((string) configKey.GetValue("ias_log_paths")).Split(';')) {
                iasWatcher.AddWatcher(ias_log_paths, ias_logfile_wildcard);
            }
            var iasHandler     = AssembleIasRecordHandler(new DsvLineConfig());
            var iasFileManager = new FileFollowerManager(iasHandler, token);
            var iasLogs        = new FolderWatcherManager(iasWatcher, iasFileManager, token);

            var dhcpTask = dhcpLogs.ProcessAsync();
            var IasTask  = iasLogs.ProcessAsync();

            Func<bool> checkIfAllOldLogsRead = () => {
                return ! (dhcpFileManager.AreAllFollowing && iasFileManager.AreAllFollowing);
            };
            Func<bool> wait15msecs = () => {
                const int _15_msecs = 15;
                Task      taskDelay = null;
                try     { taskDelay = Task.Delay(_15_msecs, token); taskDelay.Wait(); return true; }
                catch   { return false; }
                finally { if(taskDelay != null) taskDelay.Dispose(); }
            };
            var blockUntilAllOldLogsRead = new RepeatedTask(checkIfAllOldLogsRead, wait15msecs);
            blockUntilAllOldLogsRead.Process();

            if(token.IsCancellationRequested) return;

            Log.Inform(">>>>> FOLLOWING <<<<<");
            var updater = PaloAltoUserIdUpdater.Instance;
            updater.LoginCached();

            if(token.IsCancellationRequested) return;

            Func<bool> maintainDatabases = () => {
                Log.Inform(">>>>> MAINTAIN DATABASES <<<<<");
                updater.RemoveStaleEntries();
                return true;
            };
            Func<bool> wait1day = () => {
                TimeSpan _1_day    = TimeSpan.FromDays(1);
                Task     taskDelay = null;
                try     { taskDelay = Task.Delay(_1_day, token); taskDelay.Wait(); return true; }
                catch   { return false; }
                finally { if(taskDelay != null) taskDelay.Dispose(); }
            };
            var maintainDatabasesEveryDay = new RepeatedTask(maintainDatabases, wait1day);
            maintainDatabasesEveryDay.Process();
        }

        private DsvLineHandler AssembleDhcpRecordHandler(DsvLineConfig config) {
            var updater    = new UpdateDhcpRecord();
            var filterJunk = new FilterDhcpRecordUnwanted(updater);
            var converter  = new ConvertDsvRecordToDhcpRecord(filterJunk);
            var splitter   = new ConvertDsvLineToDsvRecord(config, converter);
            var filterBad  = new FilterDsvLineInvalidDhcpLogEntries(config, splitter);
            return filterBad;
        }

        private DsvLineHandler AssembleIasRecordHandler(DsvLineConfig config) {
            var updater   = new UpdateIasRecord();
            var converter = new ConvertIasMapToIasRecord(updater);
            var filter    = new FilterIasMapFromComputerAccount(converter);
            var mapper    = new ConvertDsvRecordToIasMap(filter);
            var splitter  = new ConvertDsvLineToDsvRecord(config, mapper);
            return splitter;
        }

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            Log.Inform(">>>>> EXITING <<<<<");

            if (cts != null) cts.Cancel();

            Log.RemoveAll();

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };
    }
}
