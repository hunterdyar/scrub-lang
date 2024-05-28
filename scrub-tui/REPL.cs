namespace MyGuiCsProject{
    using Terminal.Gui;
    public partial class REPL : Window
    {
        public TextField ReplInput;
        public ScrollView OutputView;
        public FrameView ReplPane;
        public FrameView StatePane;
        public FrameView ReplTopPane;
        public FrameView RepBottomPane;
        public REPL()
        {
            Border = new Border()
            {
                BorderStyle = BorderStyle.None
            };
            ReplPane = new FrameView("Read-Execute-Print-Loop")
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(60),
                Height = Dim.Fill()
            };
            StatePane = new FrameView("State")
            { 
                X = Pos.Right(ReplPane),
                Y = 0,
                Width = Dim.Percent(40),
                Height = Dim.Fill()
            };
            OutputView = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1)
            };
            ReplInput = new TextField("Input")
            {
                X = 0,
                Y = Pos.Bottom(OutputView),
                Width = Dim.Fill(),
                Height = 1
            };
            ReplPane.Add(OutputView);
            ReplPane.Add(ReplInput);
            Add(ReplPane);
            Add(StatePane);
            ReplInput.SetFocus();
            
        }
    }
}
