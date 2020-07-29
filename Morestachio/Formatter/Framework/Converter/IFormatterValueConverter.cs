using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Morestachio.Formatter.Framework.Converter
{
	/// <summary>
	///		Can be used to Convert formatter parameters
	/// </summary>
	public interface IFormatterValueConverter
	{
		///  <summary>
		/// 		Used to check if this type can be converted
		///  </summary>
		///  <param name="sourceType"></param>
		///  <param name="requestedType"></param>
		///  <returns></returns>
		bool CanConvert(Type sourceType, Type requestedType);

		/////  <summary>
		///// 		Used to check if this type can be converted
		/////  </summary>
		/////  <param name="value"></param>
		/////  <param name="requestedType"></param>
		/////  <returns></returns>
		//bool CanConvert(object value, Type requestedType);

		///  <summary>
		/// 		Should convert the given value to the requestedType
		///  </summary>
		///  <param name="value"></param>
		///  <param name="requestedType"></param>
		///  <returns></returns>
		object Convert(object value, Type requestedType);
	}
}