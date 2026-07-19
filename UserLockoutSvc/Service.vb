#Region " Imports "
Imports System.ServiceProcess
Imports System.IO
Imports System.Environment
Imports System.Configuration
Imports System.Windows.Forms
#End Region

Public Class svcMain

#Region " Inherits "

    Inherits System.ServiceProcess.ServiceBase

#End Region

#Region " Component Designer generated code "
    Friend WithEvents ApplicationEventLog As System.Diagnostics.EventLog

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer
    Friend WithEvents Control As System.ServiceProcess.ServiceController

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    Friend WithEvents tmrMain As System.Timers.Timer

    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call

    End Sub

    'UserService overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.tmrMain = New System.Timers.Timer
        Me.Control = New System.ServiceProcess.ServiceController
        Me.ApplicationEventLog = New System.Diagnostics.EventLog
        CType(Me.tmrMain, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ApplicationEventLog, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'tmrMain
        '
        Me.tmrMain.Interval = 60000
        '
        'Control
        '
        Me.Control.ServiceName = "User Lockout Monitoring Service"
        '
        'ApplicationEventLog
        '
        Me.ApplicationEventLog.Log = "Application"
        Me.ApplicationEventLog.Source = "User Account Lockout Monitoring Service"
        '
        'svcMain
        '
        Me.CanPauseAndContinue = True
        Me.ServiceName = "User Lockout Monitoring Service"
        CType(Me.tmrMain, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ApplicationEventLog, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

    ' The main entry point for the process
    <MTAThread()> _
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New svcMain}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

#End Region

#Region " Public Variables "

    Public bDebugMode As Boolean = False
    Public doPause As Boolean = False
    Public doStop As Boolean = False
    Public intMonitoringInterval As Integer
    Public intTabDepth As Integer
    Public oDebugLog As StreamWriter
    Public oUsers() As User
    Public WithEvents oWorker As New Worker
    Public strAppName As String
    Public strCurrentDir As String
    Public strDebugLogfilePath As String
    Public strDefaultMailTo As String
    Public strMailFrom As String
    Public strSMTPServer As String
    Public strSubject As String

#End Region

#Region " Structures "

    Public Structure User
        Dim Path As String
        Dim Name As String
        Dim LockedOut As Boolean
        Dim AlertEmailAddress As String
    End Structure

#End Region

#Region " Subroutines "

    Private Sub CreateSubdirectories()
        If Not My.Computer.FileSystem.DirectoryExists(strCurrentDir & "Logs") Then
            My.Computer.FileSystem.CreateDirectory(strCurrentDir & "Logs")
        End If

        If Not My.Computer.FileSystem.DirectoryExists(strCurrentDir & "Config") Then
            My.Computer.FileSystem.CreateDirectory(strCurrentDir & "Config")
        End If
    End Sub

    Private Sub FinishLogging()
        oDebugLog.Flush()
        oDebugLog.Close()
    End Sub

    Private Sub GetConfig()
        Dim ConfigSettings As AppSettingsReader = New AppSettingsReader

        Try
            bDebugMode = CType(ConfigSettings.GetValue("DebugMode", GetType(System.Boolean)), Boolean)
            strAppName = CType(ConfigSettings.GetValue("AppName", GetType(System.String)), String)
            strSMTPServer = CType(ConfigSettings.GetValue("smtpServer", GetType(System.String)), String)
            strDefaultMailTo = CType(ConfigSettings.GetValue("DefaultMailTo", GetType(System.String)), String)
            strMailFrom = CType(ConfigSettings.GetValue("MailFrom", GetType(System.String)), String)
            strSubject = CType(ConfigSettings.GetValue("Subject", GetType(System.String)), String)
            intMonitoringInterval = CType(ConfigSettings.GetValue("MonitoringIntervalMinutes", GetType(System.String)), Integer)
        Catch
            bDebugMode = True
            strAppName = "User Account Lockout Monitoring Service"
            strSMTPServer = "smtphost"
            strDefaultMailTo = ""
            strMailFrom = ""
            strSubject = "User Account Lockout Monitoring Alert"
            intMonitoringInterval = 1
        End Try

        strDebugLogfilePath = strCurrentDir & "Logs\debug.log"

    End Sub

    Private Sub GetCurrentDir()
        ' Get the current directory, ie the directory where the application lives
        strCurrentDir = Application.StartupPath

        If Not strCurrentDir.EndsWith("\") Then
            strCurrentDir += "\"
        End If
    End Sub

    Private Sub GetUserList()
        Dim i As Integer
        Dim strDataPortions() As String
        Dim strLeftPortion As String
        Dim strRightPortion As String

        WriteToDebugLog("Inside GetUserList")

        If System.IO.File.Exists(strCurrentDir & "Config\Userlist.txt") = True Then
            Dim oTextReader As New System.IO.StreamReader(strCurrentDir & "Config\Userlist.txt")
            Dim strData As String

            Try
                Do
                    strData = oTextReader.ReadLine()
                    ReDim Preserve oUsers(i)
                    If strData Is Nothing Then Exit Do
                    If strData <> "" And Trim(strData.StartsWith("#")) = False Then
                        strDataPortions = Split(strData, "|", -1, CompareMethod.Text)
                        strLeftPortion = strDataPortions(0)
                        oUsers(i).Path = strLeftPortion
                        WriteToDebugLog("Loaded User Path:" & oUsers(i).Path)

                        ' Just in case there is no pipe (|), place the code to extract the 2nd portion inside try.catch.end try
                        Try
                            strRightPortion = strDataPortions(1)
                        Catch
                            strRightPortion = strDefaultMailTo
                            WriteToDebugLog("No 'TO:' email address specified for user.  Using default.")
                        End Try

                        oUsers(i).AlertEmailAddress = strRightPortion
                        WriteToDebugLog("Loaded Email Address:" & oUsers(i).AlertEmailAddress)
                        i += 1
                    End If
                Loop Until strData Is Nothing
            Catch
                WriteToDebugLog("Error " & Err.Number & " (" & Err.Description & ") in GetUserList.")
            End Try
            oTextReader.Close()
        Else
            WriteToDebugLog("No users specified.")
        End If
        WriteToDebugLog("Leaving GetUserList")
    End Sub

    Protected Overrides Sub OnContinue()

        WriteToDebugLog(strAppName & " continued.")

        GetConfig()

        doPause = False

        Me.tmrMain.Enabled = True

    End Sub

    Protected Overrides Sub OnPause()

        WriteToDebugLog(strAppName & " paused.")

        doPause = True

        Me.tmrMain.Enabled = False

    End Sub

    Protected Overrides Sub OnStart(ByVal args() As String)
        GetCurrentDir()

        GetConfig()

        CreateSubdirectories()

        doStop = False

        StartLogging()

        ' Output startup mode to eventlog
        WriteToDebugLog(strAppName & " started.")

        GetUserList()

        WriteToDebugLog(UBound(oUsers) & " User(s) to monitor")

        Me.tmrMain.Enabled = True

    End Sub

    Protected Overrides Sub OnStop()
        WriteToDebugLog(strAppName & " stopped.")

        doStop = True

        FinishLogging()

        Me.tmrMain.Enabled = False

    End Sub

    Private Sub oWorker_Alert(ByVal Message As String) Handles oWorker.Alert
        WriteToDebugLog(Message)
    End Sub

    Private Sub StartLogging()
        If File.Exists(strDebugLogfilePath) Then ' Rename the log
            ' Remove any old logs first
            If File.Exists(strDebugLogfilePath & ".old") Then
                File.Delete(strDebugLogfilePath & ".old")
            End If

            ' Rename (move) the file
            File.Move(strDebugLogfilePath, strDebugLogfilePath & ".old")
        End If
        oDebugLog = File.CreateText(strDebugLogfilePath)
    End Sub

    ' This is what actually kicks off the work...
    Private Sub tmrMain_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles tmrMain.Elapsed
        Static iCounter As Integer
        Dim i As Int32

        iCounter += 1

        WriteToDebugLog("Timer fired.  Waiting for Counter to equal " & intMonitoringInterval.ToString & ".  Counter now equals " & iCounter.ToString & ".")

        If iCounter < intMonitoringInterval Then
            WriteToDebugLog("Leaving Timer subroutine.")
            Exit Sub
        End If

        iCounter = 0

        WriteToDebugLog("Inside Main Worker thread")
        WriteToDebugLog("--------------------------------------------------------------------------------")

        ' Disable timer to prevent recursive events from being triggered
        Me.tmrMain.Enabled = False

        For i = 0 To UBound(oUsers) - 1
            If oUsers(i).Path <> "" Then
                WriteToDebugLog("User Path: " & oUsers(i).Path)

                oWorker.UserPath = oUsers(i).Path
                oWorker.SMTPServer = strSMTPServer
                oWorker.MailFrom = strMailFrom
                oWorker.MailTo = oUsers(i).AlertEmailAddress
                oWorker.Subject = strSubject

                oWorker.GetLockoutStatus()

                oUsers(i).LockedOut = oWorker.AccountLockedOut

                WriteToDebugLog("--------------------------------------------------------------------------------")

            End If
        Next i

        ' Force Garbage Collection
        GC.Collect()
        GC.WaitForPendingFinalizers()

        WriteToDebugLog("Leaving Main Worker thread")
        WriteToDebugLog("################################################################################")

        ' Re-enable timer
        Me.tmrMain.Enabled = True
    End Sub

    Public Sub WriteToDebugLog(ByVal strMessage As String)
        If strMessage <> "" Then
            Try
                If bDebugMode Then
                    oDebugLog.WriteLine(Now & " " & Space(intTabDepth) & strMessage)
                    oDebugLog.Flush()
                End If
            Catch
                ' Doh.
            End Try
        End If
    End Sub

#End Region

End Class
