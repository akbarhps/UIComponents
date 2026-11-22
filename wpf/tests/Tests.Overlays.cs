// Covered: modal form, sidebar modal form, modal dialog, loading modal, command palette.
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
            IntPtr hwnd = new IntPtr(root.Current.NativeWindowHandle);
            double windowLeft = RectPart(root, "Left");
            double windowTop = RectPart(root, "Top");

            // Modal form (07 O3, F1-F5).
            var formSection = Find(root, "FormModalCard");
            Check("form modal card found", formSection != null);
            var openFormModal = Find(formSection ?? root, "OpenFormModal");
            Check("OpenFormModal found", openFormModal != null);
            if (openFormModal != null)
            {
                try
                {
                    Invoke(openFormModal);
                    var formDialog = Find(root, "FormModalDialog");
                    var fmName = Find(root, "FmName");
                    var fmEmail = Find(root, "FmEmail");
                    var fmDesc = Find(root, "FmDesc");
                    var fmRole = Find(root, "FmRole");
                    var nameError = Find(root, "FmNameError");
                    var emailError = Find(root, "FmEmailError");
                    Check("F1 form dialog shown", formDialog != null && WaitFor(
                        () => !formDialog.Current.IsOffscreen, 1000));
                    Check("F1 first field focused", fmName != null && WaitFor(
                        () => fmName.Current.HasKeyboardFocus, 1000));
                    Check("F1 form reset", fmName != null && GetValue(fmName) == "" &&
                        fmEmail != null && GetValue(fmEmail) == "" &&
                        fmDesc != null && GetValue(fmDesc) == "" &&
                        fmRole != null && OverlaysFormComboSelected(fmRole, "Design"),
                        "name=[" + (fmName == null ? "?" : GetValue(fmName)) + "] email=[" +
                        (fmEmail == null ? "?" : GetValue(fmEmail)) + "] desc=[" +
                        (fmDesc == null ? "?" : GetValue(fmDesc)) + "] role=[" +
                        (fmRole == null ? "?" : GetValue(fmRole)) + "]");
                    Func<string, bool> errorShown = delegate(string id)
                    {
                        AutomationElement el = Find(root, id);
                        return el != null && !el.Current.IsOffscreen;
                    };
                    Check("F1 errors hidden", !errorShown("FmNameError") && !errorShown("FmEmailError"));

                    // O5 on the form: one Tab per stop, then wrap back to the first field.
                    string[] tabStops = { "FmEmail", "FmRole", "FmDesc", "FormModalCancel",
                        "FormModalSubmit", "FormModalClose", "FmName" };
                    Drv.Focus(fmName);
                    foreach (string stopId in tabStops)
                    {
                        PostKey(hwnd, 0x09);
                        string expected = stopId;
                        Check("form Tab -> " + expected, WaitFor(delegate
                        {
                            var el = Find(root, expected);
                            return el != null && el.Current.HasKeyboardFocus;
                        }, 1000));
                    }

                    // F2: empty submit shows both errors and focuses the first invalid field.
                    Invoke(Find(root, "FormModalSubmit"));
                    Check("F2 both errors shown", errorShown("FmNameError") && errorShown("FmEmailError"));
                    Check("F2 first invalid focused", WaitFor(() => fmName.Current.HasKeyboardFocus, 1000));
                    Check("F2 invalid marked", fmName.Current.ItemStatus == "invalid" &&
                        fmEmail.Current.ItemStatus == "invalid");

                    // F4: typing in a field clears only its own error.
                    SetValue(fmName, "Atlas");
                    Check("F4 name error clears", !errorShown("FmNameError") && errorShown("FmEmailError"));

                    // F3: valid submit closes, toasts the trimmed name and resets.
                    SetValue(fmEmail, "  a@b.co  ");
                    Invoke(Find(root, "FormModalSubmit"));
                    Check("F3 valid closes form", WaitFor(() => formDialog.Current.IsOffscreen, 1000));
                    var createdToast = FindByName(root, "Project created");
                    Check("F3 created toast", createdToast != null);
                    // Read inside the toast immediately: it auto-dismisses after 3.6s.
                    Check("F3 toast text", createdToast != null && WaitFor(
                        () => FindByName(createdToast, "“Atlas” is ready.") != null, 500));
                    Invoke(openFormModal);
                    Check("F3 form reset after submit", GetValue(Find(root, "FmName")) == "" &&
                        GetValue(Find(root, "FmEmail")) == "");
                    Invoke(Find(root, "FormModalCancel"));

                    // F5: Cancel and X close without validating.
                    Invoke(openFormModal);
                    Invoke(Find(root, "FormModalCancel"));
                    Check("F5 Cancel closes form", WaitFor(() => formDialog.Current.IsOffscreen, 1000));
                    Invoke(openFormModal);
                    Invoke(Find(root, "FormModalClose"));
                    Check("F5 X closes form", WaitFor(() => formDialog.Current.IsOffscreen, 1000));
                }
                finally
                {
                    var dialog = Find(root, "FormModalDialog");
                    if (dialog != null && !dialog.Current.IsOffscreen) PostKey(hwnd, 0x1B);
                }
            }

            // Modal dialog (07 O1-O7, M1-M3): in-window overlay, no OS window.
            var modalSection = Find(root, "ModalCard");
            Check("modal card found", modalSection != null);
            var openModal = Find(modalSection ?? root, "OpenModal");
            Check("open modal button found", openModal != null);
            if (openModal != null)
            {
                try
                {
                    // M1/O1: opener focus is remembered; the X takes focus in the layer.
                    Drv.Focus(openModal);
                    Invoke(openModal);
                    var modalDialog = Find(root, "ModalDialog");
                    var modalClose = Find(root, "ModalClose");
                    var modalCancel = Find(root, "ModalCancel");
                    var modalConfirm = Find(root, "ModalConfirm");
                    Check("modal dialog root shown", modalDialog != null && WaitFor(
                        () => !modalDialog.Current.IsOffscreen, 1000));
                    Check("modal close found from window root", modalClose != null);
                    Check("modal close focused on open", modalClose != null && WaitFor(
                        () => modalClose.Current.HasKeyboardFocus, 1000));

                    // O5: the window PreviewKeyDown handler traps Tab on the dialog.
                    PostKey(hwnd, 0x09); // VK_TAB
                    Check("Tab close -> cancel", modalCancel != null && WaitFor(
                        () => modalCancel.Current.HasKeyboardFocus, 1000));
                    PostKey(hwnd, 0x09);
                    Check("Tab cancel -> confirm", modalConfirm != null && WaitFor(
                        () => modalConfirm.Current.HasKeyboardFocus, 1000));
                    PostKey(hwnd, 0x09);
                    Check("Tab wraps confirm -> close", WaitFor(
                        () => modalClose.Current.HasKeyboardFocus, 1000));

                    // O4: Escape closes and restores focus to the opener.
                    PostKey(hwnd, 0x1B); // VK_ESCAPE
                    Check("Escape closes modal", WaitFor(() => modalDialog.Current.IsOffscreen, 1000));
                    Check("Escape restores opener focus", WaitFor(
                        () => openModal.Current.HasKeyboardFocus, 1000));

                    // M2: Cancel and the X both close.
                    Invoke(openModal);
                    Invoke(modalCancel);
                    Check("Cancel closes modal", WaitFor(() => modalDialog.Current.IsOffscreen, 1000));
                    Invoke(openModal);
                    Invoke(modalClose);
                    Check("X closes modal", WaitFor(() => modalDialog.Current.IsOffscreen, 1000));

                    // O3 is CODE-ONLY: PostMouseClick on this hidden desktop produces no
                    // WPF mouse event at all (verified with a down-event probe). The scrim
                    // handler is the shipping path; card clicks never reach it (sibling).
                    Console.WriteLine("CODE-ONLY  backdrop click closes modal (no mouse on hidden desktop)");

                    // M3: Delete closes and raises the success toast.
                    Invoke(openModal);
                    Invoke(modalConfirm);
                    Check("Delete closes modal", WaitFor(() => modalDialog.Current.IsOffscreen, 1000));
                    Check("Delete raises Deleted toast", FindByName(root, "Deleted") != null);
                }
                finally
                {
                    var dialog = Find(root, "ModalDialog");
                    if (dialog != null && !dialog.Current.IsOffscreen) PostKey(hwnd, 0x1B);
                }
            }

            // Loading modal (07 O2, L1-L3): blocking, 3s timer, then a Ready toast.
            var loadingSection = Find(root, "LoadingCard");
            Check("loading card found", loadingSection != null);
            var openLoading = Find(loadingSection ?? root, "OpenLoading");
            Check("OpenLoading found", openLoading != null);
            if (openLoading != null)
            {
                try
                {
                    Invoke(openLoading);
                    var loadingDialog = Find(root, "LoadingDialog");
                    Check("loading dialog shown", loadingDialog != null && WaitFor(
                        () => !loadingDialog.Current.IsOffscreen, 1000));
                    // Focus first, then post Escape: a key routed from the opener (outside
                    // the layer) would not exercise the layer's _blocking guard.
                    Check("loading card focused", loadingDialog != null && WaitFor(
                        () => loadingDialog.Current.HasKeyboardFocus, 1000));
                    PostKey(hwnd, 0x1B); // VK_ESCAPE
                    Check("L1 Escape does not close loading",
                        !WaitFor(() => loadingDialog.Current.IsOffscreen, 300));
                    Console.WriteLine("CODE-ONLY  backdrop ignored while loading (no mouse on hidden desktop)");
                    Check("L2 loading closes within 4s", WaitFor(
                        () => loadingDialog.Current.IsOffscreen, 4000));
                    Check("L2 Ready toast shown", FindByName(root, "Ready") != null);
                }
                finally
                {
                    var dialog = Find(root, "LoadingDialog");
                    if (dialog != null && !dialog.Current.IsOffscreen) PostKey(hwnd, 0x1B);
                }
            }

            // Sidebar modal form (07 O4, D1-D11).
            var drawerSection = Find(root, "DrawerCard");
            Check("drawer card found", drawerSection != null);
            var openDrawer = Find(drawerSection ?? root, "OpenDrawer");
            var openEditDrawer = Find(drawerSection ?? root, "OpenEditDrawer");
            Check("drawer openers found", openDrawer != null && openEditDrawer != null);
            if (openDrawer != null && openEditDrawer != null)
            {
                Func<string, bool> drawerShown = delegate(string id)
                {
                    return WaitFor(delegate
                    {
                        var el = Find(root, id);
                        return el != null && !el.Current.IsOffscreen;
                    }, 500);
                };
                try
                {
                    // D1: invite opens reset and focused on the email.
                    Invoke(openDrawer);
                    var invite = Find(root, "DrawerInvite");
                    var drEmail = Find(root, "DrEmail");
                    var drEmailError = Find(root, "DrEmailError");
                    var drEmailHint = Find(root, "DrEmailHint");
                    Check("D1 invite shown", invite != null && WaitFor(() => !invite.Current.IsOffscreen, 1000));
                    Check("D1 email focused", drEmail != null && WaitFor(() => drEmail.Current.HasKeyboardFocus, 1000));
                    Check("D1 invite reset", GetValue(drEmail) == "" && GetValue(Find(root, "DrNote")) == "" &&
                        OverlaysFormComboSelected(Find(root, "DrRole"), "Member") &&
                        FastToggle.Of(Find(root, "DrSendEmail")).Current.ToggleState == ToggleState.On);
                    Check("D1 invite error hidden", !drawerShown("DrEmailError"));

                    // D2/D3: invalid submit keeps the hint visible, typing clears the error.
                    Invoke(Find(root, "DrawerSubmit"));
                    Check("D2 invite error shown", drawerShown("DrEmailError") && drawerShown("DrEmailHint"));
                    Check("D2 invalid email focused", WaitFor(() => drEmail.Current.HasKeyboardFocus, 1000));
                    SetValue(drEmail, "a@b.co");
                    Check("D3 invite error clears", !drawerShown("DrEmailError"));
                    Invoke(Find(root, "DrawerSubmit"));
                    Check("D2 valid invite closes", WaitFor(() => invite.Current.IsOffscreen, 1000));
                    Check("D2 invite toast", FindByName(root, "Invite sent") != null);

                    // D4: edit opens with defaults and no confirm.
                    Invoke(openEditDrawer);
                    var edit = Find(root, "DrawerEdit");
                    var edEmail = Find(root, "EdEmail");
                    Check("D4 edit shown", edit != null && WaitFor(() => !edit.Current.IsOffscreen, 1000));
                    Check("D4 name focused", WaitFor(() => Find(root, "EdName").Current.HasKeyboardFocus, 1000));
                    Check("D4 defaults", GetValue(Find(root, "EdName")) == "Jamie Rivera" &&
                        GetValue(edEmail) == "jamie@example.com" &&
                        OverlaysFormComboSelected(Find(root, "EdRole"), "Member") &&
                        FastToggle.Of(Find(root, "EdActive")).Current.ToggleState == ToggleState.On);
                    Check("D4 confirm hidden", !drawerShown("KeepMember"));

                    // D5/D6: save validates email only.
                    SetValue(edEmail, "bad");
                    Invoke(Find(root, "EditSave"));
                    Check("D5 invalid edit error", drawerShown("EdEmailError"));
                    Check("D5 invalid email focused", WaitFor(() => edEmail.Current.HasKeyboardFocus, 1000));
                    SetValue(edEmail, "ok@a.co");
                    Check("D6 edit error clears", !drawerShown("EdEmailError"));
                    Invoke(Find(root, "EditSave"));
                    Check("D5 valid edit closes", WaitFor(() => edit.Current.IsOffscreen, 1000));
                    Check("D5 edit toast", FindByName(root, "Changes saved") != null);

                    // D7/D8: delete shows the confirm and Keep member returns.
                    Invoke(openEditDrawer);
                    edit = Find(root, "DrawerEdit"); // re-find: UIA elements go stale after a close/reopen
                    Invoke(Find(root, "EditDelete"));
                    Check("D7 confirm shown", drawerShown("KeepMember"));
                    Check("D7 keep focused", WaitFor(() => Find(root, "KeepMember").Current.HasKeyboardFocus, 1000));
                    // D7 scoped Tab cycle: Keep member -> Delete member -> wrap.
                    PostKey(hwnd, 0x09);
                    Check("confirm Tab -> Delete member", WaitFor(() =>
                        Find(root, "ConfirmDelete").Current.HasKeyboardFocus, 1000));
                    PostKey(hwnd, 0x09);
                    Check("confirm Tab wraps -> Keep member", WaitFor(() =>
                        Find(root, "KeepMember").Current.HasKeyboardFocus, 1000));
                    Invoke(Find(root, "KeepMember"));
                    Check("D8 confirm hidden", !drawerShown("KeepMember"));
                    Check("D8 delete focused", WaitFor(() => Find(root, "EditDelete").Current.HasKeyboardFocus, 1000));

                    // D10: Escape hides only the confirm.
                    Invoke(Find(root, "EditDelete"));
                    // Let the confirm's focus change settle before posting the key.
                    WaitFor(() => drawerShown("KeepMember") && drawerShown("EdName"), 500);
                    PostKey(hwnd, 0x1B);
                    Check("D10 Escape hides confirm", !drawerShown("KeepMember"));
                    Check("D10 drawer stays open", drawerShown("EdName"));
                    Check("D10 delete focused", WaitFor(delegate
                    {
                        var del = Find(root, "EditDelete");
                        return del != null && del.Current.HasKeyboardFocus;
                    }, 1000));

                    // D9: Delete member closes and toasts.
                    Invoke(Find(root, "EditDelete"));
                    Invoke(Find(root, "ConfirmDelete"));
                    Check("D9 delete closes drawer", WaitFor(() => edit.Current.IsOffscreen, 1000));
                    Check("D9 delete toast", FindByName(root, "Member deleted") != null);

                    // D11: Cancel and X close without validating.
                    Invoke(openEditDrawer);
                    edit = Find(root, "DrawerEdit");
                    Invoke(Find(root, "EditCancel"));
                    Check("D11 Cancel closes edit", WaitFor(() => edit.Current.IsOffscreen, 1000));
                    Invoke(openDrawer);
                    Invoke(Find(root, "DrawerClose"));
                    Check("D11 X closes invite", WaitFor(() => invite.Current.IsOffscreen, 1000));
                }
                finally
                {
                    var open = Find(root, "DrawerInvite");
                    if (open != null && !open.Current.IsOffscreen) PostKey(hwnd, 0x1B);
                    open = Find(root, "DrawerEdit");
                    if (open != null && !open.Current.IsOffscreen) PostKey(hwnd, 0x1B);
                }
            }

            // Command palette (07 O5, K1-K7).
            var cmdkSection = Find(root, "CmdkCard");
            Check("cmdk card found", cmdkSection != null);
            var openCmdk = Find(cmdkSection ?? root, "OpenCmdk");
            Check("OpenCmdk found", openCmdk != null);
            if (openCmdk != null)
            {
                try
                {
                    Invoke(openCmdk);
                    var cmdkDialog = Find(root, "CmdkDialog");
                    var cmdkInput = Find(root, "CmdkInput");
                    Check("K1 palette shown", cmdkDialog != null && WaitFor(
                        () => !cmdkDialog.Current.IsOffscreen, 1000));
                    Check("K1 input focused", cmdkInput != null && WaitFor(
                        () => cmdkInput.Current.HasKeyboardFocus, 1000));
                    Console.WriteLine("CODE-ONLY  palette placeholder visible (TextBox template children are not in UIA; see capture)");
                    Check("K1 all eight items", Find(root, "CmdkItem7") != null && Find(root, "CmdkItem8") == null);
                    Check("K1 first item active", WaitFor(delegate
                    {
                        var item = Find(root, "CmdkItem0");
                        return item != null && item.Current.ItemStatus == "active";
                    }, 1000));

                    SetValue(Find(root, "CmdkInput"), "go");
                    Check("K2 filter leaves four items", WaitFor(
                        () => Find(root, "CmdkItem3") != null && Find(root, "CmdkItem4") == null, 1000));
                    Check("K2 one group header", Find(root, "CmdkGroup0") != null &&
                        Find(root, "CmdkGroup1") == null);
                    SetValue(Find(root, "CmdkInput"), "zzz");
                    Check("K2 empty state", WaitFor(() => Find(root, "CmdkEmpty") != null, 1000) &&
                        Find(root, "CmdkItem0") == null);

                    SetValue(Find(root, "CmdkInput"), "");
                    Drv.Focus(Find(root, "CmdkInput"));
                    PostKey(hwnd, 0x28); // VK_DOWN: item 0 -> item 1
                    Check("K3 Down moves active", WaitFor(delegate
                    {
                        var item = Find(root, "CmdkItem1");
                        return item != null && item.Current.ItemStatus == "active";
                    }, 1000), "active=" + OverlaysActivePaletteItems(root));
                    PostKey(hwnd, 0x0D); // VK_RETURN runs "Invite a member"
                    Check("K4 Enter closes palette", WaitFor(delegate
                    {
                        var dialog = Find(root, "CmdkDialog");
                        return dialog == null || dialog.Current.IsOffscreen;
                    }, 1000));
                    Check("K4 Enter runs active command", FindByName(root, "Invite member") != null);

                    Invoke(openCmdk);
                    cmdkDialog = Find(root, "CmdkDialog");
                    SetValue(Find(root, "CmdkInput"), "");
                    Drv.Focus(Find(root, "CmdkInput"));
                    PostKey(hwnd, 0x26); // VK_UP from item 0 wraps to the last item
                    Check("K3 Up wraps to last", WaitFor(delegate
                    {
                        var item = Find(root, "CmdkItem7");
                        return item != null && item.Current.ItemStatus == "active";
                    }, 1000), "active=" + OverlaysActivePaletteItems(root));

                    Invoke(Find(root, "CmdkItem0"));
                    Check("K5 click closes palette", WaitFor(() => cmdkDialog.Current.IsOffscreen, 1000));
                    Check("K5 click runs command", FindByName(root, "Create project") != null);

                    // K6: "Go to Buttons" must target the section title, not the sidebar
                    // label. The page is scrolled down at this point, so the title is off
                    // screen until the command brings it into view.
                    var scroller = Find(root, "ContentScroll");
                    var scrollPattern = scroller == null ? null :
                        scroller.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
                    double scrollBefore = scrollPattern == null ? -1 : scrollPattern.Current.VerticalScrollPercent;
                    double scrollAfter = scrollBefore;
                    Invoke(openCmdk);
                    Invoke(Find(root, "CmdkItem3")); // "Go to Buttons"
                    Check("K6 Go to Buttons", WaitFor(delegate
                    {
                        var live = Find(root, "ContentScroll");
                        var sp = live == null ? null : live.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
                        scrollAfter = sp == null ? -1 : sp.Current.VerticalScrollPercent;
                        // Page was near the bottom; "Go to Buttons" is at the top, so it must scroll UP.
                        return scrollBefore >= 0 && scrollAfter < scrollBefore - 5;
                    }, 2000), "before=" + scrollBefore + " after=" + scrollAfter);

                    Invoke(openCmdk);
                    cmdkDialog = Find(root, "CmdkDialog");
                    PostKey(hwnd, 0x1B);
                    Check("K7 Escape closes palette", WaitFor(() => cmdkDialog.Current.IsOffscreen, 1000));
                }
                finally
                {
                    var dialog = Find(root, "CmdkDialog");
                    if (dialog != null && !dialog.Current.IsOffscreen) PostKey(hwnd, 0x1B);
                }
            }
        }

        private static string OverlaysActivePaletteItems(AutomationElement root)
        {
            string active = "";
            for (int i = 0; i < 9; i++)
            {
                var el = Find(root, "CmdkItem" + i);
                if (el != null && el.Current.ItemStatus == "active") active += i + ",";
            }
            return active.Length > 0 ? active : "none";
        }

        private static bool OverlaysFormComboSelected(AutomationElement combo, string name)
        {
            var selection = combo.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
            if (selection == null) return false;
            var items = selection.Current.GetSelection();
            return items.Length > 0 && items[0].Current.Name == name;
        }
    }
}
