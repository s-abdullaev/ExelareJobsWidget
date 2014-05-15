Imports System.ServiceModel.Dispatcher
Imports System.ServiceModel.Channels
Imports System.ServiceModel.Description
Imports System.ServiceModel.Configuration
Imports System.ServiceModel
Imports System.Configuration
Imports System.Text
Imports System.Security.Cryptography
Imports System.ComponentModel

#If REQUIRED_MULTIPLE_LOGINS Then
''' <summary>
''' Multiple-login per all threads behaviour extension classes
''' </summary>
''' <remarks></remarks>

Public Class HttpAuthTokenMessageInspector
    Implements IClientMessageInspector

    Private Const AUTH_HTTP_HEADER As String = "CBSAUTH_HTTP_HEADER"
    Private AuthToken As String

    Public Sub New(at As String)
        AuthToken = at
    End Sub

#Region "IClientMessageInspector Members"
    Public Sub AfterReceiveReply(ByRef reply As System.ServiceModel.Channels.Message, ByVal correlationState As Object) Implements IClientMessageInspector.AfterReceiveReply
    End Sub

    Public Function BeforeSendRequest(ByRef request As System.ServiceModel.Channels.Message, ByVal channel As System.ServiceModel.IClientChannel) As Object Implements IClientMessageInspector.BeforeSendRequest
        If Not AuthToken Is Nothing Then
            request.Headers.Add(MessageHeader.CreateHeader(AUTH_HTTP_HEADER, "", AuthToken))
        End If
        Return Nothing
    End Function
#End Region

End Class

Public Class HttpAuthTokenEndpointBehavior
    Implements IEndpointBehavior

    Private AuthToken As String

    Public Sub New(at As String)
        AuthToken = at
    End Sub

#Region "IEndpointBehavior Members"

    Public Sub AddBindingParameters(ByVal endpoint As ServiceEndpoint, ByVal bindingParameters As System.ServiceModel.Channels.BindingParameterCollection) Implements IEndpointBehavior.AddBindingParameters

    End Sub

    Public Sub ApplyClientBehavior(ByVal endpoint As ServiceEndpoint, ByVal clientRuntime As System.ServiceModel.Dispatcher.ClientRuntime) Implements IEndpointBehavior.ApplyClientBehavior
        Dim inspector As New HttpAuthTokenMessageInspector(AuthToken)
        clientRuntime.MessageInspectors.Add(inspector)
    End Sub

    Public Sub ApplyDispatchBehavior(ByVal endpoint As ServiceEndpoint, ByVal endpointDispatcher As System.ServiceModel.Dispatcher.EndpointDispatcher) Implements IEndpointBehavior.ApplyDispatchBehavior

    End Sub

    Public Sub Validate(ByVal endpoint As ServiceEndpoint) Implements IEndpointBehavior.Validate

    End Sub

#End Region

End Class

#Else

''' <summary>
''' Single-login per all threads behaviour extension classes
''' </summary>
''' <remarks></remarks>

Public Class SessionLifeTimeMonitor
    Implements INotifyPropertyChanged

    Public Property IsEnabled As Boolean = False

    Public Property SessionLifeTime As TimeSpan
        Get
            Return _sessionTimeLife
        End Get
        Set(value As TimeSpan)
            _sessionTimeLife = value
            If Not IsEnabled Then Return
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SessionLifeTime"))
            If value = HttpAuthTokenMessageInspector.SessionDuration Then _timer.Change(0, _tick)
        End Set
    End Property

    Private _sessionTimeLife As TimeSpan
    Private _tick As Long = TimeSpan.FromSeconds(1).TotalMilliseconds ' in milliseconds
    Private _timer As New System.Threading.Timer(AddressOf OnTick)

    Private Sub OnTick()
        If Not IsEnabled Then Return
#If SILVERLIGHT Then
        Deployment.Current.Dispatcher.BeginInvoke(
            Sub()
                If SessionLifeTime.Ticks > 0 Then
                    SessionLifeTime = SessionLifeTime.Subtract(TimeSpan.FromMilliseconds(_tick))
                Else
                    RaiseEvent SessionExpired(Me, New EventArgs)
                End If
            End Sub)
#End If
    End Sub

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event SessionExpired(sender As Object, e As EventArgs)
End Class


Public Class HttpAuthTokenMessageInspector
    Implements IClientMessageInspector

    Public Shared AUTH_HTTP_HEADER = "CBSAUTH_HTTP_HEADER"
    Public Shared AuthToken As String
    Public Shared SessionDuration As TimeSpan ' Should be set once per login - constant through session
    Public Shared Property Monitor As New SessionLifeTimeMonitor

    Private dt As DateTime

#Region "IClientMessageInspector Members"
    Public Sub AfterReceiveReply(ByRef reply As System.ServiceModel.Channels.Message, ByVal correlationState As Object) Implements IClientMessageInspector.AfterReceiveReply
