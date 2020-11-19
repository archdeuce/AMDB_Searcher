using System.Net.NetworkInformation;
using System.Windows.Controls;

namespace AMDB_Searcher.Model.Connections
{
    public class Server
    {
        public string Address { get; set; }
        public bool IsAvailable { get; private set; }

        public Server(string serverAddress)
        {
            this.Address = serverAddress;
            this.IsAvailable = this.CheckAvailability();
        }

        private bool CheckAvailability()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(this.Address);

                if (pingReply.Status == IPStatus.Success)
                    return true;
            }
            catch (PingException) { }

            return false;
        }
    }
}
