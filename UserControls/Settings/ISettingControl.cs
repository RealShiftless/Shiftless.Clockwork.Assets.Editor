using System.Windows.Controls;

namespace Shiftless.Clockwork.Assets.Editor.UserControls.Settings
{
    public interface ISettingControl
    {
        event Action<object>? ValueChanged;

        void Awake(object obj);

        UserControl AsControl();
    }
}
