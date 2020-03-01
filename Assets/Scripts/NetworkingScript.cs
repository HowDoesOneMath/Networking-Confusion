using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.SceneManagement;

enum PacketType
{
    OtherClientData,
    JoinRequest,
    Message,
    UpdatePlayerData,
    ID
}

public class Client
{
    public bool SKIPPED = false;
    public int ID = -1;
    public string USERNAME = "Nameless Potato";
    public int requestedPartners = 0;
    public int currentPartner = -1;
}

public class NetworkingScript
{
    public static int maxTexts = 30;
    public static List<string> texts;
    public static bool CONNECTION = false;

    public static List<Client> allClients;

    public static int MAX_USERS = 12;
    public static int MAX_PACKET_SIZE = 5000;
    public static int INITIAL_OFFSET = 20;
    public static int STAMP_OFFSET = 4;
    public static int OK_PACKET_STAMP = 123456789;
    public static int MY_ID;

    const string DLL_NAME = "NetDLL Assignment 1";

    [DllImport(DLL_NAME)]
    internal static extern int WSAGetErr();
    [DllImport(DLL_NAME)]
    internal static extern bool StartWINSOCK();
    [DllImport(DLL_NAME)]
    internal static extern bool ConnectToServer(string IP, string PORT);
    [DllImport(DLL_NAME)]
    internal static extern bool CleanUpWINSOCK();
    [DllImport(DLL_NAME)]
    internal static extern int CheckForData();
    [DllImport(DLL_NAME)]
    internal static extern IntPtr GetData();
    [DllImport(DLL_NAME)]
    internal static extern bool SendData(IntPtr buffer, int length);

    static byte[] sendBuffer;
    static byte[] receiveBuffer;

    public static void StartUp()
    {
        allClients = new List<Client>();
        texts = new List<string>();

        for (int i = 0; i < MAX_USERS; ++i)
        {
            allClients.Add(new Client());
        }

        sendBuffer = new byte[MAX_PACKET_SIZE];
        receiveBuffer = new byte[MAX_PACKET_SIZE];
        if (!StartWINSOCK())
        {
            Debug.Log("GU");
        }
    }

    public static bool JoinServer(string ip, string port)
    {
        if (ConnectToServer(ip, port))
        {
            CONNECTION = true;
            return true;
        }
        Debug.Log(WSAGetErr());

        return false;
    }

    public static void CloseUp()
    {
        CleanUpWINSOCK();
    }

    public static void ProcessPackets()
    {
        int test = CheckForData();
        while (test > 0)
        {
            Debug.Log("GOT PACKET");
            IntPtr data = GetData();
            Marshal.Copy(data, receiveBuffer, 0, test);

            SwitchPacket();

            test = CheckForData();
        }
    }

    static void SwitchPacket()
    {
        int loc = STAMP_OFFSET;

        int length = 0;
        int receivers = 0;
        int type = -1;
        int senders = -2;

        UnpackInt(ref receiveBuffer, ref loc, ref length);
        UnpackInt(ref receiveBuffer, ref loc, ref receivers);
        UnpackInt(ref receiveBuffer, ref loc, ref type);
        UnpackInt(ref receiveBuffer, ref loc, ref senders);

        switch((PacketType)type)
        {
            case PacketType.ID:
                UnpackInt(ref receiveBuffer, ref loc, ref MY_ID);
                allClients[MY_ID].USERNAME = OpeningManager.uname;
                SceneManager.LoadScene(1);
                SendPlayerData();
                break;
            case PacketType.Message:
                string newText = "";
                UnpackString(ref receiveBuffer, ref loc, ref newText);
                texts.Add(newText);
                if (texts.Count > maxTexts)
                {
                    texts.RemoveAt(0);
                }
                break;
            case PacketType.JoinRequest:
                break;
            case PacketType.OtherClientData:
                for (int i = 0; i < MAX_USERS; ++i)
                {
                    allClients[i].SKIPPED = true;
                }
                while (loc < length)
                {
                    int ID = -1;
                    string uname = "";
                    int requestedPart = 0;
                    int part = -1;
                    UnpackInt(ref receiveBuffer, ref loc, ref ID);
                    UnpackString(ref receiveBuffer, ref loc, ref uname);
                    UnpackInt(ref receiveBuffer, ref loc, ref requestedPart);
                    UnpackInt(ref receiveBuffer, ref loc, ref part);
                    allClients[ID].ID = ID;
                    allClients[ID].USERNAME = uname;
                    allClients[ID].requestedPartners = requestedPart;
                    allClients[ID].currentPartner = part;
                    allClients[ID].SKIPPED = false;
                }
                break;
            case PacketType.UpdatePlayerData:
                break;
        }
    }

