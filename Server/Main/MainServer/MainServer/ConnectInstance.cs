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
        public volatile int HeadIndex = 0;
        public volatile int TailIndex = 0;
        public byte[] ByteStream = new byte[1024 * 8];
        public void Reset()
        {
            HeadIndex = 0;
            TailIndex = 0;
        }
    }
    class ConnectInstance
    {
        private Socket m_Client = null;
        private CycleStream m_InputStream = new CycleStream();
        private CycleStream m_OutputStream = new CycleStream();
        public Socket Client
        {
            get { return m_Client; }
        }
        public ConnectInstance(Socket client)
        {
            //走到这里并不代表Socket已经连接成功，因为是NoBlocking的
            //第一次SelectWrite为true时才表示Succeed
            m_Client = client;
            m_Client.NoDelay = true;
            m_Client.LingerState = new LingerOption(false, 0);
            m_Client.Blocking = false;
        }

        public void ProcessInput()
        {
           
        }

        public void ProcessOutput()
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