#If DEBUG And SILVERLIGHT Then
        Diagnostics.Debug.WriteLine(DateTime.Now.ToShortTimeString() & ": " & reply.ToString())
#End If
        Dim str As String = reply.ToString()
        If String.IsNullOrEmpty(str) OrElse (Not str.Contains("SendEMail") AndAlso Not str.Contains("ReSendEMail") AndAlso Not str.Contains("GetOutboxItems")) Then
            CBSServiceFactory.DelRefCount()
        End If
    End Sub

    Public Function BeforeSendRequest(ByRef request As System.ServiceModel.Channels.Message, ByVal channel As System.ServiceModel.IClientChannel) As Object Implements IClientMessageInspector.BeforeSendRequest
#If DEBUG And SILVERLIGHT Then
        Diagnostics.Debug.WriteLine(DateTime.Now.ToShortTimeString() & ": " & request.ToString())
#End If
        Monitor.SessionLifeTime = SessionDuration
        Dim str As String = request.Headers.Action
        If String.IsNullOrEmpty(str) OrElse (Not str.Contains("SendEMail") AndAlso Not str.Contains("ReSendEMail") AndAlso Not str.Contains("GetOutboxItems")) Then
#If DEBUG Then
            CBSServiceFactory.AddRefCount(request.Headers.Action)
#Else
            CBSServiceFactory.AddRefCount()
#End If
        End If
        If Not AuthToken Is Nothing Then
            request.Headers.Add(MessageHeader.CreateHeader(AUTH_HTTP_HEADER, "", AuthToken))
        End If
        'dt = Now
        Return Nothing
    End Function

#End Region
End Class

Public Class HttpAuthTokenEndpointBehavior
    Implements IEndpointBehavior

#Region "IEndpointBehavior Members"

    Public Sub AddBindingParameters(ByVal endpoint As ServiceEndpoint, ByVal bindingParameters As System.ServiceModel.Channels.BindingParameterCollection) Implements IEndpointBehavior.AddBindingParameters

    End Sub

    Public Sub ApplyClientBehavior(ByVal endpoint As ServiceEndpoint, ByVal clientRuntime As System.ServiceModel.Dispatcher.ClientRuntime) Implements IEndpointBehavior.ApplyClientBehavior
        Dim inspector As New HttpAuthTokenMessageInspector()
        clientRuntime.MessageInspectors.Add(inspector)
    End Sub

    Public Sub ApplyDispatchBehavior(ByVal endpoint As ServiceEndpoint, ByVal endpointDispatcher As System.ServiceModel.Dispatcher.EndpointDispatcher) Implements IEndpointBehavior.ApplyDispatchBehavior

    End Sub

    Public Sub Validate(ByVal endpoint As ServiceEndpoint) Implements IEndpointBehavior.Validate

    End Sub

#End Region

End Class

#End If


Public Class CookieInspector
    Implements IClientMessageInspector
    Public Shared SessionID As String

    Public Sub AfterReceiveReply(ByRef reply As System.ServiceModel.Channels.Message, correlationState As Object) Implements System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply
        CBSServiceFactory.DelRefCount()
    End Sub

    Public Function BeforeSendRequest(ByRef request As System.ServiceModel.Channels.Message, channel As System.ServiceModel.IClientChannel) As Object Implements System.ServiceModel.Dispatcher.IClientMessageInspector.BeforeSendRequest
#If DEBUG Then
        CBSServiceFactory.AddRefCount(request.Headers.Action)
#Else
        CBSServiceFactory.AddRefCount()
#End If
        If Not SessionID Is Nothing Then
            request.Headers.Add(MessageHeader.CreateHeader("session_id", "", SessionID))
        End If
        Return Nothing
    End Function
End Class

Public Class CookieBehaviour
    Implements IEndpointBehavior

    Public Sub AddBindingParameters(endpoint As System.ServiceModel.Description.ServiceEndpoint, bindingParameters As System.ServiceModel.Channels.BindingParameterCollection) Implements System.ServiceModel.Description.IEndpointBehavior.AddBindingParameters

    End Sub

    Public Sub ApplyClientBehavior(endpoint As System.ServiceModel.Description.ServiceEndpoint, clientRuntime As System.ServiceModel.Dispatcher.ClientRuntime) Implements System.ServiceModel.Description.IEndpointBehavior.ApplyClientBehavior
        Dim cookInspector As New CookieInspector
        clientRuntime.MessageInspectors.Add(cookInspector)
    End Sub

    Public Sub ApplyDispatchBehavior(endpoint As System.ServiceModel.Description.ServiceEndpoint, endpointDispatcher As System.ServiceModel.Dispatcher.EndpointDispatcher) Implements System.ServiceModel.Description.IEndpointBehavior.ApplyDispatchBehavior

    End Sub

    Public Sub Validate(endpoint As System.ServiceModel.Description.ServiceEndpoint) Implements System.ServiceModel.Description.IEndpointBehavior.Validate

    End Sub
End Class