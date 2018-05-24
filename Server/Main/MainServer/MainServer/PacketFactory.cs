using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    public class PacketFactory<T> where T : Packet, new()
    {
        private List<T> m_PacketList = new List<T>();
        public T CreatePacket()
        {
            for (int i = 0; i < m_PacketList.Count; i++)
            {
                if (!m_PacketList[i].IsInUse)
                {
                    m_PacketList[i].IsInUse = true;
                    return m_PacketList[i];
                }
            }
            T packet = new T();
            m_PacketList.Add(packet);
            return packet;
        }
    }
}
