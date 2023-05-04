using System;

namespace Cheetah.WebApi.Shared.Infrastructure.Services.indexFragments
{
    public class IndexFragmentException : ArgumentException
    {
        public override string Message =>
            "Value must be a non empty string consisting of letters and numbers";
    }
}
