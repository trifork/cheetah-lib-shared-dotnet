namespace Cheetah.Core.Infrastructure.Auth
{
    public class OAuth2TokenException : Exception
    {
        public string Error { get; }
        public OAuth2TokenException(string error) : base(error)
        {
            Error = error;
        }
    }
}
