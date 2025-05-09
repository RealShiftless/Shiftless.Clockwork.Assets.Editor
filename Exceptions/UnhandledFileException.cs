using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiftless.Clockwork.Assets.Editor.Exceptions
{
    public class UnhandledFileException(string path) : Exception($"Asset at {path} went unhandled!")
    {
    }
}
