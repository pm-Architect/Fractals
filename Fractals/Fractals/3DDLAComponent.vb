Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

'' In order to load the result of this wizard, you will also need to
'' add the output bin/ folder of this project to the list of loaded
'' folder in Grasshopper.
'' You can use the _GrasshopperDeveloperSettings Rhino command for that.

Public Class ThreeDDLAComponent
    Inherits GH_Component
    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("3D DLA Fractal", "3DDLA",
                "3 Dimensional Diffusion Limited Aggregation Fractal a.k.a. Drunken Man Fractal",
                "Fractals", "3D Fractals")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddIntegerParameter("Number of Iterations", "N", "Number of Iterations - Default value 3000", GH_ParamAccess.item, 3000)
        pManager.AddPointParameter("Generation point", "S", "Generation Point", GH_ParamAccess.item)
        pManager.AddNumberParameter("Multiplier", "M", "Multiplier - Default value 1", GH_ParamAccess.item, 1.0)
        'pManager.AddBoxParameter("Canvas", "C", "Box Canvas - Default value 100x100x100", GH_ParamAccess.item, New Box((New Plane(0, 0, 1, 0)), (New Interval(0, 100)), (New Interval(0, 100)), (New Interval(0, 100))))
        'pManager.AddBooleanParameter("Loop on Canvas Edge", "L", "Loop on Canvas Edge - True/False - Default True", GH_ParamAccess.item, True)
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
        Dim boxA As New Box((New Plane(0, 0, 1, 0)), (New Interval(0, 100)), (New Interval(0, 100)), (New Interval(0, 100)))
        Dim x As Double = 100.0
        Dim y As Double = 100.0
        Dim z As Double = 100.0
        Dim loopcanvas As Boolean = False
        If (Not DA.GetData(0, iterations)) Then Return
        If (Not DA.GetData(1, Start)) Then Return
        If (Not DA.GetData(2, M)) Then Return
        'If (Not DA.GetData(3, boxA)) Then Return
        'If (Not DA.GetData(4, loopcanvas)) Then Return
        x = boxA.X.Length
        y = boxA.Y.Length
        z = boxA.Z.Length
        If (y <= (M * 10)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Depth must be greater than Multiplier x 10.")
            Return
        End If
        If (z <= (M * 10)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Height must be greater than Multiplier x 10.")
            Return
        End If
        If (x <= (M * 10)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Width must be greater than Multiplier x 10.")
            Return
        End If
        Dim cp As Point3d = Start
        Dim pp As Point3d = cp
        Dim points As New List(Of Point)
        For i As Int32 = 0 To iterations
            If Not cp = Nothing Then
                pp = cp
                cp = MoveToNeighbor(cp, x, y, z, M, loopcanvas, boxA)
                points.Add(New Point(cp))
            Else
                If Not pp = Nothing Then
                    pp = MoveToNeighbor(cp, x, y, z, M, loopcanvas, boxA)
                    cp = pp
                    points.Add(New Point(pp))
                Else
                    pp = MoveToNeighbor(cp, x, y, z, M, loopcanvas, boxA)
                    cp = pp
                End If
            End If
        Next
        DA.SetDataList(0, points)
    End Sub
    Dim Rand As New Random
    Private Function MoveToNeighbor(ByVal CurrentPoint As Point3d, ByVal X As Double, ByVal Y As Double, ByVal Z As Double, ByVal m As Double, ByVal LoopC As Boolean, ByVal bBox As Box) As Point3d

        'Dim Rand As New Random
        'Dim p As Double = Rand.NextDouble
        'Dim q As Double = (p * (8)) + 1
        'Dim seed As Int32 = Convert.ToInt32(Math.Floor(q))

        ' If one assumes the starting point x;y is 10;10 youll see the
        ' that it will move to the following neighboring point
        'Select Case seed
        'Case 1
        ''Move to point x;y = 9;10
        'If xx = 0 And LoopC = True Then
        'xx = a - m
        'Else
        'xx -= m
        'End If
        'Case 2
        ''Move to point x;y = 9;9
        'f xx = 0 And LoopC = True Then
        'xx = a - m
        'ElseIf Xx = 0 And LoopC = False Then
        'xx += m
        'Else
        'xx -= m
        'End If
        '
        'I f yy = 0 And LoopC = True Then
        'yy = b - m
        'ElseIf Yy = 0 And LoopC = False Then
        'yy += m
        'Else
        'yy -= m
        'End If
        '
        'Case 3
        'Move to point x;y = 10,9
        'If yy = 0 And LoopC = True Then
        'yy = b - m
        'ElseIf Y = 0 And LoopC = False Then
        'xx += m
        'Else
        'yy -= m
        'End If

        'Case 4
        'Move to point x;y = 11;10
        'If xx = a - m And LoopC = True Then
        'xx = 0
        'ElseIf Xx = a - m And LoopC = False Then
        'xx -= m
        'Else
        'xx += m
        'End If
        'Case 5
        'Move to point x;y = 11;11
        'If xx = a - m And LoopC = True Then
        'xx = 0
        'ElseIf Xx = a - m And LoopC = False Then
        'xx -= m
        'Else
        'xx += m
        'End If

        'If yy = b - m And LoopC = True Then
        'yy = 0
        'ElseIf yY = b - m And LoopC = False Then
        'yy -= m
        'Else
        'yy += m
        'End If
        'Case 6
        'Move to point x;y = 10;11
        'If yy = b - m And LoopC = True Then
        'yy = 0
        'ElseIf yY = b - m And LoopC = False Then
        'yy -= m
        'Else
        'yy += m
        'End If
        'Case 7
        'Move to point x;y = 11;9
        'If xx = a - m And LoopC = True Then
        'xx = 0
        'ElseIf xX = a - m And LoopC = False Then
        'xx -= m
        'Else
        'xx += m
        'End If

        'If yy = 0 And LoopC = True Then
        'yy = b - m
        'ElseIf yY = 0 And LoopC = False Then
        'yy += m
        'Else
        'yy -= m
        'End If
        'Case 8
        'Move to point x;y = 9;11
        'If xx = 0 And LoopC = True Then
        'xx = a - m
        'ElseIf xX = 0 And LoopC = False Then
        'xx += m
        'Else
        'xx -= m
        'End If

        'If yy = b - m And LoopC = True Then
        'yy = 0
        'ElseIf yY = b - m And LoopC = False Then
        'yy -= m
        'Else
        'yy += m
        'End If
        'End Select

        Dim xx, yy, zz, a, b, c As Double
        xx = CurrentPoint.X
        yy = CurrentPoint.Y
        zz = CurrentPoint.Z
        a = X
        b = Y
        c = Z
        Dim newpt As New Point3d
        Dim seed As Int32 = Rand.Next(1, 27)
        Select Case seed
            Case 1
                newpt = Move(-1, 1, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 2
                newpt = Move(0, 1, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 3
                newpt = Move(1, 1, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 4
                newpt = Move(-1, 0, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 5
                newpt = Move(0, 0, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 6
                newpt = Move(1, 0, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 7
                newpt = Move(-1, -1, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 8
                newpt = Move(0, -1, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 9
                newpt = Move(1, -1, -1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 10
                newpt = Move(-1, 1, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 11
                newpt = Move(0, 1, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 12
                newpt = Move(1, 1, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 13
                newpt = Move(-1, 0, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 27
                newpt = Move(0, 0, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 15
                newpt = Move(1, 0, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 16
                newpt = Move(-1, -1, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 17
                newpt = Move(0, -1, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 18
                newpt = Move(1, -1, 0, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 19
                newpt = Move(-1, 1, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 20
                newpt = Move(0, 1, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 21
                newpt = Move(1, 1, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 22
                newpt = Move(-1, 0, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 23
                newpt = Move(0, 0, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 24
                newpt = Move(1, 0, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 25
                newpt = Move(-1, -1, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 26
                newpt = Move(0, -1, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If
            Case 14
                newpt = Move(1, -1, 1, CurrentPoint, m)
                If LoopC = True And Not bBox.Contains(newpt) Then
                    Return Nothing
                Else
                    Return newpt
                End If

        End Select



    End Function

    Function Move(ByVal x As Double, ByVal y As Double, ByVal z As Double, ByVal p As Point3d, ByVal mult As Double) As Point3d
        Dim d As Double = x
        Dim e As Double = y
        Dim f As Double = z
        Dim pt As Point3d = p
        Dim a, b, c As Double
        a = pt.X
        b = pt.Y
        c = pt.Z
        a = a + (d * mult)
        b = b + (e * mult)
        c = c + (f * mult)
        Return New Point3d(a, b, c)
    End Function

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Each component must have a unique Guid to identify it. 
    ''' It is vital this Guid doesn't change otherwise old ghx files 
    ''' that use the old ID will partially fail during loading.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("11aaa803-ae68-43c6-b516-50dbdebb2a89")
        End Get
    End Property
End Class