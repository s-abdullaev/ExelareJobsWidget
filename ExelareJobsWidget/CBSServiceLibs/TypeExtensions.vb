
Imports System.IO
Imports System.IO.Compression
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Security
Imports ICSharpCode.SharpZipLib
Imports System.Globalization
Imports System.Runtime.Caching
Imports ExelareJobsWidget.svcUserMgr

Module TypeExtensions
    Private cache1 As ObjectCache = MemoryCache.Default

    Public ReadOnly KINDS As String() = {"Entities", "Entity", "DViews", "DView", "SViews", "SView", "Reports", "Report", "Posting", "Outbox"}

    <Runtime.CompilerServices.Extension()> _
    Public Function xHasProperty(ByRef jobj As JObject, prop_name As String) As Boolean
        Return jobj.Properties.Any(Function(c) c.Name = prop_name)
    End Function

    <Runtime.CompilerServices.Extension()> _
    Public Function xGetFilterXml(ByRef jobj As JObject) As XElement
        Dim el As XElement

        If jobj.xHasProperty("FilterCol") Then
            el = New XElement(jobj("FilterColType").ToString)
            For Each j As JObject In jobj("FilterCol")
                el.Add(j.xGetFilterXml)
            Next
        Else
            el = <Filter></Filter>

            If jobj.xHasProperty("FilterType") Then
                el.@Type = jobj("FilterType").ToString
            Else
                el.@Type = jobj("DataType").ToString.xGetFilterType
            End If

            el.@FieldName = jobj("FieldName")
            el.@FieldValue1 = jobj("FieldValue1").ToString.xGetFilterValue(jobj("DataType").ToString, el.@Type)
            If jobj.xHasProperty("FieldValue2") Then el.@FieldValue2 = jobj("FieldValue2").ToString.xGetFilterValue(jobj("DataType").ToString, el.@Type)
        End If

        Return el
    End Function

    <Runtime.CompilerServices.Extension()> _
    Public Function xGetOrderXml(ByRef jobj As JObject) As XElement
        Dim el As XElement = <OrderBy></OrderBy>

        For Each j As JObject In jobj("OrderBy")
            Dim oEl As XElement = <Order></Order>

            oEl.@FieldName = j("fieldname")
            oEl.@Order = j("direction")

            el.Add(oEl)
        Next

        Return el
    End Function


    <Runtime.CompilerServices.Extension()> _
    Public Function xGetFilterType(ByRef type As String) As String
        Select Case type
            Case "String"
                Return "Like"
            Case Else
                Return "="
        End Select
    End Function

    <Runtime.CompilerServices.Extension()> _
    Public Function xGetFilterValue(ByRef value As String, type As String, filterType As String) As String
        Select Case type
            Case "String"
                If filterType = "Like" Then Return value.xLastPercentage.Replace("*", "%").xWrapWithSingleQuotes

                Return value.xWrapWithSingleQuotes
            Case Else
                Return value
        End Select
    End Function

    <Runtime.CompilerServices.Extension()> _
    Public Function xIsInKinds(ByVal str As String) As Boolean
        Return KINDS.Any(Function(c) c.ToLower = str.ToLower)
    End Function

    <Runtime.CompilerServices.Extension()> _
    Public Function xToXDoc(ByVal Bytes() As Byte) As XDocument
        Dim ResponseXML As String = Bytes.xConvertToUtf8String
        Return XDocument.Parse(ResponseXML)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetUserIdentifier(ByRef usr As dcUser) As String
        Return (usr.CompanyID & "_" & usr.UserID.Replace(" ", "_")).ToLower
    End Function


    <System.Runtime.CompilerServices.Extension()> _
    Public Sub xSaveCache(Of T)(name As String, obj As T, Optional keepTime As DateTimeOffset = Nothing)
        If keepTime = Nothing Then keepTime = DateTimeOffset.Now.AddMinutes(5)

        If cache1.Contains(name) Then
            cache1.Set(name, obj, keepTime)
        Else
            cache1.Add(name, obj, keepTime)
        End If
    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetCache(Of T)(name As String) As T
        Return cache1.Get(name)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xCacheContains(name As String) As Boolean
        Return cache1.Contains(name)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xSetTime(ByRef d As Date, ByVal t As String) As Date
        Dim m As Date
        If Date.TryParse(Format(d, "yyyy-MM-dd") & " " & t, m) Then Return m

        Return Date.Now

    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetTime(ByRef d As Date) As String
        Return Format(d, "H:m")
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToBoolean(ByRef str As String) As Boolean
        Return str.ToLower = "true"
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xNewLine(ByRef str As String, Optional ByVal appendVal As String = "") As String
        Return str & ControlChars.NewLine & appendVal
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToByteArray(ByRef str As String) As Byte()
        Return System.Text.Encoding.Unicode.GetBytes(str)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xParseJSON(ByRef str As String) As JObject
        Return JObject.Parse(str)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xCompress(ByRef d As Byte()) As Byte()

        If d Is Nothing Then Return d
        If d.Length = 0 Then Return d

        Dim msMemoryStream As New MemoryStream()
        Dim dfDeflater As New Zip.Compression.Deflater(Zip.Compression.Deflater.BEST_COMPRESSION, False)
        Dim dfDeflaterOutputStream As New Zip.Compression.Streams.DeflaterOutputStream(msMemoryStream, dfDeflater)

        dfDeflaterOutputStream.Write(d, 0, d.Length)
        dfDeflaterOutputStream.Close()

        Return msMemoryStream.ToArray
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xFindBy(ByRef jarr As JArray, ByVal propname As String, ByVal propvalue As String) As JObject
        Return jarr.FirstOrDefault(Function(c) c.xToJObject.Property(propname).Value.ToString = propvalue)
    End Function


    <System.Runtime.CompilerServices.Extension()> _
    Public Sub xSetPropertyValue(ByRef jobj As JObject, ByVal propname As String, ByVal propvalue As String)
        If Not jobj.Properties.Any(Function(c) c.Name.ToLower = propname.ToLower) Then jobj.Add(New JProperty(propname, ""))
        Dim prop As JProperty = jobj.Properties.FirstOrDefault(Function(c) c.Name.ToLower = propname.ToLower)

        If prop IsNot Nothing Then
            prop.Value = propvalue
        End If
    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Sub xSetPropertyValueAsString(ByRef jobj As JObject, ByVal propname As String, ByVal propvalue As Object, Optional ByVal IsAppend As Boolean = False)
        If Not jobj.Properties.Any(Function(c) c.Name.ToLower = propname.ToLower) Then jobj.Add(New JProperty(propname, Nothing))
        Dim prop As JProperty = jobj.Properties.FirstOrDefault(Function(c) c.Name.ToLower = propname.ToLower)

        If prop IsNot Nothing Then
            If IsAppend Then prop.Value = prop.Value.ToString & propvalue.ToString Else prop.Value = propvalue.ToString
        End If
    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Sub xSetPropertyValue(ByRef jobj As JObject, ByVal propname As String, ByVal propvalue As Object)
        If Not jobj.Properties.Any(Function(c) c.Name.ToLower = propname.ToLower) Then jobj.Add(New JProperty(propname, Nothing))
        Dim prop As JProperty = jobj.Properties.FirstOrDefault(Function(c) c.Name.ToLower = propname.ToLower)

        If prop IsNot Nothing Then
            prop.Value = propvalue
        End If

    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Sub xSetPropertyValueFromRow(ByRef jobj As JObject, ByVal colname As String, ByVal row As DataRow)
        If Not jobj.Properties.Any(Function(c) c.Name.ToLower = colname.ToLower) Then jobj.Add(New JProperty(colname, Nothing))
        Dim prop As JProperty = jobj.Properties.FirstOrDefault(Function(c) c.Name.ToLower = colname.ToLower)

        If prop IsNot Nothing Then
            prop.Value = row.xGetValue(colname)
        End If
    End Sub


    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetPropertyValue(ByRef jobj As JObject, ByVal propname As String) As String
        If jobj.Property(propname) Is Nothing Then Return ""

        Return jobj.Property(propname).Value.ToString
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetPropertyValueAsJObject(ByRef jobj As JObject, ByVal propname As String) As JObject
        If jobj.Property(propname) Is Nothing Then Return New JObject

        Return jobj.Property(propname).Value
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetPropertyValueAsJArray(ByRef jobj As JObject, ByVal propname As String) As JArray
        If jobj.Property(propname) Is Nothing Then Return New JArray

        Return jobj.Property(propname).Value
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToJObject(ByRef jobj As JToken) As JObject
        Return jobj
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToXElement(ByRef jobj As String) As XElement
        If String.IsNullOrWhiteSpace(jobj) Then Return Nothing

        Return JsonConvert.DeserializeXNode(jobj).Root
    End Function

    Public Function Login(companyId As String, userId As String) As Boolean
        Dim curName As String = "{0}{1}".xFillBlanks(companyId, userId)
        Dim user As dcUser

        HttpContext.Current.Trace.Write("Checking cache")
        If curName.xCacheContains Then
            HttpContext.Current.Trace.Write("Cache found")
            user = xGetCache(Of dcUser)(curName)


            HttpAuthTokenMessageInspector.AuthToken = user.rAuthToken
            HttpContext.Current.Trace.Write("Cache returned")
            Return True
        Else
            user = New dcUser
            user.CompanyID = companyId
            user.UserID = userId

