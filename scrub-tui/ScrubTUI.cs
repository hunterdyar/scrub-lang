using MyGuiCsProject.Views;
using scrub_lang.VirtualMachine;

namespace MyGuiCsProject{
    using Terminal.Gui;
    public class ScrubTUI : Window
    {
        //Left Tabs
        public TabView ProgramTabs;
        //right tabs
        public TabView StatusStabs;
        //views
        private readonly FilesView _filesView;
        private readonly REPLView _replPane;
        private readonly VariableStateView _statePane;
        public VMRunner Runner => _runner; 
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
            StatusStabs = new TabView()
            {
                X = Pos.Right(ProgramTabs),
                Y = 0,
                Width = Dim.Percent(40),
                Height = Dim.Fill(timelineHeight + controlsHeight), 
            };
            _replPane = new REPLView(this);
            _statePane = new VariableStateView("State", this)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _controls = new PlayBar(this)
            {
                X = 0,
                Y = Pos.Bottom(ProgramTabs),
                Width = Dim.Percent(60),
                Height = controlsHeight,
                CanFocus = true,
                Title = "Controls",
            };

            _lastResult = new LastResultView(this)
            {
                X = Pos.Right(_controls),
                Y = Pos.Bottom(StatusStabs),
                Height = controlsHeight,
                Width = Dim.Percent(40),
                CanFocus = false,
                Title = "Last Result"
            };

            _filesView = new FilesView(this);

            var filetab = new TabView.Tab("Open File", _filesView);
            var repltab = new TabView.Tab("REPL", _replPane);
            var variablesTab = new TabView.Tab("Variables", _statePane);
            
            var timeline = new TimelineView(this)
            {
                X = 0,
                Y = Pos.Bottom(_controls),
                Height = timelineHeight,
                Width = Dim.Percent(100),
            };
            var oplog = new OpLogView(this)
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            var oplogtab = new TabView.Tab("Log", oplog);
            
            ProgramTabs.AddTab(repltab, true);
            ProgramTabs.AddTab(filetab, false);
            
            StatusStabs.AddTab(oplogtab, false);
            StatusStabs.AddTab(variablesTab,false);
            
            Add(ProgramTabs);
            Add(StatusStabs);
            Add(_controls);
            Add(_lastResult);
            Add(timeline);

            
            //todo: how input should work. https://gui-cs.github.io/Terminal.Gui/docs/keyboard.htmls
            KeyPress+= OnKeyPress;
        }

        private void OnKeyPress(KeyEventEventArgs obj)
        {
            if (_replPane.Visible)
            {
                _replPane.HandleKey(obj);
            }
        }

        public void RunFile(string filePath)
        {
            _replPane.TabIndex = 0;//set to program view.
            using (StreamReader reader = new StreamReader(filePath))
            {
                Runner.Run(reader);
            }
        }
    }
}
