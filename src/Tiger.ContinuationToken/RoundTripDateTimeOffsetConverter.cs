﻿// <copyright file="RoundTripDateTimeOffsetConverter.cs" company="Cimpress, Inc.">
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
using System.Globalization;

namespace Tiger.ContinuationToken
{
    /// <summary>
    /// Provides a type converter to convert <see cref="DateTimeOffset"/>
    /// objects to and from various other representations.
    /// </summary>
    public sealed class RoundTripDateTimeOffsetConverter
        : DateTimeOffsetConverter
    {
        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DateTimeOffset dto)
            {
                return dto.ToString("O", culture);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}