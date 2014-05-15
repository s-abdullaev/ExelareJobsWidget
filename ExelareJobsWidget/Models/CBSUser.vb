Imports ExelareJobsWidget.svcUserMgr
Imports ExelareJobsWidget.svcDataMgr
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Namespace Models

    Public Class CBSUser
        Public Property CompanyId As String
        Public Property UserId As String
        Public Property Password As String
    End Class

    Public Class CBSAuthResponse
        Private _usr As dcUser
        Private _appXml As XDocument

        Public Sub New(usr As dcUser)
            _usr = usr
            _appXml = _usr.rAppXML.xDecompress.xToXDoc
        End Sub

        Public ReadOnly Property companyId As String
            Get
                Return _usr.CompanyID
            End Get
        End Property

        Public ReadOnly Property userId As String
            Get
                Return _usr.UserID
            End Get
        End Property

        Public ReadOnly Property authToken As String
            Get
                Return _usr.rAuthToken
            End Get
        End Property

        Public ReadOnly Property isAuth As Boolean
            Get
                Return Not String.IsNullOrEmpty(_usr.rAuthToken)
            End Get
        End Property

        Public ReadOnly Property entitiesTree As JObject
            Get
                If _appXml.Root.<Entities>.Any Then
                    Return BuildTree(_appXml.Root.<Entities>.First)
                End If

                Return New JObject
            End Get
        End Property

        Public ReadOnly Property entityPKs As Dictionary(Of String, JObject)
            Get
                Dim entPks As New Dictionary(Of String, JObject)
                For Each el In _appXml.Root.<EntityPKs>.Elements
                    entPks(el.@ID) = el.xToJObj
                Next

                Return entPks
            End Get
        End Property

        Public ReadOnly Property itemForms As IDictionary(Of String, CBSItemForm)
            Get
                Dim itmFrms As New Dictionary(Of String, CBSItemForm)

                For Each x In _appXml.Root.<ItemForm>
                    itmFrms.Add(x.@ID + x.@SubEntityID, New CBSItemForm(x))
                Next

                Return itmFrms
            End Get
        End Property

        Private Function BuildTree(xel As XElement, Optional namePrefix As String = "") As JObject
            Dim jobj As New JObject

            If Not xel.Name.LocalName.xIsInKinds Then Return Nothing

            jobj.Add("type", xel.Name.LocalName)

            For Each attr In xel.Attributes
                If attr.Name.LocalName.ToLower = "id" Then
                    jobj.Add("itemId", attr.Value)
                    jobj.Add("id", namePrefix & attr.Value)
                Else
                    jobj.Add(attr.Name.LocalName.ToLower, attr.Value)
                End If
            Next

            If Not jobj.Properties.Any(Function(c) c.Name = "itemId") Then jobj.Add("itemId", xel.Name.LocalName)
            If Not jobj.Properties.Any(Function(c) c.Name.ToLower = "id") Then jobj.Add("id", namePrefix & xel.Name.LocalName)

            If xel.HasElements Then
                jobj.Add("children", New JArray())
                For Each el In xel.Elements
                    Dim jobj2 As JObject = BuildTree(el, jobj("id"))
                    If jobj2 IsNot Nothing Then CType(jobj("children"), JArray).Add(jobj2)
                Next
            Else
                jobj.Add("leaf", True)
            End If

            Return jobj
        End Function
    End Class

    Public Class CBSItemForm
        Private _xel As XElement
        Private _tabCtrl As XElement

        Public Sub New(xel As XElement)
            _xel = xel
            _tabCtrl = _xel.Elements.First(Function(c) c.Name.LocalName = "C1TabControl")
        End Sub

        Public ReadOnly Property id As String
            Get
                Return _xel.@ID
            End Get
        End Property

        Public ReadOnly Property subEntityId As String
            Get
                Return _xel.@SubEntityID
            End Get
        End Property

        Public ReadOnly Property height As Integer
            Get
                Return _tabCtrl.@Height
            End Get
        End Property

        Public ReadOnly Property width As Integer
            Get
                Return _tabCtrl.@Width
            End Get
        End Property

        'Public ReadOnly Property tag As JObject
        '    Get
        '        Dim tagXml As XElement = XElement.Parse(_tabCtrl.@Tag)

        '        Return tagXml.xToJObj
        '    End Get
        'End Property

        Public ReadOnly Property tabs As IEnumerable(Of CBSItemFormTab)
            Get
                Dim tbs As New List(Of CBSItemFormTab)
                For Each x In _xel.Descendants.Where(Function(c) c.Name.LocalName = "C1TabItem")
                    tbs.Add(New CBSItemFormTab(x))
                Next
                Return tbs
            End Get
        End Property
    End Class

    Public Class CBSItemFormTab
        Private _xel As XElement

        Public Sub New(xel As XElement)
            _xel = xel
        End Sub

        Public ReadOnly Property header As String
            Get
                Return _xel.@Header
            End Get
        End Property

        Public ReadOnly Property controls As IEnumerable(Of CBSItemFormControl)
            Get
                Dim ctrls As New List(Of CBSItemFormControl)
                For Each x In _xel.Elements.First.Elements.Where(Function(c) c.Name.LocalName = "XField")
                    ctrls.Add(New CBSItemFormControl(x))
                Next
                Return ctrls
            End Get
        End Property
    End Class

    Public Class CBSItemFormControl
        Private _xel As XElement

        Public Sub New(xel As XElement)
            _xel = xel
        End Sub

        Public ReadOnly Property itemId As String
            Get
                Return _xel.Attributes.First(Function(c) c.Name.LocalName = "Name").Value
            End Get
        End Property

        Public ReadOnly Property height As Integer
            Get
                Return _xel.@Height
            End Get
        End Property

        Public ReadOnly Property width As Integer
            Get
                Return _xel.@Width
            End Get
        End Property

        Public ReadOnly Property top As Integer
            Get
                If _xel.Attributes.Any(Function(c) c.Name.LocalName = "Canvas.Top") Then
                    Return _xel.Attribute("Canvas.Top").Value
                End If

                Return Nothing
            End Get
        End Property

        Public ReadOnly Property left As Integer
            Get
                If _xel.Attributes.Any(Function(c) c.Name.LocalName = "Canvas.Left") Then
                    Return _xel.Attribute("Canvas.Left").Value
                End If

                Return Nothing
            End Get
        End Property

        Public ReadOnly Property bitProps As String
            Get
                Return _xel.@BitProps
            End Get
        End Property

        Public ReadOnly Property items As JArray
            Get
                Dim itms As New JArray
                For Each x As XElement In _xel.Descendants.Where(Function(c) c.Name.LocalName = "XItem")
                    Dim itm As JObject = x.xToJObj

                    If x.@Type = "Answer" Then itm.Add("itemId", itemId)

                    itms.Add(itm)
                Next

                Return itms
            End Get
        End Property
    End Class

    Public Class CBSOrderBy
        Public Property [property] As String
        Public Property direction As String

        Public Overridable ReadOnly Property ToXml As XElement
            Get
                Dim r As XElement = <Order></Order>

                If String.IsNullOrWhiteSpace([property]) Then Return Nothing Else r.@FieldName = [property]
                If String.IsNullOrWhiteSpace(direction) Then r.@Order = "ASC" Else r.@Order = direction.ToUpper()

                Return r
            End Get
        End Property
    End Class


    Public Class CBSGetPageRequest
        Public Property CompanyId As String
        Public Property UserId As String = "Admin"
        Public Property EntityId As String = "Requirements"
        Public Property Which As String = "DView"
        Public Property WhichId As String = "Open"
        Public Property FilterBy As JObject
        Public Property OrderBy As IEnumerable(Of CBSOrderBy)
        Public Property PageNumber As String
        Public Property PageSize As String
    End Class

    Public Class CBSGetPageResponse
        Private _data As dcDataResp
        Private _dataReq As dcDataReq
        Private _xRecsDoc As XDocument
        Private _xSchemaDoc As XDocument

        Public Sub New(data As dcDataResp, dataReq As dcDataReq)
            _data = data
            _dataReq = dataReq
            _xRecsDoc = _data.RecordSet.xDecompress.xToXDoc
            _xSchemaDoc = _data.Schema.xDecompress.xToXDoc
        End Sub

        Public ReadOnly Property RecordCount As Integer
            Get
                Return _data.RecordCount
            End Get
        End Property

        Public ReadOnly Property HasMorePages As Boolean
            Get
                Return _data.HasMorePages
            End Get
        End Property

        Public ReadOnly Property PageNumber As Integer
            Get
                Return _data.PageNumber
            End Get
        End Property

        Public ReadOnly Property Schema As Dictionary(Of String, JObject)
            Get
                Dim schemaDic As New Dictionary(Of String, JObject)

                For Each el In _xSchemaDoc.Root.Elements
                    schemaDic(el.@FieldName) = el.xToJObj
                Next

                Return schemaDic
            End Get
        End Property

        Public ReadOnly Property ActiveColumns As Dictionary(Of String, JObject)
            Get
                Dim colsDic As New Dictionary(Of String, JObject)
                Dim sch As Dictionary(Of String, JObject) = Schema
                Dim seqEl As XElement = _xRecsDoc.Root.Descendants.First(Function(c) c.Name.LocalName = "sequence")
                For Each el In seqEl.Elements
                    If sch.ContainsKey(el.@name) Then colsDic(el.@name) = sch(el.@name)
                Next

                Return colsDic
            End Get
        End Property

        Public ReadOnly Property Records As JArray
            Get
                Dim recEls As IEnumerable(Of XElement) = _xRecsDoc.Root.Elements.Where(Function(c) c.Name = "{0}.{1}.{2}".xFillBlanks(_dataReq.EntityID, _dataReq.Which, _dataReq.WhichID))
                Dim recs As New JArray

                For Each recEl In recEls
                    recs.Add(recEl.xToJObj)
                Next

                Return recs
            End Get
        End Property
    End Class

End Namespace

