namespace BW.Libs.Config.Tests.Framework.AwsCredentials;

public static class AwsCredentialsParsingUtilities
{
    public static TestAwsSessionCredentials GetAwsCredentialsFromFile(string filePath)
    {
        var textBlock = File.ReadAllText(filePath);
        return ParseAwsCredentials(textBlock);
    }

    private static TestAwsSessionCredentials ParseAwsCredentials(string textBlock)
    {
        var lines = textBlock.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToArray();

        var dict = lines.Skip(1)
            .Select(line => line.Split('='))
            .ToDictionary(parts => parts[0], parts => parts[1]);

        var accessKey = dict["aws_access_key_id"];
        var secretKey = dict["aws_secret_access_key"];
        var sessionToken = dict.ContainsKey("aws_session_token") ? dict["aws_session_token"] : string.Empty;

        return new TestAwsSessionCredentials(accessKey, secretKey, sessionToken);
    }
}
