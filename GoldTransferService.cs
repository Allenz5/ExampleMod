using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

public class GoldTransferService
{
    private readonly IRunState _runState;

    public GoldTransferService(IRunState runState)
    {
        _runState = runState;
        RunManager.Instance.NetService.RegisterMessageHandler<GoldTransferMessage>(OnGoldTransferReceived);
    }

    public void Dispose()
    {
        RunManager.Instance.NetService.UnregisterMessageHandler<GoldTransferMessage>(OnGoldTransferReceived);
    }

    public void SendGold(Player sender, Player receiver, int amount)
    {
        // Apply on the local machine immediately so UI feels responsive.
        sender.Gold -= amount;
        receiver.Gold += amount;

        // Broadcast so all other machines update their copies.
        var message = new GoldTransferMessage
        {
            SenderNetId = sender.NetId,
            ReceiverNetId = receiver.NetId,
            Amount = amount
        };
        RunManager.Instance.NetService.SendMessage(message);
    }

    private void OnGoldTransferReceived(GoldTransferMessage message, ulong senderId)
    {
        var sender = _runState.GetPlayer(message.SenderNetId);
        var receiver = _runState.GetPlayer(message.ReceiverNetId);
        if (sender == null || receiver == null) return;

        sender.Gold -= message.Amount;
        receiver.Gold += message.Amount;
    }
}
