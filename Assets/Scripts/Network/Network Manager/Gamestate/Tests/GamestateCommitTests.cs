using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamestate;

public class GamestateCommitTests
{
    [SetUp]
    public void Setup() {

    }

    [TearDown]
    public void Teardown() {

    }

    [Test]
    public void GamestateCommitPlayerTest() {
        short actorNumber = 1;
        short entryId = 2;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable();

        PlayerEntry playerEntry = players.Create(entryId);

        playerEntry.name        = "AAA";
        playerEntry.isBot       = false;
        playerEntry.ready       = true;
        playerEntry.actorNumber = actorNumber;
        playerEntry.role        = 5;
        playerEntry.character   = 2;
        playerEntry.teamId      = 9;

        playerEntry.Commit();

        Assert.That(testHelper.commitedPackets.Count, Is.EqualTo(1));

        GamestatePacket packet = testHelper.commitedPackets[0];
        Assert.That(packet.revisionNumber,    Is.EqualTo(1));
        Assert.That(packet.revisionActor,     Is.EqualTo(actorNumber));

        Assert.That(packet.id, Is.EqualTo(entryId));

        Assert.That(packet.hasName, Is.EqualTo(true ));
        Assert.That(packet.name,    Is.EqualTo("AAA"));

        bool[] hasShortValues = new bool[GamestatePacket.maxShorts];
        hasShortValues[(int)PlayerEntry.ShortFields.ActorNumber] = true;
        hasShortValues[(int)PlayerEntry.ShortFields.Role]        = true;
        hasShortValues[(int)PlayerEntry.ShortFields.Character]   = true;
        hasShortValues[(int)PlayerEntry.ShortFields.TeamId]      = true;
        Assert.That(packet.hasShortValues, Is.EquivalentTo(hasShortValues));
        Assert.That(packet.shortValues,    Is.EquivalentTo(new short[]{actorNumber, 5, 2, 9}));

        bool[] boolValues = new bool[GamestatePacket.maxBools];
        boolValues[0] = false;
        boolValues[1] = true;
        Assert.That(packet.boolValues, Is.EquivalentTo(boolValues));
    }

    [Test]
    public void GamestateCommitTeamTest() {
        short actorNumber = 17;
        short entryId     = -100;
        GamestateCommitTestHelper<TeamEntry> testHelper = new GamestateCommitTestHelper<TeamEntry>(actorNumber);
        GamestateTable<TeamEntry> teams = testHelper.TestTable();

        TeamEntry teamEntry = teams.Create(entryId);

        Assert.That(teamEntry,    Is.Not.Null     );
        Assert.That(teamEntry.id, Is.EqualTo(entryId));

        teamEntry.name       = "xrtycuhEEEEE";
        teamEntry.isDead     = true;
        teamEntry.kills      = 67;
        teamEntry.deaths     = 222;
        teamEntry.assists    = 0;
        teamEntry.checkpoint = -9;

        teamEntry.Commit();

        Assert.That(testHelper.commitedPackets.Count, Is.EqualTo(1));

        GamestatePacket packet = testHelper.commitedPackets[0];
        Assert.That(packet.revisionNumber,    Is.EqualTo(1));
        Assert.That(packet.revisionActor,     Is.EqualTo(actorNumber));

        Assert.That(packet.id, Is.EqualTo(entryId));

        Assert.That(packet.hasName, Is.EqualTo(true));
        Assert.That(packet.name,    Is.EqualTo("xrtycuhEEEEE"));

        bool[] hasShortValues = new bool[GamestatePacket.maxShorts];
        hasShortValues[(int)TeamEntry.ShortFields.Kills]      = true;
        hasShortValues[(int)TeamEntry.ShortFields.Deaths]     = true;
        hasShortValues[(int)TeamEntry.ShortFields.Assists]    = true;
        hasShortValues[(int)TeamEntry.ShortFields.Checkpoint] = true;
        Assert.That(packet.hasShortValues, Is.EquivalentTo(hasShortValues));
        Assert.That(packet.shortValues,    Is.EquivalentTo(new short[]{67, 222, 0, -9}));

        bool[] boolValues = new bool[GamestatePacket.maxBools];
        boolValues[0] = true;
        Assert.That(packet.boolValues, Is.EquivalentTo(boolValues));
    }
}
