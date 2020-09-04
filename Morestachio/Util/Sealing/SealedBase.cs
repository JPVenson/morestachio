using System;
using System.Text;

namespace Morestachio.Util.Sealing
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class SealedBase : ISealed
	{
		/// <inheritdoc />
		public bool IsSealed { get; protected set; }

		/// <summary>
		///		Checks for the <see cref="IsSealed"/> flag and throws an exception
		/// </summary>
		protected void CheckSealed()
		{
			if (IsSealed)
			{
				throw new InvalidOperationException($"This instance of '{GetType().Name}' is sealed and cannot be modified anymore");
			}
		}
		
		/// <inheritdoc />
		public virtual void Seal()
		{
			IsSealed = true;
		}
	}
}
