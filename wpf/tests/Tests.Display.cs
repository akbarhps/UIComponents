// Covered: cards, badges, alerts, progress bar (idle/start/ticks/complete/restart/mid-run and after-complete reset), avatars, empty state, skeleton.
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
        static partial void RunDisplay(AutomationElement root, ref bool ran)
        {
            ran = true;
            // Cards & badges (05-display). Scope: Draft/Active/Pending/Saved also occur
            // as data-table statuses or toast text elsewhere on the page.
            var displayCards = Find(root, "DisplayCards");
            var displayBadges = Find(root, "DisplayBadges");
            Check("DisplayCards found", displayCards != null);
            Check("DisplayBadges found", displayBadges != null);
            if (displayCards != null)
            {
                Check("card title Workspace settings found", FindByName(displayCards, "Workspace settings") != null);
                Check("card title Your plan found", FindByName(displayCards, "Your plan") != null);
                Check("card button Open settings found", FindByName(displayCards, "Open settings") != null);
                Check("card button Upgrade plan found", FindByName(displayCards, "Upgrade plan") != null);
            }
            if (displayBadges != null)
            {
                Check("badge Draft found", FindByName(displayBadges, "Draft") != null);
                Check("badge In review found", FindByName(displayBadges, "In review") != null);
                Check("badge Active found", FindByName(displayBadges, "Active") != null);
                Check("badge Pending found", FindByName(displayBadges, "Pending") != null);
                Check("badge Overdue found", FindByName(displayBadges, "Overdue") != null);
                Check("badge v1.0 found", FindByName(displayBadges, "v1.0") != null);
            }

            // Alerts + empty state (05-display phase 2)
            var displayAlerts = Find(root, "DisplayAlerts");
            var emptyState = Find(root, "EmptyState");
            Check("DisplayAlerts found", displayAlerts != null);
            Check("EmptyState found by id", emptyState != null);
            if (displayAlerts != null)
            {
                Check("alert Good to know found", FindByName(displayAlerts, "Good to know") != null);
                Check("alert Saved found", FindByName(displayAlerts, "Saved") != null);
                Check("alert Check your input found", FindByName(displayAlerts, "Check your input") != null);
                Check("alert Something went wrong found", FindByName(displayAlerts, "Something went wrong") != null);
            }
            if (emptyState != null)
            {
                Check("EmptyGuide found", Find(emptyState, "EmptyGuide") != null);
                Check("EmptyClear found", Find(emptyState, "EmptyClear") != null);
                Check("No results found", FindByName(emptyState, "No results found") != null);
            }

            // Progress bar (05-display phase 3): B2-B5 via RangeValuePattern.
            var progress = Find(root, "ProgressBar");
            var progressLabel = Find(root, "ProgressLabel");
            var progressValue = Find(root, "ProgressValue");
            var progressStart = Find(root, "ProgressStart");
            var progressReset = Find(root, "ProgressReset");
            Check("progress bar found", progress != null);
            Check("progress label found", progressLabel != null);
            Check("progress value found", progressValue != null);
            Check("progress start found", progressStart != null);
            Check("progress reset found", progressReset != null);
            if (progress != null)
            {
                var range = (RangeValuePattern)progress.GetCurrentPattern(RangeValuePattern.Pattern);
                Check("progress initial value 0", range.Current.Value == 0, "value=" + range.Current.Value);
                Check("progress initial label", progressLabel != null && GetValue(progressLabel) == "Uploading files…",
                    progressLabel == null ? "missing" : GetValue(progressLabel));
                Check("progress initial value text 0%", progressValue != null && GetValue(progressValue) == "0%",
                    progressValue == null ? "missing" : GetValue(progressValue));

                if (progressStart != null && progressReset != null)
                {
                    Invoke(progressStart);
                    // B3: let a real tick land before pressing Start again.
                    double first = 0;
                    DateTime firstDeadline = DateTime.Now.AddSeconds(4);
                    while (DateTime.Now < firstDeadline && first <= 0)
                    {
                        Thread.Sleep(100);
                        AutomationElement live = Find(root, "ProgressBar");
                        if (live == null) continue;
                        first = ((RangeValuePattern)live.GetCurrentPattern(RangeValuePattern.Pattern)).Current.Value;
                    }
                    Check("progress first tick > 0", first > 0, "value=" + first);
                    Invoke(progressStart);   // a second Start while running must be ignored
                    Check("progress label after second start", progressLabel != null && GetValue(progressLabel) == "Uploading files…",
                        progressLabel == null ? "missing" : GetValue(progressLabel));

                    double last = -1, current = first;
                    bool monotone = true;
                    DateTime deadline = DateTime.Now.AddSeconds(12);
                    while (DateTime.Now < deadline)
                    {
                        Thread.Sleep(100);
                        AutomationElement live = Find(root, "ProgressBar");
                        if (live == null) continue;
                        current = ((RangeValuePattern)live.GetCurrentPattern(RangeValuePattern.Pattern)).Current.Value;
                        if (current < last - 0.01) monotone = false;
                        last = current;
                        if (current >= 100) break;
                    }
                    Check("progress reaches 100", current >= 100, "value=" + current);
                    Check("progress non-decreasing", monotone);
                    Check("progress complete label", progressLabel != null && GetValue(progressLabel) == "Upload complete",
                        progressLabel == null ? "missing" : GetValue(progressLabel));
                    Check("progress complete value text", progressValue != null && GetValue(progressValue) == "100%",
                        progressValue == null ? "missing" : GetValue(progressValue));

                    // B2 edge: Start after completion keeps 100% until the first tick (no visual reset).
                    Invoke(progressStart);
                    double afterRestart = ((RangeValuePattern)progress.GetCurrentPattern(RangeValuePattern.Pattern)).Current.Value;
                    Check("progress no visual reset on restart", afterRestart >= 99.9, "value=" + afterRestart);
                    Check("progress label uploads on restart", progressLabel != null && GetValue(progressLabel) == "Uploading files…",
                        progressLabel == null ? "missing" : GetValue(progressLabel));
                    Check("progress text still 100% on restart", progressValue != null && GetValue(progressValue) == "100%",
                        progressValue == null ? "missing" : GetValue(progressValue));

                    // B5 after completion: Reset immediately, before the new run's first tick.
                    Invoke(progressReset);
                    Thread.Sleep(300);
                    AutomationElement afterReset = Find(root, "ProgressBar");
                    var resetRange = afterReset == null ? null : (RangeValuePattern)afterReset.GetCurrentPattern(RangeValuePattern.Pattern);
                    Check("progress reset to 0 after completion", resetRange != null && resetRange.Current.Value == 0,
                        resetRange == null ? "missing" : "value=" + resetRange.Current.Value);
                    Check("progress reset label after completion", progressLabel != null && GetValue(progressLabel) == "Uploading files…",
                        progressLabel == null ? "missing" : GetValue(progressLabel));
                    Check("progress reset text after completion", progressValue != null && GetValue(progressValue) == "0%",
                        progressValue == null ? "missing" : GetValue(progressValue));

                    // B5 mid-run: Start, wait for a tick, then Reset.
                    Invoke(progressStart);
                    double mid = 0;
                    DateTime midDeadline = DateTime.Now.AddSeconds(4);
                    while (DateTime.Now < midDeadline && mid <= 0)
                    {
                        Thread.Sleep(100);
                        AutomationElement live = Find(root, "ProgressBar");
                        if (live == null) continue;
                        mid = ((RangeValuePattern)live.GetCurrentPattern(RangeValuePattern.Pattern)).Current.Value;
                    }
                    Check("progress mid-run tick > 0", mid > 0, "value=" + mid);
                    Invoke(progressReset);
                    Thread.Sleep(300);
                    afterReset = Find(root, "ProgressBar");
                    resetRange = afterReset == null ? null : (RangeValuePattern)afterReset.GetCurrentPattern(RangeValuePattern.Pattern);
                    Check("progress reset to 0 mid-run", resetRange != null && resetRange.Current.Value == 0,
                        resetRange == null ? "missing" : "value=" + resetRange.Current.Value);
                    Check("progress reset label mid-run", progressLabel != null && GetValue(progressLabel) == "Uploading files…",
                        progressLabel == null ? "missing" : GetValue(progressLabel));
                    Check("progress reset text mid-run", progressValue != null && GetValue(progressValue) == "0%",
                        progressValue == null ? "missing" : GetValue(progressValue));
                }
            }

            // Avatars (05-display phase 3) — scoped: AL also appears in the data table
            var displayAvatars = Find(root, "DisplayAvatars");
            Check("DisplayAvatars found", displayAvatars != null);
            if (displayAvatars != null)
            {
                int alCount = CountByName(displayAvatars, "AL");
                Check("AL avatars found at least 4", alCount >= 4, "count=" + alCount);
                Check("avatar JS found", FindByName(displayAvatars, "JS") != null);
                Check("avatar MK found", FindByName(displayAvatars, "MK") != null);
                Check("avatar name Ada Lovelace found", FindByName(displayAvatars, "Ada Lovelace") != null);
            }

            // Empty state
            var empty = Find(root, "EmptyState") ?? FindByName(root, "No projects yet");
            Check("empty state found", empty != null);
            var emptyAction = Find(root, "EmptyAction");
            Check("empty state action found", emptyAction != null);
            if (emptyAction != null) Invoke(emptyAction);

            // Skeleton (05-display phase 4)
            var skeleton = Find(root, "Skeleton");
            Check("skeleton found by id", skeleton != null);
            if (skeleton != null)
                Check("Table rows found", FindByName(skeleton, "Table rows") != null);
            // The id is on a Border (no UIA peer); the TextBlock is the findable node.
            var cueScope = skeleton ?? root;
            var loadingCue = FindByName(cueScope, "Loading content…");
            Check("motion cue hidden", loadingCue == null || loadingCue.Current.IsOffscreen);
        }
    }
}
