// <copyright file="AwsKmsEncryption{TData}.cs" company="Cimpress, Inc.">
//   Copyright 2020–2022 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

namespace Tiger.ContinuationToken;

/// <summary>
/// Provides symmetric encryption and decryption utilities
/// for <see cref="ContinuationToken{TData}"/> via AWS KMS.
/// </summary>
/// <typeparam name="TData">The type on which to perform operations.</typeparam>
sealed partial class AwsKmsEncryption<TData>
    : IEncryption<TData>
    where TData : notnull
{
    const string EnvironmentContextKey = "Environment";

    static readonly Encoding s_encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /* note(cosborn)
     * DateTimeOffset is a special type. We want its string representation to be
     * the roundtrip kind, not the weirdo kind that the default TypeConverter gives us.
     * Yay for reified types, I guess? With erasure, this would be next to impossible.
     * I mean, Java-style type erasure, where you don't have typeclasses to opt into.
     * Wanna know how to do it despite erasure? No, you don't. Don't look. Don't read on.
     * [Attach TypeConverterAttribute to DateTimeOffset at runtime, convert, remove,
     * wash hands forever, never stop washing hands. And that might *still* not work!
     * It's certainly not thread-safe, as well.]
     */
    static readonly System.ComponentModel.TypeConverter s_typeConverter = typeof(TData) == typeof(DateTimeOffset)
        ? new RoundTripDateTimeOffsetConverter()
        : TypeDescriptor.GetConverter(typeof(TData));

    readonly IAwsEncryptionSdk _serde;
    readonly IKeyring _keyring;
    readonly IHostEnvironment _env;
    readonly ILogger _logger;

    /// <summary>Initializes a new instance of the <see cref="AwsKmsEncryption{TData}"/> class.</summary>
    /// <param name="serde">A KMS Encrypttion SDK client.</param>
    /// <param name="keyring">The application's KMS keychain.</param>
    /// <param name="env">The environment in which the application is running.</param>
    /// <param name="logger">
    /// The appliation's logger, specialized for <see cref="AwsKmsEncryption{TData}"/>.
    /// </param>
    public AwsKmsEncryption(IAwsEncryptionSdk serde, IKeyring keyring, IHostEnvironment env, ILogger<AwsKmsEncryption<TData>> logger)
    {
        _serde = serde;
        _keyring = keyring;
        _env = env;
        _logger = logger;
    }

    /// <inheritdoc/>
    TData? IEncryption<TData>.Decrypt(string ciphertext)
    {
        if (ciphertext is not { } ct)
        {
            return default;
        }

        DecryptOutput decryptOutput;
        try
        {
            using var stream = new MemoryStream(Convert.FromBase64String(ct));
            decryptOutput = _serde.Decrypt(new()
            {
                Ciphertext = stream,
                Keyring = _keyring,
            });
        }
        catch (AwsEncryptionSdkException aese)
        {
            FromTokenFailed(typeof(TData), aese);
            throw new CryptographicException("The value cannot be decrypted.", aese);
        }

        if (!decryptOutput.EncryptionContext.TryGetValue(EnvironmentContextKey, out var environmentContextValue)
            || environmentContextValue != _env.EnvironmentName)
        {
            IncorrectEncryptionContext(environmentContextValue);
            throw new CryptographicException("The value cannot be decrypted.");
        }

        try
        {
            var plaintext = s_encoding.GetString(decryptOutput.Plaintext.ToArray());
            return (TData?)s_typeConverter.ConvertFromInvariantString(plaintext);
        }
        catch (NotSupportedException nse)
        {
            FromTokenFailed(typeof(TData), nse);
            throw new CryptographicException("The value cannot be decrypted.", nse);
        }
    }

    /// <inheritdoc/>
    string IEncryption<TData>.Encrypt(TData? value)
    {
        if (value is not { } v)
        {
            return string.Empty;
        }

        string? plaintext;
        try
        {
            plaintext = s_typeConverter.ConvertToInvariantString(v);
        }
        catch (NotSupportedException nse)
        {
            ToTokenFailed(typeof(TData), nse);
            throw new CryptographicException("The value cannot be converted into a string for encryption.", nse);
        }

        if (plaintext is not { } pt)
        {
            return string.Empty;
        }

        EncryptOutput encryptOutput;
        try
        {
            using var stream = new MemoryStream(s_encoding.GetBytes(pt));
            encryptOutput = _serde.Encrypt(new()
            {
                EncryptionContext = new()
                {
                    [EnvironmentContextKey] = _env.EnvironmentName,
                },
                Keyring = _keyring,
                Plaintext = stream,
            });
        }
        catch (AwsEncryptionSdkException aese)
        {
            ToTokenFailed(typeof(TData), aese);
            throw new CryptographicException("The value cannot be encrypted.", aese);
        }

        return Convert.ToBase64String(encryptOutput.Ciphertext.ToArray());
    }

    [LoggerMessage(eventId: 1, Error, "Can't convert a value of type {Type} into a token!")]
    partial void ToTokenFailed(Type type, Exception e);

    [LoggerMessage(eventId: 2, Error, "Can't convert a token into a value of type {Type}!")]
    partial void FromTokenFailed(Type type, Exception e);

    [LoggerMessage(eventId: 3, Error, "Value's encryption context's environment '{Environment:l}' failed to match the deployed environment!")]
    partial void IncorrectEncryptionContext(string? environment);
}
