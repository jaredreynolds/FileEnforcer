using System;

namespace FileEnforcer.Services
{
    public interface IService : IDisposable
    {
        void Start();
    }
}
