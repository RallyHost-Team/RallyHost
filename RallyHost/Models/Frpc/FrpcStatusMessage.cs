using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RallyHost.Models.Frpc;

public class FrpcStatusMessage : ValueChangedMessage<FrpcStatus>
{
    public FrpcStatusMessage(FrpcStatus status) : base(status)
    {
    }
}