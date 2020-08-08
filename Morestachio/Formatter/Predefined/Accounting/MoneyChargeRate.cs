namespace Morestachio.Formatter.Predefined.Accounting
{
	/// <summary>
	///		Defines the possible rates an Money object can be created with
	/// </summary>
	public enum MoneyChargeRate
	{
		/// <summary>
		///		Apply one rate per 60 seconds
		/// </summary>
		PerMinute,

		/// <summary>
		///		Apply one rate per 60 minutes
		/// </summary>
		PerHour,

		/// <summary>
		///		Apply one rate per 15 minutes
		/// </summary>
		PerQuarterHour,

		/// <summary>
		///		Apply one rate per 30 minutes
		/// </summary>
		PerHalfHour,

		/// <summary>
		///		Apply one rate per 24 hours
		/// </summary>
		PerDay,

		/// <summary>
		///		Apply one rate per started 60 minutes
		/// </summary>
		PerStartedHour
	}
}