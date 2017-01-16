namespace Messages
{
	public class MessageBase<MessageType> : AIMessage where MessageType : class, AIMessage
	{
		public void DispatchTo(IHandlerP handler, AIMemory memSender)
		{
			MessageType messageAsType = this as MessageType;
			if (messageAsType != null)
			{
				DynamicDispatch(handler, messageAsType, memSender);
			}
		}

		protected void DynamicDispatch(IHandlerP handler, MessageType mS, AIMemory memSender)
		{
			IMessageHandler<MessageType> matchingHandler = handler as IMessageHandler<MessageType>;
			if (matchingHandler != null)
			{
				matchingHandler.ProcessMessage(mS, memSender);
			}
		}
	}
}