using System.Threading;
using System.Threading.Tasks;

namespace Morestachio;

/// <summary>
///		Delegate for the result of an Compile() call
/// </summary>
/// <param name="data"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
public delegate Task<MorestachioDocumentResult> CompilationResult(object data, CancellationToken cancellationToken);