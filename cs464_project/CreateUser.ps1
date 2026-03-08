param(
    [string]$Username = "nhanvien1",
    [string]$Password = "123456",
    [string]$FullName = "Nhan Vien 1",
    [int]$RoleId = 2   # 1=Admin, 2=Nhan vien
)

$salt = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($salt)

$passBytes = [System.Text.Encoding]::UTF8.GetBytes($Password)
$combined = New-Object byte[] ($salt.Length + $passBytes.Length)
[System.Buffer]::BlockCopy($salt, 0, $combined, 0, $salt.Length)
[System.Buffer]::BlockCopy($passBytes, 0, $combined, $salt.Length, $passBytes.Length)

$hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($combined)

$saltHex = "0x" + [BitConverter]::ToString($salt).Replace("-","")
$hashHex = "0x" + [BitConverter]::ToString($hash).Replace("-","")

$sql = "INSERT INTO Users(Username,PasswordHash,Salt,FullName,RoleId,IsActive,CreatedAt) VALUES(N'$Username',$hashHex,$saltHex,N'$FullName',$RoleId,1,GETDATE())"

sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d QLCH1 -Q $sql
Write-Host "Tao thanh cong: $Username / $Password (RoleId=$RoleId)"
