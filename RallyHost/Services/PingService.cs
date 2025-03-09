using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace RallyHost.Services
{
    public class PingService
    {
        public static async Task<long?> Ping(string host)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(host);
                    if (reply.Status == IPStatus.Success)
                    {
                        return reply.RoundtripTime;
                    }

                    return -1;
                }
            }
            catch (Exception)
            {
                
            }
            return null;
        }
    }
}