using System.Windows;
using System.Windows.Controls;

namespace UIComponents.Wpf
{
    public partial class TablePagination : UserControl
    {
        public static readonly DependencyProperty InfoTextProperty =
            DependencyProperty.Register("InfoText", typeof(string), typeof(TablePagination),
                new PropertyMetadata("Showing 1–5 of 5"));

        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        // Each pager gets a distinct id so tests can scope a Find to one pager.
        public static readonly DependencyProperty PagerIdProperty =
            DependencyProperty.Register("PagerId", typeof(string), typeof(TablePagination),
                new PropertyMetadata("PageSize"));

        public string PagerId
        {
            get { return (string)GetValue(PagerIdProperty); }
            set { SetValue(PagerIdProperty, value); }
        }

        public TablePagination()
        {
            InitializeComponent();
        }
    }
}
