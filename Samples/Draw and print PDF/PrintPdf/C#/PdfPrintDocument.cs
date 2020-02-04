﻿using System;
using System.Drawing;
using System.Drawing.Printing;
using BitMiracle.Docotic.Pdf;

namespace BitMiracle.Docotic.Samples.PrintPdf
{
    class PdfPrintDocument : IDisposable
    {
        private readonly PrintDocument m_printDocument;
        private readonly PrintSize m_printSize;

        private PdfDocument m_pdf;

        private PrintAction m_printAction;
        private int m_pageIndex;
        private int m_lastPageIndex;
        private RectangleF m_printableAreaInPoints;


        public PdfPrintDocument(PdfDocument pdf, PrintSize printSize)
        {
            if (pdf == null)
                throw new ArgumentNullException("pdf");

            m_pdf = pdf;
            m_printSize = printSize;

            m_printDocument = new PrintDocument();
            m_printDocument.BeginPrint += printDocument_BeginPrint;
            m_printDocument.QueryPageSettings += printDocument_QueryPageSettings;
            m_printDocument.PrintPage += printDocument_PrintPage;
            m_printDocument.EndPrint += printDocument_EndPrint;
        }

        public PrintDocument PrintDocument
        {
            get { return m_printDocument; }
        }

        public void Dispose()
        {
            m_printDocument.Dispose();
        }

        public void Print(PrinterSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            m_printDocument.PrinterSettings = settings;
            m_printDocument.Print();
        }

        private void printDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            PrintDocument printDocument = (PrintDocument)sender;
            printDocument.OriginAtMargins = false;

            m_printAction = e.PrintAction;

            switch (printDocument.PrinterSettings.PrintRange)
            {
                case PrintRange.Selection:
                case PrintRange.CurrentPage:
                    {
                        m_pageIndex = 0;
                        m_lastPageIndex = 0;
                        break;
                    }

                case PrintRange.SomePages:
                    {
                        m_pageIndex = Math.Max(0, printDocument.PrinterSettings.FromPage - 1);
                        m_lastPageIndex = Math.Min(m_pdf.PageCount - 1, printDocument.PrinterSettings.ToPage - 1);
                        break;
                    }

                case PrintRange.AllPages:
                default:
                    {
                        m_pageIndex = 0;
                        m_lastPageIndex = m_pdf.PageCount - 1;
                        break;
                    }
            }
        }

        private void printDocument_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            PdfPage page = m_pdf.Pages[m_pageIndex];

            // Auto-detect portrait/landscape orientation.
            // Printer settings for orientation are ignored in this sample.
            PdfSize pageSize = getPageSizeInPoints(page);
            e.PageSettings.Landscape = pageSize.Width > pageSize.Height;

            m_printableAreaInPoints = getPrintableAreaInPoints(e.PageSettings);
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics gr = e.Graphics;

            // Work in points to have consistent units for all contexts:
            // 1. Printer
            // 2. Print preview
            // 3. PDF
            gr.PageUnit = GraphicsUnit.Point;

            if (m_printAction == PrintAction.PrintToPreview)
            {
                gr.Clear(Color.LightGray);
                gr.FillRectangle(Brushes.White, m_printableAreaInPoints);
            }

            PdfPage page = m_pdf.Pages[m_pageIndex];
            if (m_printSize == PrintSize.FitPage)
            {
                if (m_printAction == PrintAction.PrintToPreview)
                    gr.TranslateTransform(m_printableAreaInPoints.X, m_printableAreaInPoints.Y);

                PdfSize pageSizeInPoints = getPageSizeInPoints(page);

                float sx = (float)(m_printableAreaInPoints.Width / pageSizeInPoints.Width);
                float sy = (float)(m_printableAreaInPoints.Height / pageSizeInPoints.Height);
                float scaleFactor = Math.Min(sx, sy);
                gr.ScaleTransform(scaleFactor, scaleFactor);
            }
            else if (m_printSize == PrintSize.ActualSize)
            {
                gr.TranslateTransform(-m_printableAreaInPoints.X, -m_printableAreaInPoints.Y);
            }

            page.Draw(gr);

            ++m_pageIndex;
            e.HasMorePages = (m_pageIndex <= m_lastPageIndex);
        }

        private void printDocument_EndPrint(object sender, PrintEventArgs e)
        {
        }

        private static PdfSize getPageSizeInPoints(PdfPage page)
        {
            PdfRotation rotation = page.Rotation;
            if (rotation == PdfRotation.Rotate90 || rotation == PdfRotation.Rotate270)
                return new PdfSize(page.Height, page.Width);

            return new PdfSize(page.Width, page.Height);
        }

        private static RectangleF getPrintableAreaInPoints(PageSettings pageSettings)
        {
            RectangleF printableArea = pageSettings.PrintableArea;
            if (pageSettings.Landscape)
            {
                float tmp = printableArea.Width;
                printableArea.Width = printableArea.Height;
                printableArea.Height = tmp;
            }

            // PrintableArea is expressed in hundredths of an inch
            const float PrinterSpaceToPoint = 72.0f / 100.0f;
            return new RectangleF(
                printableArea.X * PrinterSpaceToPoint,
                printableArea.Y * PrinterSpaceToPoint,
                printableArea.Width * PrinterSpaceToPoint,
                printableArea.Height * PrinterSpaceToPoint
            );
        }
    }
}