
namespace KSoft.Phoenix.Resource
{
	public sealed class SaveFile
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		Runtime.BSettings mSettings = new Runtime.BSettings();
		Runtime.BSaveGame mSaveGame = new Runtime.BSaveGame();

		long mLeftoversPos;
		byte[] mLeftovers;

		#region IEndianStreamSerializable Members
		void SerializeLeftovers(IO.EndianStream s)
		{
			s.TraceAndDebugPosition(ref mLeftoversPos);

			if (s.IsReading)
				mLeftovers = new byte[s.BaseStream.Length - s.BaseStream.Position];

			s.Stream(mLeftovers, 0, mLeftovers.Length);
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(mSettings);
			s.Stream(mSaveGame);
			SerializeLeftovers(s);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				mSaveGame.ToStream(s);
			}
		}
#endif
		#endregion
	};
}