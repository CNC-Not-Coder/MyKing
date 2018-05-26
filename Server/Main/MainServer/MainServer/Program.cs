using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessageDefine;
using ProtoBuf;
using MyNetwork;

namespace MainServer
{
    class Program
    {
        ConnectManager m_ConnectManager;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Server !");

            //byte[] test = new byte[10];
            //test[0] = 5;
            //test[1] = 6;
            //test[2] = 7;

            //MemoryStream testStream = new MemoryStream(test);
            //testStream.WriteByte(1);
            //testStream.WriteByte(2);
            //testStream.WriteByte(3);
            //testStream.WriteByte(4);
            //test[0] = 5;
            //testStream.Seek(1, SeekOrigin.Begin);
            //testStream.WriteByte(7);
            //testStream.Seek(13, SeekOrigin.Begin);
            //testStream.WriteByte(8);

            //Person person = new Person();
            //person.Id = 1000;
            //person.Name = "mordy";
            //person.Address = new Address();
            //person.Address.Line1 = "shijingshan";
            //person.Address.Line2 = "beijing";
            //object p = person;
            //MemoryStream ms = new MemoryStream();
            //Serializer.Serialize(ms, p);

            //ms.Seek(5, SeekOrigin.Begin);
            //Serializer.Serialize(ms, p);

            //ms.Seek(5, SeekOrigin.Begin);
            //object pp = Serializer.Deserialize(typeof(Person), ms);
            //Person person1 = pp as Person;

            //ms.Seek(2, SeekOrigin.Begin);
            //pp = Serializer.Deserialize(typeof(Person), ms);
            //Person person2 = pp as Person;

            ushort a = 65535;
            short b = (short)a;

            Console.ReadLine();
        }
    }
}
