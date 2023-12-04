using System;

namespace Cheetah.Core.Authentication
{
    /// <summary>
    /// Exception thrown when an OAuth2 token could not be retrieved
    /// </summary>
    public class OAuth2TokenException : Exception
    {
        /// <summary>
        /// Create a new instance of <see cref="OAuth2TokenException"/>
        /// </summary>
        /// <param name="error">An error message describing what went wrong</param>
        public OAuth2TokenException(string? error) : base(error) { }
    }
}
