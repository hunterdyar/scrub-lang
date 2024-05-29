using MyGuiCsProject;
using Terminal.Gui;

Application.Init();

try
{
    Application.Run<ScrubTUI>();
}
finally
{
    Application.Shutdown();
}