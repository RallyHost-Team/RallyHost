namespace RallyHost.Models.Frpc;

public class FrpcStatus
{
    public string? message { get; set; }
    public static FrpcStatus FromLog(string? log)
    {
        return new() {
            message = log
        };
    }
}