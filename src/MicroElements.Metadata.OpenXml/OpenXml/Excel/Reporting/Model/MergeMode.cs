﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.OpenXml.Excel.Styling
{
    /// <summary>
    /// Style merge mode.
    /// </summary>
    public enum MergeMode
    {
        /// <summary>
        /// Replace style.
        /// </summary>
        Set,

        /// <summary>
        /// Merge style. New style appends on Current.
        /// </summary>
        Merge,

        /// <summary>
        /// Merge in reverse order. Current style appends on New.
        /// </summary>
        ReverseMerge,
    }
}
