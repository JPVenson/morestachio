namespace Morestachio.Document.TextOperations
{
	/// <summary>
	/// 
	/// </summary>
	public static class LineBreakTrimDirectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public static bool HasFlagFast(this LineBreakTrimDirection value, LineBreakTrimDirection flag)
		{
			return (value & flag) != 0;
		}
	}
}