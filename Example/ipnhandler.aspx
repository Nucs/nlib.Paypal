<%@ Page  AutoEventWireup="false" CodeFile="ipnhandler.aspx.cs" Inherits="IpnHandler" Language="C#"%>
<% Response.Cache.SetCacheability(HttpCacheability.NoCache); %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <%= Export ?? "" %>
</body>
</html>
