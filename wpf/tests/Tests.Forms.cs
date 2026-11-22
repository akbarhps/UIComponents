// Covered: text fields, forms (search round-trip, multiline textarea, username error, disabled checkbox, radio group, switch, role select), checkbox, combobox (select).
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
        static partial void RunForms(AutomationElement root, ref bool ran)
        {
            ran = true;
            // Text fields — set via ValuePattern, no typing, no cursor
            var name = Find(root, "FieldName");
            Check("name field found", name != null);
            if (name != null)
            {
                SetValue(name, "Grace Hopper");
                Check("name field value set", GetValue(name) == "Grace Hopper", "value=" + GetValue(name));
            }

            var email = Find(root, "FieldEmail");
            Check("email field found", email != null);
            if (email != null)
            {
                SetValue(email, "grace@example.com");
                Check("email field value set", GetValue(email) == "grace@example.com");
            }

            // O0 probe (overlays): PostMessage Tab from the first Forms input. Decides K vs C for O5.
            var o0Name = Find(root, "FieldName");
            var o0Email = Find(root, "FieldEmail");
            if (o0Name != null && o0Email != null)
            {
                Drv.Focus(o0Name);
                Thread.Sleep(100);
                bool o0Before = o0Name.Current.HasKeyboardFocus;
                PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x09); // VK_TAB
                Thread.Sleep(150);
                var o0Focused = AutomationElement.FocusedElement;
                Check("O0 probe Tab moves FieldName -> FieldEmail", o0Before && o0Email.Current.HasKeyboardFocus,
                    "before=" + o0Before + " focusedId=" + (o0Focused == null ? "none" : o0Focused.Current.AutomationId));
            }

            // Forms — search round-trip, multiline textarea, error-state field
            var search = Find(root, "FieldSearch");
            Check("search field found", search != null);
            if (search != null)
            {
                SetValue(search, "needle");
                Check("search field value set", GetValue(search) == "needle", "value=" + GetValue(search));
            }

            var message = Find(root, "FieldMessage");
            Check("message field found", message != null);
            if (message != null)
            {
                string text = "first line\nsecond line";
                SetValue(message, text);
                string got = GetValue(message).Replace("\r\n", "\n");
                Check("message field multiline value", got == text, "value=" + got);
            }

            var username = Find(root, "FieldUsername");
            Check("username field found", username != null);
            if (username != null) Check("username value is ada#", GetValue(username) == "ada#", "value=" + GetValue(username));
            Check("username error found", Find(root, "FieldUsernameError") != null);

            // CheckBox — toggle via TogglePattern
            var check = Find(root, "ChkNotifications");
            Check("checkbox found", check != null);
            if (check != null)
            {
                var pattern = FastToggle.Of(check);
                var before = pattern.Current.ToggleState;
                pattern.Toggle();
                Check("checkbox toggles", pattern.Current.ToggleState != before);
                pattern.Toggle();
            }

            // Selection controls — disabled checkbox, radio group, switch
            var unavailable = Find(root, "ChkUnavailable");
            Check("unavailable checkbox found", unavailable != null);
            if (unavailable != null) Check("unavailable checkbox disabled", !unavailable.Current.IsEnabled);

            var starter = Find(root, "RadStarter");
            var pro = Find(root, "RadPro");
            Check("radio buttons found", starter != null && pro != null);
            if (starter != null && pro != null)
            {
                var proSelect = FastSelectionItem.Of(pro);
                var starterSelect = FastSelectionItem.Of(starter);
                Check("starter selected initially", starterSelect.Current.IsSelected);
                proSelect.Select();
                Check("radio selection moves", proSelect.Current.IsSelected && !starterSelect.Current.IsSelected);
                starterSelect.Select(); // restore the demo's default state
            }

            var sw = Find(root, "SwDarkMode");
            Check("switch found", sw != null);
            if (sw != null)
            {
                var toggleSwitch = FastToggle.Of(sw);
                if (toggleSwitch.Current.ToggleState == ToggleState.On) toggleSwitch.Toggle();
                toggleSwitch.Toggle();
                Check("switch toggles on", toggleSwitch.Current.ToggleState == ToggleState.On);
                toggleSwitch.Toggle();
                Check("switch toggles off", toggleSwitch.Current.ToggleState == ToggleState.Off);
                toggleSwitch.Toggle(); // restore the demo's default On state
            }

            // ComboBox — select via SelectionPattern / ExpandCollapsePattern
            var combo = Find(root, "CmbRole");
            Check("combobox found", combo != null);
            if (combo != null)
            {
                var expand = FastExpandCollapse.Of(combo);
                expand.Expand();
                Check("combobox expands", expand.Current.ExpandCollapseState == ExpandCollapseState.Expanded);
                var engineer = combo.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "Engineer"));
                Check("role item Engineer found", engineer != null);
                if (engineer != null)
                {
                    var engineerSelect = FastSelectionItem.Of(engineer);
                    engineerSelect.Select();
                    var selection = (SelectionPattern)combo.GetCurrentPattern(SelectionPattern.Pattern);
                    var selected = selection.Current.GetSelection();
                    Check("role selection changes value", selected.Length > 0 && selected[0].Current.Name == "Engineer",
                        "count=" + selected.Length);
                }
                expand.Collapse();
            }
        }
    }
}
