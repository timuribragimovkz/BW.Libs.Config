namespace BW.Libs.Config.Tests.Framework.AwsCredentials;

public sealed class TestAwsSessionCredentials
{
    public string AwsAccessKeyId { get; }
    public string AwsSecretAccessKey { get; }
    public string AwsSessionToken { get; }

    public TestAwsSessionCredentials(string awsAccessKeyId, string awsSecretAccessKey, string awsSessionToken)
    {
        AwsAccessKeyId = awsAccessKeyId;
        AwsSecretAccessKey = awsSecretAccessKey;
        AwsSessionToken = awsSessionToken;
    }
}
