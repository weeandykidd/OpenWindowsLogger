﻿using OpenWindowsLogger;
using System;
using System.Diagnostics;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//CREDIT https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-mouse-hook-in-c/
//Stephen Toub May 3 2006
class mouseListener

{

  private static LowLevelMouseProc _proc = HookCallback;

  private static IntPtr _hookID = IntPtr.Zero;


  public static void mouseListenerMain()

  {

    _hookID = SetHook(_proc);
    Application.Run();
    UnhookWindowsHookEx(_hookID);

  }

  private static IntPtr SetHook(LowLevelMouseProc proc)

  {

    using (Process curProcess = Process.GetCurrentProcess())

    using (ProcessModule curModule = curProcess.MainModule)

    {

      return SetWindowsHookEx(WH_MOUSE_LL, proc,

          GetModuleHandle(curModule.ModuleName), 0);

    }

  }

  private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

  private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
  {

    if (nCode >= 0 && (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam || MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam))
    {
      MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

      string mButton = MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam ? "LEFT" : "RIGHT";
      AutomationElement el = AutomationElement.FromPoint(new System.Windows.Point(hookStruct.pt.x, hookStruct.pt.y));
      string winName = "";
      if (el.Current.Name.Length>0)
      {
        winName = ", " + el.Current.Name;
      }


      //check if logging enabled and if so then write to log and console
      if (OpenWindowsLogger.OWLmain.logEnabled) {
        OpenWindowsLogger.OWLmain.log.rwlog("w", "MOUSECLICK/" + mButton + "/" + hookStruct.pt.x + ", " + hookStruct.pt.y + winName, OWLmain.HTMLout);
      }
    }

    return CallNextHookEx(_hookID, nCode, wParam, lParam);
  }

  private const int WH_MOUSE_LL = 14;

  private enum MouseMessages

  {

    WM_LBUTTONDOWN = 0x0201,

    WM_LBUTTONUP = 0x0202,

    WM_MOUSEMOVE = 0x0200,

    WM_MOUSEWHEEL = 0x020A,

    WM_RBUTTONDOWN = 0x0204,

    WM_RBUTTONUP = 0x0205

  }

  [StructLayout(LayoutKind.Sequential)]

  private struct POINT

  {

    public int x;

    public int y;

  }

  [StructLayout(LayoutKind.Sequential)]

  private struct MSLLHOOKSTRUCT

  {

    public POINT pt;

    public uint mouseData;

    public uint flags;

    public uint time;

    public IntPtr dwExtraInfo;

  }

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

  private static extern IntPtr SetWindowsHookEx(int idHook,

      LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

  [return: MarshalAs(UnmanagedType.Bool)]

  private static extern bool UnhookWindowsHookEx(IntPtr hhk);

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

  private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

      IntPtr wParam, IntPtr lParam);

  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

  private static extern IntPtr GetModuleHandle(string lpModuleName);

}

