﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Context for row customization.
    /// </summary>
    public class RowContext
    {
        /// <summary>
        /// Gets cells in this row.
        /// </summary>
        public IReadOnlyCollection<CellContext> Cells { get; }

        /// <summary>
        /// Gets OpenXml row.
        /// </summary>
        public Row Row { get; }

        /// <summary>
        /// Gets the source container for row.
        /// </summary>
        public IPropertyContainer RowSource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowContext"/> class.
        /// </summary>
        /// <param name="cells">Cells in this row.</param>
        /// <param name="excelRow">OpenXml row.</param>
        /// <param name="rowSource">Source container for row.</param>
        public RowContext(IReadOnlyCollection<CellContext> cells, Row excelRow, IPropertyContainer rowSource)
        {
            Cells = cells;
            Row = excelRow;
            RowSource = rowSource;
        }
    }
}
