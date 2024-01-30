using System;

namespace Cheetah.Kafka
{
    internal interface IOptionsBuilder<out TOptions> where TOptions : new()
    {
        internal TOptions Build(IServiceProvider serviceProvider);
    }
}
