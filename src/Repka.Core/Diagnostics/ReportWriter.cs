﻿namespace Repka.Diagnostics
{
    public class ReportWriter : IDisposable
    {
        public virtual void Write(Report report)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
