namespace Swarm.Application.Common;

public static class Guard
{
    public static void ValidSaveName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidSaveNameException($"{paramName} cannot be null or empty.");

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidSaveNameException($"{paramName} contains invalid characters.");
    }
}