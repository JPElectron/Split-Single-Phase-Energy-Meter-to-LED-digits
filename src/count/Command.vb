''' <summary>
''' Commands that can be sent to the display
''' </summary>
''' <remarks>IT is reccomended that commands are sent 3 times in a row, 4 if the display is set to DIM</remarks>
Public Module Command

    ''' <summary>
    ''' All displays on all addresses are blanked
    ''' </summary>
    ''' <remarks>Parameter: can be aby value</remarks>
    Public Const EmergencyStop As Byte = 69

    ''' <summary>
    ''' Each displays will display it's address
    ''' </summary>
    ''' <remarks>Parameter: don't care</remarks>
    Public Const DisplayAddressOnAllDigits As Byte = 68

    ''' <summary>
    ''' Specify individual segments
    ''' </summary>
    ''' <remarks>Parameter: 0...255: the sum of the segements to display</remarks>
    Public Const ByteCommand As Byte = 66

    ''' <summary>
    ''' Updates all displays after ByteCommand, SendASCII or DecimalPoint
    ''' Recomended to delay execution of this command for 100ms
    ''' </summary>
    ''' <remarks>Parameter: Don't Care</remarks>
    Public Const Strobe As Byte = 83

    ''' <summary>
    ''' Changes the address of the display
    ''' </summary>
    ''' <remarks>Parameter: 0...255</remarks>
    Public Const ChangeAddress As Byte = 67

    ''' <summary>
    ''' Resets all displays (use in case of strange behaviour)
    ''' </summary>
    ''' <remarks>Parameter: Don't Care</remarks>
    Public Const ResetAllDisplays As Byte = 82

    ''' <summary>
    ''' Forces the address of all displays to 1
    ''' </summary>
    ''' <remarks>Parameter: Don't Care</remarks>
    Public Const ForceAddress As Byte = 70

    ''' <summary>
    ''' Sends ASCII to either blank the display or the ASCII values 0-9
    ''' </summary>
    ''' <remarks>Parameter: 32, 48-57 (blank, 0...9)</remarks>
    Public Const SendASCII As Byte = 65

    ''' <summary>
    ''' Turns the decimal point ON
    ''' </summary>
    ''' <remarks>Parameter: 0=OFF, 255=ON</remarks>
    Public Const DecimalPoint As Byte = 80

    ''' <summary>
    ''' Intensity control
    ''' </summary>
    ''' <remarks>Parameter: 0=BRIGHT, 255=DIM</remarks>
    Public Const IntensityControl As Byte = 73

End Module
