// <copyright file="SymmetricEncryption{TData}.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using static System.Security.Cryptography.CryptoStreamMode;

namespace Tiger.ContinuationToken
{
    /// <summary>Provides symmetric encryption and decryption utilities for <see cref="ContinuationToken{TData}"/>.</summary>
    /// <typeparam name="TData">The type on which to perform operations.</typeparam>
    public sealed class SymmetricEncryption<TData>
        : IEncryption<TData>
    {
        /* note(cosborn)
         * DateTimeOffset is a special type. We want its string representation to be
         * the roundtrip kind, not the weirdo kind that the default TypeConverter gives us.
         * Yay for reified types, I guess? With erasure, this would be next to impossible.
         * Wanna know how to do it despite erasure? No, you don't. Don't look. Don't read on.
         * [Attach TypeConverterAttribute to DateTimeOffset at runtime, convert, remove,
         * wash hands forever, never stop washing hands. And that might *still* not work!
         * It's certainly not thread-safe, as well.]
         */
        static readonly System.ComponentModel.TypeConverter s_typeConverter = typeof(TData) == typeof(DateTimeOffset)
            ? new RoundTripDateTimeOffsetConverter()
            : TypeDescriptor.GetConverter(typeof(TData));

        static readonly Encoding s_encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        readonly SymmetricAlgorithm _algorithm;
        readonly ILogger _logger;

        readonly Lazy<byte[]> _key;
        readonly Lazy<byte[]> _iv;

        /// <summary>Initializes a new instance of the <see cref="SymmetricEncryption{TData}"/> class.</summary>
        /// <param name="algorithm">The algorithm with which to perform encryption and decryption operations.</param>
        /// <param name="deriveBytes">The byte derivation service.</param>
        /// <param name="logger">
        /// The appliation's logger, specialized for <see cref="SymmetricEncryption{TData}"/>.
        /// </param>
        public SymmetricEncryption(
            [NotNull] SymmetricAlgorithm algorithm,
            [NotNull] DeriveBytes deriveBytes,
            [NotNull] ILogger<SymmetricEncryption<TData>> logger)
        {
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // note(cosborn) We don't want these recalculated if the instance sticks around.
            _key = new Lazy<byte[]>(() => deriveBytes.GetBytes(_algorithm.KeySize / 8));
            _iv = new Lazy<byte[]>(() => deriveBytes.GetBytes(_algorithm.BlockSize / 8));
        }

        /// <inheritdoc/>
        TData IEncryption<TData>.Decrypt(string ciphertext)
        {
            if (ciphertext is null) { throw new ArgumentNullException(nameof(ciphertext)); }

            byte[] cipherbytes;
            try
            {
                cipherbytes = Convert.FromBase64String(ciphertext);
            }
            catch (FormatException fe)
            {
                throw new CryptographicException("The encrypted value is in a bad format.", fe);
            }

            string plaintext;
            using (var ms = new MemoryStream(cipherbytes))
            using (var de = _algorithm.CreateDecryptor(_key.Value, _iv.Value))
            using (var cs = new CryptoStream(ms, de, Read))
            using (var sr = new StreamReader(cs, s_encoding))
            {
                plaintext = sr.ReadToEnd();
            }

            try
            {
                return (TData)s_typeConverter.ConvertFromInvariantString(plaintext);
            }
            catch (NotSupportedException nse)
            {
                _logger.LogError(nse, "Can't convert token into a value of type {Type}!", typeof(TData));
                throw new CryptographicException("The decrypted value cannot be converted into the provided type.", nse);
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Roslynator.Style", "RCS1165", Justification = "Only null is bad.")]
        string IEncryption<TData>.Encrypt(TData value)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            string plainText;
            try
            {
                plainText = s_typeConverter.ConvertToInvariantString(value);
            }
            catch (NotSupportedException nse)
            {
                _logger.LogError(nse, "Can't convert a value of type {Type} into a token!", typeof(TData));
                throw new CryptographicException("The value cannot be converted into a string for encryption.", nse);
            }

            using (var ms = new MemoryStream())
            using (var en = _algorithm.CreateEncryptor(_key.Value, _iv.Value))
            using (var cs = new CryptoStream(ms, en, Write))
            using (var sw = new StreamWriter(cs, s_encoding))
            {
                sw.Write(plainText);
                sw.Flush();
                cs.FlushFinalBlock();

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
