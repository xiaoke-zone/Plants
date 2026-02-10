using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.HotUpdate
{
    internal class HotFix
    {
    }

    //internal class AOTCompilerHelper
    //{
    //    private void EnsureAOT()
    //    {
    //        // 这里的代码永远不会被执行，但会让编译器强制生成对应的泛型实例
    //        var system = QFramework.TypeEventSystem.Global;

    //        system.Send<SunChangedEvent>(default);
    //        system.Send<OnCardSelectedEvent>(default);
    //        system.Send<OnShovelSelectedEvent>(default);
    //        system.Send<StartBattleEvent>(default);

    //        // 如果有 Register 也最好写一下
    //        system.Register<SunChangedEvent>(e => { });
    //        system.Register<OnCardSelectedEvent>(e => { });
    //        system.Register<OnShovelSelectedEvent>(e => { });
    //        system.Register<StartBattleEvent>(e => { });
    //    }
    //}
}
