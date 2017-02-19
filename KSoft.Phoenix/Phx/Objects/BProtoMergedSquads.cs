
namespace KSoft.Phoenix.Phx
{
	// 828E3B38
	public sealed class BProtoMergedSquads
	{
		//List<int> mMergedSquads;

		int mSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int SquadID { get { return mSquadID; } }
	};
}