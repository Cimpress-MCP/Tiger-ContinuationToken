// <copyright file="EncryptingModelBinder.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult;
using static Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult;

namespace Tiger.ContinuationToken
{
    /// <summary>
    /// Performs model binding for action parameters declared as <see cref="ContinuationToken{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The underlying type of the continuation token.</typeparam>
    public sealed class EncryptingModelBinder<TData>
        : IModelBinder
    {
        readonly IEncryption<TData> _encryption;
        readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="EncryptingModelBinder{TData}"/> class.</summary>
        /// <param name="encryption">Utilities for encryption and decryptions of <see cref="ContinuationToken{TData}"/>.</param>
        /// <param name="logger">The application's logger, specialized for <see cref="EncryptingModelBinder{TData}"/>.</param>
        public EncryptingModelBinder(
            [NotNull] IEncryption<TData> encryption,
            [NotNull] ILogger<EncryptingModelBinder<TData>> logger)
        {
            _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            var name = bindingContext.BinderModelName ?? bindingContext.FieldName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(name);

            if (valueProviderResult == None)
            {
                bindingContext.Result = Success(ContinuationToken<TData>.Empty);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(name, valueProviderResult);

            var encryptedValue = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(encryptedValue))
            {
                bindingContext.Result = Success(ContinuationToken<TData>.Empty);
                return Task.CompletedTask;
            }

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["EncryptedValue:l"] = encryptedValue
            }))
            {
                TData decryptedValue;
                try
                {
                    decryptedValue = _encryption.Decrypt(encryptedValue);
                }
                catch (CryptographicException ce)
                {
                    _logger.LogInformation(ce, "Failed to decrypt continuation token.");
                    bindingContext.ModelState.TryAddModelError(name, "Continuation token is invalid.");
                    return Task.CompletedTask;
                }

                bindingContext.Result = Success(new ContinuationToken<TData>(decryptedValue, encryptedValue));
                return Task.CompletedTask;
            }
        }
    }
}
