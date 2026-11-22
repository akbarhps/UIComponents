using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class MainWindow
    {
        // Textarea grip: vertical resize only, never below the web's 104px minimum.
        // ponytail: single textarea; move into an attached behavior when a second FieldTextArea needs a grip
        private void MessageGrip_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double height = MessageText.Height + e.VerticalChange;
            MessageText.Height = height < 104 ? 104 : height;
        }
    }
}

