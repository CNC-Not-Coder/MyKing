using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    public class ConnectInstance
    {
        private Socket m_Client = null;
        private CycleStream m_InputStream = new CycleStream(1024 * 8);
        private CycleStream m_OutputStream = new CycleStream(1024 * 8);
        private byte[] m_temp = new byte[1024]; // 用于发送和接收的临时buffer
        public Socket Client
        {
            get { return m_Client; }
        }
        public ConnectInstance()
        {
        }
        public ConnectInstance(Socket client)
        {
            //走到这里并不代表Socket已经连接成功，因为是NoBlocking的
            //第一次SelectWrite为true时才表示Succeed
            SetClient(client);
        }
        public void SetClient(Socket client)
        {
            m_Client = client;
            if (m_Client != null)
            {
                m_Client.NoDelay = true;
                m_Client.LingerState = new LingerOption(false, 0);
                m_Client.Blocking = false;
            }
        }

        public void OnSelectError()
        {
            SocketError err = (SocketError)m_Client.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Error);

            if (err == SocketError.AlreadyInProgress ||
                err == SocketError.Success ||
                err == SocketError.WouldBlock ||
                err == SocketError.AlreadyInProgress ||
                err == SocketError.IsConnected ||
                err == SocketError.IOPending)
            {
                return;
            }

            LogModule.LogInfo("OnSelectError : {0}", err);

            Shutdown();
        }

        public void OnSelectRead()
        {
            try
            {
                if (m_InputStream.IsFull())
                {
                    return;
                }
                SocketError err = SocketError.Success;
                int offset = m_InputStream.GetWriteIndex();
                int size = Math.Min(m_InputStream.GetLeftSizeToWrite(), m_temp.Length);
                int recvSize = m_Client.Receive(m_temp, 0, size, SocketFlags.None, out err);
                int writeSize = m_InputStream.WriteBuffer(m_temp, recvSize);
                if (writeSize != recvSize)
                {
                    LogModule.LogInfo("Fatal Error, recvSize != writeSize, socket error : {0}", err);
                    Shutdown();
                }
            }
            catch (Exception e)
            {
                LogModule.LogInfo("OnSelectRead, exception message : {0}", e.Message);
            }
        }

        public void OnSelectWrite()
        {
            try
            {
                if (!m_OutputStream.HasData())
                {
                    return;
                }
                SocketError err = SocketError.Success;
                int offset = m_OutputStream.GetReadIndex();
                int size = m_OutputStream.ReadBuffer(m_temp);
                int sendSize = m_Client.Send(m_temp, 0, size, SocketFlags.None, out err);
                m_OutputStream.SetReadIndex(sendSize);
            }
            catch (Exception e)
            {
                LogModule.LogInfo("OnSelectWrite, exception message : {0}", e.Message);
            }
        }

        public void Shutdown()
        {
            if (m_Client != null)
            {
                m_Client.Close();
                m_Client = null;
            }
            Clear();
        }
        public void Clear()
        {
            m_Client = null;
            m_InputStream.Reset();
            m_OutputStream.Reset();
        }

        public void SendPacket(PacketBase packet)
        {

        }
    }
}
