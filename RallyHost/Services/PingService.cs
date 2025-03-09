using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace RallyHost.Services
{
    public class PingService
    {
        private readonly Ping _ping;

        public PingService(Ping ping)
        {
            _ping = ping;
        }

        public async Task<long?> Ping(string host)
        {
            try
            {
                PingReply reply = await _ping.SendPingAsync(host);
                if (reply.Status == IPStatus.Success)
                {
                    return reply.RoundtripTime;
                }
                return -1;
            }
            catch (Exception)
            {
                
            }
            return null;
        }
    }
}