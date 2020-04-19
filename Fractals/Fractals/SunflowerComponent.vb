Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

'' In order to load the result of this wizard, you will also need to
'' add the output bin/ folder of this project to the list of loaded
'' folder in Grasshopper.
'' You can use the _GrasshopperDeveloperSettings Rhino command for that.

Public Class SunflowerComponent
    Inherits GH_Component
    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("Vogel Sunflower", "Vogel",
                "Vogel Sunflower Fractal",
                "Fractals", "Fractals")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddIntegerParameter("Number of Iterations", "N", "Number of Iterations - Default value 3000", GH_ParamAccess.item, 3000)
        pManager.AddPointParameter("Generation point", "S", "Generation Point", GH_ParamAccess.item)
        pManager.AddIntegerParameter("Degree", "D", "Degree - Default value 5", GH_ParamAccess.item, 5)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list)
        pManager.AddNumberParameter("Radius", "R", "Radius of circles at points", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object can be used to retrieve data from input parameters and 
    ''' to store data in output parameters.</param>
    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim Angle As Double = 0
        Dim R As Double = 0
        Dim c As Double = 0
        Dim pc As New List(Of Point)
        Dim radii As New List(Of Double)
        Dim x As Double = 0
        Dim y As Double = 0
        Dim numberofseeds As Integer = 3000
        Dim initial As New Point3d(0, 0, 0)
        Dim degree As Int32 = 5
        Dim trans As New Vector3d
        If (Not DA.GetData(0, numberofseeds)) Then Return
        If (Not DA.GetData(1, initial)) Then Return
        If (Not DA.GetData(2, degree)) Then Return
        If (degree <= 1) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Degree must be greater than 1.")
            Return
        End If
        If (((Math.Sqrt(degree)) - (Math.Floor(Math.Sqrt(degree)))) = 0) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Degree must not be a perfect square.")
            Return
        End If
        c = (Math.Sqrt(degree) + 1) / 2
        For i As Integer = 0 To numberofseeds
            R = i ^ c / (numberofseeds / 2)
            Angle = 2 * Math.PI * c * i
            x = initial.X + (R * Math.Sin(Angle) + (numberofseeds / 10))
            y = initial.Y + (R * Math.Cos(Angle) + (numberofseeds / 10))
            Dim p As New Point(New Point3d(x, y, initial.Z))
            If i = 0 Then
                Dim l As New Line(initial, p.Location)
                trans = ((l.UnitTangent * l.Length) * (-1))
            End If
            p.Translate(trans)
            pc.Add(p)
            Dim rad As Double = 0
            rad = i / (numberofseeds / 10)
            radii.Add(rad)
        Next
        DA.SetDataList("Radius", radii)
        DA.SetDataList("Points", pc)
    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.Sunflower
        End Get
    End Property

    ''' <summary>
    ''' Each component must have a unique Guid to identify it. 
    ''' It is vital this Guid doesn't change otherwise old ghx files 
    ''' that use the old ID will partially fail during loading.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("11aaa803-ae68-43c6-b516-50dbdebb2a79")
        End Get
    End Property
End Class