﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;

namespace MicroElements.Parsing
{
    public class ExcelElement<TOpenXmlElement> : IMetadataProvider
    {
        /// <summary>
        /// Документ в котором содержится элемент.
        /// </summary>
        public SpreadsheetDocument Doc { get; }

        /// <summary>
        /// Данные Excel.
        /// </summary>
        public TOpenXmlElement Data { get; }

        public ExcelElement(SpreadsheetDocument doc, TOpenXmlElement data)
        {
            Doc = doc.AssertArgumentNotNull(nameof(doc));
            Data = data;
        }

        public static implicit operator TOpenXmlElement(ExcelElement<TOpenXmlElement> dataSource) => dataSource.Data;

        public T Match<T>(Func<ExcelElement<TOpenXmlElement>, T> process, Func<T> error) => Data != null ? process(this) : error();

        public bool IsEmpty() => Data == null;

        /// <inheritdoc />
        public override string ToString() => $"{Data}";
    }

    public class Column
    {
        public ExcelElement<Cell> Cell { get; }

        public string CellReference => Cell.Data.CellReference;

        public string ColumnReference => Cell.Data.CellReference.GetColumnReference();

        public string Name { get; }

        public Column(ExcelElement<Cell> cell)
        {
            Cell = cell;
            Name = cell.GetCellValue();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(ColumnReference)}: {ColumnReference}, {nameof(Name)}: {Name}";
        }
    }

    public static class ExcelExtensions
    {
        public static string GetColumnReference(this StringValue cellReference)
        {
            return cellReference.Value.GetColumnReference();
        }

        public static string GetColumnReference(this string cellReference)
        {
            cellReference.AssertArgumentNotNull(nameof(cellReference));
            if (cellReference.Length == 2)
                return cellReference.Substring(0, 1);
            return new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        }

        public static ExcelElement<Sheet> GetSheet(this SpreadsheetDocument document, string name)
        {
            var sheets = document.WorkbookPart.Workbook.Sheets.Cast<Sheet>();
            Sheet sheet = sheets.FirstOrDefault(s => s.Name == name);
            return new ExcelElement<Sheet>(document, sheet);
        }

        public static IEnumerable<ExcelElement<Row>> GetRows(this ExcelElement<Sheet> sheet)
        {
            string sheetId = sheet.Data.Id.Value;
            var worksheetPart = (WorksheetPart)sheet.Doc.WorkbookPart.GetPartById(sheetId);
            var worksheet = worksheetPart.Worksheet;
            var rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>().Select(row => new ExcelElement<Row>(sheet.Doc, row));
            return rows;
        }

        public class TableDataRow
        {
            public IReadOnlyDictionary<string, string> Data { get; }

            public ExcelElement<Row> Row { get; }

            public TableDataRow(IReadOnlyDictionary<string, string> data, ExcelElement<Row> row)
            {
                Data = data;
                Row = row;
            }
        }

        public static IEnumerable<TableDataRow> AsParsedRows(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider = null)
        {
            ExcelElement<Column>[] headers = null;
            foreach (var row in rows)
            {
                if (headers == null)
                {
                    headers = row.GetHeaders();
                    continue;
                }

                var rowValues = row.GetRowValues(headers: headers, parserProvider: parserProvider);

                // skip empty line
                if (rowValues.All(string.IsNullOrWhiteSpace))
                    continue;

                var headerNames = headers.Select(header => header.Data.Name ?? header.Data.ColumnReference).ToArrayDebug();
                var expandoObject = headerNames
                    .Zip(rowValues, (header, value) => (header, value))
                    .ToDictionary(tuple => tuple.header, tuple => tuple.value);

                yield return new TableDataRow(expandoObject, row);
            }
        }

        public static IEnumerable<IReadOnlyDictionary<string, string>> AsDictionaryList(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider)
        {
            ExcelElement<Column>[] headers = null;
            foreach (var row in rows)
            {
                if (headers == null)
                {
                    headers = row.GetHeaders();
                    continue;
                }

                var rowValues = row.GetRowValues(headers: headers, parserProvider: parserProvider);

                // skip empty line
                if (rowValues.All(string.IsNullOrWhiteSpace))
                    continue;

                var headerNames = headers.Select(header => header.Data.Name ?? header.Data.ColumnReference).ToArrayDebug();
                var expandoObject = headerNames
                    .Zip(rowValues, (header, value) => (header, value))
                    .ToDictionary(tuple => tuple.header, tuple => tuple.value);

                yield return expandoObject;
            }
        }

        public static ExcelElement<Cell>[] GetRowCells(this ExcelElement<Row> row)
        {
            if (row.IsEmpty())
                return Array.Empty<ExcelElement<Cell>>();

            var cells = row.Data.Descendants<Cell>();
            var rowCells = cells.Select(cell => new ExcelElement<Cell>(row.Doc, cell)).ToArray();
            return rowCells;
        }

        public static ExcelElement<Column>[] GetHeaders(this ExcelElement<Row> row)
        {
            if (row.IsEmpty())
                return Array.Empty<ExcelElement<Column>>();

            var cells = row.GetRowCells();
            var headers = cells.Select(cell => new ExcelElement<Column>(row.Doc, new Column(cell))).ToArray();
            return headers;
        }

        public static string[] GetRowValues(
            this ExcelElement<Row> row,
            ExcelElement<Column>[] headers,
            IParserProvider parserProvider,
            string nullValue = null)
        {
            if (row.IsEmpty())
                return Array.Empty<string>();

            var cells = row.GetRowCells();

            string[] rowValues = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                // Find cell for the same column.
                var cell = cells.FirstOrDefault(c => c.Data.CellReference.GetColumnReference() == header.Data.ColumnReference);

                if (cell != null)
                {
                    // Set propertyParser for cell according column name
                    var propertyParser = parserProvider.GetParsers().FirstOrDefault(parser => parser.SourceName == header.Data.Name);
                    if (propertyParser != null)
                    {
                        cell.SetMetadata(propertyParser);
                    }

                    rowValues[i] = cell.GetCellValue(nullValue);
                }
                else
                {
                    rowValues[i] = nullValue;
                }
            }

            return rowValues;
        }

        /// <summary>
        /// Cell value with extended info.
        /// </summary>
        public class CellValue
        {
            public static readonly CellValue Empty = new CellValue(null, null, null, null);

            public string Text { get; }

            public string ColumnName { get; }

            public string CellReference { get; }

            public IPropertyParser Parser { get; }

            public CellValue(string text, string columnName, string cellReference, IPropertyParser parser)
            {
                Text = text;
                ColumnName = columnName;
                CellReference = cellReference;
                Parser = parser;
            }
        }

        public static CellValue[] GetRowValuesExt(
            this ExcelElement<Row> row,
            ExcelElement<Column>[] headers,
            IParserProvider parserProvider,
            string nullValue = null)
        {
            if (row.IsEmpty())
                return Array.Empty<CellValue>();

            var cells = row.GetRowCells();

            CellValue[] rowValues = new CellValue[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];

                // Find cell for the same column.
                var cell = cells.FirstOrDefault(c => c.Data.CellReference.GetColumnReference() == header.Data.ColumnReference);

                if (cell != null)
                {
                    // Set propertyParser for cell according column name
                    var propertyParser = parserProvider.GetParsers().FirstOrDefault(parser => parser.SourceName == header.Data.Name);
                    if (propertyParser != null)
                    {
                        cell.SetMetadata(propertyParser);
                    }

                    var cellValue = cell.GetCellValue(nullValue);
                    string cellReference = cell.Data.CellReference.Value;
                    rowValues[i] = new CellValue(cellValue, header.Data.Name, cellReference, propertyParser);
                }
                else
                {
                    rowValues[i] = CellValue.Empty;
                }
            }

            return rowValues;
        }

        private static WorkbookPart GetWorkbookPartFromCell(Cell cell)
        {
            Worksheet workSheet = cell.Ancestors<Worksheet>().FirstOrDefault();
            SpreadsheetDocument doc = workSheet.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;
            return doc.WorkbookPart;
        }

        private static CellFormat GetCellFormat(this ExcelElement<Cell> cell)
        {
            WorkbookPart workbookPart = GetWorkbookPartFromCell(cell);
            int styleIndex = (int)cell.Data.StyleIndex.Value;
            CellFormat cellFormat = (CellFormat)workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt(styleIndex);
            return cellFormat;
        }

        private static string GetFormatedValue(this ExcelElement<Cell> cell)
        {
            var cellformat = cell.GetCellFormat();
            string value;
            if (cellformat.NumberFormatId != 0)
            {
                var elements = cell.Doc.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats.Elements<NumberingFormat>().ToList();
                string format = elements.FirstOrDefault(i => i.NumberFormatId.Value == cellformat.NumberFormatId.Value)?.FormatCode;

                //Note: Look also: https://stackoverflow.com/questions/13176832/reading-a-date-from-xlsx-using-open-xml-sdk
                format ??= "d/m/yyyy";
                double number = double.Parse(cell.Data.InnerText);
                value = number.ToString(format);
            }
            else
            {
                value = cell.Data.InnerText;
            }
            return value;
        }

        public static string GetCellValue(this ExcelElement<Cell> cell, string nullValue = null)
        {
            Cell cellData = cell.Data;
            string cellValue = cellData.CellValue?.InnerText ?? nullValue;
            string cellTextValue = null;

            if (cellValue != null && cellData.DataType != null && cellData.DataType.Value == CellValues.SharedString)
            {
                cellTextValue = cell.Doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(cellValue)).InnerText;
            }

            if (cellTextValue == null && cellValue != null && cellData.DataType == null)
            {
                var propertyParser = cell.GetMetadata<IPropertyParser>();

                if (propertyParser != null)
                {
                    if (propertyParser.TargetType == typeof(LocalDate))
                    {
                        DateTime dateTime = Prelude
                            .ParseDouble(cellValue)
                            .Match(FromExcelSerialDate, DateTime.MinValue);

                        cellTextValue = dateTime.ToString("yyyy-MM-dd");
                    }
                    else if (propertyParser.TargetType == typeof(LocalTime))
                    {
                        DateTime dateTime = Prelude
                            .ParseDouble(cellValue)
                            .Match(FromExcelSerialDate, DateTime.MinValue);

                        cellTextValue = dateTime.ToString("HH:mm:ss");
                    }
                    else if (propertyParser.TargetType == typeof(DateTime))
                    {
                        DateTime dateTime = Prelude
                            .ParseDouble(cellValue)
                            .Match(FromExcelSerialDate, DateTime.MinValue);

                        cellTextValue = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                }
            }

            return cellTextValue ?? cellValue;
        }

        /// <summary>
        /// Source: https://stackoverflow.com/questions/727466/how-do-i-convert-an-excel-serial-date-number-to-a-net-datetime
        /// </summary>
        public static DateTime FromExcelSerialDate(double serialDate)
        {
            // NOTE: DateTime.FromOADate parses 1 as 1899-12-31. Correct value is 1900-01-01
            // return DateTime.FromOADate(serialDate);
            if (serialDate > 59)
                serialDate -= 1; //Excel/Lotus 2/29/1900 bug
            return new DateTime(1899, 12, 31).AddDays(serialDate);
        }

        public static T[] GetRowsAs<T>(this ExcelElement<Sheet> sheet, IParserProvider parserProvider)
            where T : PropertyContainer
        {
            return sheet
                .GetRows()
                .AsDictionaryList(parserProvider)
                .Select(parserProvider.ParseProperties)
                .Select(list => (T)Activator.CreateInstance(typeof(T), list))
                .ToArray();
        }
    }
}
