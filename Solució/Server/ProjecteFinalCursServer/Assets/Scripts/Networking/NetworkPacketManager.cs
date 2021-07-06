using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class NetworkPacketManager<T> where T : class
{
    public event System.Action<byte[]> OnRequierePackageTransmit;

    private float m_SendSpeed = .2f;

    public float SendSpeed
    {
        get
        {
            if (m_SendSpeed < 0.1f)
                return m_SendSpeed = .1f;
            return m_SendSpeed;
        }
        set
        {
            m_SendSpeed = value;
        }
    }

    float nextTick;

    public void Tick()
    {
        nextTick += 1 / this.SendSpeed * Time.deltaTime;
        if(nextTick > 1 && Packages.Count > 0)
        {
            nextTick = 0;
            if (OnRequierePackageTransmit != null)
            {
                byte[] bytes = CreateBytes();
                Packages.Clear();
                OnRequierePackageTransmit(bytes);
            }
        }
    }

    public T GetNextDataReceived()
    {
        if (receivedPackages == null || receivedPackages.Count == 0)
            return default(T);
        return receivedPackages.Dequeue();
    }

    private byte[] CreateBytes()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            formatter.Serialize(ms, this.Packages);
            return ms.ToArray();
        }
    }
    private List<T> m_Packages;
    public List<T> Packages
    {
        get
        {
            if (m_Packages == null)
                m_Packages = new List<T>();
            return m_Packages;
        }
    }

    public Queue<T> receivedPackages;
    public void AddPackage(T package)
    {
        Packages.Add(package);
    }

    public void ReceiveData(byte[] bytes)
    {
        if (receivedPackages == null)
            receivedPackages = new Queue<T>();

        T[] packages = ReadBytes(bytes).ToArray();

        for(int i = 0; i < packages.Length; i++)
        {
            receivedPackages.Enqueue(packages[i]);
        }
    }

    public List<T> ReadBytes(byte[] bytes)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(bytes, 0, bytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (List<T>)formatter.Deserialize(ms);
        }
    }
}
