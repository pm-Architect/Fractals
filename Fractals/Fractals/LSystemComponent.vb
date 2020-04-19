Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

'' In order to load the result of this wizard, you will also need to
'' add the output bin/ folder of this project to the list of loaded
'' folder in Grasshopper.
'' You can use the _GrasshopperDeveloperSettings Rhino command for that.

Public Class LSystemComponent
    Inherits GH_Component
    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("L-Systems [Turtle]", "LSystem",
                "L-Systems 
|   'F' - Draw a line   |   
|   'f' - Move the turtle without drawing    |   
|   '+' - Add a predetermind angle to the current angle    |   
|   '-' - Withdraw a predetermined angle to the current angle    |",
                "Fractals", "Fractals")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Generation point", "S", "Generation Point", GH_ParamAccess.item)
        pManager.AddTextParameter("Generation Code", "C", "Generation Code - Default -F-F-F-F-F-F
|   'F' - Draw a line   |   
|   'f' - Move the turtle without drawing    |   
|   '+' - Add a predetermind angle to the current angle    |   
|   '-' - Withdraw a predetermined angle to the current angle    |", GH_ParamAccess.item, "-F-F-F-F-F-F")
        pManager.AddIntegerParameter("Iterations", "I", "Iterations - Default 3000", GH_ParamAccess.item, 3000)
        pManager.AddAngleParameter("Angle", "A", "Angle in degrees - Default 60 degrees", GH_ParamAccess.item, 60.0)
        pManager.AddNumberParameter("Step Length", "L", "Step Length - Default 10", GH_ParamAccess.item, 10.0)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list)
        pManager.AddCurveParameter("Lines", "L", "Lines", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object can be used to retrieve data from input parameters and 
    ''' to store data in output parameters.</param>
    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim OutputPoints As New List(Of Point3d)
        Dim OutputLines As New List(Of Line)

        Dim StartPoint As New Point3d(0.0, 0.0, 0.0)
        Dim GenerationCode As String = "-F-F-F-F-F-F"
        Dim Iterations As Int32 = 3000
        Dim Angle As Double = 60.0
        Dim Length As Double = 10.0

        If (Not DA.GetData(0, StartPoint)) Then Return
        If (Not DA.GetData(1, GenerationCode)) Then Return
        If (Not DA.GetData(2, Iterations)) Then Return
        If (Not DA.GetData(3, Angle)) Then Return
        If (Not DA.GetData(4, Length)) Then Return

        If (GenerationCode = "") Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Code to Run!")
            Return
        End If

        Dim TurtleLocation As Point3d = StartPoint
        Dim TurtleDirection As Double = 0.0
        Dim LastLocation As Point3d = TurtleLocation
        For x As Int32 = 0 To Iterations
            For Each Character As Char In GenerationCode
                Select Case Character
                    Case ("F").ElementAt(0)
                        If x = 0 Then
                            OutputPoints.Add(TurtleLocation)
                        End If
                        LastLocation = TurtleLocation
                        TurtleLocation.X = TurtleLocation.X + (Length * (Math.Cos(TurtleDirection)))
                        TurtleLocation.Y = TurtleLocation.Y + (Length * (Math.Sin(TurtleDirection)))
                        OutputPoints.Add(TurtleLocation)
                        Dim line As New Line(LastLocation, TurtleLocation)
                        OutputLines.Add(line)
                    Case ("f").ElementAt(0)
                        TurtleLocation.X = TurtleLocation.X + (Length * (Math.Cos(TurtleDirection)))
                        TurtleLocation.Y = TurtleLocation.Y + (Length * (Math.Sin(TurtleDirection)))
                    Case ("-").ElementAt(0)
                        TurtleDirection = TurtleDirection - Angle
                    Case ("+").ElementAt(0)
                        TurtleDirection = TurtleDirection + Angle
                    Case Else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in Code!")
                        Return
                End Select
            Next
        Next

        DA.SetDataList(0, OutputPoints)
        DA.SetDataList(1, OutputLines)

    End Sub


    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.LSystem
        End Get
    End Property

    ''' <summary>
    ''' Each component must have a unique Guid to identify it. 
    ''' It is vital this Guid doesn't change otherwise old ghx files 
    ''' that use the old ID will partially fail during loading.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("11aaa803-ae68-43c6-b516-50dbdebb2b90")
        End Get
    End Property
End Class