using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;


namespace MyNetwork
{
    class NetworkManager
    {
        public delegate void OnAcceptOneClientDelegate(Socket client);
        public int Port
        {
            get
            {
                return m_Port;
            }
        }
        public OnAcceptOneClientDelegate OnAcceptOneClient
        {
            get
            {
                return m_AcceptDelegate;
            }
            set
            {
                m_AcceptDelegate = value;
            }
        }

        private Socket m_Listener = null;
        private ArrayList m_Listeners = new ArrayList();
        private int m_Port = 9999;
        private OnAcceptOneClientDelegate m_AcceptDelegate = null;
        public NetworkManager()
        {
            
        }

        public void Clear()
        {
            m_Listener = null;
            m_Listeners.Clear();
            m_Port = 9999;
        }
        public bool Start(int port)
        {
            try
            {
                Clear();
                m_Port = port;

                IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = new IPAddress(new byte[] { 10, 12, 5, 162 });
                m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Listener.NoDelay = true;
                m_Listener.LingerState = new LingerOption(false, 0);
                m_Listener.Blocking = false;
                m_Listener.Bind(new IPEndPoint(ipAddress, m_Port));
                m_Listener.Listen(10); /// for 10 clients at most one time.

                LogModule.LogInfo("Start Tcp Listener succeed on port: {0}, ip: {1}", port, ipAddress.ToString());
                return true;
            }
            catch (SocketException e)
            {
                LogModule.LogInfo("Start Tcp Listener failed on port: {0}, Message: {1}", m_Port, e.Message);
            }
            catch (Exception e)
            {
                LogModule.LogInfo("Catch an exception , Message : {0}", e.Message);
            }
            return false;
        }

        public void Tick()
        {
            try
            {
                GetListenList(m_Listeners);
                Socket.Select(null, null, m_Listeners, 0);
                bool bError = m_Listeners.Count > 0;
                if (bError)
                {
                    return;
                }

                GetListenList(m_Listeners);
                Socket.Select(m_Listeners, null, null, 0);
                bool bCanRead = m_Listeners.Count > 0;
                if (!bCanRead)
                {
                    return;
                }

                Socket newClient = m_Listener.Accept();
                if (newClient != null)
                {
                    if (m_AcceptDelegate != null)
                    {
                        m_AcceptDelegate(newClient);
                    }
                    IPEndPoint ip = (IPEndPoint)newClient.RemoteEndPoint;
                    LogModule.LogInfo("Accept one client, ip={0}, port={1}", ip.Address, ip.Port);
                }
            }
            catch(Exception e)
            {
                LogModule.LogInfo("Accept client error, Message:{0}", e.Message);
            }

            
        }

        public void Stop()
        {
            m_Listener.Close();

            Clear();
        }

        private void GetListenList(ArrayList listeners)
        {
            if (listeners != null)
            {
                listeners.Clear();
                listeners.Add(m_Listener);
            }
        }
    }
}
