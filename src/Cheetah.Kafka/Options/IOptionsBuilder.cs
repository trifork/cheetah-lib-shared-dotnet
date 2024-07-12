using System;

namespace Cheetah.Kafka
{
    internal interface IOptionsBuilder<out TOptions> where TOptions : new()
    {
        TOptions Build(IServiceProvider serviceProvider);
    }
}
