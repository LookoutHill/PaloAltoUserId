using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using org.aha_net.Logging;

namespace org.aha_net.PaloAlto {
    public class MapUserIpSimple {
        private volatile static MapUserIpSimple singleton = null;

        public static void New(PanXmlApi pa) {
            singleton = new MapUserIpSimple(pa);
        }

        public static MapUserIpSimple Instance {
            get {
                return singleton;
            }
        }

        private PanXmlApi pa;

        private MapUserIpSimple(PanXmlApi _pa) {
            pa = _pa;
        }

        private static readonly string uidHeader = "<uid-message><type>update</type><payload>\r\n";
        private static readonly string uidTrailer = "</payload></uid-message>";
        private static readonly string loginHeader = "<login>\r\n";
        private static readonly string loginTrailer = "</login>\r\n";
        private static readonly string logoutHeader = "<logout>\r\n";
        private static readonly string logoutTrailer = "</logout>\r\n";

        private static readonly string loginEntryFormat = @"<entry name=""{0}"" ip=""{1}"" timeout=""{2}"" ></entry>" + Environment.NewLine;
        public void AddLogin(string logon, string ip, int timeout) {
            var cmd = uidHeader + loginHeader + string.Format(loginEntryFormat, logon, ip, timeout) + loginTrailer + uidTrailer;
            (new Thread(() => Send(cmd))).Start();
        }

        private static readonly string logoutEntryFormat = @"<entry ip=""{0}"" ></entry>" + Environment.NewLine;
        public void AddLogout(string logon, string ip) {
            var cmd = uidHeader + logoutHeader + string.Format(logoutEntryFormat, ip) + logoutTrailer + uidTrailer;
            (new Thread(() => Send(cmd))).Start();
        }

        private void Send(string cmd) {
            var sessionIdBase = Guid.NewGuid().ToString("N").Substring(0, 6) + "-";
            int retries = 3;
            int duration = 1000;
            while(retries > 0) {
                var sessionId = sessionIdBase + (3-retries);
                retries--;
                Log.Inform(string.Format("UID: {0}: {1}", sessionId, cmd));

                try {
                    pa.user_id(cmd);
                    Log.Inform(string.Format("UID: {0}: ok", sessionId));
                    return;
                } catch(Exception ex) {
                    Log.Inform("UID EXCEPTION: " + ex.ToString());
                    Log.Inform(string.Format("UID: {0}: fail", sessionId));
                    using(var taskDelay = Task.Delay(duration)) { taskDelay.Wait(); }
                    duration *= 2;
                    pa = pa.Reconnect();
                } finally {
                    Log.Flush();
                }
            }
            Log.Inform(string.Format("UID: {0}2: ABORT", sessionIdBase));
        }
    }

    public class MapUserIp {
        private volatile static MapUserIp singleton = null;

        public static void New(PanXmlApi pa) {
            singleton = new MapUserIp(pa);
        }

        public static MapUserIp Instance {
            get {
                return singleton;
            }
        }

        private PanXmlApi pa;
        private int byteCount;
        private string loginEntries;
        private string logoutEntries;
        private readonly Object entriesMutex = new Object();

        private MapUserIp(PanXmlApi _pa) {
            pa = _pa;
            Reset();
        }

        private static readonly string loginEntryFormat = @"<entry name=""{0}"" ip=""{1}"" timeout=""{2}"" ></entry>" + Environment.NewLine;
        public void AddLogin(string logon, string ip, int timeout) {
            var entry = string.Format(loginEntryFormat, logon, ip, timeout);
            string cmd = null;
            lock(entriesMutex) {
                if(byteCount == minByteCount) (new Thread(() => WaitPrepAndSend())).Start();
                if(byteCount + entry.Length >= 1900) cmd = Prep();
                byteCount += entry.Length;
                loginEntries += entry;
                if(byteCount >= 1900) cmd = Prep();
            }
            if(cmd != null) (new Thread(() => Send(cmd))).Start();
        }

        private static readonly string logoutEntryFormat = @"<entry ip=""{0}"" ></entry>" + Environment.NewLine;
        public void AddLogout(string logon, string ip) {
            var entry = string.Format(logoutEntryFormat, ip);
            string cmd = null;
            lock(entriesMutex) {
                if(byteCount == minByteCount) (new Thread(() => WaitPrepAndSend())).Start();
                if(byteCount + entry.Length >= 1900) cmd = Prep();
                byteCount += entry.Length;
                logoutEntries += entry;
                if(byteCount >= 1900) cmd = Prep();
            }
            if(cmd != null) (new Thread(() => Send(cmd))).Start();
        }

        private void WaitPrepAndSend() {
            using(var taskDelay = Task.Delay(100)) { taskDelay.Wait(); }
            string cmd = null;
            lock(entriesMutex) if(byteCount > minByteCount) cmd = Prep();
            if(cmd != null) Send(cmd);
        }

