using System;
using System.Drawing;

namespace Skewworks.Labs
{

    [Serializable]
    public delegate void OnClearScreen(SBASIC sender);

    [Serializable]
    public delegate void OnBackColor(SBASIC sender, Color color);

    [Serializable]
    public delegate void OnForeColor(SBASIC sender, Color color);

    [Serializable]
    public delegate void OnInKey(SBASIC sender, ref int keyCode);

    [Serializable]
    public delegate void OnInput(SBASIC sender, ref string text);

    [Serializable]
    public delegate void OnPrint(SBASIC sender, string value);


}
