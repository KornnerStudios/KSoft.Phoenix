using System;

namespace KSoft.Phoenix.Resource
{
	public sealed class SaveFile
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		readonly Runtime.BSettings mSettings = new();
		readonly Runtime.BSaveGame mSaveGame = new();

		long mLeftoversPos;
		byte[]? mLeftovers;

		#region IEndianStreamSerializable Members
		void SerializeLeftovers(IO.EndianStream s)
		{
			s.TraceAndDebugPosition(ref mLeftoversPos);

			if (s.IsReading)
			{
				mLeftovers = new byte[s.BaseStream.Length - s.BaseStream.Position];
			}

			var leftovers = mLeftovers ?? throw new System.InvalidOperationException("Save file leftovers are required when writing.");
			s.Stream(leftovers.AsSpan(0, leftovers.Length));
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
