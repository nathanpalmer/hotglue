<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HotGlue.Demos.Templates.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HotGlue Compiler Demo - TypeScript</title>
    <style>
        section {
            border: 1px solid black;
            padding: 10px;
            background: lightgray;
            width: 60%;
        }
    </style>
    <%= HotGlue.Script.Reference("Module1/app.js") %>
</head>
<body>
    <form id="form1" runat="server">
        <h1>jQuery Template</h1>
        <section>
            <div id="jquery"></div>
        </section>
        <h1>Mustache Template</h1>
        <section>
            <div id="mustache"></div>
        </section>
        <h1>Handlebars Template</h1>
        <section>
            <div id="handlebars"></div>
        </section>
        <h1>Dust Template</h1>
        <section>
            <div id="dust"></div>
        </section>
    </form>
</body>
</html>