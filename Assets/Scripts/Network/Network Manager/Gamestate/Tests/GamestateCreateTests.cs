using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamestate;

public class GamestateCreateTests
{
    [SetUp]
    public void Setup() {

    }

    [TearDown]
    public void Teardown() {

    }

    [Test]
    public void GamestateCreatePlayerTest() {
        short id = 7;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(1);
        GamestateTable<PlayerEntry> players = testHelper.TestTable();

        PlayerEntry playerEntry = players.Create(id);

        Assert.That(playerEntry,    Is.Not.Null   );
        Assert.That(playerEntry.id, Is.EqualTo(id));

        playerEntry.Commit();

        testHelper.Apply(testHelper.commitedPackets[0]);

        Assert.That(players.Get(id), Is.Not.Null);
    }

    [Test]
    public void GamestateCreateTeamTest() {
        short id = -100;
        GamestateCommitTestHelper<TeamEntry> testHelper = new GamestateCommitTestHelper<TeamEntry>(1);
        GamestateTable<TeamEntry> teams = testHelper.TestTable();

        TeamEntry teamEntry = teams.Create(id);

        Assert.That(teamEntry,    Is.Not.Null     );
        Assert.That(teamEntry.id, Is.EqualTo(id));

        teamEntry.Commit();

        testHelper.Apply(testHelper.commitedPackets[0]);

        Assert.That(teams.Get(id), Is.Not.Null);
    }
}
