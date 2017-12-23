Imports System.Diagnostics
Imports System.Drawing

Imports BitMiracle.Docotic.Pdf

Namespace BitMiracle.Docotic.Pdf.Samples
    Public NotInheritable Class Fonts
        Public Shared Sub Main()
            ' NOTE: 
            ' When used in trial mode, the library imposes some restrictions.
            ' Please visit http://bitmiracle.com/pdf-library/trial-restrictions.aspx
            ' for more information.

            Dim pathToFile As String = "Fonts.pdf"

            Using pdf As New PdfDocument()
                Dim canvas As PdfCanvas = pdf.Pages(0).Canvas

                ' NOTE: PdfDocument.AddFont() overloads, which load font from the collection of system fonts,
                ' are not supported in version for .NET Standard
                Dim systemFont As PdfFont = pdf.AddFont("Arial", FontStyle.Italic Or FontStyle.Strikeout)
                canvas.Font = systemFont
                canvas.DrawString(10, 50, "Hello, world!")

                Dim builtInFont As PdfFont = pdf.AddFont(PdfBuiltInFont.TimesRoman)
                canvas.Font = builtInFont
                canvas.DrawString(10, 70, "Hello, world!")

                Dim fontFromFile As PdfFont = pdf.AddFontFromFile("Sample data/HolidayPi_BT.ttf")
                canvas.Font = fontFromFile
                canvas.DrawString(10, 90, "Hello, world!")

                ' Remove unused glyphs from TrueType fonts to optimize size of result PDF file.
                systemFont.RemoveUnusedGlyphs()
                fontFromFile.RemoveUnusedGlyphs()

                pdf.Save(pathToFile)
            End Using

            Process.Start(pathToFile)
        End Sub
    End Class
End Namespace