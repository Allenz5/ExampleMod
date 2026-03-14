using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

// The game automatically discovers INetMessage types in mods via ReflectionHelper.GetSubtypesInMods,
// so this message will be registered without any patching.
public struct GoldTransferMessage : INetMessage, IPacketSerializable
{
    public ulong SenderNetId;
    public ulong ReceiverNetId;
    public int Amount;

    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.VeryDebug;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(SenderNetId);
        writer.WriteULong(ReceiverNetId);
        writer.WriteInt(Amount);
    }

    public void Deserialize(PacketReader reader)
    {
        SenderNetId = reader.ReadULong();
        ReceiverNetId = reader.ReadULong();
        Amount = reader.ReadInt();
    }
}
