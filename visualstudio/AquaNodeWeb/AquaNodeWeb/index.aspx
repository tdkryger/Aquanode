<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="AquaNodeWeb.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="/style/style1.css" />
    <title>AquaNode</title>
</head>
<body>
    <form id="form1" runat="server">
        <h1>AquaNode</h1>
        <div>
            <asp:Literal ID="litLocal" runat="server"></asp:Literal><br />
            <!--            <asp:Literal ID="litMySQLVersion" runat="server"></asp:Literal> -->
        </div>
        <div>
            <asp:Literal ID="litTankSelectorLabel" runat="server">Select tank</asp:Literal>&nbsp;&nbsp;
            <asp:DropDownList ID="ddlSelectTank" runat="server" OnSelectedIndexChanged="ddlSelectTank_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
        </div>
        <asp:Table ID="tableTank" runat="server" Visible="false" HorizontalAlign="Center">
            <asp:TableHeaderRow>
                <asp:TableHeaderCell>
                    <asp:Literal ID="litTankName" runat="server"></asp:Literal>&nbsp;
                    <asp:Label ID="lTankName" runat="server" Text="Label"></asp:Label>
                </asp:TableHeaderCell>
                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell>
                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell>
                <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell RowSpan="4">
                    <asp:Label ID="lTankDescription" runat="server" Text="Label"></asp:Label>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell RowSpan="4" HorizontalAlign="Center" >Targets:</asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>pH</asp:TableCell>
                <asp:TableCell>&nbsp;</asp:TableCell>
                <asp:TableCell>Temperature</asp:TableCell>
                <asp:TableCell>&nbsp;</asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </form>
</body>
</html>
