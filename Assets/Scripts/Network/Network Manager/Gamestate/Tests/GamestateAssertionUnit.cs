using NUnit.Framework;
using Gamestate;

public class GamestateAssertionUnit {
    public static void AssertPacketEquality(GamestatePacket a, GamestatePacket b) {
        Assert.That(b.packetType, Is.EqualTo(a.packetType));
        Assert.That(b.table,      Is.EqualTo(a.table     ));
        Assert.That(b.id,         Is.EqualTo(a.id        ));

        if (a.packetType != GamestatePacket.PacketType.Delete) {
            if (a.packetType != GamestatePacket.PacketType.Increment) {
                Assert.That(b.revisionNumber, Is.EqualTo(a.revisionNumber));
            }

            Assert.That(b.revisionActor, Is.EqualTo(a.revisionActor));

            if (a.packetType != GamestatePacket.PacketType.Increment) {
                Assert.That(b.hasName, Is.EqualTo(a.hasName));

                if (a.hasName) {
                    Assert.That(b.name, Is.EqualTo(a.name));
                }
            }
            
            Assert.That(b.boolValues, Is.EquivalentTo(a.boolValues));

            Assert.That(b.hasShortValues, Is.EquivalentTo(a.hasShortValues));
            Assert.That(b.shortValues,    Is.EquivalentTo(a.shortValues   ));
        }
    }

    public static void AssertPacketApplied(GlobalsEntry entry, GamestatePacket packet) {
        if (packet.hasName) Assert.That(entry.name, Is.EqualTo(packet.name));

        int i = 0;
        if (packet.hasShortValues[(int)GlobalsEntry.ShortFields.TimeLimit]) {
            Assert.That(entry.timeLimit, Is.EqualTo(packet.shortValues[i]));
            i++;
        }
    }

    public static void AssertPacketApplied(PlayerEntry entry, GamestatePacket packet) {
        if (packet.hasName) Assert.That(entry.name, Is.EqualTo(packet.name));

        int i = 0;
        if (packet.hasShortValues[(int)PlayerEntry.ShortFields.ActorNumber]) {
            Assert.That(entry.actorNumber, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        if (packet.hasShortValues[(int)PlayerEntry.ShortFields.Role]) {
            Assert.That(entry.role, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        if (packet.hasShortValues[(int)PlayerEntry.ShortFields.Character]) {
            Assert.That(entry.character, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        if (packet.hasShortValues[(int)PlayerEntry.ShortFields.TeamId]) {
            Assert.That(entry.teamId, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        Assert.That(entry.isBot, Is.EqualTo(packet.boolValues[(int)PlayerEntry.BoolFields.IsBot]));
        
        Assert.That(entry.ready, Is.EqualTo(packet.boolValues[(int)PlayerEntry.BoolFields.Ready]));
    }

    public static void AssertPacketApplied(TeamEntry entry, GamestatePacket packet) {
        if (packet.hasName) Assert.That(entry.name, Is.EqualTo(packet.name));

        int i = 0;
        if (packet.hasShortValues[(int)TeamEntry.ShortFields.Kills]) {
            Assert.That(entry.kills, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        if (packet.hasShortValues[(int)TeamEntry.ShortFields.Deaths]) {
            Assert.That(entry.deaths, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        if (packet.hasShortValues[(int)TeamEntry.ShortFields.Assists]) {
            Assert.That(entry.assists, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        if (packet.hasShortValues[(int)TeamEntry.ShortFields.Checkpoint]) {
            Assert.That(entry.checkpoint, Is.EqualTo(packet.shortValues[i]));
            i++;
        }

        Assert.That(entry.isDead, Is.EqualTo(packet.boolValues[(int)TeamEntry.BoolFields.IsDead]));
    }
}