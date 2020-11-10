﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Context for cell rendering.
    /// </summary>
    public class CellContext
    {
        /// <summary>
        /// Gets ColumnContext for this cell.
        /// </summary>
        public ColumnContext ColumnContext { get; }

        /// <summary>
        /// Gets cell metadata.
        /// </summary>
        public IExcelMetadata CellMetadata { get; }

        /// <summary>
        /// Gets OpenXml cell.
        /// </summary>
        public Cell Cell { get; }

        /// <summary>
        /// Gets document metadata.
        /// </summary>
        public IExcelMetadata DocumentMetadata => ColumnContext.DocumentMetadata;

        /// <summary>
        /// Gets sheet metadata.
        /// </summary>
        public IExcelMetadata SheetMetadata => ColumnContext.SheetMetadata;

        /// <summary>
        /// Gets column metadata.
        /// </summary>
        public IExcelMetadata ColumnMetadata => ColumnContext.ColumnMetadata;

        /// <summary>
        /// Gets <see cref="IPropertyRenderer"/> for this cell.
        /// </summary>
        public IPropertyRenderer PropertyRenderer => ColumnContext.PropertyRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CellContext"/> class.
        /// </summary>
        /// <param name="columnContext">ColumnContext for this cell.</param>
        /// <param name="cellMetadata">Cell metadata.</param>
        /// <param name="cell">OpenXml cell.</param>
        public CellContext(ColumnContext columnContext, IExcelMetadata cellMetadata, Cell cell)
        {
            ColumnContext = columnContext.AssertArgumentNotNull(nameof(columnContext));
            CellMetadata = cellMetadata.AssertArgumentNotNull(nameof(cellMetadata));
            Cell = cell;
        }
    }
}
