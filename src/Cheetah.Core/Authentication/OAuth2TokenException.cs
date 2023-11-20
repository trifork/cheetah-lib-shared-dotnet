using System;

namespace Cheetah.Core.Authentication
{
    public class OAuth2TokenException : Exception
    {
        public OAuth2TokenException(string? error) : base(error) { }
    }
}
