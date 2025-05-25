using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Scene.Interfaces;

public interface IMeshProviderEventListener
{
    public void OnProviderAvailable();

    public void OnProviderUnavailable();
}