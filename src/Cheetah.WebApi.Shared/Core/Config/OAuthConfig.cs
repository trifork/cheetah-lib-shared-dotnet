namespace Cheetah.WebApi.Shared.Core.Config
{
    /// <summary>
    /// OAuthConfig for IOptions. Can be used by WebApi's to authenticate user http requests.
    /// </summary>
    public class OAuthConfig
    {
        /// <summary>
        /// Prefix for options e.g. OAuth__
        /// </summary>
        public const string Position = "OAuth";

        /// <summary>
        /// OAuth url for WebApi to authenticate http requests bearer tokens
        /// </summary>
        /// <value></value>
        public string OAuthUrl { get; set; } = "http://cheetahoauthsimulator:80";

        /// <summary>
        /// Can be used to determine if authentication should be validated symmetric or asymmetric
        /// </summary>
        /// <value></value>
        public OAuthConfigMode Mode { get; set; } = OAuthConfigMode.Asymmetric;

        /// <summary>
        /// Secret used by Symmetric validation
        /// </summary>
        /// <value></value>
        public string SymmetricPrivateKey { get; set; } = "this is very secret"; //todo: SecureString or mounted secret?

        public enum OAuthConfigMode
        {
            Symmetric,
            Asymmetric
        }
    }
}
