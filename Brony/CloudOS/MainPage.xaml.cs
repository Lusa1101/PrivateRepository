namespace CloudOS
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private void Layout_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            (scrollView as IView)?.InvalidateMeasure();
        }

    }

}
