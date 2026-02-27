$connStr = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=c:\Users\Administrator\Pictures\Test\Doan_group_6\cs464_project\QLCH.mdf;Integrated Security=True"
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
    $conn.Open()

    # Check Users
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT UserId, Username, FullName, RoleId, IsActive FROM Users"
    $reader = $cmd.ExecuteReader()
    Write-Host "Users:"
    while ($reader.Read()) {
        Write-Host "  Id=$($reader['UserId']) Username=$($reader['Username']) FullName=$($reader['FullName']) RoleId=$($reader['RoleId']) Active=$($reader['IsActive'])"
    }
    $reader.Close()

    # Check Roles
    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT * FROM Roles"
    $reader2 = $cmd2.ExecuteReader()
    Write-Host "`nRoles:"
    while ($reader2.Read()) {
        Write-Host "  Id=$($reader2['RoleId']) Code=$($reader2['RoleCode']) Name=$($reader2['RoleName'])"
    }
    $reader2.Close()

    # Check record counts
    foreach ($tbl in @("Categories","Customers","Employees","Products","Orders","OrderItems","StockMovements")) {
        $cmd3 = $conn.CreateCommand()
        $cmd3.CommandText = "SELECT COUNT(*) FROM [$tbl]"
        $count = $cmd3.ExecuteScalar()
        Write-Host "$tbl : $count rows"
    }

    $conn.Close()
} catch {
    Write-Host "ERROR: $($_.Exception.Message)"
}
