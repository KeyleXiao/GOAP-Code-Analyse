namespace Messages
{
    public interface AIMessage
    {
        void DispatchTo(IHandlerP handler, AIMemory memorySender);
    }
}