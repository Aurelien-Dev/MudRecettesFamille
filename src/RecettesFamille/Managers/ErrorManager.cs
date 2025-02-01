using MudBlazor;
using RecettesFamille.Extensions;

namespace RecettesFamille.Managers;

public class ErrorManager(ISnackbar Snackbar)
{
    public void DisplayError()
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
        Snackbar.Add(ErrorMessages.GetRandomErrorMessage(), Severity.Error);
    }
    public void DisplayError(string message)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
        Snackbar.Add(message, Severity.Error);
    }

    public void DisplaySuccess(string message)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
        Snackbar.Add(message, Severity.Success);
    }
}
