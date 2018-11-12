// <copyright file="IEncryption.cs" company="Cimpress, Inc.">
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
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace Tiger.ContinuationToken
{
    /// <summary>Provides encryption and decryption utilities for <see cref="ContinuationToken{TData}"/>.</summary>
    /// <typeparam name="TData">The type on which to perform operations.</typeparam>
    public interface IEncryption<TData>
    {
        /// <summary>Decrypts a string to a continuation token value.</summary>
        /// <param name="ciphertext">The Base64-encoded value to decrypt.</param>
        /// <returns>The decrypted value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ciphertext"/> is <see langword="null"/>.</exception>
        /// <exception cref="CryptographicException">The decryption operation has failed.</exception>
        [NotNull]
        TData Decrypt([NotNull] string ciphertext);

        /// <summary>Encrypts a continuation token value to a string.</summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The Base64-encoded encrypted value.</returns>
        [NotNull]
        string Encrypt([NotNull] TData value);
    }
}
