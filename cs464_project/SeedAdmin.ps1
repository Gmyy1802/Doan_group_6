$salt = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($salt)

$passwordBytes = [System.Text.Encoding]::UTF8.GetBytes("admin123")
$saltedPassword = New-Object byte[] ($salt.Length + $passwordBytes.Length)
[System.Buffer]::BlockCopy($salt, 0, $saltedPassword, 0, $salt.Length)
[System.Buffer]::BlockCopy($passwordBytes, 0, $saltedPassword, $salt.Length, $passwordBytes.Length)
$sha = [System.Security.Cryptography.SHA256]::Create()
$hash = $sha.ComputeHash($saltedPassword)

$saltHex = "0x" + [BitConverter]::ToString($salt).Replace("-","")
$hashHex = "0x" + [BitConverter]::ToString($hash).Replace("-","")

$sql = "INSERT INTO [dbo].[Users] ([Username],[PasswordHash],[Salt],[FullName],[Email],[Phone],[RoleId],[IsActive],[CreatedAt]) VALUES (N'admin', $hashHex, $saltHex, N'Administrator', N'admin@qlch.com', N'0123456789', 1, 1, GETDATE());"

Write-Host "Running SQL insert..."
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d QLCH1 -Q $sql
Write-Host "Done! Login: admin / admin123"
