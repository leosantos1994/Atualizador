using UpdaterService.Interfaces;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public static class APIResponseHandler
    {
        private static List<string> _log = new List<string>();
        static object _lock = new object();
        static bool _complete = false;

        public static bool HasMessage()
        {
            return _log.Any();
        }
        public static void Add(string msg)
        {
            _log.Append(msg);
        }

        public static void Add(string msg, bool complete)
        {
            _log.Append(msg);
            _complete = complete;
        }

        private static string Get()
        {
            if (_log.Count > 0)
            {
                string msg = _log[0];
                _log.Remove(msg);
                return msg;
            }
            return "";

        }

        public static void SendResponse(Guid serviceId, IConfigSettings config)
        {
            lock (_lock)
            {
                string response = Get();
                APIHandler.SendUpdateInformation(new ResponseModel() { Log = response, ServiceId = serviceId, Complete = _complete }, config);
            }
        }
    }
}
