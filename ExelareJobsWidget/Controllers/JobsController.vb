Imports System.Net
Imports System.Web.Http
Imports Newtonsoft.Json.Linq
Imports System.Web.Http.Cors
Imports ExelareJobsWidget.svcUserMgr
Imports ExelareJobsWidget.Models

Public Class JobsController
    Inherits ApiController

    ' GET api/<controller>
    Public Function GetValues() As String
        Return "value"
    End Function

    ' GET api/<controller>/5
    Public Function GetValue(ByVal id As Integer) As String
        Return "value"
    End Function

    ' POST api/<controller>
    <HttpPost()>
    Public Function PostValue(ByVal dq As JObject) As Object
        Dim jobj As New JObject
        HttpContext.Current.Trace.Write("Logging in")
        If Login(dq("CompanyId"), dq("UserId")) Then
            Dim req As New svcDataMgr.dcDataReq

            HttpContext.Current.Trace.Write("Finished Login")

            req.EntityID = "Requirements"
            req.Which = "DView"
            req.WhichID = "uuJobsWidget"
            req.PageSize = dq("PageSize")
            req.PageNumber = dq("PageNumber")
            req.NeedData = True
            req.NeedSchema = True
            req.NeedRecordCount = True


            If dq.xHasProperty("FilterBy") Then
                Dim el As XElement = <FilterBy></FilterBy>

                el.Add(dq("FilterBy").xToJObject.xGetFilterXml)

                req.FilterBy = el.ToString
            End If



            If dq.xHasProperty("OrderBy") Then
                req.OrderBy = dq("OrderBy").xToJObject.xGetOrderXml.ToString
            End If

            Dim resp As svcDataMgr.dcDataResp
            Try
                HttpContext.Current.Trace.Write("Getting page")
                resp = CBSServiceFactory.DataMgr.GetPage(req)
            Catch ex As Exception
                Return New With {.ErrorMsg = ex.Message}
            End Try
            HttpContext.Current.Trace.Write("creating get page response")
            Dim getPageResp As New CBSGetPageResponse(resp, req)

            Return getPageResp
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
