﻿// <copyright file="DataProtectorEncryption{TData}.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
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

using System;
using System.ComponentModel;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace Tiger.ContinuationToken
{
    /// <summary>Provides symmetric encryption and decryption utilities for <see cref="ContinuationToken{TData}"/>.</summary>
    /// <typeparam name="TData">The type on which to perform operations.</typeparam>
    public sealed class DataProtectorEncryption<TData>
        : IEncryption<TData>
        where TData : notnull
    {
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

        readonly IDataProtector _dataProtector;
        readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="DataProtectorEncryption{TData}"/> class.</summary>
        /// <param name="dataProtectionProvider">The application's provder of instances of <see cref="IDataProtector"/>.</param>
        /// <param name="logger">
        /// The appliation's logger, specialized for <see cref="DataProtectorEncryption{TData}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="dataProtectionProvider"/> is <see langword="null"/>.</exception>
        public DataProtectorEncryption(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<DataProtectorEncryption<TData>> logger)
        {
            if (dataProtectionProvider is null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            _dataProtector = dataProtectionProvider.CreateProtector("Tiger.DataProtectorEncryption`1.v3");
            _logger = logger;
        }

        /// <inheritdoc/>
        TData IEncryption<TData>.Decrypt(string ciphertext)
        {
            var plaintext = _dataProtector.Unprotect(ciphertext);
            try
            {
                return (TData)s_typeConverter.ConvertFromInvariantString(plaintext);
            }
            catch (NotSupportedException nse)
            {
                _logger.FromTokenFailed(typeof(TData), nse);
                throw new CryptographicException("The decrypted value cannot be converted into the provided type.", nse);
            }
        }

        /// <inheritdoc/>
        string IEncryption<TData>.Encrypt(TData value)
        {
            string plaintext;
            try
            {
                plaintext = s_typeConverter.ConvertToInvariantString(value);
            }
            catch (NotSupportedException nse)
            {
                _logger.ToTokenFailed(typeof(TData), nse);
                throw new CryptographicException("The value cannot be converted into a string for encryption.", nse);
            }

            return _dataProtector.Protect(plaintext);
        }
    }
}
