namespace Morestachio.Document.Contracts;

/// <summary>
///		Should be implemented to allow custom usage reporting on the structure of a DocumentItem for the <see cref="Morestachio.Analyzer.DataAccess.DataAccessAnalyzer"/>
/// </summary>
public interface IReportUsage
{
	/// <summary>
	///		Gets all paths that will called
	/// </summary>
	/// <returns></returns>
	void ReportUsage(UsageData data);
}