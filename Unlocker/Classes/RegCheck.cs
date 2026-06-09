using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d2d.Classes
{
    internal static class RegCheck
    {
        internal static bool FirstLaunch()
        {
            RegistryKey MainKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\d2d");

            using (RegistryKey key = MainKey.CreateSubKey("Run"))
            {
                if (key != null)
                {
                    Object? FirstRun = key?.GetValue("FirstRun");
                    if (FirstRun != null)
                    {
                        if (FirstRun?.ToString() == "0")
                        {
                            return false;
                        }
                    }

                    key.SetValue("FirstRun", "0");
                    return true;
                }
            }

            return true;
        }
    }
}
