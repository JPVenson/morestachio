using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace JPB.Mustachio.Client.Wpf.Services.AvalonEditTextMarker
{
	/// <summary>
	///     Handles the text markers for a code editor.
	/// </summary>
	public sealed class TextMarkerService : DocumentColorizingTransformer, IBackgroundRenderer, ITextMarkerService,
		ITextViewConnect
	{
		private readonly TextDocument document;
		private readonly TextSegmentCollection<TextMarker> markers;

		public TextMarkerService(TextDocument document)
		{
			if (document == null)
			{
				throw new ArgumentNullException("document");
			}

			this.document = document;
			markers = new TextSegmentCollection<TextMarker>(document);
		}

		#region DocumentColorizingTransformer

		protected override void ColorizeLine(DocumentLine line)
		{
			if (markers == null)
			{
				return;
			}

			var lineStart = line.Offset;
			var lineEnd = lineStart + line.Length;
			foreach (var marker in markers.FindOverlappingSegments(lineStart, line.Length))
			{
				Brush foregroundBrush = null;
				if (marker.ForegroundColor != null)
				{
					foregroundBrush = new SolidColorBrush(marker.ForegroundColor.Value);
					foregroundBrush.Freeze();
				}

				ChangeLinePart(
					Math.Max(marker.StartOffset, lineStart),
					Math.Min(marker.EndOffset, lineEnd),
					element =>
					{
						if (foregroundBrush != null)
						{
							element.TextRunProperties.SetForegroundBrush(foregroundBrush);
						}

						var tf = element.TextRunProperties.Typeface;
						element.TextRunProperties.SetTypeface(new Typeface(
							tf.FontFamily,
							marker.FontStyle ?? tf.Style,
							marker.FontWeight ?? tf.Weight,
							tf.Stretch
						));
					}
				);
			}
		}

		#endregion

		#region ITextMarkerService

		public ITextMarker Create(int startOffset, int length)
		{
			if (markers == null)
			{
				throw new InvalidOperationException("Cannot create a marker when not attached to a document");
			}

			var textLength = document.TextLength;
			if (startOffset < 0 || startOffset > textLength)
			{
				throw new ArgumentOutOfRangeException("startOffset", startOffset,
					"Value must be between 0 and " + textLength);
			}

			if (length < 0 || startOffset + length > textLength)
			{
				throw new ArgumentOutOfRangeException("length", length,
					"length must not be negative and startOffset+length must not be after the end of the document");
			}

			var m = new TextMarker(this, startOffset, length);
			markers.Add(m);
			// no need to mark segment for redraw: the text marker is invisible until a property is set
			return m;
		}

		public IEnumerable<ITextMarker> GetMarkersAtOffset(int offset)
		{
			if (markers == null)
			{
				return Enumerable.Empty<ITextMarker>();
			}

			return markers.FindSegmentsContaining(offset);
		}

		public IEnumerable<ITextMarker> TextMarkers
		{
			get { return markers ?? Enumerable.Empty<ITextMarker>(); }
		}

		public void RemoveAll(Predicate<ITextMarker> predicate)
		{
			if (predicate == null)
			{
				throw new ArgumentNullException("predicate");
			}

			if (markers != null)
			{
				foreach (var m in markers.ToArray())
				{
					if (predicate(m))
					{
						Remove(m);
					}
				}
			}
		}

		public void Remove(ITextMarker marker)
		{
			if (marker == null)
			{
				throw new ArgumentNullException("marker");
			}

			var m = marker as TextMarker;
			if (markers != null && markers.Remove(m))
			{
				Redraw(m);
				m.OnDeleted();
			}
		}

		/// <summary>
		///     Redraws the specified text segment.
		/// </summary>
		internal void Redraw(ISegment segment)
		{
			foreach (var view in textViews)
			{
				view.Redraw(segment);
			}

			if (RedrawRequested != null)
			{
				RedrawRequested(this, EventArgs.Empty);
			}
		}

		public event EventHandler RedrawRequested;

		#endregion

		#region IBackgroundRenderer

		public KnownLayer Layer
		{
			get
			{
				// draw behind selection
				return KnownLayer.Selection;
			}
		}

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			if (textView == null)
			{
				throw new ArgumentNullException("textView");
			}

			if (drawingContext == null)
			{
				throw new ArgumentNullException("drawingContext");
			}

			if (markers == null || !textView.VisualLinesValid)
			{
				return;
			}

			var visualLines = textView.VisualLines;
			if (visualLines.Count == 0)
			{
				return;
			}

			var viewStart = visualLines.First().FirstDocumentLine.Offset;
			var viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
			foreach (var marker in markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
			{
				if (marker.BackgroundColor != null)
				{
					var geoBuilder = new BackgroundGeometryBuilder();
					geoBuilder.AlignToWholePixels = true;
					geoBuilder.CornerRadius = 3;
					geoBuilder.AddSegment(textView, marker);
					var geometry = geoBuilder.CreateGeometry();
					if (geometry != null)
					{
						var color = marker.BackgroundColor.Value;
						var brush = new SolidColorBrush(color);
						brush.Freeze();
						drawingContext.DrawGeometry(brush, null, geometry);
					}
				}

				var underlineMarkerTypes = TextMarkerTypes.SquigglyUnderline | TextMarkerTypes.NormalUnderline |
				                           TextMarkerTypes.DottedUnderline;
				if ((marker.MarkerTypes & underlineMarkerTypes) != 0)
				{
					foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
					{
						var startPoint = r.BottomLeft;
						var endPoint = r.BottomRight;

						Brush usedBrush = new SolidColorBrush(marker.MarkerColor);
						usedBrush.Freeze();
						if ((marker.MarkerTypes & TextMarkerTypes.SquigglyUnderline) != 0)
						{
							var offset = 2.5;

							var count = Math.Max((int) ((endPoint.X - startPoint.X) / offset) + 1, 4);

							var geometry = new StreamGeometry();

							using (var ctx = geometry.Open())
							{
								ctx.BeginFigure(startPoint, false, false);
								ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true,
									false);
							}

							geometry.Freeze();

							var usedPen = new Pen(usedBrush, 1);
							usedPen.Freeze();
							drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
						}

						if ((marker.MarkerTypes & TextMarkerTypes.NormalUnderline) != 0)
						{
							var usedPen = new Pen(usedBrush, 1);
							usedPen.Freeze();
							drawingContext.DrawLine(usedPen, startPoint, endPoint);
						}

						if ((marker.MarkerTypes & TextMarkerTypes.DottedUnderline) != 0)
						{
							var usedPen = new Pen(usedBrush, 1);
							usedPen.DashStyle = DashStyles.Dot;
							usedPen.Freeze();
							drawingContext.DrawLine(usedPen, startPoint, endPoint);
						}
					}
				}
			}
		}

		private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
		{
			for (var i = 0; i < count; i++)
			{
				yield return new Point(start.X + i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0));
			}
		}

		#endregion

		#region ITextViewConnect

		private readonly List<TextView> textViews = new List<TextView>();

		void ITextViewConnect.AddToTextView(TextView textView)
		{
			if (textView != null && !textViews.Contains(textView))
			{
				Debug.Assert(textView.Document == document);
				textViews.Add(textView);
			}
		}

		void ITextViewConnect.RemoveFromTextView(TextView textView)
		{
			if (textView != null)
			{
				Debug.Assert(textView.Document == document);
				textViews.Remove(textView);
			}
		}

		#endregion
	}

	public sealed class TextMarker : TextSegment, ITextMarker
	{
		private readonly TextMarkerService service;

		private Color? backgroundColor;

		private FontStyle? fontStyle;

		private FontWeight? fontWeight;

		private Color? foregroundColor;

		private Color markerColor;

		private TextMarkerTypes markerTypes;

		public TextMarker(TextMarkerService service, int startOffset, int length)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			this.service = service;
			StartOffset = startOffset;
			Length = length;
			markerTypes = TextMarkerTypes.None;
		}

		public event EventHandler Deleted;

		public bool IsDeleted
		{
			get { return !IsConnectedToCollection; }
		}

		public void Delete()
		{
			service.Remove(this);
		}

		public Color? BackgroundColor
		{
			get { return backgroundColor; }
			set
			{
				if (backgroundColor != value)
				{
					backgroundColor = value;
					Redraw();
				}
			}
		}

		public Color? ForegroundColor
		{
			get { return foregroundColor; }
			set
			{
				if (foregroundColor != value)
				{
					foregroundColor = value;
					Redraw();
				}
			}
		}

		public FontWeight? FontWeight
		{
			get { return fontWeight; }
			set
			{
				if (fontWeight != value)
				{
					fontWeight = value;
					Redraw();
				}
			}
		}

		public FontStyle? FontStyle
		{
			get { return fontStyle; }
			set
			{
				if (fontStyle != value)
				{
					fontStyle = value;
					Redraw();
				}
			}
		}

		public object Tag { get; set; }

		public TextMarkerTypes MarkerTypes
		{
			get { return markerTypes; }
			set
			{
				if (markerTypes != value)
				{
					markerTypes = value;
					Redraw();
				}
			}
		}

		public Color MarkerColor
		{
			get { return markerColor; }
			set
			{
				if (markerColor != value)
				{
					markerColor = value;
					Redraw();
				}
			}
		}

		public object ToolTip { get; set; }

		internal void OnDeleted()
		{
			if (Deleted != null)
			{
				Deleted(this, EventArgs.Empty);
			}
		}

		private void Redraw()
		{
			service.Redraw(this);
		}
	}

	/// <summary>
	///     Represents a text marker.
	/// </summary>
	public interface ITextMarker
	{
		/// <summary>
		///     Gets the start offset of the marked text region.
		/// </summary>
		int StartOffset { get; }

		/// <summary>
		///     Gets the end offset of the marked text region.
		/// </summary>
		int EndOffset { get; }

		/// <summary>
		///     Gets the length of the marked region.
		/// </summary>
		int Length { get; }

		/// <summary>
		///     Gets whether the text marker was deleted.
		/// </summary>
		bool IsDeleted { get; }

		/// <summary>
		///     Gets/Sets the background color.
		/// </summary>
		Color? BackgroundColor { get; set; }

		/// <summary>
		///     Gets/Sets the foreground color.
		/// </summary>
		Color? ForegroundColor { get; set; }

		/// <summary>
		///     Gets/Sets the font weight.
		/// </summary>
		FontWeight? FontWeight { get; set; }

		/// <summary>
		///     Gets/Sets the font style.
		/// </summary>
		FontStyle? FontStyle { get; set; }

		/// <summary>
		///     Gets/Sets the type of the marker. Use TextMarkerType.None for normal markers.
		/// </summary>
		TextMarkerTypes MarkerTypes { get; set; }

		/// <summary>
		///     Gets/Sets the color of the marker.
		/// </summary>
		Color MarkerColor { get; set; }

		/// <summary>
		///     Gets/Sets an object with additional data for this text marker.
		/// </summary>
		object Tag { get; set; }

		/// <summary>
		///     Gets/Sets an object that will be displayed as tooltip in the text editor.
		/// </summary>
		/// <remarks>Not supported in this sample!</remarks>
		object ToolTip { get; set; }

		/// <summary>
		///     Deletes the text marker.
		/// </summary>
		void Delete();

		/// <summary>
		///     Event that occurs when the text marker is deleted.
		/// </summary>
		event EventHandler Deleted;
	}

	[Flags]
	public enum TextMarkerTypes
	{
		/// <summary>
		///     Use no marker
		/// </summary>
		None = 0x0000,

		/// <summary>
		///     Use squiggly underline marker
		/// </summary>
		SquigglyUnderline = 0x001,

		/// <summary>
		///     Normal underline.
		/// </summary>
		NormalUnderline = 0x002,

		/// <summary>
		///     Dotted underline.
		/// </summary>
		DottedUnderline = 0x004,

		/// <summary>
		///     Horizontal line in the scroll bar.
		/// </summary>
		LineInScrollBar = 0x0100,

		/// <summary>
		///     Small triangle in the scroll bar, pointing to the right.
		/// </summary>
		ScrollBarRightTriangle = 0x0400,

		/// <summary>
		///     Small triangle in the scroll bar, pointing to the left.
		/// </summary>
		ScrollBarLeftTriangle = 0x0800,

		/// <summary>
		///     Small circle in the scroll bar.
		/// </summary>
		CircleInScrollBar = 0x1000
	}

	public interface ITextMarkerService
	{
		/// <summary>
		///     Gets the list of text markers.
		/// </summary>
		IEnumerable<ITextMarker> TextMarkers { get; }

		/// <summary>
		///     Creates a new text marker. The text marker will be invisible at first,
		///     you need to set one of the Color properties to make it visible.
		/// </summary>
		ITextMarker Create(int startOffset, int length);

		/// <summary>
		///     Removes the specified text marker.
		/// </summary>
		void Remove(ITextMarker marker);

		/// <summary>
		///     Removes all text markers that match the condition.
		/// </summary>
		void RemoveAll(Predicate<ITextMarker> predicate);

		/// <summary>
		///     Finds all text markers at the specified offset.
		/// </summary>
		IEnumerable<ITextMarker> GetMarkersAtOffset(int offset);
	}
}