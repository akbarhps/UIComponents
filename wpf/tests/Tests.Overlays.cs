// Covered: modal dialog, loading modal, dialog launchers.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace UIComponents.Wpf.Tests
{
    internal static partial class UIAutomationTests
    {
        static partial void RunOverlays(AutomationElement root, ref bool ran)
        {
            ran = true;
            // Modal dialog (07 O1-O7, M1-M3): in-window overlay, no OS window, no WindowPattern.
            // The OverlayLayer itself is a Grid (no UIA peer); UIA reaches the dialog through
            // its chrome-free GroupBox root and the real Buttons inside it. The layer's
            // resolution before the clicks is proved by the hidden capture target.
            var openModal = Find(root, "OpenModal");
            Check("open modal button found", openModal != null);
            if (openModal != null)
            {
                Func<string, bool> shown = delegate(string id)
                {
                    AutomationElement el = Find(root, id);
                    return el != null && !el.Current.IsOffscreen;
                };
                Func<string> focusedId = delegate
                {
                    AutomationElement f = AutomationElement.FocusedElement;
                    return f == null ? "none" : f.Current.AutomationId + "|" + f.Current.Name + "|" + f.Current.ControlType.ProgrammaticName;
                };
                IntPtr modalHwnd = new IntPtr(root.Current.NativeWindowHandle);
                try
                {
                    // M1/O1: opener remembers focus, opens in the layer and focuses the X.
                    Drv.Focus(openModal);
                    Invoke(openModal);
                    Thread.Sleep(200);
                    Check("modal dialog root shown", shown("ModalDialog"));
                    var modalClose = Find(root, "ModalClose");
                    Check("modal close found from window root", modalClose != null);
                    Check("modal close focused on open", modalClose != null && modalClose.Current.HasKeyboardFocus,
                        "focused=" + focusedId());

                    // O5: Tab is trapped by OverlayHost's window PreviewKeyDown handler.
                    var modalCancel = Find(root, "ModalCancel");
                    var modalConfirm = Find(root, "ModalConfirm");
                    PostKey(modalHwnd, 0x09); Thread.Sleep(150); // VK_TAB
                    Check("Tab close -> cancel", modalCancel != null && modalCancel.Current.HasKeyboardFocus,
                        "focused=" + focusedId());
                    PostKey(modalHwnd, 0x09); Thread.Sleep(150);
                    Check("Tab cancel -> confirm", modalConfirm != null && modalConfirm.Current.HasKeyboardFocus,
                        "focused=" + focusedId());
                    PostKey(modalHwnd, 0x09); Thread.Sleep(150);
                    Check("Tab wraps confirm -> close", modalClose != null && modalClose.Current.HasKeyboardFocus,
                        "focused=" + focusedId());

                    // O4: Escape closes and restores the opener's focus.
                    PostKey(modalHwnd, 0x1B); Thread.Sleep(200); // VK_ESCAPE
                    Check("Escape closes modal", !shown("ModalClose"));
                    Check("Escape restores opener focus", openModal.Current.HasKeyboardFocus,
                        "focused=" + focusedId());

                    // M2: Cancel and the X both close.
                    Invoke(openModal); Thread.Sleep(200);
                    var cancel = Find(root, "ModalCancel");
                    if (cancel != null) Invoke(cancel);
                    Thread.Sleep(200);
                    Check("Cancel closes modal", !shown("ModalClose"));
                    Invoke(openModal); Thread.Sleep(200);
                    var x = Find(root, "ModalClose");
                    if (x != null) Invoke(x);
                    Thread.Sleep(200);
                    Check("X closes modal", !shown("ModalClose"));

                    // O3: a mouse-up on the scrim closes. No mouse on the hidden desktop,
                    // so UIA invokes a transparent proxy that raises MouseLeftButtonUp in-process.
                    Invoke(openModal); Thread.Sleep(200);
                    var scrim = Find(root, "OverlayScrimClick");
                    if (scrim != null) Invoke(scrim);
                    Thread.Sleep(200);
                    Check("backdrop click closes modal", !shown("ModalClose"));

                    // M3: Delete closes and raises the success toast.
                    Invoke(openModal); Thread.Sleep(200);
                    var confirm = Find(root, "ModalConfirm");
                    if (confirm != null) Invoke(confirm);
                    Thread.Sleep(200);
                    Check("Delete closes modal", !shown("ModalClose"));
                    Check("Delete raises Deleted toast", FindByName(root, "Deleted") != null,
                        "n=" + CountByName(root, "Deleted"));
                }
                finally
                {
                    if (shown("ModalClose")) { PostKey(modalHwnd, 0x1B); Thread.Sleep(200); }
                }
            }

            // Loading modal (07 O2, L1-L3): blocking, 3s timer, then a Ready toast.
            var openLoading = Find(root, "OpenLoading");
            Check("OpenLoading found", openLoading != null);
            if (openLoading != null)
            {
                Func<bool> loadingShown = delegate
                {
                    AutomationElement el = Find(root, "LoadingDialog");
                    return el != null && !el.Current.IsOffscreen;
                };
                try
                {
                    // One UIA Invoke costs ~2s here and the timer is 3s: after the open,
                    // use only cheap PostMessage/property checks. L1 backdrop is code-only
                    // (a second Invoke would outrun the timer).
                    Invoke(openLoading);
                    Thread.Sleep(100);
                    // One Find (~2s here): poll the same element afterwards, never re-Find.
                    AutomationElement loadingCard = Find(root, "LoadingDialog");
                    Check("loading dialog shown", loadingCard != null && !loadingCard.Current.IsOffscreen);
                    var loadingFocus = AutomationElement.FocusedElement;
                    Check("loading card focused", loadingCard != null && loadingCard.Current.HasKeyboardFocus,
                        "card=" + (loadingCard == null ? "missing" : loadingCard.Current.Name + "/" +
                            loadingCard.Current.ControlType.ProgrammaticName) + " focused=" +
                            (loadingFocus == null ? "none" : loadingFocus.Current.AutomationId + "|" +
                                loadingFocus.Current.Name + "|" + loadingFocus.Current.ControlType.ProgrammaticName));
                    PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x1B); Thread.Sleep(100); // VK_ESCAPE
                    Check("L1 Escape does not close loading",
                        loadingCard != null && !loadingCard.Current.IsOffscreen);

                    bool gone = false;
                    for (int i = 0; i < 40; i++)
                    {
                        Thread.Sleep(100);
                        if (loadingCard != null && loadingCard.Current.IsOffscreen) { gone = true; break; }
                    }
                    Check("L2 loading closes within 4s", gone);
                    Check("L2 Ready toast shown", FindByName(root, "Ready") != null);
                }
                finally
                {
                    if (loadingShown()) PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x1B);
                }
            }

            // Dialog launchers
            Check("command palette button found", Find(root, "BtnCommandPalette") != null);
            Check("drawer button found", Find(root, "BtnDrawer") != null);
        }
    }
}
