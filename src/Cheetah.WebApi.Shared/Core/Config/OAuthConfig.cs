namespace Cheetah.WebApi.Shared.Core.Config;

public class OAuthConfig
{
    public const string Position = "OAuth";
    public string OAuthUrl { get; set; } = "http://cheetahoauthsimulator:80";
    public OAuthConfigMode Mode { get; set; } = OAuthConfigMode.Asymmetric;
    public string SymmetricPrivateKey { get; set; } = "this is very secret"; //todo: SecureString?
}

public enum OAuthConfigMode
{
    Symmetric,
    Asymmetric
}