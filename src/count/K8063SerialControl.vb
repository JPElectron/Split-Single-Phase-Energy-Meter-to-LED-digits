Imports System.IO.Ports

''' <summary>
''' 
''' </summary>
''' <remarks>Device specs located at http://store.qkits.com/moreinfo.cfm/illustrated_assembly_manual_k8063.pdf</remarks>
Public Module K8063SerialControl

    ''' <summary>
    ''' Initializes a serial port for the device
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetSerialPort(ByVal portName As String) As SerialPort
        'Constructor params from the specs
        Dim port As New SerialPort(portName, 2400, Parity.None, 8, StopBits.One)

        port.Handshake = Handshake.None
        port.DtrEnable = True

        Return port
    End Function

    ''' <summary>
    ''' Sends a command to the device
    ''' </summary>
    ''' <param name="displayAddress"></param>
    ''' <param name="command"></param>
    ''' <param name="parameter"></param>
    ''' <remarks></remarks>
    Public Sub SendCommand(ByVal portName As String, ByVal displayAddress As Byte, ByVal command As Byte, ByVal parameter As Byte)
        Dim port As SerialPort = GetSerialPort(portName)

        'The array that holds the series of bytes to send to the serial port
        Dim commandBuffer(4) As Byte 'Commands are always 5 bytes
        commandBuffer(0) = 13 'Init command
        commandBuffer(1) = displayAddress
        commandBuffer(2) = command
        commandBuffer(3) = parameter

        'Calc the checksum
        Dim checksum As Short = 256 - (Convert.ToInt16(commandBuffer(0)) + Convert.ToInt16(commandBuffer(1)) + Convert.ToInt16(commandBuffer(2)) + Convert.ToInt16(commandBuffer(3))) Mod 256
        If checksum >= 256 Then checksum = 0
        commandBuffer(4) = Convert.ToByte(checksum)

        'Send the command
        port.Open()

        For i As Integer = 0 To 4 'Recommended to send each command at least 3 times, or 4 for Dim displays
            port.Write(commandBuffer, 0, 5)
        Next

        port.Close()
    End Sub

End Module




