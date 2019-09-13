namespace Morestachio.Formatter
{
	/// <summary>
	///		Can be used to Preprovide Partials in Templates. A template here will always be overwritten by a partial provided in the template
	/// </summary>
	public interface IPartialTemplateProvider
	{
		/// <summary>
		///		Obtains the Template from the store
		/// </summary>
		/// <returns></returns>
		ExternalPartialDeclaration[] GetTemplates();
	}
}