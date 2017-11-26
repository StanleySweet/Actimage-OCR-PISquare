namespace Project4
{
    using Windows.UI.Xaml.Controls;

    public sealed partial class DisplayBox : UserControl
    {
        public DisplayBox()
        {
            this.InitializeComponent();
        }
        public string Text
        {
            get
            {
                return (this.txtBlock.Text);
            }
            set
            {
                this.txtBlock.Text = value;
            }
        }
    }
}
