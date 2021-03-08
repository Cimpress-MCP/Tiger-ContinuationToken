// <copyright file="ModelBinderProvider.cs" company="Cimpress, Inc.">
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

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Tiger.ContinuationToken
{
    /// <summary>Provides an instance of <see cref="EncryptingModelBinder{TData}"/>, if applicable.</summary>
    public sealed class ModelBinderProvider
        : IModelBinderProvider
    {
        /// <inheritdoc/>
        IModelBinder? IModelBinderProvider.GetBinder(ModelBinderProviderContext context)
        {
            if (!context.Metadata.ModelType.IsGenericType)
            {
                return null;
            }

            var genericTypeDefinition = context.Metadata.ModelType.GetGenericTypeDefinition();
            if (genericTypeDefinition != typeof(ContinuationToken<>))
            {
                return null;
            }

            var underlyingType = context.Metadata.ModelType.GenericTypeArguments[0];
            var typeConverter = TypeDescriptor.GetConverter(underlyingType);
            if (!typeConverter.CanConvertFrom(typeof(string)))
            {
                return null;
            }

            var modelBinderType = typeof(EncryptingModelBinder<>).MakeGenericType(underlyingType);

            return new BinderTypeModelBinder(modelBinderType);
        }
    }
}
