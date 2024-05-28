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
        public TabView ProgramTabs;
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
            ProgramTabs = new TabView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(60),
                Height = Dim.Fill(3)
            };
            ReplPane = new FrameView("Read-Execute-Print-Loop")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            
            StatePane = new FrameView("State")
            { 
                X = Pos.Right(ProgramTabs),
                Y = 0,
                Width = Dim.Percent(40),
                Height = Dim.Fill(3),
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
                FullRowSelect = true,
                MultiSelect = false,
            };
            
            variableTable.Table = new VariableData(_runner);
            StatePane.Add(variableTable);

            PlayBar controls = new PlayBar(_runner)
            {
                X = 0,
                Y = Pos.Bottom(ProgramTabs),
                Width = Dim.Percent(60),
                Height = 3,
                CanFocus = true,
                Title = "Controls",
            };

            LastResultView last = new LastResultView(_runner)
            {
                X = Pos.Right(controls),
                Y = Pos.Bottom(StatePane),
                Height = 3,
                Width = Dim.Percent(40),
                CanFocus = false,
                Title = "Last Result"
            };

            //todo: create a files view that shows recent files and a button that opens a dialogue.
            var files = new FilesView();

            var filetab = new TabView.Tab("Open File", files);
            var repltab = new TabView.Tab("REPL", ReplPane);
            ProgramTabs.AddTab(repltab,true);
            ProgramTabs.AddTab(filetab,false);
            Add(controls);
            Add(ProgramTabs);
            Add(StatePane);
            Add(last);

            ReplInput.SetFocus();
            //input
            KeyDown += OnKeyPress;
        }

        private void OnOutputUpdate()
        {
            OutputView.Text = _runner.Output.ToString();
            ReplInput.Enabled = _runner.State != VMState.Paused || _runner.State != VMState.Paused;
            OutputView.MoveEnd();
        }

        private void RunLine(string program)
        {
            _runner.RunWithEnvironment(program);
        }
        private void OnKeyPress(KeyEventEventArgs obj)
        {
            if (obj.KeyEvent.Key == Key.Enter)
            {
                if (!ReplInput.HasFocus)
                {
                    return;
                }
                if (_runner.State == VMState.Paused)
                {
                    _runner.RunUntilStop();
                    return;
                }
                
                if (ReplInput.Text == "")
                {
                    //resume if it's running. maybe do this no matter what
                    
                }
                var program = ReplInput.Text.ToString();
                ReplInput.Text = "";
                RunLine(program);
            }
        }
    }
}
