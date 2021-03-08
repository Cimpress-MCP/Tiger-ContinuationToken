// <copyright file="LoggerExtensions.cs" company="Cimpress, Inc.">
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
using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LoggerMessage;
using static Microsoft.Extensions.Logging.LogLevel;

namespace Tiger.ContinuationToken
{
    /// <summary>Extensions to the functionality of the <see cref="ILogger"/> interface.</summary>
    static class LoggerExtensions
    {
        static readonly Func<ILogger, string, IDisposable> s_decryptingScope =
            DefineScope<string>("EncryptedValue: {EncryptedValue:l}");

        static readonly Action<ILogger, Exception> s_decryptionFailed = Define(
            Information,
            new EventId(0, nameof(DecryptionFailed)),
            "Failed to decrypt continuation token.");

        static readonly Action<ILogger, Type, Exception> s_toTokenFailed = Define<Type>(
            Error,
            new EventId(1, nameof(ToTokenFailed)),
            "Can't convert a value of type {Type} into a token!");

        static readonly Action<ILogger, Type, Exception> s_fromTokenFailed = Define<Type>(
            Error,
            new EventId(2, nameof(FromTokenFailed)),
            "Can't convert a token into a value of type {Type}!");

        /// <summary>Creates a logging scope for a decrypting operation.</summary>
        /// <param name="logger">An application logger.</param>
        /// <param name="encryptedValue">The encrypted value to decrypt.</param>
        /// <returns>A value which, when disposed, will end the logging scope.</returns>
        public static IDisposable Decrypting(this ILogger logger, string encryptedValue) =>
            s_decryptingScope(logger, encryptedValue);

        /// <summary>
        /// Writes an informational log message corresponding to the event of decryption failure.
        /// </summary>
        /// <param name="logger">An application logger.</param>
        /// <param name="exception">The exception thrown as a result of decryption's failure.</param>
        public static void DecryptionFailed(this ILogger logger, Exception exception) =>
            s_decryptionFailed(logger, exception);

        /// <summary>
        /// Writes an error log message corresponding to the event of
        /// failure to convert a value to a token.
        /// </summary>
        /// <param name="logger">An application logger.</param>
        /// <param name="type">The source type.</param>
        /// <param name="exception">The exception thrown as a result of decryption's failure.</param>
        public static void ToTokenFailed(this ILogger logger, Type type, Exception exception) =>
            s_toTokenFailed(logger, type, exception);

        /// <summary>
        /// Writes an error log message corresponding to the event of
        /// failure to convert a value from a token.
        /// </summary>
        /// <param name="logger">An application logger.</param>
        /// <param name="type">The target type.</param>
        /// <param name="exception">The exception thrown as a result of decryption's failure.</param>
        public static void FromTokenFailed(this ILogger logger, Type type, Exception exception) =>
            s_fromTokenFailed(logger, type, exception);
    }
}
