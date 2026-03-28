using System;

namespace __TEMPLATE__.Debugging;

public interface IMetricsOverlay
{
    IDisposable StartMonitoring(string key, Func<object> function);
}
