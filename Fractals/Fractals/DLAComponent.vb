Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

'' In order to load the result of this wizard, you will also need to
'' add the output bin/ folder of this project to the list of loaded
'' folder in Grasshopper.
'' You can use the _GrasshopperDeveloperSettings Rhino command for that.

Public Class DLAComponent
    Inherits GH_Component
    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("DLA Fractal", "DLA",
                "Diffusion Limited Aggregation Fractal a.k.a. Drunken Man Fractal",
                "Fractals", "Fractals")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddIntegerParameter("Number of Iterations", "N", "Number of Iterations - Default value 3000", GH_ParamAccess.item, 3000)
        pManager.AddPointParameter("Generation point", "S", "Generation Point", GH_ParamAccess.item)
        pManager.AddNumberParameter("Multiplier", "M", "Multiplier - Default value 1", GH_ParamAccess.item, 1.0)
        pManager.AddNumberParameter("Canvas Height", "H", "Canvas Height - Default value 100", GH_ParamAccess.item, 100.0)
        pManager.AddNumberParameter("Canvas Width", "W", "Canvas Width - Default value 100", GH_ParamAccess.item, 100.0)
        pManager.AddBooleanParameter("Loop on Canvas Edge", "L", "Loop on Canvas Edge - True/False - Default True", GH_ParamAccess.item, True)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object can be used to retrieve data from input parameters and 
    ''' to store data in output parameters.</param>
    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)
        Dim iterations As Int32 = 3000
        Dim Start As New Point3d(0, 0, 0)
        Dim M As Double = 1.0
        Dim h As Double = 100.0
        Dim w As Double = 100.0
        Dim loopcanvas As Boolean = True
        If (Not DA.GetData(0, iterations)) Then Return
        If (Not DA.GetData(1, Start)) Then Return
        If (Not DA.GetData(2, M)) Then Return
        If (Not DA.GetData(3, h)) Then Return
        If (Not DA.GetData(4, w)) Then Return
        If (Not DA.GetData(5, loopcanvas)) Then Return
        If (h <= (M * 10)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Height must be greater than Multiplier x 10.")
            Return
        End If
        If (w <= (M * 10)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Width must be greater than Multiplier x 10.")
            Return
        End If
        Dim cp As Point3d = Start
        Dim points As New List(Of Point)
        For x As Int32 = 0 To iterations
            points.Add(New Point(cp))
            cp = MoveToNeighbor(cp, h, w, M, loopcanvas)
        Next
        DA.SetDataList(0, points)
    End Sub
    Dim Rand As New Random
    Private Function MoveToNeighbor(ByVal CurrentPoint As Point3d, ByVal height As Double, ByVal width As Double, ByVal m As Double, ByVal LoopC As Boolean) As Point3d
        Dim x, y, z, a, b As Double
        x = CurrentPoint.X
        y = CurrentPoint.Y
        z = CurrentPoint.Z
        a = width
        b = height
        'Dim Rand As New Random
        'Dim p As Double = Rand.NextDouble
        'Dim q As Double = (p * (8)) + 1
        'Dim seed As Int32 = Convert.ToInt32(Math.Floor(q))
        Dim seed As Int32 = Rand.Next(1, 9)
        ' If one assumes the starting point x;y is 10;10 youll see the
        ' that it will move to the following neighboring point
        Select Case seed
            Case 1
                'Move to point x;y = 9;10
                If x = 0 And LoopC = True Then
                    x = a - m
                Else
                    x -= m
                End If
            Case 2
                'Move to point x;y = 9;9
                If x = 0 And LoopC = True Then
                    x = a - m
                ElseIf x = 0 And LoopC = False Then
                    x += m
                Else
                    x -= m
                End If

                If y = 0 And LoopC = True Then
                    y = b - m
                ElseIf y = 0 And LoopC = False Then
                    y += m
                Else
                    y -= m
                End If

            Case 3
                'Move to point x;y = 10,9
                If y = 0 And LoopC = True Then
                    y = b - m
                ElseIf y = 0 And LoopC = False Then
                    x += m
                Else
                    y -= m
                End If

            Case 4
                'Move to point x;y = 11;10
                If x = a - m And LoopC = True Then
                    x = 0
                ElseIf x = a - m And LoopC = False Then
                    x -= m
                Else
                    x += m
                End If
            Case 5
                'Move to point x;y = 11;11
                If x = a - m And LoopC = True Then
                    x = 0
                ElseIf x = a - m And LoopC = False Then
                    x -= m
                Else
                    x += m
                End If

                If y = b - m And LoopC = True Then
                    y = 0
                ElseIf y = b - m And LoopC = False Then
                    y -= m
                Else
                    y += m
                End If
            Case 6
                'Move to point x;y = 10;11
                If y = b - m And LoopC = True Then
                    y = 0
                ElseIf y = b - m And LoopC = False Then
                    y -= m
                Else
                    y += m
                End If
            Case 7
                'Move to point x;y = 11;9
                If x = a - m And LoopC = True Then
                    x = 0
                ElseIf x = a - m And LoopC = False Then
                    x -= m
                Else
                    x += m
                End If

                If y = 0 And LoopC = True Then
                    y = b - m
                ElseIf y = 0 And LoopC = False Then
                    y += m
                Else
                    y -= m
                End If
            Case 8
                'Move to point x;y = 9;11
                If x = 0 And LoopC = True Then
                    x = a - m
                ElseIf x = 0 And LoopC = False Then
                    x += m
                Else
                    x -= m
                End If

                If y = b - m And LoopC = True Then
                    y = 0
                ElseIf y = b - m And LoopC = False Then
                    y -= m
                Else
                    y += m
                End If
        End Select

        Return New Point3d(x, y, z)
    End Function

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.DLA
        End Get
    End Property

    ''' <summary>
    ''' Each component must have a unique Guid to identify it. 
    ''' It is vital this Guid doesn't change otherwise old ghx files 
    ''' that use the old ID will partially fail during loading.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("11aaa803-ae68-43c6-b516-50dbdebb2a80")
        End Get
    End Property
End Class