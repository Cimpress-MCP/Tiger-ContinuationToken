// <copyright file="EncryptingModelBinder.cs" company="Cimpress, Inc.">
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

using static Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult;

namespace Tiger.ContinuationToken;

/// <summary>
/// Performs model binding for action parameters declared as <see cref="ContinuationToken{TData}"/>.
/// </summary>
/// <typeparam name="TData">The underlying type of the continuation token.</typeparam>
sealed partial class EncryptingModelBinder<TData>
    : IModelBinder
    where TData : notnull
{
    static readonly Func<ILogger, string, IDisposable> s_decryptingScope =
        LoggerMessage.DefineScope<string>("EncryptedValue: {EncryptedValue:l}");

    readonly IEncryption<TData> _encryption;
    readonly ILogger _logger;

    /// <summary>Initializes a new instance of the <see cref="EncryptingModelBinder{TData}"/> class.</summary>
    /// <param name="encryption">Utilities for encryption and decryptions of <see cref="ContinuationToken{TData}"/>.</param>
    /// <param name="logger">The application's logger, specialized for <see cref="EncryptingModelBinder{TData}"/>.</param>
    public EncryptingModelBinder(IEncryption<TData> encryption, ILogger<EncryptingModelBinder<TData>> logger)
    {
        _encryption = encryption;
        _logger = logger;
    }

    /// <inheritdoc/>
    [SuppressMessage("Roslynator.Style", "RCS1229", Justification = "Always returns a completed task.")]
    Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
    {
        var name = bindingContext.BinderModelName ?? bindingContext.FieldName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(name);

        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = Success(ContinuationToken<TData>.Empty);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(name, valueProviderResult);

        var encryptedValue = valueProviderResult.FirstValue;
        if (encryptedValue is not { Length: not 0 } ev)
        {
            bindingContext.Result = Success(ContinuationToken<TData>.Empty);
            return Task.CompletedTask;
        }

        using var @finally = s_decryptingScope(_logger, encryptedValue);
        TData decryptedValue;
        try
        {
            decryptedValue = _encryption.Decrypt(ev);
        }
        catch (CryptographicException ce)
        {
            DecryptionFailed(ce);
            _ = bindingContext.ModelState.TryAddModelError(name, "Continuation token is invalid.");
            return Task.CompletedTask;
        }

        bindingContext.Result = Success(new ContinuationToken<TData>(decryptedValue, ev));
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = Information, Message = "Failed to decrypt continuation token.")]
    partial void DecryptionFailed(Exception e);
}
