using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace MainServer
{
    class NetworkManager
    {
        public delegate void OnAcceptOneClientDelegate(ConnectInstance conn);
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
        }
        private List<ConnectInstance> m_ConnectList = new List<ConnectInstance>();
        private ArrayList m_Clients = new ArrayList();
        private Socket m_Listener = null;
        private ArrayList m_Listeners = new ArrayList();
        private int m_Port = 9999;
        private OnAcceptOneClientDelegate m_AcceptDelegate = null;
        public NetworkManager()
        {
            
        }

        public void Clear()
        {
            m_ConnectList.Clear();
            m_Clients.Clear();
            m_Listener = null;
            m_Listeners.Clear();
            m_Port = 9999;
            m_AcceptDelegate = null;
        }
        public bool Start(int port)
        {
            try
            {
                Clear();
                m_Port = port;

                IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostEntry.AddressList[0];
                m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Listener.NoDelay = true;
                m_Listener.LingerState = new LingerOption(false, 0);
                m_Listener.Blocking = false;
                m_Listener.Bind(new IPEndPoint(ipAddress, m_Port));
                m_Listener.Listen(10); /// for 10 clients at most one time.

                return true;
            }
            catch (Exception e)
            {
                LogModule.LogInfo("Start Tcp Listener failed on port: {0}, Message: {1}", m_Port, e.Message);
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
                    m_Listener.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Error);
                    ConnectInstance conn = new ConnectInstance(newClient);
                    m_ConnectList.Add(conn);
                    if (m_AcceptDelegate != null)
                    {
                        m_AcceptDelegate(conn);
                    }
                    IPEndPoint ip = (IPEndPoint)newClient.RemoteEndPoint;
                    LogModule.LogInfo("Accept one client, ip={0}, port={1}", ip.Address, ip.Port);
                }
            }
            catch(Exception e)
            {
                LogModule.LogInfo("Accept client error, Message:{0}", e.Message);
            }

            try
            {
                GetClientList(m_Clients);
                Socket.Select(null, null, m_Clients, 0);
                if (m_Clients.Count > 0)
                {
                }

                GetClientList(m_Clients);
                Socket.Select(m_Clients, null, null, 0);
                if (m_Clients.Count > 0)
                {
                }

                GetClientList(m_Clients);
                Socket.Select(null, m_Clients, null, 0);
                if (m_Clients.Count > 0)
                {
                }
            }
            catch (Exception e)
            {

                LogModule.LogInfo("Select error, Message:{0}", e.Message);
            }
        }

        public void Stop()
        {
            foreach(var connect in m_ConnectList)
            {
                connect.Shutdown();
            }
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

        private void GetClientList(ArrayList clients)
        {
            if (clients != null)
            {
                clients.Clear();
                for (int i = 0; i < m_ConnectList.Count; i++)
                {
                    clients.Add(m_ConnectList[i].Client);
                }
            }
        }
    }
}
