using MyGuiCsProject;
using Terminal.Gui;

Application.Init();

try
{
    Application.Run<REPL>();
}
finally
{
    Application.Shutdown();
}