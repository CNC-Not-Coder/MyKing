using MessageDefine;
using MyNetwork.Packets;
using System;
using System.Collections.Generic;

namespace MyNetwork
{
    public partial class ConnectManager
    {
        private Dictionary<PacketIdDefine, IPacketFactory> m_Packets = new Dictionary<PacketIdDefine, IPacketFactory>();
        private Dictionary<PacketIdDefine, IPacketHandler> m_Handlers = new Dictionary<PacketIdDefine, IPacketHandler>();
        private void Regist<T1, T2>(PacketIdDefine id) where T1 : new() where T2 : IPacketHandler, new()
        {
            PacketIdDefine key = id;
            if (m_Packets.ContainsKey(key))
            {
                LogModule.LogError("Already contains key : {0}", key);
                return;
            }
            var factory = new PacketFactory<T1>(id);
            m_Packets.Add(key, factory);
            var handler = new T2();
            m_Handlers.Add(key, handler);
        }
        public void RegisterPackets()
        {
            Regist<Person, PacketTestHandler>(PacketIdDefine.Test);
        }

       
        public T CreatePacket<T>(PacketIdDefine id) where T : Packet, new()
        {
            PacketIdDefine key = id;
            if (m_Packets.ContainsKey(key))
            {
                IPacketFactory factory = m_Packets[key];
                return factory.CreatePacket() as T;
            }
            LogModule.LogInfo("Can't find packet factory for type : {0}", key);
            return new T();
        }
        public IPacketHandler GetPacketHandler(PacketIdDefine id)
        {
            if (m_Handlers.ContainsKey(id))
            {
                IPacketHandler handler = m_Handlers[id];
                return handler;
            }
            LogModule.LogInfo("Can't find packet handler for id : {0}", id);
            return null;
        }
        public IPacketFactory GetPacketFactory(PacketIdDefine id)
        {
            if (m_Packets.ContainsKey(id))
            {
                return m_Packets[id];
            }
            LogModule.LogInfo("Can't find packet factory for id : {0}", id);
            return null;
        }
    }
}
