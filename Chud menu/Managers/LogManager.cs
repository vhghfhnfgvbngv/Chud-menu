using System;

namespace Chud.Managers;

public static class LogManager
{
	public static void Log(object message)
	{
		Console.WriteLine($"[Chud] {message}");
	}

	public static void LogError(object message)
	{
		Console.WriteLine($"[Chud:ERROR] {message}");
	}

	public static void LogWarning(object message)
	{
		Console.WriteLine($"[Chud:WARN] {message}");
	}
}
