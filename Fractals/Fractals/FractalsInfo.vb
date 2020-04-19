Imports Grasshopper.Kernel

Public Class FractalsInfo
    Inherits GH_AssemblyInfo

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "Fractals"
        End Get
    End Property
    Public Overrides ReadOnly Property Icon As System.Drawing.Bitmap
        Get
            'Return a 24x24 pixel bitmap to represent this GHA library.
            Return My.Resources.Fractals
        End Get
    End Property
    Public Overrides ReadOnly Property Description As String
        Get
            'Return a short string describing the purpose of this GHA library.
            Return "Fractals 1.1 | Built on 4th August 2018"
        End Get
    End Property
    Public Overrides ReadOnly Property Id As System.Guid
        Get
            Return New System.Guid("7f742abe-d9ba-4513-8e06-8db96d9ffe60")
        End Get
    End Property

    Public Overrides ReadOnly Property AuthorName As String
        Get
            'Return a string identifying you or your company.
            Return "Praneet Mathur"
        End Get
    End Property
    Public Overrides ReadOnly Property AuthorContact As String
        Get
            'Return a string representing your preferred contact details.
            Return "https://www.praneetmathur.me"
        End Get
    End Property
End Class
