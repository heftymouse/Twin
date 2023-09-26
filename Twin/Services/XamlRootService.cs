﻿using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twin.UI.Controls;
using WinRT.Interop;

namespace Twin.Services
{
    internal class XamlRootService
    {
        public XamlRoot Root;
        public IntPtr Hwnd => (Root.Content as RootFrame).Hwnd;
    }
}
