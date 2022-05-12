namespace Platypus.WebApi.Shared.Core.Config;

public class OAuthConfig
{
    public const string Position = "OAuth";
    public string OAuthUrl { get; set; } = "http://skagerakoauthsimulator:1752";
    public OAuthConfigMode Mode { get; set; } = OAuthConfigMode.Symmetric;
    public string SymmetricPrivateKey { get; set; } = "this is very secret"; //todo: SecureString?
}

public enum OAuthConfigMode
{
    Symmetric,
    Asymmetric
}