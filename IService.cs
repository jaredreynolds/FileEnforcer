using System;

namespace FileEnforcer
{
    public interface IService : IDisposable
    {
        void Start();
    }
}
