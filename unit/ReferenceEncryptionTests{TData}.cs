// <copyright file="ReferenceEncryptionTests{TData}.cs" company="Cimpress, Inc.">
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

using AWS.EncryptionSDK;
using AWS.EncryptionSDK.Core;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Test;

[Properties(QuietOnSuccess = true)]
public abstract class ReferenceEncryptionTests<TData>
    where TData : class
{
    [Property(DisplayName = "References can round-trip through encryption.")]
    public void RoundTripEncryption(NonNull<TData> datum, NonEmptyString environmentName)
    {
        var keyring = Mock.Of<IKeyring>();
        var env = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == environmentName.Get);
        var serde = new Mock<IAwsEncryptionSdk>();
        _ = serde
            .Setup(s => s.Encrypt(It.IsAny<EncryptInput>()))
            .Returns<EncryptInput>(ei => new()
            {
                Ciphertext = ei.Plaintext,
                EncryptionContext = ei.EncryptionContext,
            });
        _ = serde
            .Setup(s => s.Decrypt(It.IsAny<DecryptInput>()))
            .Returns<DecryptInput>(di => new()
            {
                Plaintext = di.Ciphertext,
                EncryptionContext = new()
                {
                    ["Environment"] = environmentName.Get,
                },
            });
        IEncryption<TData> sut = new AwsKmsEncryption<TData>(
            serde.Object,
            keyring,
            env,
            NullLogger<AwsKmsEncryption<TData>>.Instance);

        var actual = sut.Decrypt(sut.Encrypt(datum.Get));

        Assert.Equal(datum.Get, actual);
    }
}

public sealed class StringEncryptionTests
    : ReferenceEncryptionTests<string>
{
}
