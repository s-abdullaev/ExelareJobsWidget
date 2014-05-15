Imports System.ComponentModel
Imports System.ServiceModel

Public Class CBSServiceFactory
    Private Shared _RequestsCount As Integer
    Private Shared _Caption As String 'WALLACE
    Private Shared _Timeout As Integer 'WALLACE


    Private Shared _UserMgr As svcUserMgr.SvcUserMgrClient
    Private Shared _DataMgr As svcDataMgr.SvcDataMgrClient
    Private Shared _ItemMgr As svcItemsMgr.SvcItemsMgrClient
    Private Shared _LinkedItemMgr As svcLinkedItemsMgr.SvcLinkedItemsMgrClient
    

    Public Shared Property SvcUri As String

    Public Shared ReadOnly Property UserMgr As svcUserMgr.ISvcUserMgr
        Get
            Return DirectCast(_UserMgr, svcUserMgr.ISvcUserMgr)
        End Get
    End Property

    Public Shared ReadOnly Property DataMgr As svcDataMgr.ISvcDataMgr
        Get
            Return DirectCast(_DataMgr, svcDataMgr.ISvcDataMgr)
        End Get
    End Property

    Public Shared ReadOnly Property ItemMgr As svcItemsMgr.ISvcItemsMgr
        Get
            Return DirectCast(_ItemMgr, svcItemsMgr.ISvcItemsMgr)
        End Get
    End Property

    Public Shared ReadOnly Property LinkedItemMgr As svcLinkedItemsMgr.ISvcLinkedItemsMgr
        Get
            Return DirectCast(_LinkedItemMgr, svcLinkedItemsMgr.ISvcLinkedItemsMgr)
        End Get
    End Property


    Public Shared Property RequestsCount As Integer
        Get
            Return _RequestsCount
        End Get
        Set(ByVal value As Integer)
            _RequestsCount = value
            NotifyPropertyChanged("RequestsCount")
        End Set
    End Property

    'WALLACE
    Public Shared Property Caption As String
        Get
            Return _Caption
        End Get
        Set(ByVal value As String)
            _Caption = value
            'NotifyPropertyChanged("Caption")
        End Set
    End Property

    'WALLACE
    Public Shared Property Timeout As Integer
        Get
            Return _Timeout
        End Get
        Set(ByVal value As Integer)
            _Timeout = value
        End Set
    End Property

    'WALLACE
    'Public Shared Sub AddRefCount()
    '    SyncLock GetType(CBSServiceFactory)
    '        RequestsCount = RequestsCount + 1
    '    End SyncLock
    'End Sub

    Private Shared ShowProcessing As Boolean = True

    Public Shared Sub EnableProcessing(Optional Enable As Boolean = True)
        ShowProcessing = Enable
    End Sub

    Public Shared Sub AddRefCount(Optional ByVal capt As String = "Processing", Optional ByVal Time As Integer = 1000)
        If Not ShowProcessing Then Return
        SyncLock GetType(CBSServiceFactory)
            Caption = capt
            Timeout = Time
            RequestsCount = RequestsCount + 1
        End SyncLock
    End Sub

    Public Shared Sub DelRefCount()
        If Not ShowProcessing Then Return
        SyncLock GetType(CBSServiceFactory)
            If (RequestsCount > 0) Then RequestsCount = RequestsCount - 1
        End SyncLock
    End Sub

    Shared Sub New()

        'We need this for handling Faults
        'http://msdn.microsoft.com/en-us/library/ee844556(v=vs.95).aspx


        'retrieve Auth token from webserver
        HttpAuthTokenMessageInspector.AuthToken = Nothing
        _RequestsCount = 0

        'Dim binding As New BasicHttpBinding(If(Application.Current.Host.Source.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase), BasicHttpSecurityMode.Transport, BasicHttpSecurityMode.None))
        'binding.MaxReceivedMessageSize = Int32.MaxValue
        'binding.MaxBufferSize = Int32.MaxValue
        '_UserMgr = New svcUserMgr.SvcUserMgrClient(binding, New EndpointAddress(New Uri(Application.Current.Host.Source, "../Address.svc")))

        _UserMgr = New svcUserMgr.SvcUserMgrClient
        '_UserMgr = _UserMgr.Endpoint.Address.Uri.xCreateService(Of svcUserMgr.SvcUserMgrClient)()
        '_UserMgr.Endpoint.Address = New EndpointAddress(_UserMgr.Endpoint.Address.Uri.xAdjustUri)
        _UserMgr.Endpoint.Behaviors.Add(New HttpAuthTokenEndpointBehavior())
        AddHandler _UserMgr.InnerChannel.Faulted, AddressOf OnServiceError

        _DataMgr = New svcDataMgr.SvcDataMgrClient
        '_DataMgr = _DataMgr.Endpoint.Address.Uri.xCreateService(Of svcDataMgr.SvcDataMgrClient)()
        '_DataMgr.Endpoint.Address = New EndpointAddress(_DataMgr.Endpoint.Address.Uri.xAdjustUri)
        _DataMgr.Endpoint.Behaviors.Add(New HttpAuthTokenEndpointBehavior())
        AddHandler _DataMgr.InnerChannel.Faulted, AddressOf OnServiceError

        _ItemMgr = New svcItemsMgr.SvcItemsMgrClient
        '_ItemMgr = _ItemMgr.Endpoint.Address.Uri.xCreateService(Of svcItemsMgr.SvcItemsMgrClient)()
        '_ItemMgr.Endpoint.Address = New EndpointAddress(_ItemMgr.Endpoint.Address.Uri.xAdjustUri)
        _ItemMgr.Endpoint.Behaviors.Add(New HttpAuthTokenEndpointBehavior())
        AddHandler _ItemMgr.InnerChannel.Faulted, AddressOf OnServiceError

        _LinkedItemMgr = New svcLinkedItemsMgr.SvcLinkedItemsMgrClient
        '_LinkedItemMgr = _LinkedItemMgr.Endpoint.Address.Uri.xCreateService(Of svcLinkedItemsMgr.SvcLinkedItemsMgrClient)()
        '_LinkedItemMgr.Endpoint.Address = New EndpointAddress(_LinkedItemMgr.Endpoint.Address.Uri.xAdjustUri)
        _LinkedItemMgr.Endpoint.Behaviors.Add(New HttpAuthTokenEndpointBehavior())
        AddHandler _LinkedItemMgr.InnerChannel.Faulted, AddressOf OnServiceError
        'AddHandler _LinkedItemMgr.GetItemInfoCompleted, AddressOf GetLinkedItemInfoCompleted

 

        SvcUri = _UserMgr.Endpoint.Address.Uri.AbsoluteUri
    End Sub

    Public Shared Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)

    ' NotifyPropertyChanged will raise the PropertyChanged event passing the 
    ' source property that is being updated. 
    Private Shared Sub NotifyPropertyChanged(ByVal PropertyName As String)
        RaiseEvent PropertyChanged(Nothing, New PropertyChangedEventArgs(PropertyName))
    End Sub

    Private Shared Sub OnServiceError(ByVal sender As Object, ByVal e As EventArgs)
        Dim i = 3
        'MessageBox.Show("Connection fault: " & e.ToString())
    End Sub

End Class

