using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Gamestate;

public class GamestateApplyTests
{
    [SetUp]
    public void Setup() {

    }

    [TearDown]
    public void Teardown() {

    }

    [Test]
    public void GamestateApplyPlayerPacketTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(8);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);
            playerEntry.Commit();

            GamestatePacket packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            playerEntry = players.Get(i);
            GamestateAssertionUnit.AssertPacketApplied(playerEntry, packet);
            playerEntry.Release();

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyMultiplePlayerPacketsTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(8);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);
            playerEntry.Commit();

            GamestatePacket packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            playerEntry = players.Get(i);
            GamestateAssertionUnit.AssertPacketApplied(playerEntry, packet);

            generator.RandomisePlayerEntry(playerEntry);
            playerEntry.Commit();

            packet = testHelper.commitedPackets[1];
            testHelper.Apply(packet);

            playerEntry = players.Get(i);
            GamestateAssertionUnit.AssertPacketApplied(playerEntry, packet);
            playerEntry.Release();

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyPlayerPacketListenerTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(12);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool listenerACalled = false;
            bool listenerBCalled = false;
            bool listenerCCalled = false;

            playerEntry.AddListener((PlayerEntry entry) => {
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                listenerACalled = true;
            });

            playerEntry.AddListener((PlayerEntry entry) => {
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                listenerBCalled = true;
            });

            playerEntry.AddListener((PlayerEntry entry) => {
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                listenerCCalled = true;
            });

            playerEntry.Commit();

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(listenerACalled, Is.EqualTo(true));
            Assert.That(listenerBCalled, Is.EqualTo(true));
            Assert.That(listenerCCalled, Is.EqualTo(true));

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyPlayerPacketListenerDeletionTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(12);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool listenerACalled = false;
            bool listenerBCalled = false;
            bool listenerCCalled = false;
            bool listenerDCalled = false;

            int a = playerEntry.AddListener((PlayerEntry entry) => {
                entry.Release();
                listenerACalled = true;
            });

            int b = playerEntry.AddListener((PlayerEntry entry) => {
                entry.Release();
                listenerBCalled = true;
            });

            int c = playerEntry.AddListener((PlayerEntry entry) => {
                entry.Release();
                listenerCCalled = true;
            });

            playerEntry.RemoveListener(b);

            int d = playerEntry.AddListener((PlayerEntry entry) => {
                entry.Release();
                listenerDCalled = true;
            });

            playerEntry.RemoveListener(c);


            playerEntry.Commit();

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(listenerACalled, Is.EqualTo(true ));
            Assert.That(listenerBCalled, Is.EqualTo(false));
            Assert.That(listenerCCalled, Is.EqualTo(false));
            Assert.That(listenerDCalled, Is.EqualTo(true ));

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyPlayerPacketErrorCallbackTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(9);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool errorCallbackCalled = false;

            playerEntry.Commit((PlayerEntry entry, bool succeeded) => {
                Assert.That(succeeded, Is.EqualTo(true));
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                errorCallbackCalled = true;
            });

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(errorCallbackCalled, Is.EqualTo(true));

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyPlayerPacketMultipleErrorCallbacksTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(23);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool errorCallbackACalled = false;
            bool errorCallbackBCalled = false;

            playerEntry.Commit((PlayerEntry entry, bool succeeded) => {
                Assert.That(succeeded, Is.EqualTo(true));
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                errorCallbackACalled = true;
            });

            playerEntry = players.Get(i);

            playerEntry.Commit((PlayerEntry entry, bool succeeded) => {
                Assert.That(succeeded, Is.EqualTo(false));
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                errorCallbackBCalled = true;
            });

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(errorCallbackACalled, Is.EqualTo(true));
            Assert.That(errorCallbackBCalled, Is.EqualTo(true));

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateAttemptApplyPlayerPacketTest() {
        byte actorNumber = 1;
        GamestateCommitTestHelper<PlayerEntry> testHelper = new GamestateCommitTestHelper<PlayerEntry>(actorNumber);
        GamestateTable<PlayerEntry> players = testHelper.TestTable(GamestateTracker.Table.Players);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(902);

        for (short i = 0; i < 100; i++) {
            PlayerEntry playerEntry = players.Create(i);
            generator.RandomisePlayerEntry(playerEntry);
            playerEntry.Commit();

            GamestatePacket packetA = testHelper.commitedPackets[0];
            bool attemptA = testHelper.AttemptApply(packetA);

            GamestatePacket packetB = generator.GetRandomPacket();
            packetB.id = i;
            packetB.packetType = GamestatePacket.PacketType.Update;
            packetB.revisionNumber = packetA.revisionNumber;
            bool attemptB = testHelper.AttemptApply(packetB);

            GamestatePacket packetC = generator.GetRandomPacket();
            packetC.id = i;
            packetC.packetType = GamestatePacket.PacketType.Update;
            packetC.revisionNumber = packetA.revisionNumber;
            bool attemptC = testHelper.AttemptApply(packetC);

            Assert.That(attemptA, Is.EqualTo(true ));
            Assert.That(attemptB, Is.EqualTo(false));
            Assert.That(attemptC, Is.EqualTo(false));

            playerEntry = players.Get(i);
            GamestateAssertionUnit.AssertPacketApplied(playerEntry, packetA);
            playerEntry.Release();

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyTeamPacketTest() {
        short actorNumber = 17;
        GamestateCommitTestHelper<TeamEntry> testHelper = new GamestateCommitTestHelper<TeamEntry>(actorNumber);
        GamestateTable<TeamEntry> teams = testHelper.TestTable(GamestateTracker.Table.Teams);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(82);

        for (short i = 0; i < 100; i++) {
            TeamEntry teamEntry = teams.Create(i);
            generator.RandomiseTeamEntry(teamEntry);
            teamEntry.Commit();

            GamestatePacket packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            teamEntry = teams.Get(i);
            GamestateAssertionUnit.AssertPacketApplied(teamEntry, packet);
            teamEntry.Release();

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyTeamPacketListenerTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<TeamEntry> testHelper = new GamestateCommitTestHelper<TeamEntry>(actorNumber);
        GamestateTable<TeamEntry> teams = testHelper.TestTable(GamestateTracker.Table.Teams);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(145);

        for (short i = 0; i < 100; i++) {
            TeamEntry teamEntry = teams.Create(i);
            generator.RandomiseTeamEntry(teamEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool listenerACalled = false;
            bool listenerBCalled = false;
            bool listenerCCalled = false;

            teamEntry.AddListener((TeamEntry entry) => {
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                listenerACalled = true;
            });

            teamEntry.AddListener((TeamEntry entry) => {
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                listenerBCalled = true;
            });

            teamEntry.AddListener((TeamEntry entry) => {
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                listenerCCalled = true;
            });

            teamEntry.Commit();

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(listenerACalled, Is.EqualTo(true));
            Assert.That(listenerBCalled, Is.EqualTo(true));
            Assert.That(listenerCCalled, Is.EqualTo(true));

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyTeamPacketErrorCallbackTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<TeamEntry> testHelper = new GamestateCommitTestHelper<TeamEntry>(actorNumber);
        GamestateTable<TeamEntry> teams = testHelper.TestTable(GamestateTracker.Table.Teams);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(777);

        for (short i = 0; i < 100; i++) {
            TeamEntry teamEntry = teams.Create(i);
            generator.RandomiseTeamEntry(teamEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool errorCallbackCalled = false;

            teamEntry.Commit((TeamEntry entry, bool succeeded) => {
                Assert.That(succeeded, Is.EqualTo(true));
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                errorCallbackCalled = true;
            });

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(errorCallbackCalled, Is.EqualTo(true));

            testHelper.commitedPackets.Clear();
        }
    }

    [Test]
    public void GamestateApplyTeamPacketMultipleErrorCallbacksTest() {
        short actorNumber = 1;
        GamestateCommitTestHelper<TeamEntry> testHelper = new GamestateCommitTestHelper<TeamEntry>(actorNumber);
        GamestateTable<TeamEntry> teams = testHelper.TestTable(GamestateTracker.Table.Teams);
        GamestateRandomTestingUnit generator = new GamestateRandomTestingUnit(62);

        for (short i = 0; i < 100; i++) {
            TeamEntry teamEntry = teams.Create(i);
            generator.RandomiseTeamEntry(teamEntry);

            GamestatePacket packet = GamestatePacketManager.GetPacket();

            bool errorCallbackACalled = false;
            bool errorCallbackBCalled = false;

            teamEntry.Commit((TeamEntry entry, bool succeeded) => {
                Assert.That(succeeded, Is.EqualTo(true));
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                errorCallbackACalled = true;
            });

            teamEntry = teams.Get(i);

            teamEntry.Commit((TeamEntry entry, bool succeeded) => {
                Assert.That(succeeded, Is.EqualTo(false));
                GamestateAssertionUnit.AssertPacketApplied(entry, packet);
                entry.Release();
                errorCallbackBCalled = true;
            });

            packet = testHelper.commitedPackets[0];
            testHelper.Apply(packet);

            Assert.That(errorCallbackACalled, Is.EqualTo(true));
            Assert.That(errorCallbackBCalled, Is.EqualTo(true));

            testHelper.commitedPackets.Clear();
        }
    }
}
