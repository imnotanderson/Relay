using System;
using System.Collections.Generic;

public class Relay:IRelay
{
    int id = -1;
    RelaySocket socket;
    List<byte> recvData = new List<byte>();
    int actionSize = 0;
    Action<byte[]> loopAction;

    ~Relay()
    {
        if (socket != null)
        {
            socket.Destroy();
        }
    }
    
    public void Init(string host, int ip, int pkgSize, int splitCount, Action<byte[]> loopAction)
    {
        socket = new RelaySocket(host, ip);
        actionSize = pkgSize * splitCount;
    }

    public void Update() {
        recvData.AddRange(socket.RecvData());
        if (id < 0 && recvData.Count>0)
        {
            id = (int)recvData[0];
            recvData = recvData.GetRange(1, recvData.Count - 1);
        }
        if (recvData.Count >= actionSize)
        {
            loopAction(recvData.GetRange(0, actionSize).ToArray());
            recvData = recvData.GetRange(actionSize, recvData.Count - actionSize);
        }
    }

    public void Send(byte[] data){
        socket.SendData(data);
    }

    public int GetId() {
        return id;
    }
}
