// <copyright file="ValueEncryptionTests{TData}.cs" company="Cimpress, Inc.">
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

namespace Test;

[Properties(QuietOnSuccess = true)]
public abstract class ValueEncryptionTests<TData>
    where TData : struct
{
    [Property(DisplayName = "Values can round-trip through encryption.")]
    public void RoundTripEncryption(TData datum)
    {
        IEncryption<TData> sut = new DataProtectorEncryption<TData>(
            new EphemeralDataProtectionProvider(NullLoggerFactory.Instance),
            NullLogger<DataProtectorEncryption<TData>>.Instance);

        var actual = sut.Decrypt(sut.Encrypt(datum));

        Assert.Equal(datum, actual);
    }
}

public sealed class DateTimeOffsetEncryptionTests
    : ValueEncryptionTests<DateTimeOffset>
{
}

public sealed class Int32EncryptionTests
    : ValueEncryptionTests<int>
{
}

public sealed class Int64EncryptionTests
    : ValueEncryptionTests<long>
{
}

public sealed class GuidEncryptionTests
    : ValueEncryptionTests<Guid>
{
}
