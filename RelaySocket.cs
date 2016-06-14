using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class RelaySocket
{
    public bool Connected
    {
        get
        {
            return socket != null && socket.Connected;
        }
    }
    object recvDataLockObj = new object();
    List<byte> recvData = new List<byte>();

    object sendDataLockObj = new object();
    List<byte> sendData = new List<byte>();

    Socket socket;
    Thread recvTh, sendTh;

    public RelaySocket(string host, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true;
        socket.BeginConnect(host, port, (obj) =>
        {
            if (socket.Connected)
            {
                Run();
            }
            socket.EndConnect(obj);
        }, socket);
    }

    void Run()
    {
        sendTh = new Thread(Send);
        recvTh = new Thread(Recv);
        sendTh.Start();
        recvTh.Start();
    }

    public void Destroy()
    {
        sendTh.Abort();
        recvTh.Abort();
    }

    void Send()
    {
        for (; ; )
        {
            Thread.Sleep(1);
            if (sendData.Count > 0)
            {
                byte[] data;
                lock (sendDataLockObj)
                {
                    data = sendData.ToArray();
                    sendData.Clear();
                }
                socket.Send(data);
            }
        }
    }

    void Recv()
    {
        for (; ; )
        {
            Thread.Sleep(1);
            if (socket.Available > 0)
            {
                byte[] data = new byte[128];
                int len = socket.Receive(data);
                lock (recvDataLockObj)
                {
                    for (int i = 0; i < len; i++)
                    {
                        recvData.Add(data[i]);
                    }
                }
            }
        }
    }

    public void SendData(byte[] data)
    {
        if (data.Length == 0) return;
        lock (sendDataLockObj)
        {
            sendData.AddRange(data);
        }
    }

    public byte[] RecvData()
    {
        if (recvData.Count == 0) return new byte[0];
        byte[] data;
        lock (recvDataLockObj)
        {
            data = recvData.ToArray();
            recvData.Clear();
        }
        return data;
    }

}