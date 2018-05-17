using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    public class ProtoBuffData
    {

    }
    public abstract class PacketBase<T> where T : ProtoBuffData, new()
    {
        public abstract int GetPacketId();
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
        private T m_dataObj;
        private bool m_isInUse;
        public PacketBase()
        {
            m_dataObj = new T();
            m_isInUse = false;
        }
        public void ProcessBuffer(byte[] buffer)
        {
            
        }

        public virtual void Handle(ConnectInstance conn, T data)
        {

        }
    }

    public class Packet : PacketBase<ProtoBuffData> 
    {
        public override int GetPacketId()
        {
            return -1;
        }

        public override void Handle(ConnectInstance conn, ProtoBuffData data)
        {
            
        }
    }

}
