using System;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments
{
    public class CustomerIdentifier : IndexFragment
    {
        public CustomerIdentifier(string value)
            : base(value) { }

        public bool IsMatch(string thatValue)
        {
            return string.Equals(Value, thatValue, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
