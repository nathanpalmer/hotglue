<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HotGlue.Demos.Compilers.TypeScript.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HotGlue Compiler Demo - TypeScript</title>
    <%= HotGlue.Script.Reference("TypeScriptModule1/app.js") %>
</head>
<body>
    <form id="form1" runat="server">
    <div id="container"></div>
    </form>
</body>
</html>
