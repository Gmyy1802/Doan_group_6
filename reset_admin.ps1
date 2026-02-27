$connStr = "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=c:\Users\Administrator\Pictures\Test\Doan_group_6\cs464_project\QLCH.mdf;Integrated Security=True"
$password = "123"

# Generate salt
$salt = New-Object byte[] 16
$rng = [System.Security.Cryptography.RNGCryptoServiceProvider]::new()
$rng.GetBytes($salt)

# Hash: SHA256(salt + password)
$sha = [System.Security.Cryptography.SHA256]::Create()
$pwBytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$combined = $salt + $pwBytes
$hash = $sha.ComputeHash($combined)

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "UPDATE Users SET PasswordHash = @hash, Salt = @salt WHERE Username = 'admin'"
$cmd.Parameters.Add("@hash", [System.Data.SqlDbType]::VarBinary, 64).Value = $hash
$cmd.Parameters.Add("@salt", [System.Data.SqlDbType]::VarBinary, 32).Value = $salt
$rows = $cmd.ExecuteNonQuery()
Write-Host "Updated $rows row(s). Admin password set to '$password'"
$conn.Close()
