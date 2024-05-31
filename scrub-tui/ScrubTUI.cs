using MyGuiCsProject.Views;
using scrub_lang.VirtualMachine;

namespace MyGuiCsProject{
    using Terminal.Gui;
    public class ScrubTUI : Window
    {
        //Left Tabs
        public TabView ProgramTabs;
        
        //views
        private readonly FilesView _filesView;
        private readonly REPLView _replPane;
        private readonly VariableStateView _statePane;
        private readonly VMRunner _runner;
        private readonly LastResultView _lastResult;
        private readonly PlayBar _controls;
        
        public ScrubTUI()
        {
            int controlsHeight = 3;
            int timelineHeight = 1;
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
                Height = Dim.Fill(timelineHeight + controlsHeight)
            };
            _replPane = new REPLView(_runner);
            _statePane = new VariableStateView("State", _runner)
            {
                X = Pos.Right(ProgramTabs),
                Y = 0,
                Width = Dim.Percent(40),
                Height = Dim.Fill(timelineHeight+controlsHeight),
            };

            _controls = new PlayBar(_runner)
            {
                X = 0,
                Y = Pos.Bottom(ProgramTabs),
                Width = Dim.Percent(60),
                Height = controlsHeight,
                CanFocus = true,
                Title = "Controls",
            };

            _lastResult = new LastResultView(_runner)
            {
                X = Pos.Right(_controls),
                Y = Pos.Bottom(_statePane),
                Height = controlsHeight,
                Width = Dim.Percent(40),
                CanFocus = false,
                Title = "Last Result"
            };

            //todo: create a files view that shows recent files and a button that opens a dialogue.
            _filesView = new FilesView();

            var filetab = new TabView.Tab("Open File", _filesView);
            var repltab = new TabView.Tab("REPL", _replPane);
            var timeline = new TimelineView(_runner)
            {
                X = 0,
                Y = Pos.Bottom(_controls),
                Height = timelineHeight,
                Width = Dim.Percent(100),
            };
            var oplog = new OpLogView(_runner)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            var oplogtab = new TabView.Tab("Log", oplog);
            
            ProgramTabs.AddTab(repltab, true);
            ProgramTabs.AddTab(filetab, false);
            ProgramTabs.AddTab(oplogtab, false);
            
            Add(_controls);
            Add(ProgramTabs);
            Add(_statePane);
            Add(_lastResult);
            Add(timeline);

            
            //todo: how input should work. https://gui-cs.github.io/Terminal.Gui/docs/keyboard.html
            
            KeyPress+= OnKeyPress;
        }

        private void OnKeyPress(KeyEventEventArgs obj)
        {
            if (_replPane.Visible)
            {
                _replPane.HandleKey(obj);
            }
        }
    }
}
