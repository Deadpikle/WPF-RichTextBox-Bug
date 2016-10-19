using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BrokenRichTextBox
{
    class CustomRichTextBox : RichTextBox
    {
        private bool _didAddLayoutUpdatedEvent = false;

        public CustomRichTextBox() : base()
        {
            UpdateAdorner();
            if (!_didAddLayoutUpdatedEvent)
            {
                _didAddLayoutUpdatedEvent = true;
                LayoutUpdated += updateAdorner;
            }
        }

        public void UpdateAdorner()
        {
            updateAdorner(null, null);
        }

        private void updateAdorner(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Selection.GetType().GetMethod("System.Windows.Documents.ITextSelection.UpdateCaretAndHighlight", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(
                    Selection, null);
                var caretElement = Selection.GetType().GetProperty("CaretElement", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Selection, null);
                if (caretElement == null)
                    return;
                var caretSubElement = caretElement.GetType().GetField("_caretElement", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(caretElement) as UIElement;
                if (caretSubElement == null) return;
                // Scale slightly differently if in italic just so it looks a little bit nicer
                bool isItalic = (bool)caretElement.GetType().GetField("_italic", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(caretElement);
                double scaleX = 1;
                if (!isItalic)
                    scaleX = (1 / ScaleX);
                else
                    scaleX = 0.685;// output;
                double scaleY = 1;
                var scaleTransform = new ScaleTransform(scaleX, scaleY);
                caretSubElement.RenderTransform = scaleTransform; // The line of trouble
            }), DispatcherPriority.ContextIdle);
        }

        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }
        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register("ScaleX", typeof(double), typeof(CustomRichTextBox), new UIPropertyMetadata(1.0));

        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }
        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register("ScaleY", typeof(double), typeof(CustomRichTextBox), new UIPropertyMetadata(1.0));

    }
}
