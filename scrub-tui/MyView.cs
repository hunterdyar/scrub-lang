namespace MyGuiCsProject{
    using Terminal.Gui;
    public partial class MyView {
        
        public MyView() {
            InitializeComponent();
            button1.Clicked += () => MessageBox.Query("Hello", "Hello There!", "Ok");
        }
    }
}
