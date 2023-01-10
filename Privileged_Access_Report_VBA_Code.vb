ThisWorkBook

Public Sub Workbook_open()
    Dim wb          As Workbook
    Dim ws          As Worksheet
    
    Dim lResult     As Long
    Dim addin       As COMAddIn
    
    Set wb = ThisWorkbook
    
    Call OnStart
    
    For Each ws In ActiveWorkbook.Worksheets
        ActiveSheet.Unprotect
        ws.Cells.Locked = False
        ws.Cells.FormulaHidden = False
    Next ws
    
    For Each addin In Application.COMAddIns
        If addin.progID = "SapExcelAddIn" Then
            If addin.Connect = False Then
                addin.Connect = True
            ElseIf addin.Connect = True Then
                addin.Connect = False
                addin.Connect = True
            End If
        End If
    Next
    
    lResult = Application.Run("SAPLogOff", "True")
    lResult = Application.Run("SAPExecuteCommand", "Refresh", "All")
    lResult = Application.Run("SAPSetRefreshBehaviour", "Off")
    lResult = Application.Run("SAPExecuteCommand", "Show", "Ribbon", "All")
    lResult = Application.Run("SAPExecuteCommand", "Show", "TaskPane", "Default")
    
    wb.Activate
    wb.Sheets("Edit").Activate
    ActiveSheet.Range("A1").Activate
    
    Call DataValidationList
    
'    MsgBox "Before working With ""Edit"" tab, please make sure You are connected to the system"
    
    ActiveSheet.Range("A1").Activate
    
    Call OnEnd
    
End Sub

Private Sub Workbook_SheetActivate(ByVal Sh As Object)
    
    Dim lResult     As Long
    Dim lRefreshDate As Double
    Dim lDS_Alias   As String
    
    Dim AckTime As Integer, InfoBox As Object
    Set InfoBox = CreateObject("WScript.Shell")
    AckTime = 5
    Select Case InfoBox.Popup("Refresh on tab """ & Sh.Name & """ in progress...", _
    AckTime, "Data refresh", 0)
    End Select
    
'    MsgBox Prompt:="Data refresh on tab """ & Sh.Name & """ in progress..."
    
    Call OnStart
    
    lResult = Application.Run("SAPSetRefreshBehaviour", "On")
    
    If Sh.Name = "Edit" Then
        lDS_Alias = "DS_3"
    ElseIf Sh.Name = "Export" Then
        lDS_Alias = "DS_2"
        Call Remove_Hash_Characters
    ElseIf Sh.Name = "Changelog" Then
        lDS_Alias = "DS_4"
    ElseIf Sh.Name = "Sensitive Profiles" Then
        lDS_Alias = "DS_5"
    End If
    
    lResult = Application.Run("SAPGetProperty", "IsConnected", lDS_Alias)
    
    If lResult = True Then
        
        StartTime = Timer
        
        lResult = Application.Run("SAPExecuteCommand", "Restart", lDS_Alias)
        lResult = Application.Run("SAPExecuteCommand", "RefreshData", lDS_Alias)
        lResult = Application.Run("SAPSetRefreshBehaviour", "Off")
        
        lRefreshDate = Application.Run("SAPGetSourceInfo", lDS_Alias, "QueryLastRefreshedAt")
        For Each ws In Application.ActiveWorkbook.Worksheets
            ws.PageSetup.CenterHeader = "QueryLastRefreshedAt: " & Format(lRefreshDate, "dddd, mmmm d, yyyy h:mm:ss")
        Next
        
        If Sh.Name = "Edit" Then
            Call DataValidationList
        End If
        
        EndTime = Timer
        '        wb.Sheets("Edit").Range("E1").Value = Format((EndTime - StartTime) / 86400, "hh:mm:ss") & " [hh:mm:ss]"
        MsgBox "Data refreshed in : " & Format((EndTime - StartTime) / 86400, "hh:mm:ss") & " [hh:mm:ss]"
        
    Else
        MsgBox "You are Not connected To the system"
    End If
    
'    MsgBox Prompt:="Data refresh on tab """ & Sh.Name & """ finished..."
    
End Sub

-------------
Sheet3(Edit)

Private Sub CommandButton1_Click()
    
    Dim lResult     As Long
    Dim range_1 As Range
    
    Call OnStart
    
    lResult = Application.Run("SAPGetProperty", "IsConnected", "DS_2")
    If lResult = False Then
        
        ThisWorkbook.Sheets("Edit").Activate
        ActiveSheet.Range("A1").Activate
        
        lResult = Application.Run("SAPLogOff", "True")
        lResult = Application.Run("SAPExecuteCommand", "Refresh", "ALL")
        lResult = Application.Run("SAPSetRefreshBehaviour", "Off")
        
        Call DataValidationList
        
        ActiveSheet.Range("A1").Activate
        
    Else
        MsgBox "You are already connected to the system"
    End If
    
    Call OnEnd
    
End Sub

Private Sub CommandButton2_Click()
    
    Dim lResult     As Long
    Dim lRefreshDate As Double
    Dim StartTime   As Double
    Dim EndTime     As Double
    Dim wb          As Workbook
    
    Set wb = ThisWorkbook
    
    Call OnStart
    
    lResult = Application.Run("SAPGetProperty", "IsConnected", "DS_2")
    If lResult = True Then
        
        StartTime = Timer
        
        lResult = Application.Run("SAPSetRefreshBehaviour", "On")
        lResult = Application.Run("SAPExecuteCommand", "PlanDataSave")
'        lResult = Application.Run("SAPExecuteCommand", "Restart", "DS_2")
'        lResult = Application.Run("SAPExecuteCommand", "RefreshData", "ALL")
'
        lResult = Application.Run("SAPSetRefreshBehaviour", "Off")
'
'        lRefreshDate = Application.Run("SAPGetSourceInfo", "DS_2", "QueryLastRefreshedAt")
'        For Each ws In Application.ActiveWorkbook.Worksheets
'            ws.PageSetup.CenterHeader = "QueryLastRefreshedAt: " & Format(lRefreshDate, "dddd, mmmm d, yyyy h:mm:ss")
'        Next
    
        Call DataValidationList
        
'        ThisWorkbook.Sheets("Export").Activate
        
        Call Remove_Hash_Characters
        
        ActiveSheet.Range("A1").Activate
        
        EndTime = Timer
        wb.Sheets("Edit").Range("E1").Value = Format((EndTime - StartTime) / 86400, "hh:mm:ss") & " [hh:mm:ss]"
        MsgBox "Data saved in : " & Format((EndTime - StartTime) / 86400, "hh:mm:ss") & " [hh:mm:ss]"
    
    Else
        
        MsgBox "You are not connected to the system"
        
    End If
    
    Call OnEnd
    
End Sub