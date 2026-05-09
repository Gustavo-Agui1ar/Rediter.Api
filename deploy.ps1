$key = "C:\Users\gusta\ssh-oracle-key\private_key.key"

$server = "opc@163.176.243.127"

$dest = "/home/opc/rediter/"

Write-Host ""
Write-Host "==============================="
Write-Host " GERANDO BUILD"
Write-Host "==============================="
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao gerar build."
    exit
}

Write-Host ""
Write-Host "==============================="
Write-Host " LIMPANDO SERVIDOR"
Write-Host "==============================="
ssh -i $key $server "rm -rf /home/opc/rediter/*"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao limpar servidor."
    exit
}

Write-Host ""
Write-Host "==============================="
Write-Host " ENVIANDO ARQUIVOS"
Write-Host "==============================="
scp -i $key -r ./publish/* "${server}:${dest}"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao copiar arquivos."
    exit
}

Write-Host ""
Write-Host "==============================="
Write-Host " REINICIANDO API"
Write-Host "==============================="
ssh -i $key $server "sudo systemctl restart rediter"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao reiniciar API."
    exit
}

Write-Host ""
Write-Host "==============================="
Write-Host " DEPLOY FINALIZADO"
Write-Host "==============================="