$connStr = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=c:\Users\Administrator\Pictures\Test\Doan_group_6\cs464_project\QLCH.mdf;Integrated Security=True"
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
    $conn.Open()
    Write-Host "Connected OK"

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
    $reader = $cmd.ExecuteReader()
    Write-Host "`nTables:"
    while ($reader.Read()) {
        Write-Host "  - $($reader[0])"
    }
    $reader.Close()

    # Get columns for each table
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_NAME, ORDINAL_POSITION"
    $reader2 = $cmd2.ExecuteReader()
    Write-Host "`nColumns:"
    $currentTable = ""
    while ($reader2.Read()) {
        $tbl = $reader2["TABLE_NAME"]
        if ($tbl -ne $currentTable) {
            Write-Host "`n[$tbl]"
            $currentTable = $tbl
        }
        $col = $reader2["COLUMN_NAME"]
        $dt = $reader2["DATA_TYPE"]
        $len = $reader2["CHARACTER_MAXIMUM_LENGTH"]
        $nullable = $reader2["IS_NULLABLE"]
        if ($len -and $len -ne [DBNull]::Value) {
            Write-Host "  $col ($dt($len)) nullable=$nullable"
        } else {
            Write-Host "  $col ($dt) nullable=$nullable"
        }
    }
    $reader2.Close()

    $conn.Close()
} catch {
    Write-Host "ERROR: $($_.Exception.Message)"
}
