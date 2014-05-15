Imports System.Net
Imports System.Web.Http
Imports Newtonsoft.Json.Linq
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Web
Imports System.Net.Http
Imports System.IO
Imports System.Xml
Imports System.Web.Http.Cors


Public Class FormController
    Inherits ApiController

    ' GET api/<controller>
    Public Function GetValues() As IEnumerable(Of String)
        Return New String() {"value1", "value2"}
    End Function

    ' GET api/<controller>/5
    Public Function GetValue(ByVal id As Integer) As String
        Return "value"
    End Function

    ' POST api/<controller>
    <HttpPost()>
    Public Async Function PostValue() As Task(Of HttpResponseMessage)
        If (Not Request.Content.IsMimeMultipartContent()) Then
            Throw New HttpResponseException(HttpStatusCode.UnsupportedMediaType)
        End If

        Dim tempFolder As String = Path.GetTempPath
        Dim provider As New MultipartFormDataStreamProvider(tempFolder)

        Try
            Await Request.Content.ReadAsMultipartAsync(provider)

            Dim applyReq As XElement = <CandidateSubmitRequest></CandidateSubmitRequest>
            Dim formData As NameValueCollection = provider.FormData
            Dim resumeFile As MultipartFileData = Nothing

            If provider.FileData.Count > 0 Then
                resumeFile = provider.FileData(0)
                applyReq.Add(<ResumeInfo>
                                 <FileName><%= resumeFile.Headers.ContentDisposition.FileName.Trim(ControlChars.Quote) %></FileName>
                                 <FileContent><%= Convert.ToBase64String(File.ReadAllBytes(resumeFile.LocalFileName)) %></FileContent>
                             </ResumeInfo>)
            End If

            applyReq.@SubmitType = formData("AccountInfo.SubmitType")

            applyReq.Add(<AccountInfo>
                             <CompanyID><%= formData("AccountInfo.CompanyID") %></CompanyID>
                             <UserID><%= formData("AccountInfo.UserID") %></UserID>
                         </AccountInfo>)

            applyReq.Add(<JobInfo>
                             <ReqID><%= formData("JobInfo.ReqID") %></ReqID>
                         </JobInfo>)

            Dim candInfo As XElement = <CandidateInfo></CandidateInfo>

            For Each key As String In formData.Keys
                If key.StartsWith("CandidateInfo.") Then
                    candInfo.Add(New XElement(key.Replace("CandidateInfo.", ""), formData(key)))
                End If
            Next

            applyReq.Add(candInfo)

            Dim req As WebRequest = WebRequest.Create("http://jobs.cbizsoft.com/ApplyWithService/submitCandidate.aspx")


            req.Method = "POST"
            req.ContentType = "text/xml; encoding='utf-8'"

            Dim reqStream As Stream = req.GetRequestStream
            Dim xmlWriter As XmlWriter = xmlWriter.Create(reqStream)

            applyReq.WriteTo(xmlWriter)

            xmlWriter.Flush()

            req.GetResponse()

            If resumeFile IsNot Nothing AndAlso File.Exists(resumeFile.LocalFileName) Then
                File.Delete(resumeFile.LocalFileName)
            End If

            Return Request.CreateResponse(HttpStatusCode.OK)
        Catch ex As Exception
            Return Request.CreateResponse(HttpStatusCode.InternalServerError, ex)
        End Try
    End Function

    ' PUT api/<controller>/5
    Public Sub PutValue(ByVal id As Integer, <FromBody()> ByVal value As String)

    End Sub

    ' DELETE api/<controller>/5
    Public Sub DeleteValue(ByVal id As Integer)

    End Sub
End Class
