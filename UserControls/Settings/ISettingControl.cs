using Shiftless.Clockwork.Assets.Editor.AssetManagement.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
