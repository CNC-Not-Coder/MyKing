using MyNetwork.Packets;
using System;
using System.Collections.Generic;


namespace MyNetwork
{
    public interface IPacketFactory
    {
        object CreatePacket();
        Type GetDataType();
        PacketIdDefine GetPacketId();
    }
    public class PacketFactory<T> : IPacketFactory where T : new()
    {
        private List<PacketBase<T>> m_PacketList = new List<PacketBase<T>>();
        private PacketIdDefine m_PacketId = PacketIdDefine.Invalid;
        public PacketFactory(PacketIdDefine id)
        {
            m_PacketId = id;
        }
        public object CreatePacket()
        {
            for (int i = 0; i < m_PacketList.Count; i++)
            {
                if (!m_PacketList[i].IsInUse)
                {
                    m_PacketList[i].IsInUse = true;
                    return m_PacketList[i];
                }
            }
            PacketBase<T> packet = new PacketBase<T>();
            packet.SetPacketId((ushort)m_PacketId);
            m_PacketList.Add(packet);
            return packet;
        }

        public Type GetDataType()
        {
            return typeof(T);
        }
        public PacketIdDefine GetPacketId()
        {
            return m_PacketId;
        }
    }
}
