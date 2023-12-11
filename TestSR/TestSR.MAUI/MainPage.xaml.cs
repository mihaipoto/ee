namespace TestSR.MAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly MainVM _vm;

        public MainPage(MainVM vm)
        {
            BindingContext = vm;
            this.Loaded += MainPage_Loaded;
            InitializeComponent();
            _vm = vm;
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            _vm.PageLoaded();
        }
    }

}
