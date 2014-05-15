Imports System.Web.Http

Public Class WebApiApplication
    Inherits System.Web.HttpApplication

    Sub Application_Start()
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
    End Sub

    Sub Application_BeginRequest(sender As Object, e As EventArgs)
        'HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*")

        'If (HttpContext.Current.Request.HttpMethod = "OPTIONS") Then

        '    'These headers are handling the "pre-flight" OPTIONS call sent by the browser
        '    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "HEAD, GET, POST, PUT, DELETE")
        '    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept")
        '    HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000")
        '    HttpContext.Current.Response.End()
        'End If


    End Sub
End Class
