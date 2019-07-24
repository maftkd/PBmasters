//PacketLib.cs
//
//Description - A library of methods to send various arrangements of data over socket
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public static class PacketLib
{
    //just postcode
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        peer.Send(writer, dm);
    }
    //int
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, int data, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(data);
        peer.Send(writer, dm);
    }
    //int and bool
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, int data, bool left, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(data);
        writer.Put(left);
        peer.Send(writer, dm);
    }
    //bool
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, bool left, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(left);
        peer.Send(writer, dm);
    }
    //bool and bool
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, bool left, bool active, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(left);
        writer.Put(active);
        peer.Send(writer, dm);
    }

    //string
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, string data, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(data);
        peer.Send(writer, dm);
    }

    //Vector3
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, Vector3 data, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(data.x);
        writer.Put(data.y);
        writer.Put(data.z);
        peer.Send(writer, dm);
    }

    //2 Vector3's
    public static void SendMessage(NetPeer peer, NetDataWriter writer, int pc, Vector3 data1, Vector3 data2, DeliveryMethod dm = DeliveryMethod.ReliableOrdered){
        writer.Reset();
        writer.Put(pc);
        writer.Put(data1.x);
        writer.Put(data1.y);
        writer.Put(data1.z);
        writer.Put(data2.x);
        writer.Put(data2.y);
        writer.Put(data2.z);
        peer.Send(writer, dm);
    }
}