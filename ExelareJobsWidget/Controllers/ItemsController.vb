Imports System.Net
Imports System.Web.Http
Imports Newtonsoft.Json.Linq
Imports System.Web.Http.Cors

Public Class ItemsController
    Inherits ApiController

    ' GET api/<controller>
    Public Function GetValues() As IEnumerable(Of String)
        Return New String() {"huy1", "huy2"}
    End Function

    ' GET api/<controller>/5
    Public Function GetValue(ByVal id As Integer) As String
        Return "value"
    End Function

    ' POST api/<controller>
    <HttpPost()>
    Public Function PostValue(jobj As JObject) As Object
        If Login(jobj("companyId"), jobj("userId")) Then
            Dim respDoc As XDocument = CBSServiceFactory.ItemMgr.GetItem(jobj("reqIntId")).xDecompress.xToXDoc

            Return respDoc.Root.Elements.Skip(1).Take(1)(0).xToJObj
        End If

        Return New With {.ErrorMsg = "Login Failed"}
    End Function

    ' PUT api/<controller>/5
    Public Sub PutValue(ByVal id As Integer, <FromBody()> ByVal value As String)

    End Sub

    ' DELETE api/<controller>/5
    Public Sub DeleteValue(ByVal id As Integer)

    End Sub
End Class