    public static void SendPlayerData()
    {
        int loc = INITIAL_OFFSET;
        int receivers = (1 << 30);
        PackData(ref sendBuffer, ref loc, allClients[MY_ID].USERNAME);

        SendIntPtr(loc, receivers, (int)PacketType.UpdatePlayerData);
    }

    public static void SendMessage(string myMsg)
    {
        string msg = allClients[MY_ID].USERNAME + ": " + myMsg;
        int loc = INITIAL_OFFSET;
        int receivers = 0;
        receivers |= (1 << MY_ID);
        if (allClients[MY_ID].currentPartner >= 0)
            receivers |= (1 << allClients[MY_ID].currentPartner);

        PackData(ref sendBuffer, ref loc, msg);

        SendIntPtr(loc, receivers, (int)PacketType.Message);
    }

    public static void SendJoinRequest(int joinID)
    {
        int loc = INITIAL_OFFSET;
        int receivers = (1 << 31);
        PackData(ref sendBuffer, ref loc, joinID);

        SendIntPtr(loc, receivers, (int)PacketType.JoinRequest);
    }

    #region packingData
    public static void PackData(ref byte[] bytes, ref int loc, bool data)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, loc);
        loc += Marshal.SizeOf(data);
    }
    public static void PackData(ref byte[] bytes, ref int loc, int data)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, loc);
        loc += Marshal.SizeOf(data);
    }
    public static void PackData(ref byte[] bytes, ref int loc, float data)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, loc);
        loc += Marshal.SizeOf(data);
    }
    public static void PackData(ref byte[] bytes, ref int loc, char data)
    {
        //BitConverter.GetBytes(data).CopyTo(bytes, loc);
        bytes[loc] = (byte)data;
        loc += Marshal.SizeOf(data);
    }

    public static void PackData(ref byte[] bytes, ref int loc, string data)
    {
        PackData(ref bytes, ref loc, data.Length);

        for (int i = 0; i < data.Length; ++i)
        {
            PackData(ref bytes, ref loc, data[i]);
        }
    }

    static int InitialOffset = 16;

    static bool SendIntPtr(int length, int receiver, int packetType)
    {
        bool returnVal = false;

        BitConverter.GetBytes(OK_PACKET_STAMP).CopyTo(sendBuffer, 0);
        BitConverter.GetBytes(length).CopyTo(sendBuffer, 4);
        BitConverter.GetBytes(receiver).CopyTo(sendBuffer, 8);
        BitConverter.GetBytes(packetType).CopyTo(sendBuffer, 12);
        BitConverter.GetBytes(MY_ID).CopyTo(sendBuffer, 16);

        //SendDebugOutput("ID: " + playerID.ToString() + ", Type: " + packetType.ToString() + ", LENGTH: " + length.ToString());

        IntPtr ptr = Marshal.AllocCoTaskMem(length);

        Marshal.Copy(sendBuffer, 0, ptr, length);

        //SendDataFunc

        //SendDebugOutput("C#: SENDING PACKET");
        returnVal = SendData(ptr, length);

        Marshal.FreeCoTaskMem(ptr);

        return returnVal;
    }

    public static void UnpackBool(ref byte[] byteArray, ref int loc, ref bool output)
    {
        output = BitConverter.ToBoolean(byteArray, loc);
        loc += Marshal.SizeOf(output);
    }

    public static void UnpackInt(ref byte[] byteArray, ref int loc, ref int output)
    {
        output = BitConverter.ToInt32(byteArray, loc);
        loc += Marshal.SizeOf(output);
    }

    public static void UnpackFloat(ref byte[] byteArray, ref int loc, ref float output)
    {
        output = BitConverter.ToSingle(byteArray, loc);
        loc += Marshal.SizeOf(output);
    }
    public static void UnpackChar(ref byte[] byteArray, ref int loc, ref char output)
    {
        output = (char)byteArray[loc];
        loc += Marshal.SizeOf(output);
    }

    public static void UnpackString(ref byte[] byteArray, ref int loc, ref string output)
    {
        int strLen = 0;
        UnpackInt(ref byteArray, ref loc, ref strLen);
        strLen += loc;

        while (loc < strLen)
        {
            char c = '0';
            UnpackChar(ref byteArray, ref loc, ref c);
            output += c;
        }
    }
    #endregion
}
