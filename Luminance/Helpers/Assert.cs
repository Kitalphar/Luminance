using System.Diagnostics;

namespace Luminance.Helpers
{
    public static class Assert
    {
        public static void Equal<T>(T expected, T actual, string testName)
        {
            if (!Equals(expected, actual))
                Trace.WriteLine($"FAIL: {testName} (Expected {expected}, got {actual})");
            else
                Trace.WriteLine($"PASS: {testName}");
        }

        public static void NotEqual<T>(T expected, T actual, string testName)
        {
            if (Equals(expected, actual))
                Trace.WriteLine($"FAIL: {testName} (Expected {expected}, got {actual})");
            else
                Trace.WriteLine($"PASS: {testName}");
        }

        public static void DoesNotThrow(Action action, string testName)
        {
            try
            {
                action();
                Trace.WriteLine($"PASS: {testName}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"FAIL: {testName} (Unexpected exception: {ex.GetType().Name} - {ex.Message})");
            }
        }
    }
}
