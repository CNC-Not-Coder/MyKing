using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;
using MessageDefine;
using MainServer.Packets;

namespace MainServer
{
    public abstract class Packet
    {
        public abstract ushort GetPacketId();
        public abstract Type GetDataType();
        public abstract object GetData();
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
        public Packet()
        { 
            m_isInUse = false;
        }

        public virtual void Handle(ConnectInstance conn, object data)
        {
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
        public override Type GetDataType()
        {
            return typeof(T);
        }
        public override ushort GetPacketId()
        {
            return 0;
        }
    }
    public class PacketTest : PacketBase<Person>
    {
        public override ushort GetPacketId()
        {
            return (ushort)PacketIdDefine.Test;
        }
        public override void Handle(ConnectInstance conn, object data)
        {
        }
    }
}