        private static readonly string uidHeader = "<uid-message><type>update</type><payload>\r\n";
        private static readonly string uidTrailer = "</payload></uid-message>";
        private static readonly string loginHeader = "<login>\r\n";
        private static readonly string loginTrailer = "</login>\r\n";
        private static readonly string logoutHeader = "<logout>\r\n";
        private static readonly string logoutTrailer = "</logout>\r\n";

        private string Prep() {
            var cmdLogin = "";
            var cmdLogout = "";
            if (loginEntries.Length > 0) cmdLogin = loginHeader + loginEntries + loginTrailer;
            if (logoutEntries.Length > 0) cmdLogout = logoutHeader + logoutEntries + logoutTrailer;
            Reset();
            return uidHeader + cmdLogin + cmdLogout + uidTrailer;
        }

        private static readonly int minByteCount = uidHeader.Length + uidTrailer.Length + loginHeader.Length + loginTrailer.Length + logoutHeader.Length + logoutTrailer.Length;
        private void Reset() {
            lock(entriesMutex) {
                loginEntries = "";
                logoutEntries = "";
                byteCount = minByteCount;
            }
        }

        private void Send(string cmd) {
            var sessionIdBase = Guid.NewGuid().ToString("N").Substring(0, 6) + "-";
            var retries = 3;
            const int duration = 1000;
            while(retries > 0) {
                var sessionId = sessionIdBase + (3-retries);
                retries--;
                Log.Inform(string.Format("UID: {0}: {1}", sessionId, cmd));

                try {
                    pa.user_id(cmd);
                    Log.Inform(string.Format("UID: {0}: ok", sessionId));
                    return;
                } catch(Exception ex) {
                    Log.Inform("PanXmlApi Exception: " + ex.ToString());
                    Log.Inform(string.Format("UID: {0}: fail", sessionId));
                    using(var taskDelay = Task.Delay(duration)) { taskDelay.Wait(); }
                    Reconnect();
                } finally {
                    Log.Flush();
                }
            }
            Log.Inform(string.Format("UID: {0}2: ABORT", sessionIdBase));
        }

        private void Reconnect() {
            try {
                pa = pa.Reconnect();
            } catch(Exception ex) {
                Log.Inform("PanXmlApi Exception: " + ex.ToString());
            } finally {
                Log.Flush();
            }
        }
    }

    public class PanXmlApi {
        private readonly string address;
        private readonly string username;
        private readonly string password;
        private readonly string key;
        private readonly WebClient client;

        public PanXmlApi(string _address, string _username, string _password) {
            address = _address;
            username = _username;
            password = _password;
            client = new WebClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
            key = keygen();
        }

        public PanXmlApi Reconnect() {
            return new PanXmlApi(address, username, password);
        }
        
        private string keygen() {
            PanXmlApiResponse response;
            lock(client) response = new PanXmlApiResponse(client.DownloadString(string.Format(@"https://{0}/api/?type=keygen&user={1}&password={2}", address, username, WebUtility.UrlEncode(password))));
            validateResponse(response);
            return response.Key;
        }

        public void user_id(string cmd) {
            PanXmlApiResponse response;
            lock(client) response = new PanXmlApiResponse(client.DownloadString(string.Format(@"https://{0}/api/?type=user-id&key={1}&cmd={2}", address, key, WebUtility.UrlEncode(cmd))));
            validateResponse(response);
        }

        private void validateResponse(PanXmlApiResponse response) {
            var status = response.Status;
            if(status == null)                throw new PanXmlApiException("null", "Invalid response received. Raw response data: " + response);
            else if(status.Equals("success")) return;
            else if(status.Equals("error"))   throw new PanXmlApiException(response.Code, response.Msg);
            else                              throw new PanXmlApiException(response.Status, "Unknown response status (" + response.Status + ") received. Raw response data: " + response);
        }
    }

    public class PanXmlApiResponse {
        public PanXmlApiResponse(string _response) {
            var xr = new XmlDocument();
            xr.LoadXml(_response);
            response = xr.FirstChild;
        }

        private readonly XmlNode response;

        public string Status {
            get {
                return GetAttribute(response, "status");
            }
        }

        public string Code {
            get {
                return GetAttribute(response, "code");
            }
        }

        public XmlNode Result {
            get {
                return GetNode(response, "result");
            }
        }

        public string Key {
            get {
                return GetElement(Result, "key");
            }
        }

        public string Msg {
            get {
                return GetElement(Result, "msg");
            }
        }

        private XmlNode GetNode(XmlNode node, string name) {
            try { return node[name]; } catch { return null; }
        }

        private string GetElement(XmlNode node, string name) {
            try { return node[name].InnerText; } catch { return null; }
        }

        private string GetAttribute(XmlNode node, string name) {
            try { return node.Attributes[name].InnerText; } catch { return null; }
        }

        public override string ToString() {
            return response.OuterXml;
        }
    }

    public class PanXmlApiException : InvalidOperationException {
        public PanXmlApiException(string code, string msg) {
            Code = code;
            Msg = msg;
        }

        public readonly string Code;
        public readonly string Msg;
    }
}
