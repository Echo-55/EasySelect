using System;
using Object = UnityEngine.Object;

namespace EasySelect.Utils;

public class Preconditions
{
    public Preconditions() { }

    public static T CheckNotNull<T>(T reference, string message = null)
    {
        if (reference is Object obj && !obj)
        {
            throw new ArgumentNullException(message);
        }

        if (reference is null)
        {
            throw new ArgumentNullException(message);
        }

        return reference;
    }

    public static void CheckState(bool expression, string messageTemplate, params object[] messageArgs)
    {
        CheckState(expression, string.Format(messageTemplate, messageArgs));
    }

    private static void CheckState(bool expression, string message = null)
    {
        if (expression)
        {
            return;
        }

        throw message == null ? new InvalidOperationException() : new InvalidOperationException(message);
    }

    public static void CheckArgument(bool expression, string messageTemplate, params object[] messageArgs)
    {
        CheckArgument(expression, string.Format(messageTemplate, messageArgs));
    }

    private static void CheckArgument(bool expression, string message = null)
    {
        if (expression)
        {
            return;
        }

        throw message == null ? new ArgumentException() : new ArgumentException(message);
    }
}