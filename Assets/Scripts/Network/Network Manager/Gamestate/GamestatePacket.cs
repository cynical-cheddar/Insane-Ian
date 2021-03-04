using System.Collections.Generic;
using System;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Gamestate {
    public class GamestatePacket {
        public enum PacketType { Create, Update, Increment, Delete }

        public const int maxShorts = 16;
        public const int maxBools = 16;


        public uint revisionNumber;
        public byte revisionActor;

        public short id;

        public PacketType packetType;
        public GamestateTracker.Table table;

        public string name;
        public bool hasName;

        public List<short> shortValues;
        public bool[] hasShortValues;

        public bool[] boolValues;

        public GamestatePacket() {
            shortValues = new List<short>();
            hasShortValues = new bool[maxShorts];

            boolValues = new bool[maxBools];
        }
        
        private static UTF8Encoding stringEncoder = new UTF8Encoding();
        public static byte[] Serialize(object packetObject) {
            GamestatePacket packet = packetObject as GamestatePacket;

            if (packet == null) throw new Exception("Serialised object is not a GamestatePacket");

            int nameLength = 0;
            int shortCount = 0;
            for (int i = 0; i < maxShorts; i++) {
                if (packet.hasShortValues[i]) shortCount++;
            }
            if (shortCount > packet.shortValues.Count) throw new Exception("More short values marked as present than have been provided.");

            int arraySize = 1;              //  Packet metadata
            arraySize += 2;                 //  ID
            if (packet.packetType != PacketType.Delete) {
                if (packet.packetType != PacketType.Increment) {
                    arraySize += 4;         //  Revision number
                }
                arraySize += 1;             //  Revision actor
                arraySize += 2;             //  Short metadata
                arraySize += 2;             //  Bool values
                if (packet.packetType != PacketType.Increment) {
                    if (packet.hasName) {   //  Name
                        nameLength = stringEncoder.GetByteCount(packet.name);
                        if (nameLength > 0xFF) throw new Exception("GamestatePacket name is too long. (max 255 bytes)");
                        arraySize += 1 + nameLength;
                    }
                }
                arraySize += shortCount * 2;//  Short values
            }

            byte[] data = new byte[arraySize];
            int index = 0;

            //  Serialize metadata
            data[index] = (byte)(((int)packet.packetType & 0x03) | (((int)packet.table << 2) & 0x7C));
            if (packet.hasName) data[index] = (byte)(data[index] | 0x80);
            index++;

            //  Serialize ID
            data[index]     = (byte)((packet.id >> 8 ) & 0xFF);
            data[index + 1] = (byte)( packet.id        & 0xFF);
            index += 2;

            if (packet.packetType != PacketType.Delete) {
                if (packet.packetType != PacketType.Increment) {
                    //  Serialize revision number
                    data[index]     = (byte)((packet.revisionNumber >> 24) & 0xFF);
                    data[index + 1] = (byte)((packet.revisionNumber >> 16) & 0xFF);
                    data[index + 2] = (byte)((packet.revisionNumber >> 8 ) & 0xFF);
                    data[index + 3] = (byte)( packet.revisionNumber        & 0xFF);
                    index += 4;
                }

                //  Serialize revision actor
                data[index] = packet.revisionActor;
                index++;

                //  Serialize short metadata
                byte hasShortsA = 0x00;
                byte hasShortsB = 0x00;
                for (int i = 0; i < 8; i++) {
                    if (packet.hasShortValues[i + 8]) hasShortsA = (byte)(hasShortsA | (0x01 << i));
                    if (packet.hasShortValues[i])     hasShortsB = (byte)(hasShortsB | (0x01 << i));
                }
                data[index]     = hasShortsA;
                data[index + 1] = hasShortsB;
                index += 2;

                //  Serialize bool values
                byte hasBoolsA = 0x00;
                byte hasBoolsB = 0x00;
                for (int i = 0; i < 8; i++) {
                    if (packet.boolValues[i + 8]) hasBoolsA = (byte)(hasBoolsA | (0x01 << i));
                    if (packet.boolValues[i])     hasBoolsB = (byte)(hasBoolsB | (0x01 << i));
                }
                data[index]     = hasBoolsA;
                data[index + 1] = hasBoolsB;
                index += 2;

                if (packet.packetType != PacketType.Increment) {
                    //  Serialize name
                    if (packet.hasName) {
                        int bytes = stringEncoder.GetBytes(packet.name, 0, packet.name.Length, data, index + 1);
                        data[index] = (byte)bytes;
                        index += 1 + bytes;
                    }
                }

                //  Serialize short values
                for (int i = 0; i < shortCount; i++) {
                    data[index]     = (byte)((packet.shortValues[i] >> 8 ) & 0xFF);
                    data[index + 1] = (byte)( packet.shortValues[i]        & 0xFF);
                    index += 2;
                }
            }

            return data;
        }

        public static object Deserialize(byte[] data) {
            GamestatePacket packet = GamestatePacketManager.GetPacket();

            int index = 0;

            //  Deserialize metadata
            packet.packetType = (PacketType)(data[index] & 0x03);
            packet.hasName    = (data[index] & 0x80) == 0x80;
            index++;

            //  Deserialize id
            short idA = (short)(((uint)data[index] << 8) & 0x0000FF00);
            short idB = (short)( (uint)data[index + 1]   & 0x000000FF);
            packet.id = (short)(idA | idB);
            index += 2;

            if (packet.packetType != PacketType.Delete) {
                if (packet.packetType != PacketType.Increment) {
                    //  Deserialize revision number
                    uint revisionNumberA = ((uint)data[index]     << 24) & 0xFF000000;
                    uint revisionNumberB = ((uint)data[index + 1] << 16) & 0x00FF0000;
                    uint revisionNumberC = ((uint)data[index + 2] << 8 ) & 0x0000FF00;
                    uint revisionNumberD =  (uint)data[index + 3]        & 0x000000FF;
                    packet.revisionNumber = revisionNumberA | revisionNumberB | revisionNumberC | revisionNumberD;
                    index += 4;
                }

                //  Deserialize revision actor
                packet.revisionActor = data[index];
                index++;

                //  Deserialize short metadata
                int shortCount = 0;
                for (int i = 0; i < 8; i++) {
                    if (((data[index] >> i) & 0x01) == 0x01) {
                        packet.hasShortValues[i + 8] = true;
                        shortCount++;
                    }
                    else packet.hasShortValues[i + 8] = false;
                    
                    if (((data[index + 1] >> i) & 0x01) == 0x01) {
                        packet.hasShortValues[i] = true;
                        shortCount++;
                    }
                    else packet.hasShortValues[i] = false;
                }
                index += 2;

                //  Deserialize bool values
                for (int i = 0; i < 8; i++) {
                    if (((data[index] >> i) & 0x01) == 0x01) packet.boolValues[i + 8] = true;
                    else packet.boolValues[i + 8] = false;

                    if (((data[index + 1] >> i) & 0x01) == 0x01) packet.boolValues[i] = true;
                    else packet.boolValues[i] = false;
                }
                index += 2;

                if (packet.packetType != PacketType.Increment) {
                    //  Deserialise name
                    if (packet.hasName) {
                        int nameLength = data[index];
                        packet.name = stringEncoder.GetString(data, index + 1, nameLength);
                        index += nameLength + 1;
                    }
                }

                //  Deserialize short values
                for (int i = 0; i < shortCount; i++) {
                    short valueA = (short)(((uint)data[index] << 8) & 0x0000FF00);
                    short valueB = (short)( (uint)data[index + 1]   & 0x000000FF);
                    packet.shortValues.Add((short)(valueA | valueB));
                    index += 2;
                }
            }

            return packet;
        }
    }

    public class GamestatePacketManager {
        private static bool registered = false;
        private static Stack<GamestatePacket> packetPool = new Stack<GamestatePacket>();

        private static void RegisterPacket() {
            PhotonPeer.RegisterType(typeof(GamestatePacket), 0xFF, GamestatePacket.Serialize, GamestatePacket.Deserialize);
        }

        public static GamestatePacket GetPacket() {
            if (!registered) RegisterPacket();

            GamestatePacket packet;
            lock (packetPool) {
                if (packetPool.Count == 0) packet = new GamestatePacket();
                else packet = packetPool.Pop();
            }

            packet.hasName = false;

            packet.shortValues.Clear();
            for (int i = 0; i < GamestatePacket.maxShorts; i++) {
                packet.hasShortValues[i] = false;
            }

            return packet;
        }

        public static void ReleasePacket(GamestatePacket packet) {
            if (packet == null) throw new Exception("Cannot release null GameStatepacket.");

            lock (packetPool) {
                packetPool.Push(packet);
            }
        }
    }
}