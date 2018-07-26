using System;
using MessageDefine;
using MyNetwork.Packets;

namespace MyNetwork
{
    public abstract class Packet
    {
        public abstract object GetData();
        public ushort GetPacketId()
        {
            return m_packetId;
        }
        public void SetPacketId(ushort val)
        {
            m_packetId = val;
        }
        // 是否正在使用
        public bool IsInUse
        {
            get
            {
                return m_isInUse;
            }
            set
            {
                m_isInUse = value;
            }
        } 
        private bool m_isInUse;
        private ushort m_packetId;
        public Packet()
        {
            m_isInUse = false;
            m_packetId = 0;
        }
    }

    public class PacketBase<T> : Packet where T : new()
    {
        private T m_data;
        public PacketBase()
        {
            m_data = new T();
        }
        public override object GetData()
        {
            return m_data;
        }
    }
    public interface IPacketHandler
    {
        void Handle(ConnectInstance conn, object data);
    }
    
    public class PacketTestHandler : IPacketHandler
    {
        public void Handle(ConnectInstance conn, object data)
        {
            Person p = data as Person;
            LogModule.LogInfo("PacketTestHandler handled!, {0}, {1}, {2}, {3}", p.Id, p.Name, p.Address.Line1, p.Address.Line2);
        }
    }
}
