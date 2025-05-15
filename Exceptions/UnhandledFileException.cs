namespace Shiftless.Clockwork.Assets.Editor.Exceptions
{
    public class UnhandledFileException(string path) : Exception($"Asset at {path} went unhandled!")
    {
    }
}
