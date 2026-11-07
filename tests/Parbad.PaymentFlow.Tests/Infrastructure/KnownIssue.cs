using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.PaymentFlow.Tests.Infrastructure;

/// <summary>
/// Runs a test body that asserts the <b>correct</b> behaviour while a documented bug still exists.
/// <list type="bullet">
/// <item>If the assertions still fail, the bug reproduces: the test is reported as <i>ignored</i>
/// (shown as "skipped") with the issue, so it is visible without breaking the build.</item>
/// <item>If the assertions pass, the bug has been fixed: the test <b>fails</b> and asks you to remove the
/// known-issue marker, so from then on the test guards the fix.</item>
/// </list>
/// The issue is a short description of the bug.
/// </summary>
public static class KnownIssue
{
    public static async Task ReproducesAsync(string issue, Func<Task> assertCorrectBehaviour)
    {
        if (string.IsNullOrWhiteSpace(issue)) throw new ArgumentException("A description of the issue is required.", nameof(issue));

        try
        {
            await assertCorrectBehaviour();
        }
        catch (AssertionException failure)
        {
            Assert.Ignore($"KNOWN ISSUE {issue} reproduces: {failure.Message}");
        }

        Assert.Fail($"Known issue {issue} no longer reproduces. Remove the known-issue marker so this test guards the fix.");
    }

    public static Task ReproducesAsync(string issue, Action assertCorrectBehaviour) =>
        ReproducesAsync(issue, () =>
        {
            assertCorrectBehaviour();
            return Task.CompletedTask;
        });
}
