<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HotGlue.Demos.Templates.Mustache.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HotGlue Template Demo - Mustache</title>
    <%= HotGlue.Script.Reference("HandlebarsModule1/app.js") %>
</head>
<body>
    <form id="form1" runat="server">
    <div id="container"></div>
    </form>
</body>
</html>
