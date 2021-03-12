using Gamestate;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Text;
using System;

public class GamestateRandomTestingUnit {
    private Random.State currentState;

    public GamestateRandomTestingUnit(int seed) {
        Random.InitState(seed);
        currentState = Random.state;
    }

    private string GetRandomUTF8String() {
        string output = "";
        bool nameGenerated = false;
        UTF8Encoding nameEncoding = new UTF8Encoding(false, true);
        int stringBytes = Random.Range(3, byte.MaxValue + 1);
        while (!nameGenerated) {
            byte[] data = new byte[stringBytes];

            int continuation = 0;
            int expectedContinuation = 0;
            for (int i = 0; i < stringBytes; i++) {
                bool isValidByte = false;
                while (!isValidByte) {
                    data[i] = (byte)Random.Range(byte.MinValue, byte.MaxValue + 1);

                    if (expectedContinuation == 0) {
                        if (data[i] < 0x7F) {
                            isValidByte = true;
                            continuation = 0;
                            expectedContinuation = 0;
                        }
                        else if (data[i] >= 0xC2 && data[i] <= 0xDF) {
                            isValidByte = true;
                            continuation = 0;
                            expectedContinuation = 1;
                        }
                        else if (data[i] >= 0xE0 && data[i] <= 0xEF) {
                            isValidByte = true;
                            continuation = 0;
                            expectedContinuation = 2;
                        }
                        else if (data[i] >= 0xF0 && data[i] <= 0xF4) {
                            isValidByte = true;
                            continuation = 0;
                            expectedContinuation = 3;
                        }
                    }
                    else {
                        if (data[i] >= 0x80 && data[i] <= 0xBF) {
                            isValidByte = true;
                            continuation++;
                        }
                    }
                }

                if (expectedContinuation > 0 && continuation == expectedContinuation) {
                    try {
                        nameEncoding.GetString(data, i - continuation, continuation + 1);
                        continuation = 0;
                        expectedContinuation = 0;
                    }
                    catch (ArgumentException) {
                        i -= continuation;
                        continuation = 0;
                    }
                }
            }

            try {
                output = nameEncoding.GetString(data, 0, stringBytes);
                nameGenerated = true;
            }
            catch (ArgumentException) {
                nameGenerated = false;
            }
        }

        return output;
    }

    public GamestatePacket GetRandomPacket() {
        Random.state = currentState;

        GamestatePacket packet = GamestatePacketManager.GetPacket();

        packet.packetType = (GamestatePacket.PacketType)Random.Range(0, 4);
        packet.table = (GamestateTracker.Table)Random.Range(0, 3);

        packet.revisionNumber    = (uint)Random.Range(int.MinValue,  int.MaxValue     );
        packet.revisionActor     = (byte)Random.Range(byte.MinValue, byte.MaxValue + 1);

        packet.id = (short)Random.Range(short.MinValue, short.MaxValue + 1);

        packet.hasName = Random.Range(0, 2) == 1;
        packet.name    = GetRandomUTF8String();

        for (int i = 0; i < GamestatePacket.maxShorts; i++) {
            packet.hasShortValues[i] = Random.Range(0, 2) == 1;
            if (packet.hasShortValues[i]) {
                packet.shortValues.Add((short)Random.Range(short.MinValue, short.MaxValue + 1));
            }
        }

        for (int i = 0; i < GamestatePacket.maxBools; i++) {
            packet.boolValues[i] = Random.Range(0, 2) == 1;
        }

        currentState = Random.state;

        return packet;
    }

    public void RandomisePlayerEntry(PlayerEntry playerEntry) {
        Random.state = currentState;

        if (Random.Range(0, 2) == 1) playerEntry.name        = GetRandomUTF8String();

        if (Random.Range(0, 2) == 1) playerEntry.actorNumber = (short)Random.Range(short.MinValue, short.MaxValue + 1);
        if (Random.Range(0, 2) == 1) playerEntry.role        = (short)Random.Range(short.MinValue, short.MaxValue + 1);
        if (Random.Range(0, 2) == 1) playerEntry.character   = (short)Random.Range(short.MinValue, short.MaxValue + 1);
        if (Random.Range(0, 2) == 1) playerEntry.teamId      = (short)Random.Range(short.MinValue, short.MaxValue + 1);

        if (Random.Range(0, 2) == 1) playerEntry.isBot       = Random.Range(0, 2) == 1;
        if (Random.Range(0, 2) == 1) playerEntry.ready       = Random.Range(0, 2) == 1;

        currentState = Random.state;
    }

    public void RandomiseTeamEntry(TeamEntry teamEntry) {
        Random.state = currentState;

        if (Random.Range(0, 2) == 1) teamEntry.name       = GetRandomUTF8String();

        if (Random.Range(0, 2) == 1) teamEntry.kills      = (short)Random.Range(short.MinValue, short.MaxValue + 1);
        if (Random.Range(0, 2) == 1) teamEntry.deaths     = (short)Random.Range(short.MinValue, short.MaxValue + 1);
        if (Random.Range(0, 2) == 1) teamEntry.assists    = (short)Random.Range(short.MinValue, short.MaxValue + 1);
        if (Random.Range(0, 2) == 1) teamEntry.checkpoint = (short)Random.Range(short.MinValue, short.MaxValue + 1);

        if (Random.Range(0, 2) == 1) teamEntry.isDead     = Random.Range(0, 2) == 1;

        currentState = Random.state;
    }
}