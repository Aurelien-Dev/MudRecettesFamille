﻿using MudBlazor;
using RecettesFamille.Extensions;

namespace RecettesFamille.Managers;

public class ErrorManager(ISnackbar snackbar)
{
    public void DisplayError()
    {
        snackbar.Clear();
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
        snackbar.Add(ErrorMessages.GetRandomErrorMessage(), Severity.Error);
    }
    public void DisplayError(string message)
    {
        snackbar.Clear();
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
        snackbar.Add(message, Severity.Error);
    }

    public void DisplaySuccess(string message)
    {
        snackbar.Clear();
        snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomEnd;
        snackbar.Add(message, Severity.Success);
    }
}
