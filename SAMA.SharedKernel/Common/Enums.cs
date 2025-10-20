namespace SAMA.SharedKernel.Common
{
    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Reversed
    }

    public enum AccountStatus
    {
        Active,
        Blocked,
        Suspended,
        Closed
    }

    public enum NotificationType
    {
        Email,
        SMS,
        Push,
        Webhook
    }
}