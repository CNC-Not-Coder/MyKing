using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using ProtoBuf;
using System.IO;

public class TestNetwork : MonoBehaviour {

	public string IP = "127.0.0.1";
	public int Port = 9999;
	public int connectInterval = 1000;
	private byte[] data = new byte[1024];
	private byte[] dataRecv = new byte[1024];
	private MemoryStream msRecv = null;
	public bool Disconnet = false;
	private int intervalLeft = 0;
	private TcpClient m_Connection = null;
	private IPEndPoint m_IpEndPoint = null;
	private ushort needRecvSize = 0;
	private ushort hasRecvSize = 0;
	// Use this for initialization
	void Start () {
		msRecv = new MemoryStream (dataRecv);
		NewConnection ();
	}

	void NewConnection()
	{
		m_Connection = new TcpClient ();
		m_Connection.NoDelay = true;
		IPAddress ip;
		if (IPAddress.TryParse (IP,out ip))
		{
			m_IpEndPoint = new IPEndPoint (ip, Port);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (m_Connection == null)
			return;
		intervalLeft -= (int)(Time.deltaTime * 1000);

		if (!m_Connection.Connected)
		{
			if (intervalLeft <= 0 && m_IpEndPoint != null)
			{
				Debug.Log ("Connect to server....");
				NewConnection ();
				m_Connection.Connect (m_IpEndPoint);
				if (m_Connection.Connected)
				{
					Debug.Log ("Connect succeed!");
				}
				intervalLeft = connectInterval;
			}
		}
		else
		{
			NetworkStream stream = m_Connection.GetStream ();
			if (stream.DataAvailable)
			{
				int recvSize = stream.Read (dataRecv, 0, dataRecv.Length);
				Debug.Log ("RecvSize = " + recvSize);
				needRecvSize = BitConverter.ToUInt16 (dataRecv, 0);
				hasRecvSize += recvSize;
				if (hasRecvSize >= needRecvSize) 
				{
					
				}
				msRecv.Seek (recvSize + msRecv.Position, SeekOrigin.Begin);
				msRecv.SetLength (msRecv.Position);
				if (msRecv.Length >= 4)
				{
					ushort len = BitConverter.ToUInt16 (data, 0);
					if (msRecv.Length > len)
					{
						
					}
				}
			}

			if (intervalLeft <= 0)
			{
				try
				{
					
					MemoryStream ms = new MemoryStream(data, 4, data.Length - 4);
					MessageDefine.Person person = new MessageDefine.Person();
					person.Id = 1111;
					person.Name = "mordy";
					person.Address = new MessageDefine.Address();
					person.Address.Line1 = "qqqqqqqqqqq";
					person.Address.Line2 = "wwwwwwwwww";
					Serializer.Serialize(ms, person);

					ushort size = (ushort)ms.Position;
					size += 4;
					byte[] sizeBytes = BitConverter.GetBytes(size);
					data[0] = sizeBytes[0];
					data[1] = sizeBytes[1];
					data[2] = 1;
					data[3] = 0;

					stream.Write (data, 0, size);

					intervalLeft = connectInterval;
					Debug.Log ("Send data to server");
					stream.Flush ();
				}
				catch(Exception ee)
				{
					SocketException e = ee as SocketException;
					if (e != null)
					{
						Debug.Log(string.Format("ErrorCode : {0} , Message : {1}", e.SocketErrorCode, e.Message));
						m_Connection.Close ();
					}
				}
			}
		}
		if (Disconnet)
		{
			Disconnet = false;
			m_Connection.Close ();
		}
	}

	void Destroy()
	{
		if (m_Connection != null)
		{
			m_Connection.Close ();
			Debug.Log ("Connetion Closed!");
		}
	}
}