#If DEBUG Then
            user.Password = "test"
#Else
            user.Password = ""
#End If
            user.Device = "BindQS-JobsWidget-" & Guid.NewGuid.ToString
            user.UTCOffsetHours = CSng(TimeZoneInfo.Local.BaseUtcOffset.TotalHours)
            user.TimeZoneStandardName = TimeZoneInfo.Local.StandardName
            user.ApplyDaylightSavingsTime = TimeZoneInfo.Local.SupportsDaylightSavingTime
            user.Relogin = False

            HttpContext.Current.Trace.Write(user.CompanyID)
            HttpContext.Current.Trace.Write(user.UserID)
            '  HttpContext.Current.Trace.Write(user.rAppXML.Decompress.ToXDocument.ToString)

            Try
                HttpContext.Current.Trace.Write("Calling AuthUserPlus")
                user = CBSServiceFactory.UserMgr.AuthUserPlus(user)


                HttpContext.Current.Trace.Write("Caching AuthUser response")
                xSaveCache(Of dcUser)(curName, user)
                HttpAuthTokenMessageInspector.AuthToken = user.rAuthToken

                Return True
            Catch ex As Exception
                HttpContext.Current.Trace.Write(ex.Message)
            End Try

        End If

        Return False
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xDecompress(ByRef d As Byte()) As Byte()
        If d IsNot Nothing Then
            Dim buffer(2048) As Byte
            Dim ms As New MemoryStream(d, 0, d.Length)
            Dim ms2 As New MemoryStream
            Dim bytesRead As Integer
            Dim s2 As New ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms)

            Dim binReader As New BinaryReader(s2)

            While xReadStream(s2, buffer, bytesRead)
                ms2.Write(buffer, 0, bytesRead)
            End While

            ms2.Flush()


            s2.Close()
            ms.Close()

            Return ms2.ToArray
        End If
        Return "".xToByteArray

    End Function
    <System.Runtime.CompilerServices.Extension()> _
    Public Function xWrapWithQuotes(ByRef str As String) As String
        Return ControlChars.Quote & str & ControlChars.Quote
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xWrapWithSingleQuotes(ByRef str As String) As String
        Return "'" & str.Trim(ControlChars.Quote) & "'"
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xFillBlanks(ByRef str As String, ByVal ParamArray args() As String) As String
        Return String.Format(str, args)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xFillRecepientDetails(ByRef str As String, ByVal dt As DataTable) As String
        For Each col As DataColumn In dt.Columns
            str = str.Replace("[[_{0}]]".xFillBlanks(col.ColumnName), dt.Rows(0).xGetValue(col.ColumnName))
        Next

        Return str
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xFillAttachedDetails(ByRef str As String, ByVal dt As DataTable) As String
        For Each col As DataColumn In dt.Columns
            str = str.Replace("[[{0}]]".xFillBlanks(col.ColumnName), dt.Rows(0).xGetValue(col.ColumnName))
        Next

        Return str
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xWrapWithCommas(ByVal str As String) As String
        Return "," & str & ","
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xWrapWithPercentage(ByRef str As String) As String
        Return "%" & str & "%"
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xAddNewLine(ByRef str As String) As String
        Return str.Replace(ControlChars.NewLine, "<br>").Replace(Chr(10), "<br>").Replace(Chr(11), "<br>")
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xAddNewLineChar(ByRef str As String) As String
        Return str.Replace("\n", vbNewLine).Replace("<br>", vbNewLine)
    End Function


    <System.Runtime.CompilerServices.Extension()> _
    Public Function xLastPercentage(ByRef str As String) As String
        Return str & "%"
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xWildcarder(ByRef str As String) As String
        If Not String.IsNullOrEmpty(str) Then
            If str(0) = "*" Then str = "%" & str.TrimStart("*")
        End If

        Return str & "%"
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xTurnSingleQuotesToDouble(ByRef str As String) As String
        Return str.Replace("'", ControlChars.Quote)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToDataTable(ByRef b As Byte()) As DataTable
        Dim dt As New DataTable
        If b IsNot Nothing And b.Length > 0 Then
            Dim ms As New MemoryStream(b)
            dt.ReadXml(ms)
            ms.Close()
            ms.Dispose()
        End If

        Return dt
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToBytes(ByRef dt As DataTable) As Byte()
        Dim ms As New MemoryStream
        Dim bytes As Byte()

        dt.WriteXml(ms, XmlWriteMode.WriteSchema)

        bytes = ms.ToArray.xConvertToUtf8String.xToByteArray

        ms.Flush()
        ms.Close()

        Return bytes
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToLocalDate(ByRef str As String) As Date
        Dim d As Date
        If Date.TryParse(str, d) Then Return d

        Return Date.Now
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToLocalDateStr(ByRef d As Date) As String
        Return Format(d, "yyyy-MM-dd HH:mm:ss")
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Sub xSetValue(ByRef dr As DataRow, ByVal colname As String, ByVal val As String, Optional ByVal isAppend As Boolean = False)
        If Not dr.Table.Columns.Contains(colname) Then Return
        If String.IsNullOrWhiteSpace(val) Then dr(colname) = DBNull.Value : Return
        If val = "_" Then dr(colname) = " " : Return


        Select Case dr.Table.Columns(colname).DataType
            Case GetType(Date)
                dr(colname) = val.xToLocalDate
            Case Else
                If isAppend Then
                    dr(colname) = dr(colname) & val
                Else
                    Dim valdate As Date
                    If colname.IndexOf("Date") >= 0 AndAlso Date.TryParse(val, valdate) Then
                        val = Format(valdate, "d MMM yyyy")
                    Else
                        Dim maxLength As Integer = dr.Table.Columns(colname).MaxLength
                        If Not String.IsNullOrWhiteSpace(val) AndAlso maxLength > 0 AndAlso val.Length > maxLength Then
                            val = val.Substring(0, maxLength - 1)
                        End If
                    End If

                    dr(colname) = val
                End If
        End Select
    End Sub

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetValue(ByRef dr As DataRow, ByVal colname As String) As String
        If Not dr.Table.Columns.Contains(colname) Then Return ""
        If IsDBNull(dr(colname)) Then Return ""

        Select Case dr.Table.Columns(colname).DataType
            Case GetType(Date)
                Return CType(dr(colname), Date).xToLocalDateStr
            Case Else
                Return dr(colname).ToString
        End Select
    End Function


    <System.Runtime.CompilerServices.Extension()> _
    Public Function xConvertToASCIIString(ByRef b As Byte()) As String
        Return Encoding.ASCII.GetString(b).xCleanXml
    End Function
    <System.Runtime.CompilerServices.Extension()> _
    Public Function xConvertToUtf8String(ByRef b As Byte()) As String
        Return Encoding.UTF8.GetString(b).xCleanXml
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xFromXmlToJson(ByRef str As String) As String
        If Not String.IsNullOrWhiteSpace(str) Then
            Dim xnode As XDocument = XDocument.Parse(str.xCleanXml)
            Return JsonConvert.SerializeXNode(xnode)
        End If

        Return ""
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToJObj(ByRef xdoc As XDocument) As JObject
        Return JsonConvert.SerializeXNode(xdoc)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToJObj(ByRef xel As XElement) As JObject
        Dim jobj As New JObject

        If xel.HasAttributes Then
            For Each attr In xel.Attributes
                jobj.Add(attr.Name.LocalName, attr.Value)
            Next
        End If

        For Each el In xel.Elements
            If el.HasElements Then
                jobj.Add(el.Name.LocalName, el.xToJObj)
            Else
                jobj.Add(el.Name.LocalName, el.xValue)
            End If
        Next

        Return jobj
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xCleanXml(ByRef str As String) As String
        Dim replaceChars As New Regex("[^\u0009\u000A\u000D\u0020-\uD7FF\uE000\u10000-\u10FFFF]")
        Dim replacedChars As String = replaceChars.Replace(str, "").Replace("&", "")

        Return replacedChars
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xEncode(ByRef str As String) As String
        Return SecurityElement.Escape(str)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xNewLineToBR(ByRef str As String) As String
        Return str.Replace(Chr(10), "<br>")
    End Function


    <System.Runtime.CompilerServices.Extension()> _
    Public Function xSplitCapitals(ByRef str As String) As String
        Dim result As String = ""
        For i As Integer = 0 To str.Length - 1
            If i > 0 And Char.IsUpper(str(i)) Then result &= " "
            result &= str(i)
        Next
        Return result
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetPKField(ByRef entityID As String, appXml As XDocument) As String
        Dim entID As String = entityID

        Return (From x As XElement In appXml...<EntityPKs>.Elements Where x.@ID.ToLower = entID.ToLower Select x.@PKFieldName).FirstOrDefault
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetEntityID(ByRef PKValue As String, acctoken As String) As String
        Dim pfx As String = Left(PKValue, 2)
        Dim mycache = cache1.Get(acctoken)
        Dim xdoc As XDocument = mycache.AppXml
        Return (From x As XElement In xdoc...<EntityPKs>.Elements Where x.@PKPrefix.ToLower = pfx.ToLower Select x.@ID).FirstOrDefault
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xToFirstCapital(ByRef str As String) As String
        Dim pfx As String = Left(str, 1)

        Return pfx.ToUpper & str.Substring(1)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xIsPK(ByRef colname As String, appXml As XDocument) As Boolean
        Dim col As String = colname
        Dim entityID As String = (From x As XElement In appXml...<EntityPKs>.Elements Where x.@PKFieldName.ToLower = col.ToLower Select x.@ID).FirstOrDefault

        Return entityID IsNot Nothing
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xHasAttribute(ByRef el As XElement, ByVal att As String) As Boolean
        Return el.Attributes.Any(Function(c) c.Name.ToString.ToLower = att.ToLower)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xGetAttribute(ByRef el As XElement, ByVal att As String) As String
        If el.xHasAttribute(att) Then
            Return el.Attribute(att).Value.ToString
        End If

        Return ""
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xValue(ByRef el As XElement) As String
        If el IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(el.Value) Then Return el.Value.ToString

        Return ""
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xIsNotBlank(ByRef str As String) As Boolean
        Return Not String.IsNullOrEmpty(str)
    End Function

    <System.Runtime.CompilerServices.Extension()> _
    Public Function xIsBlank(ByRef str As String) As Boolean
        Return String.IsNullOrEmpty(str)
    End Function

    Private Function xReadStream(ByRef s As Stream, ByRef b As Byte(), ByRef readBytes As Integer) As Boolean
        readBytes = s.Read(b, 0, b.Length)
        Return readBytes > 0
    End Function

End Module


