using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamestate;

public class GamestatePacketSerializationTests
{
    [Test]
    public void GamestatePacketSerializeTestA() {
        GamestatePacket gamestatePacket = new GamestatePacket();

        gamestatePacket.revisionNumber     = 17;
        gamestatePacket.revisionActor      = 0x03;
        gamestatePacket.id                 = 5;
        gamestatePacket.hasName            = true;
        gamestatePacket.name               = "AAAAABBB";

        gamestatePacket.hasShortValues[0]  = true;
        gamestatePacket.shortValues.Add(-12);
        gamestatePacket.hasShortValues[1]  = false;
        gamestatePacket.hasShortValues[2]  = false;
        gamestatePacket.hasShortValues[3]  = false;
        gamestatePacket.hasShortValues[4]  = true;
        gamestatePacket.shortValues.Add(9999);
        gamestatePacket.hasShortValues[5]  = false;
        gamestatePacket.hasShortValues[6]  = true;
        gamestatePacket.shortValues.Add(11);
        gamestatePacket.hasShortValues[7]  = false;
        gamestatePacket.hasShortValues[8]  = false;
        gamestatePacket.hasShortValues[9]  = true;
        gamestatePacket.shortValues.Add(56);
        gamestatePacket.hasShortValues[10] = true;
        gamestatePacket.shortValues.Add(-56);
        gamestatePacket.hasShortValues[11] = true;
        gamestatePacket.shortValues.Add(0);
        gamestatePacket.hasShortValues[12] = true;
        gamestatePacket.shortValues.Add(745);
        gamestatePacket.hasShortValues[13] = true;
        gamestatePacket.shortValues.Add(3);
        gamestatePacket.hasShortValues[14] = false;
        gamestatePacket.hasShortValues[15] = true;
        gamestatePacket.shortValues.Add(15);

        gamestatePacket.boolValues[0]  = true;
        gamestatePacket.boolValues[1]  = false;
        gamestatePacket.boolValues[2]  = false;
        gamestatePacket.boolValues[3]  = false;
        gamestatePacket.boolValues[4]  = true;
        gamestatePacket.boolValues[5]  = false;
        gamestatePacket.boolValues[6]  = true;
        gamestatePacket.boolValues[7]  = false;
        gamestatePacket.boolValues[8]  = false;
        gamestatePacket.boolValues[9]  = true;
        gamestatePacket.boolValues[10] = true;
        gamestatePacket.boolValues[11] = true;
        gamestatePacket.boolValues[12] = true;
        gamestatePacket.boolValues[13] = true;
        gamestatePacket.boolValues[14] = false;

        byte[] serializedPacket = GamestatePacket.Serialize(gamestatePacket);

        Assert.That(serializedPacket, Is.Not.Null);
        Assert.That(serializedPacket, Is.Not.Empty);

        GamestatePacket newPacket = GamestatePacket.Deserialize(serializedPacket) as GamestatePacket;

        Assert.That(newPacket, Is.Not.Null);

        GamestateAssertionUnit.AssertPacketEquality(gamestatePacket, newPacket);
    }

    [Test]
    public void GamestatePacketSerializeTestB() {
        GamestatePacket gamestatePacket = new GamestatePacket();

        gamestatePacket.revisionNumber     = 5899;
        gamestatePacket.revisionActor      = 0xF7;
        gamestatePacket.id                 = 12;
        gamestatePacket.hasName            = false;
        gamestatePacket.name               = "AAAAABBB";

        gamestatePacket.hasShortValues[0]  = false;
        gamestatePacket.hasShortValues[1]  = true;
        gamestatePacket.shortValues.Add(6);
        gamestatePacket.hasShortValues[2]  = true;
        gamestatePacket.shortValues.Add(1359);
        gamestatePacket.hasShortValues[3]  = true;
        gamestatePacket.shortValues.Add(13590);
        gamestatePacket.hasShortValues[4]  = false;
        gamestatePacket.hasShortValues[5]  = true;
        gamestatePacket.shortValues.Add(-13590);
        gamestatePacket.hasShortValues[6]  = false;
        gamestatePacket.hasShortValues[7]  = true;
        gamestatePacket.shortValues.Add(67);
        gamestatePacket.hasShortValues[8]  = false;
        gamestatePacket.hasShortValues[9]  = false;
        gamestatePacket.hasShortValues[10] = false;
        gamestatePacket.hasShortValues[11] = false;
        gamestatePacket.hasShortValues[12] = false;
        gamestatePacket.hasShortValues[13] = false;
        gamestatePacket.hasShortValues[14] = true;
        gamestatePacket.shortValues.Add(0);
        gamestatePacket.hasShortValues[15] = false;

        gamestatePacket.boolValues[0]  = false;
        gamestatePacket.boolValues[1]  = true;
        gamestatePacket.boolValues[2]  = true;
        gamestatePacket.boolValues[3]  = true;
        gamestatePacket.boolValues[4]  = false;
        gamestatePacket.boolValues[5]  = true;
        gamestatePacket.boolValues[6]  = false;
        gamestatePacket.boolValues[7]  = true;
        gamestatePacket.boolValues[8]  = false;
        gamestatePacket.boolValues[9]  = false;
        gamestatePacket.boolValues[10] = true;
        gamestatePacket.boolValues[11] = true;
        gamestatePacket.boolValues[12] = false;
        gamestatePacket.boolValues[13] = false;
        gamestatePacket.boolValues[14] = true;

        byte[] serializedPacket = GamestatePacket.Serialize(gamestatePacket);

        Assert.That(serializedPacket, Is.Not.Null);
        Assert.That(serializedPacket, Is.Not.Empty);

        GamestatePacket newPacket = GamestatePacket.Deserialize(serializedPacket) as GamestatePacket;

        Assert.That(newPacket, Is.Not.Null);

        GamestateAssertionUnit.AssertPacketEquality(gamestatePacket, newPacket);
    }

    [Test]
    public void GamestatePacketSerializeTestC() {
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(5);
        for (int i = 0; i < 100; i++) {
            GamestatePacket gamestatePacket = generator.GetRandomPacket();

            byte[] serializedPacket = GamestatePacket.Serialize(gamestatePacket);

            Assert.That(serializedPacket, Is.Not.Null);
            Assert.That(serializedPacket, Is.Not.Empty);

            GamestatePacket newPacket = GamestatePacket.Deserialize(serializedPacket) as GamestatePacket;

            Assert.That(newPacket, Is.Not.Null);

            GamestateAssertionUnit.AssertPacketEquality(gamestatePacket, newPacket);
        }
    }
}
