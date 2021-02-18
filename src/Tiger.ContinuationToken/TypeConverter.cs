// <copyright file="TypeConverter.cs" company="Cimpress, Inc.">
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
using System.Diagnostics.CodeAnalysis;

namespace Tiger.ContinuationToken
{
    /// <summary>
    /// Converts values of the <see cref="ContinuationToken{TData}"/> struct to and from other representations.
    /// </summary>
    /// <remarks><para>
    /// Convinces Swashbuckle that this type can be converted from a <see cref="string"/>.
    /// </para></remarks>
    [SuppressMessage("Microsoft.Style", "CA1812", Justification = "Resolved by reflection.")]
    sealed class TypeConverter
        : System.ComponentModel.TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }
}
