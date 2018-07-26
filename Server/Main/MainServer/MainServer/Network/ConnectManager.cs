using MessageDefine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MyNetwork
{
    public partial class ConnectManager
    {
        private Dictionary<Socket, ConnectInstance> m_ConnectDict = new Dictionary<Socket, ConnectInstance>();
        private List<ConnectInstance> m_ConnectList = new List<ConnectInstance>();
        private List<Socket> m_Clients = null;
        private List<Socket> m_ErrorCheck = new List<Socket>();
        private List<Socket> m_ReadCheck = new List<Socket>();
        private List<Socket> m_WriteCheck = new List<Socket>();
        public ConnectManager()
        {
            RegisterPackets();
        }

        public void OnNewConnectInstance(Socket client)
        {
            if (client.Connected)
            {
                ConnectInstance conn = NewConnectInstance();
                if (conn != null)
                {
                    conn.SetClient(client);
                    m_ConnectDict.Add(client, conn);
                }
            }
        }

        public void ClearInstance()
        {
            for (int i = 0; i < m_ConnectList.Count; i++)
            {
                if (m_ConnectList[i].Client != null)
                {
                    m_ConnectList[i].Client.Close();
                }
            }
            m_ConnectList.Clear();
        }
        
        public void Select()
        {
            try
            {
                bool hasData = GetClientList(m_ErrorCheck);
                hasData = hasData && GetClientList(m_ReadCheck);
                hasData = hasData && GetClientList(m_WriteCheck);
                if (hasData)
                {
                    Socket.Select(m_ReadCheck, m_WriteCheck, m_ErrorCheck, 0);
                }
            }
            catch(Exception e)
            {
                LogModule.LogInfo(string.Format("Select call error, meassage : {0}", e.Message));
            }
        }

        public void ProcessError()
        {
            //select error
            m_Clients = m_ErrorCheck;
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
            m_Clients = m_ReadCheck;
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
            m_Clients = m_WriteCheck;
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

        public void ProcessCommands()
        {
            for (int i = 0; i < m_ConnectList.Count; i++)
            {
                if (m_ConnectList[i].Client != null)
                {
                    m_ConnectList[i].ProcessCommands(this);
                }
            }
        }

        public void Tick()
        {
            try
            {
                var packet = CreatePacket<Person>(Packets.PacketIdDefine.Test);
                Person p = packet.GetData() as Person;
                p.Id = 222222;
                p.Name = "mordyyyy";
                p.Address = new Address();
                p.Address.Line1 = "ddddd";
                p.Address.Line2 = "ttttt";

                for (int i = 0; i < m_ConnectList.Count; i++)
                {
                    if (m_ConnectList[i].Client != null)
                    {
                        m_ConnectList[i].SendPacket(packet);
                    }
                    
                }

            }
            catch (Exception e)
            {

                LogModule.LogInfo("Select error, Message:{0}", e.Message);
            }
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
            ConnectInstance conn = new ConnectInstance();
            m_ConnectList.Add(conn);
            return conn;
        }
        private bool GetClientList(List<Socket> clients)
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
                return clients.Count > 0;
            }
            return false;
        }
    }
}
