using System;

interface IRelay
{
    void Init(string host, int ip, int pkgSize, int splitCount, Action<byte[]> loopAction);

    void Update();

    void Send(byte[] data);

    int GetId();
}