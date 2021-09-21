using System;
using System.ComponentModel;
using System.Drawing;           // NOTE: Project + Add Reference required
using System.Windows.Forms;     // NOTE: Project + Add Reference required
using System.Runtime.InteropServices;

public static class NativeMethods
{
    public static void CenterConsole()
    {
        IntPtr hWin = GetConsoleWindow();
        RECT rc;
        GetWindowRect(hWin, out rc);
        Screen scr = Screen.FromPoint(new Point(rc.left, rc.top));
        int x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
        int y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;
        MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

    private struct RECT { public int left, top, right, bottom; }
}