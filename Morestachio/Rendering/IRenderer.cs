using System.Threading;
using System.Threading.Tasks;
#if ValueTask
using MorestachioDocumentResultPromise = System.Threading.Tasks.ValueTask<Morestachio.MorestachioDocumentResult>;
#else
using MorestachioDocumentResultPromise = System.Threading.Tasks.Task<Morestachio.MorestachioDocumentResult>;
#endif
namespace Morestachio.Rendering
{
	/// <summary>
	///		Contains all logic for invoking the Document items
	/// </summary>
	public interface IRenderer
	{
		/// <summary>
		///		Will be invoked before the rendering step
		/// </summary>
		void PreRender();

		/// <summary>
		///		Will be invoked after the rendering
		/// </summary>
		void PostRender();

		/// <summary>
		///		Renders a document with the given object
		/// </summary>
		/// <param name="data"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		MorestachioDocumentResultPromise Render(object data, CancellationToken cancellationToken);
	}
}