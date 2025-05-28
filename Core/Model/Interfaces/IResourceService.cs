using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Model.Interfaces
{
    public interface IResourceService
    {
        string GetString(string key, params object[] formatArgs);
    }
}
