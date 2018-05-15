using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    class CycleStream
    {
        public int HeadIndex = 0;
        public int TailIndex = 0;
        public byte[] ByteStream = null;
        public CycleStream(int size)
        {
            ByteStream = new byte[size];
            HeadIndex = 0;
            TailIndex = 0;
        }
        public void Reset()
        {
            HeadIndex = 0;
            TailIndex = 0;
        }
        public int GetSize()
        {
            return ByteStream.Length;
        }
        public bool IsFull()
        {
            if ((HeadIndex - TailIndex) == 1)
            {
                return true;
            }
            else if (TailIndex == GetSize()-1 && HeadIndex == 0)
            {
                return true;
            }
            return false;
        }
    }
    class ConnectInstance
    {
        private Socket m_Client = null;
        private CycleStream m_InputStream = new CycleStream(1024 * 8);
        private CycleStream m_OutputStream = new CycleStream(1024 * 8);
        public Socket Client
        {
            get { return m_Client; }
        }
        public ConnectInstance()
        {
            m_Client = null;
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

            m_Client.Close();
            m_Client = null;
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
                int offset = 0;
                int size = 0;
                if (m_InputStream.TailIndex < m_InputStream.HeadIndex)
                {//tail在head前面，从tail写到head - 1
                    offset = m_InputStream.TailIndex;
                    size = m_InputStream.HeadIndex - m_InputStream.TailIndex;
                }
                else
                {//tail在head前面或者重合
                    offset = m_InputStream.TailIndex;
                    
                }
                int recSize = m_Client.Receive(m_InputStream.ByteStream, offset, size, SocketFlags.None, out err);
                m_InputStream.HeadIndex += recSize;
                if (m_InputStream.HeadIndex == m_InputStream.GetSize() - 1)
                {
                    m_InputStream.HeadIndex = 0;//从0开始
                }
            }
            catch (Exception e)
            {
                LogModule.LogInfo("OnSelectRead, exception message : {0}", e.Message);
            }
        }

        public void OnSelectWrite()
        {

        }

        public void Shutdown()
        {
            if(m_Client != null)
            {
                m_Client.Close();
            }
            m_InputStream.Reset();
            m_OutputStream.Reset();
        }
    }
}
