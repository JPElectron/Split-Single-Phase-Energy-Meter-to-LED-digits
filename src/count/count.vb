Imports System.IO.Ports
Imports System.Net
Imports System.Threading
Imports System.IO
Imports System.Reflection

Module count

    Dim CountFilePath As String = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\count.dat"
    Dim SettingsFilePath As String = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\count.ini"

    Public Const ErrorByte As Byte = Bottom 'Binary representation of underscore on the display
    Public Const BlankByte As Byte = 0 'Nothing on the display

    'Unique ID for all instances of this applciation
    Public Const ApplicationID As String = "Global\B3BCB1DC-A97B-4ED4-8148-83C1916FD657"

    ''' <summary>
    ''' Entry point for the application
    ''' </summary>
    ''' <remarks></remarks>
    Sub Main()
        'Syncoronizes the instance of this applciation with other instances that may be running
        Dim singleInstanceMutex As New Mutex(False, ApplicationID)

        Try
            'Wait for any other instances of this application holding the mutex to release it
            singleInstanceMutex.WaitOne(-1)

            Dim args() As String = Environment.GetCommandLineArgs()
            Dim portName As String = Nothing
            Dim numberOfDisplays As Integer = 1 'Assume at least 1 display is hooked up to the serial port

            'Read the settings file
            If IO.File.Exists(SettingsFilePath) Then
                Using settingFile As New IO.FileStream(SettingsFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
                    Using settingsReader As New IO.StreamReader(settingFile)
                        While settingsReader.Peek > 0
                            Dim settingsLine() As String = settingsReader.ReadLine.Split("=".ToCharArray, StringSplitOptions.RemoveEmptyEntries)
                            If settingsLine.Count = 2 Then
                                Select Case settingsLine(0).ToUpperInvariant
                                    Case "COMPORT"
                                        'If it is a number, use that number as the index of ports to attempt to use
                                        Dim portIndex As Integer = -1
                                        If Integer.TryParse(settingsLine(1), portIndex) Then
                                            Dim portNames() As String = SerialPort.GetPortNames
                                            If portNames.Count >= portIndex Then
                                                portName = portNames(portIndex - 1)
                                            End If
                                        Else
                                            'The entry is not a number so it must be a port name, see if that port name exists
                                            For Each existingPortName As String In SerialPort.GetPortNames
                                                If existingPortName.Equals(settingsLine(1), StringComparison.OrdinalIgnoreCase) Then
                                                    portName = existingPortName
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Case "DIGITS"
                                        Integer.TryParse(settingsLine(1), numberOfDisplays)
                                End Select
                            End If
                        End While

                    End Using
                End Using
            End If

            If portName Is Nothing Then
                'Couldn't get a port so default to the first port
                Dim portNames() As String = SerialPort.GetPortNames
                If portNames.Count > 0 Then
                    portName = portNames(0)
                Else
                    Throw New Exception("Unable to find serial port")
                End If
            End If

            If args.Count = 1 Then 'No args passed in (first is the path to the exe)
                Dim count As Integer = GetCount()
                UpdateDisplays(portName, numberOfDisplays, count)
            Else
                Select Case args(1).ToUpperInvariant 'Upper case string comparison is faster
                    Case "-J" 'Join (+1)
                        Dim count As Integer = GetCount()
                        count += 1
                        UpdateDisplays(portName, numberOfDisplays, count)
                        UpdateCount(count)
                    Case "-U" 'Up (+1)
                        Dim count As Integer = GetCount()
                        count += 1
                        UpdateDisplays(portName, numberOfDisplays, count)
                        UpdateCount(count)
                    Case "-L" 'Leave (-1)
                        Dim count As Integer = GetCount()
                        count = Math.Max(0, count - 1) 'Dont go below 0
                        UpdateDisplays(portName, numberOfDisplays, count)
                        UpdateCount(count)
                    Case "-D" 'Down (-1)
                        Dim count As Integer = GetCount()
                        count = Math.Max(0, count - 1) 'Dont go below 0
                        UpdateDisplays(portName, numberOfDisplays, count)
                        UpdateCount(count)
                    Case "-B" 'Blank
                        UpdateDisplaysWithBlank(portName, numberOfDisplays)
                    Case "-E" 'Error
                        UpdateDisplaysWithError(portName, numberOfDisplays)
                    Case "-Z" 'Zero
                        Dim count As Integer = GetCount()
                        count = 0
                        UpdateDisplays(portName, numberOfDisplays, count)
                        UpdateCount(count)
                    Case "-R" 'Reset
                        Dim count As Integer = GetCount()
                        count = 0
                        UpdateDisplaysWithReset(portName, numberOfDisplays)
                        UpdateCount(count)
                    Case "-LO" 'Intensity dim
                        UpdateDisplaysWithDim(portName, numberOfDisplays)
                    Case "-HI" 'Intensity bright
                        UpdateDisplaysWithBright(portName, numberOfDisplays)
                End Select

            End If
        Finally
            'Let other instances continue if there are any
            singleInstanceMutex.ReleaseMutex()
        End Try
    End Sub

    Public Function GetCount() As Integer
        Dim count As Integer = 0 'Return a count of 0 if all else fails
        If IO.File.Exists(CountFilePath) Then
            Try
                Using countFile As New IO.FileStream(CountFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                    Using countFileReader As New IO.StreamReader(countFile)
                        'Try to get a number from the first line, remove any errant space that may exist
                        Integer.TryParse(countFileReader.ReadLine.Replace(" "c, String.Empty), count)
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine(ex.ToString)
            End Try
        End If

        Return count
    End Function

    Public Sub UpdateCount(ByVal count As Integer)
        Using countFile As New IO.FileStream(CountFilePath, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
            Using countFileWriter As New IO.StreamWriter(countFile)
                countFileWriter.Write(count.ToString)
            End Using
        End Using
    End Sub

    Public Sub UpdateDisplays(ByVal portName As String, ByVal numberOfDisplays As Byte, ByVal number As Integer)
        Debug.WriteLine("Using port named: " & portName & " with " & numberOfDisplays.ToString & " display(s)")
        'Get the ascii byte for each number in the count
        Dim digits() As Byte = System.Text.Encoding.ASCII.GetBytes(number.ToString.PadLeft(numberOfDisplays, "0"c))
        If digits.Count <= numberOfDisplays Then
            For i As Byte = 1 To Math.Min(digits.Count, 255) 'Max of 255 displays
                K8063SerialControl.SendCommand(portName, i, Command.SendASCII, digits(i - 1))
            Next

            'Recommended delay before refreshing the display
            Threading.Thread.Sleep(130)
            'Signal the displays to refresh
            K8063SerialControl.SendCommand(portName, 0, Command.Strobe, 0)
        Else
            UpdateDisplaysWithError(portName, numberOfDisplays)
        End If

    End Sub

    Public Sub UpdateDisplaysWithReset(ByVal portName As String, ByVal numberOfDisplays As Byte)
        'Reset each display
        For i As Byte = 1 To numberOfDisplays
            K8063SerialControl.SendCommand(portName, i, Command.ResetAllDisplays, 0)
        Next

        'Recommended delay before refreshing the display
        Threading.Thread.Sleep(130)
    End Sub

    Public Sub UpdateDisplaysWithDim(ByVal portName As String, ByVal numberOfDisplays As Byte)
        'Update each display
        For i As Byte = 1 To numberOfDisplays
            K8063SerialControl.SendCommand(portName, i, Command.IntensityControl, 255)
        Next

        'Recommended delay before refreshing the display
        Threading.Thread.Sleep(130)
    End Sub

    Public Sub UpdateDisplaysWithBright(ByVal portName As String, ByVal numberOfDisplays As Byte)
        'Update each display
        For i As Byte = 1 To numberOfDisplays
            K8063SerialControl.SendCommand(portName, i, Command.IntensityControl, 0)
        Next

        'Recommended delay before refreshing the display
        Threading.Thread.Sleep(130)
    End Sub

    Public Sub UpdateDisplaysWithBlank(ByVal portName As String, ByVal numberOfDisplays As Byte)
        'Display nothing on each display
        For i As Byte = 1 To numberOfDisplays
            K8063SerialControl.SendCommand(portName, i, Command.ByteCommand, BlankByte)
        Next

        'Recommended delay before refreshing the display
        Threading.Thread.Sleep(130)
        'Signal the displays to refresh
        K8063SerialControl.SendCommand(portName, 0, Command.Strobe, 0)
    End Sub

    Public Sub UpdateDisplaysWithError(ByVal portName As String, ByVal numberOfDisplays As Byte)
        'Display an error on each display
        For i As Byte = 1 To numberOfDisplays
            K8063SerialControl.SendCommand(portName, i, Command.ByteCommand, ErrorByte)
        Next

        'Recommended delay before refreshing the display
        Threading.Thread.Sleep(130)
        'Signal the displays to refresh
        K8063SerialControl.SendCommand(portName, 0, Command.Strobe, 0)
    End Sub

End Module
