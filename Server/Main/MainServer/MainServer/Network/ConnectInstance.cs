using ProtoBuf;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MyNetwork
{
    public class ConnectInstance
    {
        private Socket m_Client = null;
        private CycleStream m_InputStream = new CycleStream(256);
        private CycleStream m_OutputStream = new CycleStream(256);
        private byte[] m_temp = new byte[256]; // 用于发送和接收的临时buffer
        private MemoryStream m_tempStream = null;
        private byte[] m_tempSend = new byte[256]; // 用于发送和接收的临时buffer
        private MemoryStream m_tempStreamSend = null;
        public const ushort c_headSize = 4;
        public Socket Client
        {
            get { return m_Client; }
        }
        public ConnectInstance()
        {
            m_tempStream = new MemoryStream(m_temp, c_headSize, m_temp.Length - c_headSize);
            m_tempStreamSend = new MemoryStream(m_tempSend, c_headSize, m_tempSend.Length - c_headSize);
        }
        public ConnectInstance(Socket client)
        {
            m_tempStream = new MemoryStream(m_temp, c_headSize, m_temp.Length - c_headSize);
            m_tempStreamSend = new MemoryStream(m_tempSend, c_headSize, m_tempSend.Length - c_headSize);
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
                //客户端主动断开连接，也会是可读
                if (m_Client.Connected == false || m_Client.Available == 0)
                {
                    Shutdown();
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
                else
                {
                    IPEndPoint ip = m_Client.RemoteEndPoint as IPEndPoint;
                    LogModule.LogInfo("Recv data from client : {0}, port : {1}, size : {2}", ip.Address, ip.Port, recvSize);
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
                int size = m_OutputStream.TryReadBuffer(m_tempSend);
                int sendSize = m_Client.Send(m_tempSend, 0, size, SocketFlags.None, out err);
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
                IPEndPoint ip = m_Client.RemoteEndPoint as IPEndPoint;
                LogModule.LogInfo("On connection shut down, ip : {0} , port : {1}", ip.Address, ip.Port);
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

        public void SendPacket(Packet packet)
        {
            try
            {
                if (m_OutputStream.IsFull())
                {
                    return;
                }
                m_tempStreamSend.Seek(0, SeekOrigin.Begin);
                m_tempStreamSend.SetLength(0);
                Serializer.Serialize(m_tempStreamSend, packet.GetData());
                ushort size = (ushort)m_tempStreamSend.Position;
                size += c_headSize;
                GetBytes((short)size, m_tempSend, 0);
                GetBytes((short)packet.GetPacketId(), m_tempSend, 2);
                int leftSize = m_OutputStream.GetLeftSizeToWrite();
                if ( leftSize > size)
                {
                    m_OutputStream.WriteBuffer(m_tempSend, size);
                    LogModule.LogInfo("SendPacket, Packet id : {0}, size : {1}", packet.GetPacketId(), size);
                }
                else
                {
                    LogModule.LogInfo("SendPacket, Output stream is not enough, Left : {0}, Require : {1}", leftSize, size);
                }
                packet.IsInUse = false;
            }
            catch (Exception e)
            {
                LogModule.LogInfo("SendPacket, Error : {0}", e.Message);
            }
           
        }

        public void ProcessCommands(ConnectManager connMan)
        {
            // 一次最多执行10个
            for (int i = 0; i < 10; i++)
            {
                if (!m_InputStream.HasData())
                {
                    return;
                }
                int size = m_InputStream.TryReadBuffer(m_temp);
                //parse packet id and length
                if (size < 4)
                {
                    return;
                }
                ushort len = BitConverter.ToUInt16(m_temp, 0);
                ushort packetId = BitConverter.ToUInt16(m_temp, 2);
                if (size < len)
                {
                    //还没接收完或者包不完整
                    return;
                }
                m_InputStream.SetReadIndex(len);

                try
                {
                    // 调用handle
                    IPacketFactory factory = connMan.GetPacketFactory((Packets.PacketIdDefine)packetId);
                    if (factory != null)
                    {
                        m_tempStream.Seek(0, SeekOrigin.Begin);
                        m_tempStream.SetLength(len - c_headSize);
                        object data = Serializer.Deserialize(factory.GetDataType(), m_tempStream);//GC狂魔
                        IPacketHandler handler = connMan.GetPacketHandler((Packets.PacketIdDefine)packetId);
                        if (handler != null)
                        {
                            handler.Handle(this, data);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogModule.LogInfo("Handle packet error, Packet id : {0}, len : {1}, msg : {2}", packetId, len, e.Message);
                    Shutdown();
                    break;
                }
            }
        }
        public static unsafe void GetBytes(short value, byte[] buffer, int offset)
        {
            if (buffer != null && buffer.Length - offset >= 2)
            {
                fixed (byte* numRef = buffer)
                {
                    *((short*)numRef) = value;
                }
            }
        }
    }
}
