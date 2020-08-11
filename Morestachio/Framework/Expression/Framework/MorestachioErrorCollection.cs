using System.Collections;
using System.Collections.Generic;
using System.Text;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression.Framework
{
	public class MorestachioErrorCollection : ICollection<IMorestachioError>
	{
		private ICollection<IMorestachioError> _base;
		public MorestachioErrorCollection()
		{
			_base = new List<IMorestachioError>();
		}

		public string GetErrorText()
		{
			var sb = new StringBuilder();
			foreach (var err in this)
			{
				err.Format(sb);
				sb.AppendLine();
			}

			return sb.ToString();
		}

		public IEnumerator<IMorestachioError> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _base).GetEnumerator();
		}

		public void Add(IMorestachioError item)
		{
			//#if DEBUG
			//throw item.GetException();
			//#endif
			_base.Add(item);
		}

		public void Clear()
		{
			_base.Clear();
		}

		public bool Contains(IMorestachioError item)
		{
			return _base.Contains(item);
		}

		public void CopyTo(IMorestachioError[] array, int arrayIndex)
		{
			_base.CopyTo(array, arrayIndex);
		}

		public bool Remove(IMorestachioError item)
		{
			return _base.Remove(item);
		}

		public int Count
		{
			get { return _base.Count; }
		}

		public bool IsReadOnly
		{
			get { return _base.IsReadOnly; }
		}
	}
}