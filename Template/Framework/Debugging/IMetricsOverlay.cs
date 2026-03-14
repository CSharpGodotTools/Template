using System;

namespace __TEMPLATE__.Debugging;

public interface IMetricsOverlay
{
    void StartMonitoring(string key, Func<object> function);
    void StopMonitoring(string key);
}
