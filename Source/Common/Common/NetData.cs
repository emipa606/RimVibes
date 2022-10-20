using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Common;

public class NetData
{
    private static readonly ConcurrentQueue<NetData> pool = new ConcurrentQueue<NetData>();

    private readonly List<byte> rawData = new List<byte>();

    private byte[] arrayForm;

    private int readIndex;

    private NetData()
    {
    }

    public static bool UsePooling { get; set; } = false;


    public int DataLength => rawData.Count;

    private byte[] RawDataArray
    {
        get
        {
            if (arrayForm == null || arrayForm.Length != rawData.Count)
            {
                arrayForm = rawData.ToArray();
            }

            return arrayForm;
        }
    }

    public static NetData Create(byte[] data = null)
    {
        if (!UsePooling)
        {
            var netData = new NetData();
            if (data != null)
            {
                netData.rawData.AddRange(data);
            }

            return netData;
        }

        if (!pool.TryDequeue(out var result))
        {
            var netData2 = new NetData();
            if (data != null)
            {
                netData2.rawData.AddRange(data);
            }

            return netData2;
        }

        result.Reset();
        if (data != null)
        {
            result.rawData.AddRange(data);
        }

        return result;
    }

    public static void Recycle(NetData data)
    {
        if (data != null && UsePooling)
        {
            pool.Enqueue(data);
        }
    }

    private bool ReadCheck(int length)
    {
        if (length <= 0)
        {
            return false;
        }

        return readIndex + length <= rawData.Count;
    }

    public NetData Write(bool b)
    {
        rawData.Add((byte)(b ? 1 : 0));
        return this;
    }

    public NetData Write(byte b)
    {
        rawData.Add(b);
        return this;
    }

    public NetData Write(float f)
    {
        rawData.AddRange(BitConverter.GetBytes(f));
        return this;
    }

    public NetData Write(int x)
    {
        rawData.AddRange(BitConverter.GetBytes(x));
        return this;
    }

    public NetData Write(ushort us)
    {
        rawData.AddRange(BitConverter.GetBytes(us));
        return this;
    }

    public NetData Write(string s)
    {
        if (s == null)
        {
            Write(0);
            return this;
        }

        var bytes = Encoding.UTF8.GetBytes(s);
        Write(bytes.Length);
        rawData.AddRange(bytes);
        return this;
    }

    public NetData Write(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Write(0);
        }
        else
        {
            Write(bytes.Length);
        }

        if (bytes != null)
        {
            rawData.AddRange(bytes);
        }

        return this;
    }

    public NetData Write(IReadOnlyList<string> lines)
    {
        if (lines == null)
        {
            throw new ArgumentNullException(nameof(lines));
        }

        var count = lines.Count;
        Write(count);
        foreach (var line in lines)
        {
            Write(line);
        }

        return this;
    }

    public bool ReadBoolean()
    {
        if (!ReadCheck(1))
        {
            return false;
        }

        readIndex++;
        return rawData[readIndex - 1] == 1;
    }

    public byte ReadByte()
    {
        if (!ReadCheck(1))
        {
            return 0;
        }

        readIndex++;
        return rawData[readIndex - 1];
    }

    public float ReadFloat()
    {
        if (!ReadCheck(4))
        {
            return 0f;
        }

        readIndex += 4;
        return BitConverter.ToSingle(RawDataArray, readIndex - 4);
    }

    public int ReadInt32()
    {
        if (!ReadCheck(4))
        {
            return 0;
        }

        readIndex += 4;
        return BitConverter.ToInt32(RawDataArray, readIndex - 4);
    }

    public ushort ReadUInt16()
    {
        if (!ReadCheck(2))
        {
            return 0;
        }

        readIndex += 2;
        return BitConverter.ToUInt16(RawDataArray, readIndex - 2);
    }

    public string ReadString()
    {
        var num = ReadInt32();
        if (!ReadCheck(num))
        {
            return null;
        }

        readIndex += num;
        return Encoding.UTF8.GetString(RawDataArray, readIndex - num, num);
    }

    public byte[] ReadBytes()
    {
        if (!ReadCheck(4))
        {
            return null;
        }

        var num = ReadInt32();
        if (num == 0)
        {
            return null;
        }

        var array = new byte[num];
        Array.Copy(RawDataArray, readIndex, array, 0, num);
        readIndex += num;
        return array;
    }

    public string[] ReadStringArray()
    {
        var num = ReadInt32();
        var array = new string[num];
        for (var i = 0; i < num; i++)
        {
            array[i] = ReadString();
        }

        return array;
    }

    public byte[] ToArray()
    {
        return rawData.ToArray();
    }

    private void Reset()
    {
        rawData.Clear();
        readIndex = 0;
    }
}