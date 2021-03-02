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
        GamestateCommitTestHelper testHelper = new GamestateCommitTestHelper();
        GamestateTable<PlayerEntry> players = GamestateTable<PlayerEntry>.CreateTestTable(testHelper);

        PlayerEntry playerEntry = players.Create(0);

        Assert.That(playerEntry,    Is.Not.Null  );
        Assert.That(playerEntry.id, Is.EqualTo(0));
    }

    [Test]
    public void GamestateCommitCreatedPlayerTest() {
        GamestateCommitTestHelper testHelper = new GamestateCommitTestHelper();
        GamestateTable<PlayerEntry> players = GamestateTable<PlayerEntry>.CreateTestTable(testHelper);

        PlayerEntry playerEntry = players.Create(0);

        playerEntry.character = 2;
        playerEntry.isBot     = false;
        playerEntry.name      = "AAA";
        playerEntry.ready     = true;
        playerEntry.role      = 5;
        playerEntry.teamId    = 9;

        playerEntry.Commit();
    }

    [Test]
    public void GamestateCreateTeamTest() {
        GamestateCommitTestHelper testHelper = new GamestateCommitTestHelper();
        GamestateTable<TeamEntry> teams = GamestateTable<TeamEntry>.CreateTestTable(testHelper);

        TeamEntry teamEntry = teams.Create(-100);

        Assert.That(teamEntry,    Is.Not.Null     );
        Assert.That(teamEntry.id, Is.EqualTo(-100));
    }
}
