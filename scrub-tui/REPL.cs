using MyGuiCsProject.Views;
using scrub_lang.VirtualMachine;

namespace MyGuiCsProject{
    using Terminal.Gui;
    public class REPL : Window
    {
        public TextField ReplInput;
        public TextView OutputView;
        public FrameView ReplPane;
        public FrameView StatePane;
        private VMRunner _runner;
        public REPL()
        {
            _runner = new VMRunner();
            _runner.OnOutput += OnOutputUpdate;
            //load file?
            ColorScheme = Colors.ColorSchemes["TopLevel"];
            /// //Laout
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
            OutputView = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                WordWrap = true,
                ReadOnly = true,
            }; 
            OutputView.ColorScheme = Colors.Base;
            ReplInput = new TextField("")
            {
                X = 0,
                Y = Pos.Bottom(OutputView),
                Width = Dim.Fill(),
                Height = 1,
            };
            
            ReplPane.Add(OutputView);
            ReplPane.Add(ReplInput);
            ReplInput.ColorScheme = Colors.Dialog;

            TableView variableTable = new TableView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            variableTable.Table = new VariableData(_runner);
            StatePane.Add(variableTable);
            Add(ReplPane);
            Add(StatePane);
            ReplInput.SetFocus();
            //input
            KeyDown += OnKeyPress;
        }

        private void OnOutputUpdate()
        {
            OutputView.Text = _runner.Output.ToString();
        }

        private void RunLine(string program)
        {
            _runner.RunWithEnvironment(program);
        }
        private void OnKeyPress(KeyEventEventArgs obj)
        {
            if (obj.KeyEvent.Key == Key.Enter)
            {
                if (ReplInput.Text == "")
                {
                    return;
                }
                var program = ReplInput.Text.ToString();
                ReplInput.Text = "";
                RunLine(program);
            }
        }
    }
}
