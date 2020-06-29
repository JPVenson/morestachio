using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace JPB.Mustachio.Clinet.Wpf.Common
{
	public class AvalonEditTextAreaBindingBehaviour : Behavior<TextEditor>
	{
		public static readonly DependencyProperty TextAreaProperty = DependencyProperty.Register(
			"TextArea", typeof(TextArea), typeof(AvalonEditTextAreaBindingBehaviour), new FrameworkPropertyMetadata(default(TextArea), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public TextArea TextArea
		{
			get { return (TextArea)GetValue(TextAreaProperty); }
			set { SetValue(TextAreaProperty, value); }
		}

		protected override void OnAttached()
		{
			TextArea = AssociatedObject.TextArea;
		}
	}

	public sealed class AvalonEditBehaviour : Behavior<TextEditor>
	{
		public static readonly DependencyProperty TextProperty =
				DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditBehaviour),
				new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TextChangedCallback));

		public static readonly DependencyProperty CaretOffsetProperty = DependencyProperty.Register(
			"CaretOffset", typeof(int), typeof(AvalonEditBehaviour), new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CaretOffsetChangedCallback));


		private static void CaretOffsetChangedCallback(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var behavior = dependencyObject as AvalonEditBehaviour;
			if (behavior.AssociatedObject != null)
			{
				var editor = behavior.AssociatedObject;
				if (editor.Document != null)
				{
					SetCaret((int)dependencyPropertyChangedEventArgs.NewValue, editor);
				}
			}
		}

		private static void SetCaret(int newValue, TextEditor editor)
		{
			editor.CaretOffset = newValue;
		}

		public int CaretOffset
		{
			get { return (int)GetValue(CaretOffsetProperty); }
			set { SetValue(CaretOffsetProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			SetText(Text, AssociatedObject);
			if (AssociatedObject != null)
			{
				AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
				AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
			}
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			if (AssociatedObject != null)
			{
				AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
				AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
			}
		}

		private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			bool ctrl = Keyboard.Modifiers == ModifierKeys.Control;
			if (ctrl)
			{
				this.UpdateFontSize(e.Delta > 0);
				e.Handled = true;
			}
		}

		// Reasonable max and min font size values
		private const double FONT_MAX_SIZE = 60d;
		private const double FONT_MIN_SIZE = 5d;

		// Update function, increases/decreases by a specific increment
		public void UpdateFontSize(bool increase)
		{
			double currentSize = AssociatedObject.FontSize;

			if (increase)
			{
				if (currentSize < FONT_MAX_SIZE)
				{
					double newSize = Math.Min(FONT_MAX_SIZE, currentSize + 1);
					AssociatedObject.FontSize = newSize;
				}
			}
			else
			{
				if (currentSize > FONT_MIN_SIZE)
				{
					double newSize = Math.Max(FONT_MIN_SIZE, currentSize - 1);
					AssociatedObject.FontSize = newSize;
				}
			}
		}

		private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
		{
			var textEditor = sender as TextEditor;
			if (textEditor != null)
			{
				if (textEditor.Document != null)
				{
					Text = textEditor.Document.Text;
				}
			}
		}

		private static void TextChangedCallback(
			DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var behavior = dependencyObject as AvalonEditBehaviour;
			if (behavior.AssociatedObject != null)
			{
				var editor = behavior.AssociatedObject;
				if (editor.Document != null)
				{
					SetText(dependencyPropertyChangedEventArgs.NewValue?.ToString(), editor);
				}
			}
		}

		private static void SetText(string text, TextEditor editor)
		{
			var caretOffset = editor.CaretOffset;
			editor.Document.Text = text ?? "";
			if (editor.CaretOffset > caretOffset)
			{
				editor.CaretOffset = caretOffset;
			}
			else
			{
				editor.CaretOffset = editor.Document.Text.Length;
			}
		}
	}
}
