using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    public class ConnectManager
    {
        private Dictionary<Socket, ConnectInstance> m_ConnectDict = new Dictionary<Socket, ConnectInstance>();
        private List<ConnectInstance> m_ConnectList = new List<ConnectInstance>();
        private List<Socket> m_Clients = new List<Socket>();

        public ConnectManager()
        {

        }

        public void OnNewConnectInstance(Socket client)
        {
            if (client.Connected)
            {
                ConnectInstance conn = NewConnectInstance();
                if (conn != null)
                {
                    conn.SetClient(client);
                    m_ConnectList.Add(conn);
                    m_ConnectDict.Add(client, conn);
                }
            }
        }
        
        public void ProcessError()
        {
            //select error
            GetClientList(m_Clients);
            Socket.Select(null, null, m_Clients, 0);
            if (m_Clients.Count > 0)
            {
                for (int i = 0; i < m_Clients.Count; i++)
                {
                    Socket client = m_Clients[i];
                    if (m_ConnectDict.ContainsKey(client))
                    {
                        ConnectInstance conn = m_ConnectDict[client];
                        if (conn != null)
                        {
                            conn.OnSelectError();
                            if (conn.Client == null)
                            {
                                m_ConnectDict.Remove(client);
                            }
                        }
                    }

                }
            }
        }

        public void ProcessInput()
        {
            //select read
            GetClientList(m_Clients);
            Socket.Select(m_Clients, null, null, 0);
            if (m_Clients.Count > 0)
            {
                for (int i = 0; i < m_Clients.Count; i++)
                {
                    Socket client = m_Clients[i];
                    if (m_ConnectDict.ContainsKey(client))
                    {
                        ConnectInstance conn = m_ConnectDict[client];
                        if (conn != null)
                        {
                            conn.OnSelectRead();
                            if (conn.Client == null)
                            {
                                m_ConnectDict.Remove(client);
                            }
                        }
                    }

                }
            }
        }

        public void ProcessOutput()
        {
            //select write
            GetClientList(m_Clients);
            Socket.Select(null, m_Clients, null, 0);
            if (m_Clients.Count > 0)
            {
                for (int i = 0; i < m_Clients.Count; i++)
                {
                    Socket client = m_Clients[i];
                    if (m_ConnectDict.ContainsKey(client))
                    {
                        ConnectInstance conn = m_ConnectDict[client];
                        if (conn != null)
                        {
                            conn.OnSelectWrite();
                            if (conn.Client == null)
                            {
                                m_ConnectDict.Remove(client);
                            }
                        }
                    }

                }
            }
        }

        public void Tick()
        {
            try
            {
                ProcessError();

                ProcessInput();

                ProcessOutput();
                
            }
            catch (Exception e)
            {

                LogModule.LogInfo("Select error, Message:{0}", e.Message);
            }

            m_Clients.Clear();
        }

        private ConnectInstance NewConnectInstance()
        {
            for (int i = 0; i < m_ConnectList.Count; i++)
            {
                if(m_ConnectList[i].Client == null)
                {
                    return m_ConnectList[i];
                }
            }

            return new ConnectInstance();
        }
        private void GetClientList(List<Socket> clients)
        {
            if (clients != null)
            {
                clients.Clear();
                for (int i = 0; i < m_ConnectList.Count; i++)
                {
                    if (m_ConnectList[i].Client != null)
                    {
                        clients.Add(m_ConnectList[i].Client);
                    }
                }
            }
        }
    }
}
