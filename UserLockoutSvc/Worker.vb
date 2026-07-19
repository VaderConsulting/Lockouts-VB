Imports System.Windows.Forms.Application

Public Class Worker

#Region " Events "

    Public Event Alert(ByVal Message As String)

#End Region

#Region " Private variables "

    Private _ConnectionError As Boolean = False
    Private _ErrorCode As Long
    Private _ErrorDescription As String
    Private _UserPath As String
    Private _SMTPServer As String
    Private _MailTo As String
    Private _MailFrom As String
    Private _Subject As String
    Private _AccountLockedOut As Boolean

#End Region

#Region " Properties "

    Public Property UserPath() As String
        Get
            RaiseEvent Alert("Retrieving UserPath")
            Return _UserPath
        End Get
        Set(ByVal Value As String)
            RaiseEvent Alert("Setting UserPath")
            _UserPath = Value
        End Set
    End Property

    Public ReadOnly Property ConnectionError() As Boolean
        Get
            Return _ConnectionError
        End Get
    End Property

    Public ReadOnly Property ErrorCode() As Long
        Get
            Return (_ErrorCode)
        End Get
    End Property

    Public ReadOnly Property ErrorDescription() As String
        Get
            Return _ErrorDescription
        End Get
    End Property

    Public Property SMTPServer() As String
        Get
            Return _SMTPServer
        End Get
        Set(ByVal Value As String)
            _SMTPServer = Value
        End Set
    End Property

    Public Property MailTo() As String
        Get
            Return _MailTo
        End Get
        Set(ByVal Value As String)
            _MailTo = Value
        End Set
    End Property

    Public Property MailFrom() As String
        Get
            Return _MailFrom
        End Get
        Set(ByVal Value As String)
            _MailFrom = Value
        End Set
    End Property

    Public Property Subject() As String
        Get
            Return _Subject
        End Get
        Set(ByVal Value As String)
            _Subject = Value
        End Set
    End Property

    Public Property AccountLockedOut() As Boolean
        Get
            Return _AccountLockedOut
        End Get
        Set(ByVal value As Boolean)
            _AccountLockedOut = value
        End Set
    End Property

#End Region

#Region " Subroutines "

    Public Sub GetLockoutStatus()
        Dim oUser As ActiveDs.IADsUser
        Dim strMessage As String

        RaiseEvent Alert("Inside GetLockoutStatus")

        Try
            oUser = GetObject(_UserPath)

            If oUser.IsAccountLocked Then
                _AccountLockedOut = True

                strMessage = ""

                strMessage += "<HTML>" & vbCrLf
                strMessage += "<BODY>" & vbCrLf
                strMessage += "<p style='FONT-WEIGHT: bold; FONT-SIZE: 10pt; COLOR: black; FONT-FAMILY: Arial; TEXT-ALIGN: center'>User Lockout Monitoring Report</p>" & vbCrLf
                strMessage += "<p style='FONT-WEIGHT: normal; FONT-SIZE: 8pt; COLOR: blue; FONT-FAMILY: Arial; TEXT-ALIGN: left'>This email has been sent to inform you the following user has been locked out:</p>" & vbCrLf
                strMessage += "<p style='FONT-WEIGHT: normal; FONT-SIZE: 7pt; COLOR: red; FONT-FAMILY: Arial; TEXT-ALIGN: left'>" & _UserPath & "<br></p>" & vbCrLf
                strMessage += "<p style='FONT-WEIGHT: normal; FONT-SIZE: 7pt; COLOR: red; FONT-FAMILY: Arial; TEXT-ALIGN: left'>LastFailedLogin: " & oUser.LastFailedLogin & "</p>" & vbCrLf

                strMessage += "<hr>" & vbCrLf

                strMessage += "<p style='FONT-WEIGHT: bold; FONT-SIZE: 8pt; COLOR: blue; FONT-FAMILY: Arial; TEXT-ALIGN: left'>" & vbCrLf
                strMessage += "To prevent this message from being resent continuously, you must unlock this account as soon as possible.<br></p>" & vbCrLf
                strMessage += "<p style='FONT-WEIGHT: normal; FONT-SIZE: 8pt; COLOR: blue; FONT-FAMILY: Arial; TEXT-ALIGN: left'>" & vbCrLf
                strMessage += "Time of extraction: " & Now & "<br>" & vbCrLf

                strMessage += "<br>The service that produced this report is running on " & Microsoft.VisualBasic.Environ("COMPUTERNAME") & "." & vbCrLf

                strMessage += "</p>" & vbCrLf

                strMessage += "</BODY>" & vbCrLf
                strMessage += "</HTML>" & vbCrLf


                SendEmail(_MailFrom, _MailTo, _Subject, strMessage, _SMTPServer, True)
            Else
                _AccountLockedOut = False
            End If
        Catch
            RaiseEvent Alert("Error binding to User account.  The error was: " & Err.Number & " (" & Err.Description & ").")
        End Try

        RaiseEvent Alert("Leaving GetLockoutStatus")

    End Sub

#End Region

#Region " Functions "

    Private Function SendEmail(ByVal strFrom As String, ByVal strTo As String, ByVal strSubject As String, ByVal strMessage As String, ByVal strServer As String, Optional ByVal bHTML As Boolean = False) As Boolean
        Dim objEmail As Object

        RaiseEvent Alert("Inside SendEmail")

        If Trim(strFrom) = "" Or Trim(strTo) = "" Or Trim(strSubject) = "" Or Trim(strMessage) = "" Or Trim(strServer) = "" Then
            Exit Function
        End If

        SendEmail = True

        objEmail = CreateObject("CDO.Message")

        objEmail.From = strFrom
        objEmail.To = strTo
        objEmail.Subject = strSubject

        If bHTML = True Then
            objEmail.Htmlbody = strMessage
        Else
            objEmail.Textbody = strMessage
        End If

        ' sendusing:
        ' 1 use local SMTP service (local to the specified smtpserver)
        ' 2 use port
        objEmail.Configuration.Fields.Item("http://schemas.microsoft.com/cdo/configuration/sendusing") = 2
        objEmail.Configuration.Fields.Item("http://schemas.microsoft.com/cdo/configuration/smtpserver") = strServer
        objEmail.Configuration.Fields.Item("http://schemas.microsoft.com/cdo/configuration/smtpserverport") = 25
        objEmail.Configuration.Fields.Update()

        On Error Resume Next

        RaiseEvent Alert("Attempting to send email to " & strTo & ".")
        objEmail.Send()
        objEmail = Nothing

        'Return True if it worked
        If Err.Number = 0 Then
            RaiseEvent Alert("Email sent")
            RaiseEvent Alert("Leaving SendEmail")
            Return True
        Else
            RaiseEvent Alert("Error sending email")
            RaiseEvent Alert("Leaving SendEmail")
            Return False
        End If
    End Function

#End Region

End Class
