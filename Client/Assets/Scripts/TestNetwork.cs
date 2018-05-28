using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class TestNetwork : MonoBehaviour {

	public string IP = "127.0.0.1";
	public int Port = 9999;
	public int connectInterval = 1000;
	public byte[] data = new byte[4];
	public bool Disconnet = false;
	private int intervalLeft = 0;
	private TcpClient m_Connection = null;
	private IPEndPoint m_IpEndPoint = null;
	// Use this for initialization
	void Start () {
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
			if (intervalLeft <= 0)
			{
				try
				{
					NetworkStream stream = m_Connection.GetStream ();
					stream.Write (data, 0, data.Length);
					intervalLeft = connectInterval;
					Debug.Log ("Send data to server");
					stream.Flush ();
				}
				catch(Exception ee)
				{
					SocketException e = ee as SocketException;
					Debug.Log(string.Format("ErrorCode : {0} , Message : {1}", e.SocketErrorCode, e.Message));
					m_Connection.Close ();
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
		}
	}
}
