using System;
using System.Security.Cryptography;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Tiger.ContinuationToken;
using Xunit;

namespace Test
{
    [Properties(
        Arbitrary = new[] { typeof(Generators) },
        QuietOnSuccess = true)]
    public abstract class EncryptionTests
    {
    }

    public abstract class ValueEncryptionTests<TData>
        : EncryptionTests
        where TData: struct
    {
        [Property(DisplayName = "Values can round-trip through encryption.")]
        public void RoundTripEncryption(NonNull<string> password, byte[] salt, PositiveInt iterations, TData datum)
        {
            IEncryption<TData> sut = new SymmetricEncryption<TData>(
                new AesManaged(),
                new Rfc2898DeriveBytes(password.Get, salt, iterations.Get),
                new NullLogger<SymmetricEncryption<TData>>());

            var actual = sut.Decrypt(sut.Encrypt(datum));

            Assert.Equal(datum, actual);
        }
    }

    public abstract class ReferenceEncryptionTests<TData>
        : EncryptionTests
        where TData : class
    {
        [Property(DisplayName = "References can round-trip through encryption.")]
        public void RoundTripEncryption(NonNull<string> password, byte[] salt, PositiveInt iterations, NonNull<TData> datum)
        {
            IEncryption<TData> sut = new SymmetricEncryption<TData>(
                new AesManaged(),
                new Rfc2898DeriveBytes(password.Get, salt, iterations.Get),
                new NullLogger<SymmetricEncryption<TData>>());

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
