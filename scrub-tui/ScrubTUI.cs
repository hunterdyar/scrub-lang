using MyGuiCsProject.Views;
using scrub_lang.VirtualMachine;

namespace MyGuiCsProject{
    using Terminal.Gui;
    public class ScrubTUI : Window
    {
        //Left Tabs
        public TabView ProgramTabs;
        
        //views
        private FilesView FilesView;
        private REPLView ReplPane;
        private VariableStateView StatePane;
        private VMRunner _runner;
        private LastResultView lastResult;
        private PlayBar controls;
        
        public ScrubTUI()
        {
            _runner = new VMRunner();
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
            ReplPane = new REPLView(_runner);
            StatePane = new VariableStateView("State", _runner)
            {
                X = Pos.Right(ProgramTabs),
                Y = 0,
                Width = Dim.Percent(40),
                Height = Dim.Fill(3),
            };

            controls = new PlayBar(_runner)
            {
                X = 0,
                Y = Pos.Bottom(ProgramTabs),
                Width = Dim.Percent(60),
                Height = 3,
                CanFocus = true,
                Title = "Controls",
            };

            lastResult = new LastResultView(_runner)
            {
                X = Pos.Right(controls),
                Y = Pos.Bottom(StatePane),
                Height = 3,
                Width = Dim.Percent(40),
                CanFocus = false,
                Title = "Last Result"
            };

            //todo: create a files view that shows recent files and a button that opens a dialogue.
            FilesView = new FilesView();

            var filetab = new TabView.Tab("Open File", FilesView);
            var repltab = new TabView.Tab("REPL", ReplPane);
            ProgramTabs.AddTab(repltab, true);
            ProgramTabs.AddTab(filetab, false);
            Add(controls);
            Add(ProgramTabs);
            Add(StatePane);
            Add(lastResult);

            
            //todo: how input should work. https://gui-cs.github.io/Terminal.Gui/docs/keyboard.html
            
            KeyPress+= OnKeyPress;
        }

        private void OnKeyPress(KeyEventEventArgs obj)
        {
            if (ReplPane.Visible)
            {
                ReplPane.HandleKey(obj);
            }
        }
    }
}
