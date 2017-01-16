namespace Messages
{
    public interface IMessageHandler<MessageType> : IHandlerP
    {
        void ProcessMessage(MessageType message, AIMemory memSender);
    }
}