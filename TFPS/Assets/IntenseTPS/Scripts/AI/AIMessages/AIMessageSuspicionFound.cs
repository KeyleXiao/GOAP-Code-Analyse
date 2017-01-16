namespace Messages
{
    public class AIMessageSuspicionFound : MessageBase<AIMessageSuspicionFound>
    {
        public Information.InformationSuspicion Info { get; private set; }

        public AIMessageSuspicionFound(Information.InformationSuspicion _info)
        {
            Info = _info;
        }
    }
}