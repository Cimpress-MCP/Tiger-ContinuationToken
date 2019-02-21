using System;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Tiger.ContinuationToken;
using Xunit;

namespace Test
{
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true)]
    public abstract class EncryptionTests
    {
    }

    public abstract class ValueEncryptionTests<TData>
        : EncryptionTests
        where TData: struct
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

    public abstract class ReferenceEncryptionTests<TData>
        : EncryptionTests
        where TData : class
    {
        [Property(DisplayName = "References can round-trip through encryption.")]
        public void RoundTripEncryption(NonNull<TData> datum)
        {
            IEncryption<TData> sut = new DataProtectorEncryption<TData>(
                new EphemeralDataProtectionProvider(NullLoggerFactory.Instance),
                NullLogger<DataProtectorEncryption<TData>>.Instance);

            var actual = sut.Decrypt(sut.Encrypt(datum.Get));

            Assert.Equal(datum.Get, actual);
        }
    }

    public class DateTimeOffsetEncryptionTests
        : ValueEncryptionTests<DateTimeOffset>
    {
    }

    public class Int32EncryptionTests
        : ValueEncryptionTests<int>
    {
    }

    public class Int64EncryptionTests
        : ValueEncryptionTests<long>
    {
    }

    public class GuidEncryptionTests
        : ValueEncryptionTests<Guid>
    {
    }

    public class StringEncryptionTests
        : ReferenceEncryptionTests<string>
    {
    }
}
